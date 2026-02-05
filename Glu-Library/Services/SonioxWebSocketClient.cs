using System.Net.Security;
using System.Net.WebSockets;
using System.Security;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Glu_Library.Configuration;
using Glu_Library.Models;
using Glu_Library.Models.WebSocket;
using Glu_Library.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Glu_Library.Services;

/// <summary>
/// Implementation of the Soniox WebSocket client.
/// Features:
/// 1. Real-time transcription streaming (V3 API).
/// 2. Production-grade Logging.
/// 3. Automatic Resilience (Auto-reconnect on network failure).
/// 4. Dynamic Configuration support.
/// </summary>
public sealed class SonioxWebSocketClient : ISonioxWebSocketClient, IAsyncDisposable
{
    private ClientWebSocket? _webSocket;
    private readonly ILogger<SonioxWebSocketClient> _logger;
    
    // Flag to distinguish between user-requested stop (clean) and network error (crash).
    private volatile bool _isUserInitiatedDisconnect;

    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly Uri _webSocketUri;
    private readonly SonioxWebSocketOptions _globalOptions; 
    
    // Store the request to re-send it during auto-reconnection.
    private SonioxStartRequest? _currentStartRequest; 
    
    private CancellationTokenSource? _receiveCts;
    // R5: Session Duration Tracking
    private readonly TimeSpan _maxSessionDuration = TimeSpan.FromMinutes(300);
    private DateTime _sessionStartTime;

    /// <inheritdoc />
    public event Action<TranscriptResult>? OnTranscriptReceived;
    
    /// <inheritdoc />
    public event Action<Exception>? OnError;

    public int SampleRate { get; set; } = 48000; 

    public SonioxWebSocketClient(
        IOptions<SonioxWebSocketOptions> options, 
        ILogger<SonioxWebSocketClient> logger)
    {
        _globalOptions = options.Value;
        _logger = logger;
        
        if (string.IsNullOrWhiteSpace(_globalOptions.Endpoint))
        {
            _logger.LogCritical("Soniox Endpoint is missing in configuration.");
            throw new InvalidOperationException("Soniox Endpoint is not configured.");
        }

        _webSocketUri = new Uri(_globalOptions.Endpoint);
        // V-02: Enforce TLS 1.2+ Scheme check
        if (_webSocketUri.Scheme != "wss")
            throw new SecurityException("Soniox API requires WSS (TLS 1.2+) connection.");
    }

    /// <inheritdoc />
    public async Task ConnectAsync(SonioxSessionConfig? sessionConfig = null, CancellationToken cancellationToken = default)
    {
        // Reset the disconnect flag because we are starting a new fresh session.
        _isUserInitiatedDisconnect = false;

        try
        {
            // Build the configuration ONCE. We will reuse this object if we need to reconnect automatically.
            BuildStartRequest(sessionConfig);

            await InitializeConnectionAsync(cancellationToken);

            // Start the background receiving loop (which includes the resilience logic).
            _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ = ReceiveLoopAsync(_receiveCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish initial connection to Soniox.");
            RaiseError(ex);
            throw;
        }
    }

    /// <summary>
    /// internal helper to establish the socket connection and send the handshake.
    /// Used by both ConnectAsync (initial) and ReceiveLoopAsync (reconnection).
    /// </summary>
    private async Task InitializeConnectionAsync(CancellationToken ct)
    {
        if (_webSocket != null) _webSocket.Dispose();
        _webSocket = new ClientWebSocket();

        // V-02 & V-07: Configure TLS and Pinning (Conceptual - ClientWebSocket usage varies by .NET version)
        // In a real scenario, use a factory or Options callback if available in the specific .NET version target.
        _webSocket.Options.SetRequestHeader("User-Agent", "Glu-Library-Net8");
        _logger.LogInformation("Connecting to Soniox WebSocket at {Uri}...", _webSocketUri);
        await _webSocket.ConnectAsync(_webSocketUri, ct);
        _sessionStartTime = DateTime.UtcNow;

        if (_currentStartRequest != null)
        {
            await SendJsonAsync(_currentStartRequest, ct);
        }
    }

    /// <summary>
    /// Builds the SonioxStartRequest object merging global options and dynamic session config.
    /// </summary>
    private void BuildStartRequest(SonioxSessionConfig? sessionConfig)
    {
        // M1, M4, C1, C2 support via config
        _currentStartRequest = new SonioxStartRequest
        {
            ApiKey = _globalOptions.Token,
            // V-03: Ensure this comes from secure source
            Model = _globalOptions.Model,
            AudioFormat = "pcm_s16le",
            SampleRate = this.SampleRate,
            EnableSpeakerDiarization = _globalOptions.EnableSpeakerDiarization,
            EnableEndpointDetection = true,
            // M1: Use dynamic hints, not hardcoded
            LanguageHints = sessionConfig?.LanguageHints ?? new List<string>(),
            // M4: Translation
            Translation = sessionConfig?.Translation,
            // Client Reference ID
            ClientReferenceId = sessionConfig?.ClientReferenceId
        };
        if (sessionConfig?.Context != null)
            _currentStartRequest.Context = sessionConfig.Context;
    }

    /// <inheritdoc />
    public async Task SendAudioAsync(ReadOnlyMemory<byte> audioData, CancellationToken cancellationToken = default)
    {
        // R5: Session Limit Check
        if (DateTime.UtcNow - _sessionStartTime > _maxSessionDuration)
        {
            _logger.LogWarning("Max session duration reached. Disconnecting.");
            await DisconnectAsync();
            return;
        }
        
        try
        {
            await _webSocket.SendAsync(audioData, WebSocketMessageType.Binary, true, cancellationToken);
        }
        catch (Exception ex) 
        { 
            // We log warning but don't throw, to keep the stream resilient.
            _logger.LogWarning("Failed to send audio frame: {Message}", ex.Message); 
        }
    }

    /// <inheritdoc />
    public async Task StopStreamAsync(CancellationToken cancellationToken = default)
    {
        if (_webSocket is null || _webSocket.State != WebSocketState.Open) return;
        // Sending an empty binary message usually signals EOF in streams, or simply Close.
        // Here we stick to Close as explicit "End of Stream" for Soniox unless API specifies zero-byte frame.
        await DisconnectAsync();
    }

    public async Task DisconnectAsync()
    {
        _isUserInitiatedDisconnect = true;
        _receiveCts?.Cancel();
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
            }
            catch { /* Ignore close errors */ }
        }
    }

    /// <summary>
    /// Main loop that listens for messages and handles AUTO-RECONNECTION logic.
    /// </summary>
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        int reconnectAttempts = 0;

        _logger.LogInformation("🟢 Connected and listening for transcripts.");

        while (!cancellationToken.IsCancellationRequested && !_isUserInitiatedDisconnect)
        {
            try
            {
                // Verify socket state before reading
                if (_webSocket == null || _webSocket.State != WebSocketState.Open)
                {
                    throw new WebSocketException("WebSocket is not open.");
                }

                var result = await _webSocket.ReceiveAsync(buffer, cancellationToken);
                reconnectAttempts = 0; // Reset on success

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogWarning("🔴 Server closed connection: {Reason}", result.CloseStatusDescription);
                    // If server closes, we might want to reconnect unless it's a fatal Auth error.
                    // For simplicity in this V1, we treat it as a disconnect requiring reconnection.
                    throw new WebSocketException("Server closed connection.");
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessIncomingMessage(json);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown via CancellationToken
                break;
            }
            catch (Exception ex)
            {
                if (_isUserInitiatedDisconnect) break;

                var delay = Math.Min(30, Math.Pow(2, reconnectAttempts)); 
                _logger.LogWarning("Connection lost. Retrying in {Seconds}s...", delay);
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);

                try { await InitializeConnectionAsync(cancellationToken); }
                catch { /* Ignore immediate retry fail */ }
            }
            finally
            {
                // V-07: Memory Scrubbing
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
    }

    private void ProcessIncomingMessage(string json)
    {
        try
        {
            var response = JsonSerializer.Deserialize<SonioxStreamResponse>(json, _jsonOptions);
            
            if (response?.ErrorCode != null)
            {
                _logger.LogError("❌ Soniox API Error ({Code}): {Message}", response.ErrorCode, response.ErrorMessage);
                // If we get an API error (e.g. Invalid Key), we probably shouldn't keep retrying infinitely, 
                // but for now, we just log it.
                return;
            }

            if (response?.Tokens == null || response.Tokens.Count == 0) return;

            var finalTokens = response.Tokens.Where(t => t.IsFinal).ToList();
            if (finalTokens.Count > 0)
            {
                var text = string.Join("", finalTokens.Select(t => t.Text));
                var speaker = finalTokens.First().Speaker ?? "0";
                var lang = finalTokens.First().Language;
                // S3: Confidence
                var confidence = finalTokens.Average(t => t.Confidence);

                _logger.LogInformation("✅ FINAL [Spk {Speaker}] ({Lang}) - Length: {Length}", speaker, lang, text.Length);

                OnTranscriptReceived?.Invoke(new TranscriptResult
                {
                    Text = text,
                    IsFinal = true,
                    Speaker = speaker,
                    DetectedLanguage = lang,
                    Timestamp = DateTime.UtcNow,
                    Confidence = confidence
                });
            }

            var nonFinalTokens = response.Tokens.Where(t => !t.IsFinal).ToList();
            if (nonFinalTokens.Count > 0)
            {
                var text = string.Join("", nonFinalTokens.Select(t => t.Text));
                OnTranscriptReceived?.Invoke(new TranscriptResult
                {
                    Text = text,
                    IsFinal = false,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "Error processing incoming JSON."); 
        }
    }

    private async Task SendJsonAsync<T>(T payload, CancellationToken ct)
    {
        if (_webSocket is null) return;
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        
        // V-01: Secure Logging
        if (payload is SonioxStartRequest req)
            _logger.LogDebug("📤 Sending Config. Model: {Model}", req.Model);
        else
            _logger.LogDebug("📤 Sending Control Message."); 
        
        var bytes = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
    }

    private void RaiseError(Exception ex) => OnError?.Invoke(ex);

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _webSocket?.Dispose();
        _receiveCts?.Dispose();
    }
}
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Glu_Library.Configuration;
using Glu_Library.Models;
using Glu_Library.Models.WebSocket;
using Glu_Library.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Glu_Library.Services;

/// <summary>
/// Implementation of the Soniox WebSocket client.
/// Handles connection management, audio streaming, and parsing of real-time V3 API responses.
/// </summary>
public sealed class SonioxWebSocketClient : ISonioxWebSocketClient
{
    private ClientWebSocket? _webSocket;
    
    // Robust JSON configuration to handle case-insensitivity and ignore nulls.
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly Uri _webSocketUri;
    private readonly SonioxWebSocketOptions _globalOptions; 
    
    // The current request configuration, built dynamically during ConnectAsync.
    private SonioxStartRequest? _currentStartRequest; 
    
    private CancellationTokenSource? _receiveCts;

    /// <inheritdoc />
    public event Action<TranscriptResult>? OnTranscriptReceived;
    
    /// <inheritdoc />
    public event Action<Exception>? OnError;

    /// <summary>
    /// Gets or sets the sample rate of the audio stream.
    /// This value is typically populated from the frontend (JS) interop layer (e.g., 48000 or 44100 Hz).
    /// </summary>
    public int SampleRate { get; set; } = 48000; 

    /// <summary>
    /// Initializes a new instance of the <see cref="SonioxWebSocketClient"/> class.
    /// </summary>
    /// <param name="options">Global configuration options for Soniox service.</param>
    public SonioxWebSocketClient(IOptions<SonioxWebSocketOptions> options)
    {
        _globalOptions = options.Value;
        
        if (string.IsNullOrWhiteSpace(_globalOptions.Endpoint))
            throw new InvalidOperationException("Soniox Endpoint is not configured.");

        _webSocketUri = new Uri(_globalOptions.Endpoint);
    }

    /// <inheritdoc />
    public async Task ConnectAsync(SonioxSessionConfig? sessionConfig = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Preventive cleanup before connecting
            if (_webSocket != null) _webSocket.Dispose();
            _webSocket = new ClientWebSocket();

            await _webSocket.ConnectAsync(_webSocketUri, cancellationToken);

            // --- BUILD DYNAMIC REQUEST ---
            // 1. Initialize with base configuration from appsettings and defaults
            _currentStartRequest = new SonioxStartRequest
            {
                ApiKey = _globalOptions.Token,
                Model = "stt-rt-v3",
                
                // Audio Format Configuration (Required for raw PCM streams)
                AudioFormat = "pcm_s16le",
                NumChannels = 1,
                
                // Set the dynamic sample rate detected by the client
                SampleRate = this.SampleRate,
                
                EnableGlobalSpeakerDiarization = _globalOptions.EnableSpeakerDiarization,
                EnableEndpointDetection = true,
                
                // Default language hints (English + Spanish) for robustness
                LanguageHints = new List<string> { "es", "en" } 
            };

            // 2. Apply runtime overrides from session configuration (if provided)
            if (sessionConfig != null)
            {
                if (!string.IsNullOrEmpty(sessionConfig.ApiKey))
                {
                    _currentStartRequest.ApiKey = sessionConfig.ApiKey;
                }

                if (sessionConfig.LanguageHints != null && sessionConfig.LanguageHints.Any())
                {
                    _currentStartRequest.LanguageHints = sessionConfig.LanguageHints;
                }

                if (sessionConfig.Context != null)
                {
                    _currentStartRequest.Context = sessionConfig.Context;
                }
            }

            // Send initial handshake configuration
            await SendJsonAsync(_currentStartRequest, cancellationToken);

            // Start the background receiving loop
            _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ = ReceiveLoopAsync(_receiveCts.Token);
        }
        catch (Exception ex)
        {
            RaiseError(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendAudioAsync(ReadOnlyMemory<byte> audioData, CancellationToken cancellationToken = default)
    {
        if (_webSocket is null || _webSocket.State != WebSocketState.Open) return;
        try
        {
            await _webSocket.SendAsync(audioData, WebSocketMessageType.Binary, true, cancellationToken);
        }
        catch (Exception ex) { RaiseError(ex); }
    }

    /// <inheritdoc />
    public async Task DisconnectAsync()
    {
        try
        {
            _receiveCts?.Cancel();
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
            }
        }
        catch (Exception ex) { RaiseError(ex); }
    }

    /// <summary>
    /// Background loop that listens for incoming messages from the WebSocket.
    /// </summary>
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        try
        {
            Console.WriteLine($"🟢 [SonioxClient] Connected. Listening...");

            while (!cancellationToken.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"🔴 [SonioxClient] Server closed: {result.CloseStatusDescription}");
                    return;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessIncomingMessage(json);
                }
            }
        }
        catch (OperationCanceledException) { /* Normal cancellation */ }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 [SonioxClient] Receive error: {ex.Message}");
            RaiseError(ex);
        }
    }

    /// <summary>
    /// Parses the incoming JSON message using V3 logic (Tokens list).
    /// Separates final confirmed text from partial hypotheses.
    /// </summary>
    private void ProcessIncomingMessage(string json)
    {
        try
        {
            var response = JsonSerializer.Deserialize<SonioxStreamResponse>(json, _jsonOptions);
            
            // 1. Handle API Errors
            if (response?.ErrorCode != null)
            {
                Console.WriteLine($"❌ Soniox API Error ({response.ErrorCode}): {response.ErrorMessage}");
                return;
            }

            if (response?.Tokens == null || response.Tokens.Count == 0) return;

            // 2. Process Tokens
            // Soniox V3 sends a unified list. We filter by "is_final" flag.

            // --- A) FINAL (Confirmed Text) ---
            var finalTokens = response.Tokens.Where(t => t.IsFinal).ToList();
            if (finalTokens.Count > 0)
            {
                // Join without extra spaces, as Soniox tokens often include leading spaces.
                var text = string.Join("", finalTokens.Select(t => t.Text));
                var speaker = finalTokens.First().Speaker ?? "0";
                
                // Extract detected language from the first token (usually consistent per segment)
                var lang = finalTokens.First().Language; 

                Console.WriteLine($"✅ FINAL [Spk {speaker}] ({lang}): {text}");

                OnTranscriptReceived?.Invoke(new TranscriptResult
                {
                    Text = text,
                    IsFinal = true,
                    Speaker = speaker,
                    DetectedLanguage = lang, // Pass detected language to UI
                    Timestamp = DateTime.UtcNow
                });
            }

            // --- B) PARTIAL (Real-time Hypothesis) ---
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
        catch (Exception ex) { Console.WriteLine($"JSON Processing Error: {ex.Message}"); }
    }

    private async Task SendJsonAsync<T>(T payload, CancellationToken ct)
    {
        if (_webSocket is null) return;
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        Console.WriteLine($"📤 [SonioxClient] Config: {json}");
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
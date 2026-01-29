using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Glu_Library.Configuration;
using Glu_Library.Models;
using Glu_Library.Models.WebSocket;
using Glu_Library.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Glu_Library.Services;

/// <summary>
/// WebSocket client responsible for:
/// - Connecting to Soniox real-time transcription service
/// - Streaming raw audio data
/// - Receiving partial and final transcription results
/// </summary>
public sealed class SonioxWebSocketClient : ISonioxWebSocketClient
{
    /// <summary>
    /// Underlying WebSocket implementation provided by .NET.
    /// Manages the low-level WebSocket connection.
    /// </summary>
    private readonly ClientWebSocket _webSocket = new();

    /// <summary>
    /// JSON serializer configuration optimized for web payloads
    /// (camelCase, relaxed formatting, etc.).
    /// </summary>
    private readonly JsonSerializerOptions _jsonOptions =
        new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Fully qualified WebSocket endpoint URI.
    /// </summary>
    private readonly Uri _webSocketUri;

    /// <summary>
    /// Initial "start" message sent to Soniox to initialize
    /// the transcription session.
    /// </summary>
    private readonly SonioxStartRequest _startRequest;

    /// <summary>
    /// Cancellation token source used to stop the receive loop
    /// when the client disconnects or is disposed.
    /// </summary>
    private CancellationTokenSource? _receiveCts;

    /// <summary>
    /// Event raised whenever a transcription result
    /// (partial or final) is received from Soniox.
    /// </summary>
    public event Action<TranscriptResult>? OnTranscriptReceived;

    /// <summary>
    /// Event raised when a fatal WebSocket or processing error occurs.
    /// </summary>
    public event Action<Exception>? OnError;

    /// <summary>
    /// Creates a new Soniox WebSocket client using dependency-injected options.
    /// </summary>
    /// <param name="options">
    /// Configuration options bound from appsettings.json or provided programmatically.
    /// </param>
    public SonioxWebSocketClient(IOptions<SonioxWebSocketOptions> options)
    {
        var opts = options.Value;

        // Validate mandatory configuration
        if (string.IsNullOrWhiteSpace(opts.Endpoint))
            throw new InvalidOperationException(
                "Soniox WebSocket endpoint is not configured.");

        _webSocketUri = new Uri(opts.Endpoint);

        // Build the initial start request sent immediately after connecting
        _startRequest = new SonioxStartRequest
        {
            Token = opts.Token,
            Model = opts.Model,
            EnableDiarization = opts.EnableSpeakerDiarization,
            EnablePartialResults = opts.EnablePartialResults,
            Language = opts.Language
        };
    }

    /// <summary>
    /// Opens the WebSocket connection and starts the transcription session.
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Establish WebSocket connection
            await _webSocket.ConnectAsync(_webSocketUri, cancellationToken);

            // Send the initial "start" message required by Soniox
            await SendJsonAsync(_startRequest, cancellationToken);

            // Start background receive loop (fire-and-forget)
            _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _ = ReceiveLoopAsync(_receiveCts.Token);
        }
        catch (Exception ex)
        {
            RaiseError(ex);
            throw;
        }
    }

    /// <summary>
    /// Sends raw PCM audio bytes to Soniox over the WebSocket connection.
    /// </summary>
    public async Task SendAudioAsync(
        ReadOnlyMemory<byte> audioData,
        CancellationToken cancellationToken = default)
    {
        // Do nothing if the socket is not open
        if (_webSocket.State != WebSocketState.Open)
            return;

        try
        {
            await _webSocket.SendAsync(
                audioData,
                WebSocketMessageType.Binary,
                endOfMessage: true,
                cancellationToken);
        }
        catch (Exception ex)
        {
            RaiseError(ex);
        }
    }

    /// <summary>
    /// Gracefully closes the WebSocket connection and stops the receive loop.
    /// </summary>
    public async Task DisconnectAsync()
    {
        try
        {
            // Stop receive loop
            _receiveCts?.Cancel();

            // Close WebSocket if still open
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client disconnect",
                    CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            RaiseError(ex);
        }
    }

    /// <summary>
    /// Background loop that continuously listens for incoming
    /// WebSocket messages from Soniox.
    /// </summary>
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];

        try
        {
            while (!cancellationToken.IsCancellationRequested &&
                   _webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(buffer, cancellationToken);

                // Soniox sends transcription results as text frames (JSON)
                if (result.MessageType != WebSocketMessageType.Text)
                    continue;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ProcessIncomingMessage(json);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during normal shutdown
        }
        catch (Exception ex)
        {
            RaiseError(ex);
        }
    }

    /// <summary>
    /// Parses and processes a JSON message received from Soniox.
    /// Converts it into a domain-level TranscriptResult.
    /// </summary>
    private void ProcessIncomingMessage(string json)
    {
        try
        {
            var response =
                JsonSerializer.Deserialize<SonioxStreamResponse>(json, _jsonOptions);

            // Ignore empty or malformed messages
            if (!string.IsNullOrWhiteSpace(response?.Text))
            {
                OnTranscriptReceived?.Invoke(new TranscriptResult
                {
                    Text = response.Text!,
                    IsFinal = response.IsFinal,
                    Speaker = response.Speaker,
                    Confidence = response.Confidence,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            RaiseError(ex);
        }
    }

    /// <summary>
    /// Sends a JSON payload as a text WebSocket frame.
    /// Used mainly for the initial start request.
    /// </summary>
    private async Task SendJsonAsync<T>(T payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        await _webSocket.SendAsync(
            bytes,
            WebSocketMessageType.Text,
            endOfMessage: true,
            ct);
    }

    /// <summary>
    /// Safely raises the error event.
    /// </summary>
    private void RaiseError(Exception ex)
        => OnError?.Invoke(ex);

    /// <summary>
    /// Disposes the WebSocket client and releases all resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _webSocket.Dispose();
        _receiveCts?.Dispose();
    }
}

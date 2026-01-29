using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Glu_Library.Models;
using Glu_Library.Models.WebSocket;
using Glu_Library.Services.Interfaces;

namespace Glu_Library.Services;

/// <summary>
/// WebSocket client responsible for streaming audio to Soniox
/// and receiving real-time transcription results.
/// </summary>
public sealed class SonioxWebSocketClient : ISonioxWebSocketClient
{
    private readonly ClientWebSocket _webSocket = new();
    private readonly JsonSerializerOptions _jsonOptions =
        new(JsonSerializerDefaults.Web);

    private CancellationTokenSource? _receiveCts;
    private Task? _receiveLoopTask;

    private readonly Uri _webSocketUri;
    private readonly SonioxStartRequest _startRequest;

    /// <inheritdoc />
    public event Action<TranscriptResult>? OnTranscriptReceived;

    /// <inheritdoc />
    public event Action<Exception>? OnError;

    /// <summary>
    /// Creates a new Soniox WebSocket client.
    /// </summary>
    /// <param name="webSocketUrl">Authenticated WebSocket URL provided by Soniox.</param>
    /// <param name="startRequest">Initial start request payload.</param>
    public SonioxWebSocketClient(
        string webSocketUrl,
        SonioxStartRequest startRequest)
    {
        _webSocketUri = new Uri(webSocketUrl);
        _startRequest = startRequest;
    }

    // --- Connection Lifecycle ---

    /// <inheritdoc />
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _webSocket.ConnectAsync(_webSocketUri, cancellationToken);

            // Send initial "start" message
            await SendJsonAsync(_startRequest, cancellationToken);

            // Start receive loop
            _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _receiveLoopTask = Task.Run(
                () => ReceiveLoopAsync(_receiveCts.Token),
                _receiveCts.Token);
        }
        catch (Exception ex)
        {
            RaiseError(ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendAudioAsync(
        ReadOnlyMemory<byte> audioData,
        CancellationToken cancellationToken = default)
    {
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

    /// <inheritdoc />
    public async Task DisconnectAsync()
    {
        try
        {
            _receiveCts?.Cancel();

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

    // --- Receive Loop ---

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];

        try
        {
            while (!cancellationToken.IsCancellationRequested &&
                   _webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(
                    buffer,
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                if (result.MessageType != WebSocketMessageType.Text)
                    continue;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ProcessIncomingMessage(json);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            RaiseError(ex);
        }
    }

    // --- Message Processing ---

    private void ProcessIncomingMessage(string json)
    {
        try
        {
            // Try stream response first
            var streamResponse =
                JsonSerializer.Deserialize<SonioxStreamResponse>(json, _jsonOptions);

            if (!string.IsNullOrWhiteSpace(streamResponse?.Text))
            {
                var result = new TranscriptResult
                {
                    Text = streamResponse.Text ?? string.Empty,
                    IsFinal = streamResponse.IsFinal,
                    Speaker = streamResponse.Speaker,
                    Confidence = streamResponse.Confidence,
                    Timestamp = DateTime.UtcNow
                };

                OnTranscriptReceived?.Invoke(result);
                return;
            }

            // Try error response
            var errorResponse =
                JsonSerializer.Deserialize<SonioxErrorResponse>(json, _jsonOptions);

            if (!string.IsNullOrWhiteSpace(errorResponse?.Error))
            {
                RaiseError(new InvalidOperationException(
                    $"{errorResponse.Error}: {errorResponse.Message}"));
            }
        }
        catch (Exception ex)
        {
            RaiseError(ex);
        }
    }

    // --- Helpers ---

    private async Task SendJsonAsync<T>(
        T payload,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        await _webSocket.SendAsync(
            bytes,
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken);
    }

    private void RaiseError(Exception ex)
        => OnError?.Invoke(ex);

    // --- Disposal ---

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        _webSocket.Dispose();
        _receiveCts?.Dispose();
    }
}

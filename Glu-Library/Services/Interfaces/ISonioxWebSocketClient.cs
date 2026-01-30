using Glu_Library.Models;

namespace Glu_Library.Services.Interfaces;

/// <summary>
/// Defines a WebSocket client capable of streaming audio to Soniox
/// and receiving real-time transcription results.
/// </summary>
public interface ISonioxWebSocketClient : IAsyncDisposable
{
    /// <summary>
    /// Event raised whenever a transcription result (partial or final) is received.
    /// Consumers are expected to forward this to a state manager.
    /// </summary>
    event Action<TranscriptResult>? OnTranscriptReceived;

    /// <summary>
    /// Event raised when a fatal WebSocket or transcription error occurs.
    /// </summary>
    event Action<Exception>? OnError;

    /// <summary>
    /// Establishes the WebSocket connection and starts the transcription session.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the connection.</param>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends raw PCM audio data to the Soniox WebSocket stream.
    /// </summary>
    /// <param name="audioData">Audio buffer containing PCM data.</param>
    /// <param name="cancellationToken">Token used to cancel the send operation.</param>
    Task SendAudioAsync(
        ReadOnlyMemory<byte> audioData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gracefully closes the WebSocket connection.
    /// </summary>
    Task DisconnectAsync();
}
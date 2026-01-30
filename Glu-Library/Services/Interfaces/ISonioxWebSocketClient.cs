using Glu_Library.Models;

namespace Glu_Library.Services.Interfaces;

/// <summary>
/// Defines the contract for a WebSocket client capable of streaming audio to Soniox
/// and receiving real-time transcription results.
/// </summary>
public interface ISonioxWebSocketClient : IAsyncDisposable
{
    /// <summary>
    /// Event raised whenever a transcription result (partial or final) is received from the server.
    /// Consumers are expected to subscribe to this event to update the UI or application state.
    /// </summary>
    event Action<TranscriptResult>? OnTranscriptReceived;

    /// <summary>
    /// Event raised when a fatal WebSocket connection error or transcription failure occurs.
    /// </summary>
    event Action<Exception>? OnError;

    /// <summary>
    /// Establishes the WebSocket connection with the Soniox API and performs the initial handshake
    /// sending the configuration payload.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the connection attempt.</param>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a chunk of raw PCM audio data to the Soniox WebSocket stream.
    /// </summary>
    /// <param name="audioData">The audio buffer containing raw PCM data (usually 16-bit, 1 channel).</param>
    /// <param name="cancellationToken">A token used to cancel the send operation.</param>
    Task SendAudioAsync(
        ReadOnlyMemory<byte> audioData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gracefully closes the WebSocket connection and releases resources.
    /// </summary>
    Task DisconnectAsync();
}
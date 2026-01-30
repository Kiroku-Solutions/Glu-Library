using Glu_Library.Models;

namespace Glu_Library.Services.Interfaces;

/// <summary>
/// Defines the contract for managing the state of a transcription session.
/// Responsible for aggregating partial results and maintaining a history of finalized segments.
/// </summary>
public interface ITranscriptState
{
    /// <summary>
    /// Gets the read-only collection of finalized speaker segments.
    /// This list represents the confirmed history of the conversation.
    /// </summary>
    IReadOnlyList<SpeakerSegment> Segments { get; }

    /// <summary>
    /// Gets the current partial (non-finalized) transcription hypothesis.
    /// Represents real-time feedback that may change as more audio is processed.
    /// Returns null if there is no active partial speech.
    /// </summary>
    TranscriptResult? CurrentPartial { get; }

    /// <summary>
    /// Event raised whenever the transcript state (Segments or CurrentPartial) changes.
    /// UI components should subscribe to this event to trigger re-rendering.
    /// </summary>
    event Action? OnStateChanged;

    /// <summary>
    /// Processes a raw transcript result received from the WebSocket client.
    /// Logic includes determining if the result is final or partial and updating the internal lists accordingly.
    /// </summary>
    /// <param name="result">The normalized transcript result to process.</param>
    void ProcessResult(TranscriptResult result);

    /// <summary>
    /// Resets the internal state, clearing all history and partial text.
    /// Typically called when starting a new recording session.
    /// </summary>
    void Reset();
}
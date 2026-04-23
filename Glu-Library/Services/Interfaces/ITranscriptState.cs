using Glu_Library.Models;

namespace Glu_Library.Services.Interfaces;

/// <summary>
/// Defines the state management contract for a transcription session.
/// Responsible for maintaining finalized speaker segments and the current partial transcript.
/// </summary>
public interface ITranscriptState
{
    /// <summary>
    /// Gets the read-only collection of finalized speaker segments.
    /// This list represents the confirmed history of the conversation to be bound to the UI.
    /// </summary>
    IReadOnlyList<SpeakerSegment> Segments { get; }

    /// <summary>
    /// Gets the current partial (non-finalized) transcription hypothesis.
    /// Represents real-time feedback that may change as more audio is processed.
    /// </summary>
    TranscriptResult? CurrentPartial { get; }
    
    /// <summary>
    /// Gets or sets the ID of the speaker considered to be the Agent (e.g., Doctor).
    /// Defaults to "2". When this changes, subsequent segments will be classified accordingly.
    /// </summary>
    string AgentSpeakerId { get; set; }

    /// <summary>
    /// Event raised whenever the transcript state (Segments or CurrentPartial) changes.
    /// UI components should subscribe to this event to trigger re-rendering.
    /// </summary>
    event Action? OnStateChanged;

    /// <summary>
    /// Processes a raw transcript result received from the WebSocket client.
    /// </summary>
    /// <param name="result">The normalized transcript result to process.</param>
    void ProcessResult(TranscriptResult result);

    /// <summary>
    /// Clears all transcript state, including history and partial text.
    /// </summary>
    void Reset();
}
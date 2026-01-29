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
    /// Intended for UI binding or transcript rendering.
    /// </summary>
    IReadOnlyList<SpeakerSegment> Segments { get; }

    /// <summary>
    /// Gets the current partial (non-finalized) transcript result.
    /// Represents live transcription that may still change.
    /// </summary>
    TranscriptResult? CurrentPartial { get; }

    /// <summary>
    /// Event raised whenever the transcript state changes.
    /// Consumers (UI, loggers) should subscribe to update their views.
    /// </summary>
    event Action? OnStateChanged;

    /// <summary>
    /// Processes a normalized transcript result coming from the WebSocket stream.
    /// Final results are committed to the segment history,
    /// while partial results update the current transient state.
    /// </summary>
    /// <param name="result">The normalized transcript result.</param>
    void ProcessResult(TranscriptResult result);

    /// <summary>
    /// Clears all transcript state, including history and partial text.
    /// Typically called when starting a new transcription session.
    /// </summary>
    void Reset();
}
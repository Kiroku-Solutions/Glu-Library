using Glu_Library.Models;

namespace Glu_Library.Services.Interfaces;

/// <summary>
/// Defines the state management contract for the transcription session.
/// This interface is responsible for maintaining the history of speaker segments and the current partial transcription.
/// </summary>
public interface ITranscriptState
{
    /// <summary>
    /// Gets the read-only list of finalized speaker segments.
    /// Use this property to bind the transcript history to the UI.
    /// External modification is prevented to ensure state consistency.
    /// </summary>
    IReadOnlyList<SpeakerSegment> Segments { get; }

    /// <summary>
    /// Gets the current partial (provisional) transcript result.
    /// Represents the "ghost" text that is currently being spoken but has not yet been finalized.
    /// </summary>
    TranscriptResult? CurrentPartial { get; }
    
    /// <summary>
    /// Event raised whenever the transcript state changes (e.g., new partial text, new segment added).
    /// The UI should subscribe to this event to trigger re-rendering.
    /// </summary>
    event Action? OnStateChanged;
   
    /// <summary>
    /// Processes a new transcript result received from the audio stream.
    /// Handles logic for updating partial text or committing final segments to the history.
    /// </summary>
    /// <param name="result">The transcript result data object.</param>
    void ProcessResult(TranscriptResult result);

    /// <summary>
    /// Resets the state, clearing all segments and partial text.
    /// Typically called when starting a new session or clearing the screen.
    /// </summary>
    void Reset();
}
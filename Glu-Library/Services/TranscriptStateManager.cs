using Glu_Library.Models;
using Glu_Library.Services.Interfaces;

namespace Glu_Library.Services;

/// <summary>
/// Manages the state of the transcription session, including history and real-time updates.
/// Implements logic for speaker diarization (grouping consecutive segments from the same speaker)
/// and handles the distinction between partial (provisional) and final results.
/// </summary>
public class TranscriptStateManager : ITranscriptState
{
    // --- 1. Current State ---
    
    /// <summary>
    /// Internal storage list that allows modification (Add/Clear).
    /// </summary>
    private readonly List<SpeakerSegment> _segments = new();

    /// <summary>
    /// Public read-only view that satisfies the Interface contract.
    /// External components (like UI) read from here but cannot modify the list directly.
    /// </summary>
    public IReadOnlyList<SpeakerSegment> Segments => _segments;

    /// <summary>
    /// The "ghost" text that is currently being spoken/processed but is not yet final.
    /// This property is updated frequently (streaming) and cleared once a segment is finalized.
    /// </summary>
    public TranscriptResult? CurrentPartial { get; private set; }

    // --- 2. Events ---

    /// <summary>
    /// Event triggered whenever the state changes (e.g., partial text update, new segment added).
    /// UI components should subscribe to this to know when to re-render.
    /// </summary>
    public event Action? OnStateChanged;

    // --- 3. Main Logic ---

    /// <summary>
    /// Processes a new result arriving from the transcription engine (e.g., via JavaScript Interop).
    /// </summary>
    /// <param name="result">The transcript result to process.</param>
    public void ProcessResult(TranscriptResult result)
    {
        if (result.IsFinal)
        {
            // A. If the result is final, consolidate it into the permanent history.
            AddFinalSegment(result);
            
            // Clear the partial view because this text has now "solidified" into a segment.
            CurrentPartial = null; 
        }
        else
        {
            // B. If the result is partial, only update the temporary view for real-time feedback.
            CurrentPartial = result;
        }

        // Notify subscribers (e.g., the UI) that the state has changed and a repaint is needed.
        NotifyStateChanged();
    }

    /// <summary>
    /// Diarization Logic: Groups consecutive messages from the same speaker into a single segment block.
    /// </summary>
    /// <param name="result">The final transcript result to add.</param>
    private void AddFinalSegment(TranscriptResult result)
    {
        // Validate speaker (Soniox sometimes sends null if unsure; assume "Unknown").
        var speaker = result.Speaker ?? "Unknown";
        
        // Inspect the last recorded segment in the history.
        var lastSegment = _segments.LastOrDefault();

        // If the last segment belongs to the SAME speaker, append the text instead of creating a new block.
        if (lastSegment != null && lastSegment.SpeakerId == speaker)
        {
            lastSegment.Text += " " + result.Text;
            
            // Update the end timestamp of the existing segment.
            lastSegment.EndTimeMs = DateTime.UtcNow.TimeOfDay.TotalMilliseconds; 
        }
        else
        {
            // If the speaker changed (or this is the first segment), create a NEW segment block.
            var newSegment = new SpeakerSegment
            {
                SpeakerId = speaker,
                Text = result.Text,
                IsAgent = IdentifyAgent(speaker), // Logic to determine if the speaker is an Agent.
                StartTimeMs = DateTime.UtcNow.TimeOfDay.TotalMilliseconds,
                EndTimeMs = DateTime.UtcNow.TimeOfDay.TotalMilliseconds
            };

            // Add to the PRIVATE internal list.
            _segments.Add(newSegment);
        }
    }

    /// <summary>
    /// Resets the conversation state (e.g., when the Clear or Stop button is pressed).
    /// </summary>
    public void Reset()
    {
        _segments.Clear();
        CurrentPartial = null;
        NotifyStateChanged();
    }

    // --- 4. Helpers ---

    /// <summary>
    /// Helper method to safely invoke the state change event.
    /// </summary>
    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    /// <summary>
    /// Simple logic to identify if a speaker ID corresponds to the Agent.
    /// Note: This logic could be enhanced with external configuration in the future.
    /// </summary>
    /// <param name="speakerId">The ID of the speaker.</param>
    /// <returns>True if the speaker is identified as an Agent; otherwise, false.</returns>
    private bool IdentifyAgent(string speakerId)
    {
        // Example: We assume that Speaker "1" or "A" is always the Agent.
        return speakerId == "1" || speakerId == "A";
    }
}
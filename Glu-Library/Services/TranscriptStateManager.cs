using Glu_Library.Models;
using Glu_Library.Services.Interfaces;

namespace Glu_Library.Services;

/// <summary>
/// Default implementation of <see cref="ITranscriptState"/>.
/// Manages the state of the transcription session, organizing finalized segments
/// and handling real-time partial updates with configurable diarization logic.
/// </summary>
public class TranscriptStateManager : ITranscriptState
{
    // --- Internal State ---
    private readonly List<SpeakerSegment> _segments = new();

    /// <inheritdoc />
    public IReadOnlyList<SpeakerSegment> Segments => _segments;
    
    /// <inheritdoc />
    public TranscriptResult? CurrentPartial { get; private set; }

    /// <inheritdoc />
    // Defaulting to "2" as the initial Agent ID, but this can be changed at runtime via UI.
    public string AgentSpeakerId { get; set; } = "2";

    public event Action? OnStateChanged;

    /// <inheritdoc />
    public void ProcessResult(TranscriptResult result)
    {
        if (result.IsFinal)
        {
            AddFinalSegment(result);
            // Clear partial state since the text has been confirmed/finalized
            CurrentPartial = null;
        }
        else
        {
            // Update the transient/partial view
            CurrentPartial = result;
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Adds a finalized result to the history. 
    /// Merges text if the speaker is the same as the last segment.
    /// </summary>
    private void AddFinalSegment(TranscriptResult result)
    {
        var speaker = result.Speaker ?? "Unknown";
        var lastSegment = _segments.LastOrDefault();

        // Basic diarization logic: Merge if same speaker to keep the UI clean
        if (lastSegment != null && lastSegment.SpeakerId == speaker)
        {
            // Append text (Soniox tokens usually include spacing)
            lastSegment.Text = $"{lastSegment.Text}{result.Text}";
            lastSegment.EndTimeMs = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
        }
        else
        {
            // New speaker detected or start of stream
            var timestamp = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
            
            _segments.Add(new SpeakerSegment
            {
                SpeakerId = speaker,
                Text = result.Text,
                // Determine if this is the agent based on the current configuration
                IsAgent = IdentifyAgent(speaker), 
                StartTimeMs = timestamp,
                EndTimeMs = timestamp
            });
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        _segments.Clear();
        CurrentPartial = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    /// <summary>
    /// Helper logic to visually distinguish speakers in the UI.
    /// Compares the current speaker ID against the configured AgentSpeakerId.
    /// </summary>
    private bool IdentifyAgent(string speakerId)
    {
        // Dynamic comparison instead of hardcoded value
        return speakerId == AgentSpeakerId;
    }
}
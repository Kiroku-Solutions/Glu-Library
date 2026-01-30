using Glu_Library.Models;
using Glu_Library.Services.Interfaces;

namespace Glu_Library.Services;

/// <summary>
/// Default implementation of <see cref="ITranscriptState"/>.
/// Manages the state of the transcription session, organizing finalized segments
/// and handling real-time partial updates with basic diarization logic.
/// </summary>
public class TranscriptStateManager : ITranscriptState
{
    // --- Internal State ---
    private readonly List<SpeakerSegment> _segments = new();

    public IReadOnlyList<SpeakerSegment> Segments => _segments;
    
    public TranscriptResult? CurrentPartial { get; private set; }

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

        // Basic diarization logic: Merge if same speaker
        if (lastSegment != null && lastSegment.SpeakerId == speaker)
        {
            // Append text (Soniox tokens usually include spacing)
            lastSegment.Text = $"{lastSegment.Text}{result.Text}"; 
            lastSegment.EndTimeMs = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
        }
        else
        {
            // New speaker or start of stream
            var timestamp = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
            _segments.Add(new SpeakerSegment
            {
                SpeakerId = speaker,
                Text = result.Text,
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
    /// Helper to identify if a speaker ID belongs to an agent/system.
    /// Current logic is a placeholder.
    /// </summary>
    private bool IdentifyAgent(string speakerId)
        => speakerId == "1" || speakerId == "A";
}
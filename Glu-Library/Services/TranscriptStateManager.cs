using Glu_Library.Models;
using Glu_Library.Services.Interfaces;

namespace Glu_Library.Services;

/// <summary>
/// Manages the state of a transcription session, including finalized segments
/// and real-time partial updates. Implements basic speaker diarization by
/// grouping consecutive segments from the same speaker.
/// </summary>
public class TranscriptStateManager : ITranscriptState
{
    // --- 1. Internal State ---

    /// <summary>
    /// Internal mutable list of finalized speaker segments.
    /// </summary>
    private readonly List<SpeakerSegment> _segments = new();

    /// <summary>
    /// Read-only view of the finalized transcript segments.
    /// </summary>
    public IReadOnlyList<SpeakerSegment> Segments => _segments;

    /// <summary>
    /// Current partial (non-finalized) transcript result.
    /// Represents live transcription feedback.
    /// </summary>
    public TranscriptResult? CurrentPartial { get; private set; }

    // --- 2. Events ---

    /// <inheritdoc />
    public event Action? OnStateChanged;

    // --- 3. Core Logic ---

    /// <inheritdoc />
    public void ProcessResult(TranscriptResult result)
    {
        if (result.IsFinal)
        {
            AddFinalSegment(result);
            CurrentPartial = null;
        }
        else
        {
            CurrentPartial = result;
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Applies diarization logic by merging consecutive results
    /// from the same speaker into a single segment.
    /// </summary>
    private void AddFinalSegment(TranscriptResult result)
    {
        var speaker = result.Speaker ?? "Unknown";
        var lastSegment = _segments.LastOrDefault();

        if (lastSegment != null && lastSegment.SpeakerId == speaker)
        {
            // Append text with proper spacing
            lastSegment.Text = $"{lastSegment.Text} {result.Text}".Trim();

            // Update end timestamp (wall-clock based)
            lastSegment.EndTimeMs = GetCurrentTimestampMs();
        }
        else
        {
            var timestamp = GetCurrentTimestampMs();

            var newSegment = new SpeakerSegment
            {
                SpeakerId = speaker,
                Text = result.Text,
                IsAgent = IdentifyAgent(speaker),
                StartTimeMs = timestamp,
                EndTimeMs = timestamp
            };

            _segments.Add(newSegment);
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
        _segments.Clear();
        CurrentPartial = null;
        NotifyStateChanged();
    }

    // --- 4. Helpers ---

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    /// <summary>
    /// Returns a wall-clock based timestamp in milliseconds.
    /// Note: This is not audio-time accurate and may be replaced
    /// by engine-provided timestamps in the future.
    /// </summary>
    private static double GetCurrentTimestampMs()
        => DateTime.UtcNow.TimeOfDay.TotalMilliseconds;

    /// <summary>
    /// Identifies whether a given speaker ID corresponds to the agent.
    /// This logic is intentionally simple and can be replaced
    /// with configuration-based or metadata-driven logic.
    /// </summary>
    private bool IdentifyAgent(string speakerId)
        => speakerId == "1" || speakerId == "A";
}

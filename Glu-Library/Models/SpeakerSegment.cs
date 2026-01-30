namespace Glu_Library.Models;

/// <summary>
/// Represents a continuous segment of speech attributed to a single speaker
/// as a result of speaker diarization.
/// </summary>
public class SpeakerSegment
{
    /// <summary>
    /// Unique identifier assigned to the speaker (e.g., "spk_0", "agent", "caller").
    /// </summary>
    public string SpeakerId { get; set; } = string.Empty;

    /// <summary>
    /// Full transcribed text spoken by this speaker during the segment.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Start time of the segment in milliseconds from the beginning of the audio stream.
    /// </summary>
    public double StartTimeMs { get; set; }

    /// <summary>
    /// End time of the segment in milliseconds from the beginning of the audio stream.
    /// </summary>
    public double EndTimeMs { get; set; }

    /// <summary>
    /// Indicates whether the speaker is an agent/system representative
    /// as opposed to an external speaker (e.g., customer or patient).
    /// </summary>
    public bool IsAgent { get; set; }
}
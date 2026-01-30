namespace Glu_Library.Models;

/// <summary>
/// Represents a continuous segment of speech attributed to a single speaker.
/// This structure is used by the state manager to organize the linear transcript
/// into readable, speaker-separated blocks.
/// </summary>
public class SpeakerSegment
{
    /// <summary>
    /// Unique identifier assigned to the speaker (e.g., "1", "2", "Unknown").
    /// </summary>
    public string SpeakerId { get; set; } = string.Empty;

    /// <summary>
    /// Accumulated text spoken by this speaker during this specific segment.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Approximate start time of the segment in milliseconds (relative to stream start).
    /// </summary>
    public double StartTimeMs { get; set; }

    /// <summary>
    /// Approximate end time of the segment in milliseconds (relative to stream start).
    /// </summary>
    public double EndTimeMs { get; set; }

    /// <summary>
    /// Indicates if this speaker is identified as an agent or system representative.
    /// Useful for UI styling (e.g., aligning agent text to the right).
    /// </summary>
    public bool IsAgent { get; set; }
}
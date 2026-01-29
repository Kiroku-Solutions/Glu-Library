namespace Glu_Library.Models;

/// <summary>
/// Represents a group of words spoken by a specific speaker (Diarization).
/// </summary>
public class SpeakerSegment
{
    public string SpeakerId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public double StartTimeMs { get; set; }
    public double EndTimeMs { get; set; }
    public bool IsAgent { get; set; }
}
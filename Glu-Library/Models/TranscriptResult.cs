namespace Glu_Library.Models;

/// <summary>
/// Represents a single transcription event (partial or final).
/// </summary>
public class TranscriptResult
{
    public string Text { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Speaker { get; set; }
    public double Confidence { get; set; }
}
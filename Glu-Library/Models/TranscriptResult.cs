namespace Glu_Library.Models;

/// <summary>
/// Represents a standardized transcription result produced by the WebSocket client.
/// This acts as a unified data transfer object between the Soniox API response
/// and the library's state management logic.
/// </summary>
public class TranscriptResult
{
    /// <summary>
    /// The transcribed text content.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this result is final (confirmed text) or partial (provisional text).
    /// </summary>
    public bool IsFinal { get; set; }

    /// <summary>
    /// UTC timestamp indicating when this result was processed by the client.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The speaker identifier associated with this text segment, if available.
    /// </summary>
    public string? Speaker { get; set; }

    /// <summary>
    /// The confidence score of the transcription (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; set; }
}
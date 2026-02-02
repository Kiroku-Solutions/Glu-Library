namespace Glu_Library.Models;

/// <summary>
/// Represents a single transcription result produced during
/// a speech-to-text streaming session.
/// </summary>
public class TranscriptResult
{
    /// <summary>
    /// Transcribed text content.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this transcription result is final
    /// or an intermediate (partial) hypothesis.
    /// </summary>
    public bool IsFinal { get; set; }

    /// <summary>
    /// UTC timestamp indicating when the transcription result was generated.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Identifier of the speaker associated with this transcription,
    /// if speaker diarization is enabled.
    /// </summary>
    public string? Speaker { get; set; }

    /// <summary>
    /// Confidence score of the transcription result, typically between 0.0 and 1.0.
    /// </summary>
    public double Confidence { get; set; }
    
    /// <summary>
    /// The detected language code (e.g., "es", "en") for this segment.
    /// Essential for UI filtering or routing to specific translation models.
    /// </summary>
    public string? DetectedLanguage { get; set; }
}
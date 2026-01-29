namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Represents a transcription message received
/// from the Soniox WebSocket stream.
/// </summary>
public class SonioxStreamResponse
{
    /// <summary>
    /// Transcribed text for the current segment.
    /// May be partial or final depending on IsFinal.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Indicates whether this transcription result
    /// is final or an intermediate (partial) result.
    /// </summary>
    public bool IsFinal { get; set; }

    /// <summary>
    /// Identifier of the detected speaker.
    /// Only present if diarization is enabled.
    /// </summary>
    public string? Speaker { get; set; }

    /// <summary>
    /// Confidence score of the transcription.
    /// Typically ranges between 0.0 and 1.0.
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Start time of the spoken segment.
    /// Time unit depends on Soniox configuration (usually seconds).
    /// </summary>
    public double StartTime { get; set; }

    /// <summary>
    /// End time of the spoken segment.
    /// Time unit depends on Soniox configuration (usually seconds).
    /// </summary>
    public double EndTime { get; set; }
}
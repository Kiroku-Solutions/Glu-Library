namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Initial payload sent to the Soniox WebSocket endpoint
/// to start a transcription session.
/// </summary>
public class SonioxStartRequest
{
    /// <summary>
    /// Authentication token provided by Soniox.
    /// Required to authorize the WebSocket session.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Name of the speech recognition model to use.
    /// Example: "en_v2", "es_v1", etc.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Enables speaker diarization (speaker separation).
    /// When enabled, the transcription will attempt
    /// to identify different speakers.
    /// </summary>
    public bool EnableDiarization { get; set; } = true;

    /// <summary>
    /// Enables partial (intermediate) transcription results.
    /// Useful for real-time captions.
    /// </summary>
    public bool EnablePartialResults { get; set; } = true;

    /// <summary>
    /// Language code for the transcription.
    /// Examples: "en", "es", "en-US".
    /// </summary>
    public string Language { get; set; } = "en";
}
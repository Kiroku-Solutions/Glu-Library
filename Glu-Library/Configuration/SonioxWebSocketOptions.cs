namespace Glu_Library.Configuration;

/// <summary>
/// Configuration options for the Soniox WebSocket service.
/// </summary>
public class SonioxWebSocketOptions
{
    public const string SectionName = "SonioxWebSocket";

    /// <summary>
    /// WebSocket endpoint.
    /// </summary>
    public string Endpoint { get; set; } =
        "wss://stt-rt.soniox.com/transcribe-websocket";

    /// <summary>
    /// Soniox API authentication token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Speech recognition model to use.
    /// </summary>
    public string Model { get; set; } = "stt-rt-preview";

    /// <summary>
    /// Enables speaker diarization.
    /// </summary>
    public bool EnableSpeakerDiarization { get; set; } = true;

    /// <summary>
    /// Enables partial transcription results.
    /// </summary>
    public bool EnablePartialResults { get; set; } = true;

    /// <summary>
    /// Language code (e.g. "en", "es").
    /// </summary>
    public string Language { get; set; } = "en";
}
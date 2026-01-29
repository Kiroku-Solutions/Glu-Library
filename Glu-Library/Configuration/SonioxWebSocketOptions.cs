namespace Glu_Library.Configuration;

/// <summary>
/// Configuration options for the Soniox WebSocket real-time transcription service.
/// This class defines how the WebSocket session is initialized and behaves.
/// </summary>
public class SonioxWebSocketOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "SonioxWebSocket";

    /// <summary>
    /// WebSocket endpoint used for real-time transcription.
    /// Default points to Soniox production endpoint.
    /// </summary>
    public string Endpoint { get; set; } =
        "wss://stt-rt.soniox.com/transcribe-websocket";

    /// <summary>
    /// Real-time speech-to-text model to use.
    /// Example: "stt-rt-preview".
    /// </summary>
    public string Model { get; set; } = "stt-rt-preview";

    /// <summary>
    /// Audio format sent through the WebSocket.
    /// Use "auto" for automatic format detection.
    /// </summary>
    public string AudioFormat { get; set; } = "auto";

    /// <summary>
    /// Enables speaker diarization (speaker separation).
    /// </summary>
    public bool EnableSpeakerDiarization { get; set; } = true;

    /// <summary>
    /// Enables automatic language identification.
    /// Useful for multilingual environments.
    /// </summary>
    public bool EnableLanguageIdentification { get; set; } = true;

    /// <summary>
    /// Whether partial (non-final) transcription results are emitted.
    /// This enables real-time "live typing" behavior.
    /// </summary>
    public bool EnablePartialResults { get; set; } = true;

    /// <summary>
    /// Optional language hints to improve recognition accuracy.
    /// Example: ["en", "es"].
    /// </summary>
    public string[]? LanguageHints { get; set; }

    /// <summary>
    /// Optional client-defined identifier to track the session.
    /// Useful for debugging and observability.
    /// </summary>
    public string? ClientReferenceId { get; set; }

    /// <summary>
    /// Timeout (in seconds) before the WebSocket connection is considered inactive.
    /// </summary>
    public int InactivityTimeoutSeconds { get; set; } = 15;
}

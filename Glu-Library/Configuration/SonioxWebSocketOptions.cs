using Glu_Library.Models;

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
    /// Soniox API authentication token. Must be loaded from a secure source (e.g., Environment Variable, Key Vault).
    /// </summary>
    public string Token { get; set; } = null!; // Enforce explicit configuration, not hardcoded default.

    /// <summary>
    /// Speech recognition model to use.
    /// Default is <see cref="SonioxModels.RealTimeV3"/>.
    /// </summary>
    public string Model { get; set; } = SonioxModels.RealTimeV3;

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
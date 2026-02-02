using System.Text.Json.Serialization;

namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Represents the initial configuration payload sent to establish a Soniox WebSocket session.
/// Configured for pure transcription (English/Spanish) with speaker diarization.
/// </summary>
public class SonioxStartRequest
{
    /// <summary>
    /// Your Soniox API key.
    /// </summary>
    [JsonPropertyName("api_key")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// The model ID to use for transcription (e.g., "stt-rt-v3").
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    // --- Audio Configuration ---
    
    /// <summary>
    /// Audio format encoding. For raw streams, use "pcm_s16le".
    /// </summary>
    [JsonPropertyName("audio_format")]
    public string AudioFormat { get; set; } = "pcm_s16le";

    /// <summary>
    /// Sample rate of the audio stream in Hertz (e.g., 16000, 44100, 48000).
    /// </summary>
    [JsonPropertyName("sample_rate")]
    public int SampleRate { get; set; }

    /// <summary>
    /// Number of audio channels (e.g., 1 for mono).
    /// </summary>
    [JsonPropertyName("num_channels")]
    public int NumChannels { get; set; } = 1;

    // --- Features ---

    /// <summary>
    /// List of ISO language codes to bias the model towards. 
    /// Defaults to ["en", "es"] to support both English and Spanish mixed speech.
    /// </summary>
    [JsonPropertyName("language_hints")]
    public List<string> LanguageHints { get; set; } = new() { "en", "es" };

    /// <summary>
    /// Enables detection and labeling of different speakers in the audio stream.
    /// </summary>
    // ⚠️ FIX: Changed from "enable_global_speaker_diarization" to "enable_speaker_diarization"
    [JsonPropertyName("enable_speaker_diarization")] 
    public bool EnableGlobalSpeakerDiarization { get; set; } = true;

    /// <summary>
    /// Enables automatic detection of end-of-utterance to finalize text segments faster.
    /// </summary>
    [JsonPropertyName("enable_endpoint_detection")]
    public bool EnableEndpointDetection { get; set; } = true;

    // --- Advanced Features ---

    /// <summary>
    /// Optional context information to improve transcription accuracy for specific domains.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SonioxContext? Context { get; set; }
}
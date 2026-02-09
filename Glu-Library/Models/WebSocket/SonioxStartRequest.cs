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
    /// Enables automatic detection of the spoken language.
    /// </summary>
    [JsonPropertyName("enable_language_identification")]
    public bool EnableLanguageIdentification { get; set; } = true;

    /// <summary>
    /// Enables detection and labeling of different speakers in the audio stream.
    /// </summary>
    [JsonPropertyName("enable_speaker_diarization")] 
    public bool EnableSpeakerDiarization { get; set; } = true;

    /// <summary>
    /// Optional: Specify the number of speakers if known (2-5).
    /// </summary>
    [JsonPropertyName("num_speakers")]
    public int? NumSpeakers { get; set; }

    /// <summary>
    /// Enables automatic detection of end-of-utterance to finalize text segments faster.
    /// </summary>
    [JsonPropertyName("enable_endpoint_detection")]
    public bool EnableEndpointDetection { get; set; } = true;

    // --- Advanced Features ---

    /// <summary>
    /// Configuration for real-time translation (one-way or two-way).
    /// </summary>
    [JsonPropertyName("translation")]
    public SonioxTranslationConfig? Translation { get; set; }

    /// <summary>
    /// Optional context information to improve transcription accuracy for specific domains.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SonioxContext? Context { get; set; }

    /// <summary>
    /// Client-defined identifier to track this request in logs/webhooks.
    /// </summary>
    [JsonPropertyName("client_reference_id")]
    public string? ClientReferenceId { get; set; }
}
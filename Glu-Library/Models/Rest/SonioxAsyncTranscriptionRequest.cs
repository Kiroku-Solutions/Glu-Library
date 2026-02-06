using System.Text.Json.Serialization;
using Glu_Library.Models.WebSocket; // Reuse Shared Models

namespace Glu_Library.Models.Rest;

/// <summary>
/// Represents the configuration for a Soniox Async Transcription request.
/// </summary>
public class SonioxAsyncTranscriptionRequest
{
    /// <summary>
    /// The model to use (e.g., "stt-async-v3").
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = "stt-async-v3";

    /// <summary>
    /// The File ID of the uploaded audio file.
    /// Exactly one of FileId or AudioUrl must be provided.
    /// </summary>
    [JsonPropertyName("file_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FileId { get; set; }

    /// <summary>
    /// Public URL of the audio file.
    /// Exactly one of FileId or AudioUrl must be provided.
    /// </summary>
    [JsonPropertyName("audio_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AudioUrl { get; set; }

    /// <summary>
    /// Client-defined identifier to track this request.
    /// </summary>
    [JsonPropertyName("client_reference_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ClientReferenceId { get; set; }

    // --- Features ---

    /// <summary>
    /// List of ISO language codes to bias the model towards.
    /// </summary>
    [JsonPropertyName("language_hints")]
    public List<string>? LanguageHints { get; set; }

    /// <summary>
    /// Enable language identification.
    /// </summary>
    [JsonPropertyName("enable_language_identification")]
    public bool? EnableLanguageIdentification { get; set; }

    /// <summary>
    /// Enable speaker diarization.
    /// </summary>
    [JsonPropertyName("enable_speaker_diarization")]
    public bool? EnableSpeakerDiarization { get; set; }

    /// <summary>
    /// Enable global speaker identification.
    /// </summary>
    [JsonPropertyName("enable_global_speaker_identification")]
    public bool? EnableGlobalSpeakerIdentification { get; set; }

    /// <summary>
    /// Specify the number of speakers if known.
    /// </summary>
    [JsonPropertyName("num_speakers")]
    public int? NumSpeakers { get; set; }

    // --- Advanced Features ---

    /// <summary>
    /// Configuration for translation.
    /// </summary>
    [JsonPropertyName("translation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SonioxTranslationConfig? Translation { get; set; }

    /// <summary>
    /// Context information for domain-specific accuracy.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SonioxContext? Context { get; set; }
}

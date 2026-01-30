using System.Text.Json.Serialization;

namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Configuración inicial para la sesión de WebSocket.
/// </summary>
public class SonioxStartRequest
{
    [JsonPropertyName("api_key")]
    public string? ApiKey { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    // --- Audio Configuration ---
    
    [JsonPropertyName("audio_format")]
    public string AudioFormat { get; set; } = "pcm_s16le";

    [JsonPropertyName("sample_rate")]
    public int SampleRate { get; set; }

    [JsonPropertyName("num_channels")]
    public int NumChannels { get; set; } = 1;

    // --- Features ---

    [JsonPropertyName("language_hints")]
    public List<string> LanguageHints { get; set; } = new();

    [JsonPropertyName("enable_global_speaker_diarization")]
    public bool EnableGlobalSpeakerDiarization { get; set; } = true;

    [JsonPropertyName("enable_language_identification")]
    public bool EnableLanguageIdentification { get; set; } = false;

    [JsonPropertyName("enable_endpoint_detection")]
    public bool EnableEndpointDetection { get; set; } = true;

    // --- Advanced Features (Nuevos) ---

    /// <summary>
    /// Contexto para mejorar la precisión (términos médicos, nombres, etc).
    /// </summary>
    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // <-- CORREGIDO
    public SonioxContext? Context { get; set; }

    /// <summary>
    /// Configuración para traducción en tiempo real.
    /// </summary>
    [JsonPropertyName("translation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // <-- CORREGIDO
    public SonioxTranslationConfig? Translation { get; set; }
}
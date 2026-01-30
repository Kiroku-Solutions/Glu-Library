using System.Text.Json.Serialization;

namespace Glu_Library.Models.WebSocket;

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

    // ✅ RESTAURADO: Necesario para el endpoint V3
    [JsonPropertyName("language_hints")]
    public List<string> LanguageHints { get; set; } = new();

    [JsonPropertyName("enable_global_speaker_diarization")]
    public bool EnableGlobalSpeakerDiarization { get; set; } = true;

    [JsonPropertyName("enable_endpoint_detection")]
    public bool EnableEndpointDetection { get; set; } = true;

    // --- Advanced Features ---

    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SonioxContext? Context { get; set; }

    [JsonPropertyName("translation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SonioxTranslationConfig? Translation { get; set; }
}
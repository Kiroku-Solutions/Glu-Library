using System.Text.Json.Serialization;

namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Configuration for real-time translation during the transcription session.
/// Supports both one-way (all to target) and two-way (bi-directional) translation modes.
/// </summary>
public class SonioxTranslationConfig
{
    /// <summary>
    /// The translation mode: "one_way" or "two_way".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "one_way";

    /// <summary>
    /// The target language code (e.g., "es", "fr") for "one_way" translation.
    /// All detected speech will be translated into this language.
    /// </summary>
    [JsonPropertyName("target_language")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TargetLanguage { get; set; }

    /// <summary>
    /// The first language code for "two_way" translation (e.g., "en").
    /// </summary>
    [JsonPropertyName("language_a")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LanguageA { get; set; }

    /// <summary>
    /// The second language code for "two_way" translation (e.g., "es").
    /// </summary>
    [JsonPropertyName("language_b")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LanguageB { get; set; }
}
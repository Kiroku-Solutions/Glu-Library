using System.Text.Json.Serialization;

namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Configuration for real-time translation.
/// </summary>
public class SonioxTranslationConfig
{
    /// <summary>
    /// Translation type: "one_way" or "two_way".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "one_way";

    /// <summary>
    /// Target language code (ISO) for one-way translation (e.g., "es").
    /// Required if Type is "one_way".
    /// </summary>
    [JsonPropertyName("target_language")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TargetLanguage { get; set; }

    /// <summary>
    /// First language code for two-way translation.
    /// Required if Type is "two_way".
    /// </summary>
    [JsonPropertyName("language_a")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LanguageA { get; set; }

    /// <summary>
    /// Second language code for two-way translation.
    /// Required if Type is "two_way".
    /// </summary>
    [JsonPropertyName("language_b")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LanguageB { get; set; }
}

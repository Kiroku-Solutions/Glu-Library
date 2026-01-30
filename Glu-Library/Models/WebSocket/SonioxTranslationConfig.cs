using System.Text.Json.Serialization;

namespace Glu_Library.Models.WebSocket;

public class SonioxTranslationConfig
{
    /// <summary>
    /// Tipo de traducción: "one_way" o "two_way".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "one_way";

    // Para One-Way (Ej: Traducir todo a Español "es")
    [JsonPropertyName("target_language")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // <-- CORREGIDO
    public string? TargetLanguage { get; set; }

    // Para Two-Way (Ej: Inglés <-> Español)
    [JsonPropertyName("language_a")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // <-- CORREGIDO
    public string? LanguageA { get; set; }

    [JsonPropertyName("language_b")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // <-- CORREGIDO
    public string? LanguageB { get; set; }
}
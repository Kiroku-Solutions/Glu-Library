using System.Text.Json.Serialization;

namespace Glu_Library.Models.WebSocket;

public class SonioxContext
{
    /// <summary>
    /// Información clave-valor (domain, topic, doctor, patient).
    /// </summary>
    [JsonPropertyName("general")]
    public List<ContextKeyValuePair>? General { get; set; }

    /// <summary>
    /// Texto libre para contexto (resúmenes, historias clínicas, etc).
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Lista de palabras específicas (nombres de medicamentos, marcas).
    /// </summary>
    [JsonPropertyName("terms")]
    public List<string>? Terms { get; set; }

    /// <summary>
    /// Mapeo de traducciones específicas.
    /// </summary>
    [JsonPropertyName("translation_terms")]
    public List<TranslationTerm>? TranslationTerms { get; set; }
}

public class ContextKeyValuePair
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class TranslationTerm
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;
}
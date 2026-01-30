using System.Text.Json.Serialization;

namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Represents the context configuration used to improve transcription accuracy.
/// Context allows the model to understand the domain, recognize specific terms,
/// and apply custom translation rules.
/// </summary>
public class SonioxContext
{
    /// <summary>
    /// A list of structured key-value pairs providing general metadata
    /// about the conversation (e.g., domain, topic, doctor, patient).
    /// </summary>
    [JsonPropertyName("general")]
    public List<ContextKeyValuePair>? General { get; set; }

    /// <summary>
    /// Free-form text providing background information or reference material
    /// (e.g., medical history, meeting summaries) to help the model infer context.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// A list of domain-specific words or phrases (e.g., medication names, brand names, jargon)
    /// that should be recognized with higher priority.
    /// </summary>
    [JsonPropertyName("terms")]
    public List<string>? Terms { get; set; }

    /// <summary>
    /// A list of custom translation mappings to enforce specific translations
    /// for certain terms (e.g., keeping brand names unchanged).
    /// </summary>
    [JsonPropertyName("translation_terms")]
    public List<TranslationTerm>? TranslationTerms { get; set; }
}

/// <summary>
/// Represents a single piece of metadata within the general context.
/// </summary>
public class ContextKeyValuePair
{
    /// <summary>
    /// The category or identifier of the information (e.g., "domain", "topic").
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The specific value associated with the key (e.g., "Healthcare", "Cardiology").
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Defines a custom translation rule for a specific term.
/// </summary>
public class TranslationTerm
{
    /// <summary>
    /// The original term in the source language.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// The enforced translation for the target language.
    /// </summary>
    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;
}
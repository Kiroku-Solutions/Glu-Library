using Glu_Library.Models.WebSocket;

namespace Glu_Library.Models;

/// <summary>
/// Configuration parameters for a specific transcription session.
/// Allows overriding global settings at runtime (e.g., from UI dropdowns).
/// </summary>
public class SonioxSessionConfig
{
    /// <summary>
    /// Optional API Key override. If null, the one from appsettings.json is used.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// List of language codes to hint (e.g., "es", "en", "fr").
    /// Matches the "Language Selection" UI.
    /// </summary>
    public List<string>? LanguageHints { get; set; }

    /// <summary>
    /// Optional context (medical terms, patient names) for this specific session.
    /// </summary>
    public SonioxContext? Context { get; set; }
}
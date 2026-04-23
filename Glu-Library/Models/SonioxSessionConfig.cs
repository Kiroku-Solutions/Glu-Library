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
    /// Audio Sample Rate (e.g., 16000, 44100).
    /// </summary>
    public int? SampleRate { get; set; }

    /// <summary>
    /// Optional Model override (e.g., "es_v2", "en_v2"). If null, the global default is used.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// List of language codes to hint (e.g., "es", "en", "fr").
    /// Matches the "Language Selection" UI.
    /// </summary>
    public List<string>? LanguageHints { get; set; }

    /// <summary>
    /// When true, the model will strongly prefer producing output only in the specified languages.
    /// Helpful to prevent language bleeding.
    /// </summary>
    public bool LanguageHintsStrict { get; set; }



    /// <summary>
    /// Configuration for real-time translation (one-way or two-way).
    /// </summary>
    public SonioxTranslationConfig? Translation { get; set; }

    /// <summary>
    /// Optional client reference ID for tracking.
    /// </summary>
    public string? ClientReferenceId { get; set; }

    /// <summary>
    /// Optional context (medical terms, patient names) for this specific session.
    /// </summary>
    public SonioxContext? Context { get; set; }

    /// <summary>
    /// Enable global speaker diarization.
    /// </summary>
    public bool EnableGlobalSpeakerDiarization { get; set; }

    /// <summary>
    /// Optional: Force a specific number of speakers (2-5). 
    /// If null, the model attempts to detect automatically (if diarization is enabled).
    /// </summary>
    public int? NumSpeakers { get; set; }



    /// <summary>
    /// Enable automatic endpoint detection to finish phrases.
    /// </summary>
    public bool EnableEndpointDetection { get; set; }

    /// <summary>
    /// Enable language identification to detect the language of each phrase.
    /// </summary>
    public bool EnableLanguageIdentification { get; set; }
}
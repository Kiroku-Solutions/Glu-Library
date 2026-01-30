using System.Text.Json.Serialization;

namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Representa la respuesta JSON que envía Soniox en tiempo real.
/// Basado en la documentación stt-rt-v3.
/// </summary>
public class SonioxStreamResponse
{
    /// <summary>
    /// Lista unificada de tokens (palabras, subpalabras, signos).
    /// Contiene tanto texto provisional como final.
    /// </summary>
    [JsonPropertyName("tokens")]
    public List<SonioxToken>? Tokens { get; set; }

    /// <summary>
    /// Cantidad de audio (ms) procesado en tokens finales.
    /// </summary>
    [JsonPropertyName("final_audio_proc_ms")]
    public long FinalAudioProcMs { get; set; }

    /// <summary>
    /// Cantidad total de audio (ms) procesado.
    /// </summary>
    [JsonPropertyName("total_audio_proc_ms")]
    public long TotalAudioProcMs { get; set; }

    /// <summary>
    /// Indica si la sesión ha terminado.
    /// </summary>
    [JsonPropertyName("finished")]
    public bool IsFinished { get; set; }
    
    // Campos de Error (para manejo robusto)
    [JsonPropertyName("error_code")]
    public int? ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Representa una unidad de texto (palabra/subpalabra) con sus metadatos.
/// </summary>
public class SonioxToken
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Si es true, este token está confirmado y no cambiará.
    /// Si es false, es provisional y puede cambiar o desaparecer.
    /// </summary>
    [JsonPropertyName("is_final")]
    public bool IsFinal { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("start_ms")]
    public long StartMs { get; set; }

    [JsonPropertyName("end_ms")]
    public long EndMs { get; set; }

    /// <summary>
    /// ID del hablante (si la diarización está activa).
    /// </summary>
    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    /// <summary>
    /// Idioma detectado para este token.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Estado de traducción: "none", "original", "translation".
    /// </summary>
    [JsonPropertyName("translation_status")]
    public string? TranslationStatus { get; set; }
}
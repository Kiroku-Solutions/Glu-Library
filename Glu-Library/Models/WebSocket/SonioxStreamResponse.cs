using System.Text.Json.Serialization;

namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Represents the real-time response message received from the Soniox WebSocket API (v3).
/// Contains the stream of transcribed tokens and processing metadata.
/// </summary>
public class SonioxStreamResponse
{
    /// <summary>
    /// A unified list of tokens (words, sub-words, punctuation).
    /// This list may contain both provisional (non-final) and confirmed (final) tokens.
    /// </summary>
    [JsonPropertyName("tokens")]
    public List<SonioxToken>? Tokens { get; set; }

    /// <summary>
    /// The duration of audio (in milliseconds) that has been fully processed into final tokens.
    /// </summary>
    [JsonPropertyName("final_audio_proc_ms")]
    public long FinalAudioProcMs { get; set; }

    /// <summary>
    /// The total duration of audio (in milliseconds) processed so far, including non-final segments.
    /// </summary>
    [JsonPropertyName("total_audio_proc_ms")]
    public long TotalAudioProcMs { get; set; }

    /// <summary>
    /// Indicates whether the transcription session has officially finished.
    /// </summary>
    [JsonPropertyName("finished")]
    public bool IsFinished { get; set; }
    
    // --- Error Handling Fields ---

    /// <summary>
    /// The numeric error code returned by the server, if an error occurred.
    /// </summary>
    [JsonPropertyName("error_code")]
    public int? ErrorCode { get; set; }

    /// <summary>
    /// A human-readable message explaining the error, if present.
    /// </summary>
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents a single unit of text (word, sub-word, or punctuation) 
/// returned by the recognition engine, along with its metadata.
/// </summary>
public class SonioxToken
{
    /// <summary>
    /// The text content of the token.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this token is confirmed (true) or provisional (false).
    /// Final tokens will not change, whereas non-final tokens are hypotheses that may be updated.
    /// </summary>
    [JsonPropertyName("is_final")]
    public bool IsFinal { get; set; }

    /// <summary>
    /// The confidence score of the recognition, typically between 0.0 and 1.0.
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    /// <summary>
    /// The start time of the token in the audio stream (in milliseconds).
    /// </summary>
    [JsonPropertyName("start_ms")]
    public long StartMs { get; set; }

    /// <summary>
    /// The end time of the token in the audio stream (in milliseconds).
    /// </summary>
    [JsonPropertyName("end_ms")]
    public long EndMs { get; set; }

    /// <summary>
    /// The identifier of the detected speaker (e.g., "1", "2") if speaker diarization is enabled.
    /// </summary>
    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    /// <summary>
    /// The ISO language code detected for this specific token.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Indicates the translation status of the token.
    /// Possible values: "none", "original", or "translation".
    /// </summary>
    [JsonPropertyName("translation_status")]
    public string? TranslationStatus { get; set; }
}
namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Initial message sent to Soniox WebSocket to start transcription.
/// </summary>
public class SonioxStartRequest
{
    public string? Token { get; set; }
    public string? Model { get; set; }
    public bool EnableDiarization { get; set; } = true;
    public bool EnablePartialResults { get; set; } = true;
    public string Language { get; set; } = "en";
}
namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Represents a transcription message received from Soniox WebSocket.
/// </summary>
public class SonioxStreamResponse
{
    public string? Text { get; set; }
    public bool IsFinal { get; set; }
    public string? Speaker { get; set; }
    public double Confidence { get; set; }
    public double StartTime { get; set; }
    public double EndTime { get; set; }
}
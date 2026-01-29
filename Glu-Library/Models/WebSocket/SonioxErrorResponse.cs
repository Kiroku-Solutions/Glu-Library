namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Represents an error message received from Soniox WebSocket.
/// </summary>
public class SonioxErrorResponse
{
    public string? Error { get; set; }
    public string? Message { get; set; }
    public int? Code { get; set; }
}
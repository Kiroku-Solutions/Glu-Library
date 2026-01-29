namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Represents token payload received via WebSocket.
/// </summary>
public class SonioxToken
{
    public string? Token { get; set; }
    public long Exp { get; set; }
}
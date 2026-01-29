namespace Glu_Library.Models;

/// <summary>
/// Represents the authentication token response from Soniox.
/// </summary>
public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string WebSocketUrl { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
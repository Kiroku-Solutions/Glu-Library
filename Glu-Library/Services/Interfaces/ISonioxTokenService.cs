using Glu_Library.Models;

namespace Glu_Library.Services.Interfaces;

/// <summary>
/// Interface defining the contract for the Soniox token generation service.
/// Handles the retrieval of temporary authentication tokens required for the client-side WebSocket connection.
/// </summary>
public interface ISonioxTokenService
{
    /// <summary>
    /// Requests a temporary API token from the Soniox backend.
    /// This token is used to authenticate the browser session without exposing the master API key.
    /// </summary>
    /// <param name="durationMinutes">The validity duration of the token in minutes (default: 5).</param>
    /// <returns>A <see cref="TokenResponse"/> containing the token string and expiration details.</returns>
    Task<TokenResponse> GetTemporaryTokenAsync(int durationMinutes = 5);
}
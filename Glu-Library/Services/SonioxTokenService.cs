using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Glu_Library.Configuration;
using Glu_Library.Models;
using Glu_Library.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Glu_Library.Services;

/// <summary>
/// Service responsible for managing authentication with the Soniox API.
/// Handles the creation of temporary tokens to allow secure client-side WebSocket connections
/// without exposing the server-side Master API Key.
/// </summary>
public class SonioxTokenService : ISonioxTokenService
{
    private readonly HttpClient _httpClient;
    private readonly SonioxOptions _options;
    private readonly ILogger<SonioxTokenService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SonioxTokenService"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance configured via dependency injection.</param>
    /// <param name="options">The configuration options containing the Soniox API key and base URL.</param>
    /// <param name="logger">The logger instance for capturing errors and operational information.</param>
    public SonioxTokenService(HttpClient httpClient, IOptions<SonioxOptions> options, ILogger<SonioxTokenService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TokenResponse> GetTemporaryTokenAsync(int durationMinutes = 5)
    {
        // 1. Security Validation
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            throw new InvalidOperationException("Glu Error: Soniox API Key is not configured.");
        }

        // 2. Prepare the API Call
        // Use the Base URL from configuration (trimming trailing slashes) or fallback to default
        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var requestUrl = $"{baseUrl}/v1/create_temporary_api_key";
        
        var requestBody = new
        {
            usage_type = "transcribe_websocket",
            expires_in_s = durationMinutes * 60
        };

        // Server-to-Server Authentication using the Master Key
        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        try
        {
            // 3. Execute Request
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Soniox Error ({response.StatusCode}): {errorContent}");
                throw new HttpRequestException($"Failed to generate token: {response.ReasonPhrase}");
            }

            // 4. Parse Response (Map snake_case JSON to C# Object)
            var rawResult = await response.Content.ReadFromJsonAsync<SonioxRawResponse>();

            if (rawResult == null || string.IsNullOrEmpty(rawResult.Key))
            {
                throw new Exception("The response from Soniox did not contain a valid token.");
            }

            // 5. Return Clean Model
            // We transform the HTTPS base URL to a WSS URL for the WebSocket connection
            return new TokenResponse
            {
                Token = rawResult.Key,
                ExpiresAt = DateTime.UtcNow.AddSeconds(rawResult.ExpiresInSeconds),
                WebSocketUrl = $"{baseUrl.Replace("https://", "wss://")}/transcribe-websocket",
                Model = _options.DefaultModel
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical failure in Glu Token Service");
            throw;
        }
    }

    /// <summary>
    /// Private internal class to map the raw "dirty" JSON response from the Soniox API.
    /// Matches the snake_case format returned by the endpoint (e.g., "expires_in_s").
    /// </summary>
    private class SonioxRawResponse
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = "";

        [JsonPropertyName("expires_in_s")]
        public int ExpiresInSeconds { get; set; }
    }
}
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Text.Json;
using Glu_Library.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Glu_Library.Services;

/// <summary>
/// Client for Soniox Async API (HTTP/REST).
/// Covers A1-A9 features.
/// </summary>
public class SonioxRestClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SonioxRestClient> _logger;
    private readonly string _apiKey;

    public SonioxRestClient(HttpClient httpClient, IOptions<SonioxWebSocketOptions> options, ILogger<SonioxRestClient> logger)
    {
        _logger = logger;
        _apiKey = options.Value.Token;
        _httpClient = httpClient;
        
        // V-02: TLS Enforcement and Base setup
        _httpClient.BaseAddress = new Uri("https://api.soniox.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    /// <summary>
    /// Uploads a file for transcription.
    /// </summary>
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync("files", content, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return result.GetProperty("file_id").GetString()!;
    }

    /// <summary>
    /// Starts a transcription job for an uploaded file.
    /// </summary>
    public async Task<string> TranscribeAsync(string fileId, string model = "en_v2", CancellationToken ct = default)
    {
        var request = new { file_id = fileId, model = model };
        var response = await _httpClient.PostAsJsonAsync("transcriptions", request, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return result.GetProperty("transcription_id").GetString()!;
    }

    /// <summary>
    /// Gets the status/result of a transcription job.
    /// </summary>
    public async Task<string> GetTranscriptionStatusAsync(string transcriptionId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"transcriptions/{transcriptionId}", ct);
        response.EnsureSuccessStatusCode();
        // Return full JSON string for the caller to parse
        return await response.Content.ReadAsStringAsync(ct);
    }
    
    /// <summary>
    /// Creates a temporary API key for client-side usage.
    /// </summary>
    public async Task<string> CreateTemporaryKeyAsync(string usageType, int expiresInSeconds, CancellationToken ct = default)
    {
        var request = new { usage_type = usageType, expires_in = expiresInSeconds };
        var response = await _httpClient.PostAsJsonAsync("api_keys", request, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return result.GetProperty("key").GetString()!;
    }
}

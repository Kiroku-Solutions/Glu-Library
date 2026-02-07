using System.Net.Http.Json;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;
using Glu_Library.Configuration;
using Glu_Library.Models.Rest;
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
        
        // Fix: API returns "id", not "file_id"
        if (result.TryGetProperty("id", out var idProp))
        {
            return idProp.GetString()!;
        }
        
        // Fallback or throws
        return result.GetProperty("id").GetString()!;
    }

    /// <summary>
    /// Starts a transcription job for an uploaded file or URL.
    /// </summary>
    public async Task<string> TranscribeAsync(SonioxAsyncTranscriptionRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("transcriptions", request, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        
        // Fix: API returns "id", not "transcription_id"
        return result.GetProperty("id").GetString()!;
    }

    /// <summary>
    /// Gets the status/result of a transcription job.
    /// </summary>
    public async Task<string> GetTranscriptionStatusAsync(string transcriptionId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"transcriptions/{transcriptionId}", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }
    
    /// <summary>
    /// Retrieves the full transcript for a completed transcription.
    /// </summary>
    public async Task<string> GetTranscriptAsync(string transcriptionId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"transcriptions/{transcriptionId}/transcript", ct);
        response.EnsureSuccessStatusCode();
        // Returns the raw JSON of the transcript
        return await response.Content.ReadAsStringAsync(ct);
    }

    /// <summary>
    /// Deletes a file from Soniox storage.
    /// </summary>
    public async Task DeleteFileAsync(string fileId, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"files/{fileId}", ct);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Deletes a transcription job.
    /// </summary>
    public async Task DeleteTranscriptionAsync(string transcriptionId, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"transcriptions/{transcriptionId}", ct);
        response.EnsureSuccessStatusCode();
    }
    
    /// <summary>
    /// Creates a temporary API key for client-side usage.
    /// </summary>
    public async Task<string> CreateTemporaryKeyAsync(string usageType, int expiresInSeconds, string? apiKeyOverride = null, CancellationToken ct = default)
    {
        // Manual JSON construction
        var json = JsonSerializer.Serialize(new
        {
            usage_type = usageType,
            expires_in_seconds = expiresInSeconds
        });

        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        HttpResponseMessage response;

        if (!string.IsNullOrEmpty(apiKeyOverride))
        {
            // Use a temporary client for the custom key override
            using var tempClient = new HttpClient();
            tempClient.BaseAddress = _httpClient.BaseAddress;
            tempClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKeyOverride);
            response = await tempClient.PostAsync("auth/temporary-api-key", content, ct);
        }
        else
        {
            // Use the default configured client
            response = await _httpClient.PostAsync("auth/temporary-api-key", content, ct);
        }
        
        if (!response.IsSuccessStatusCode)
        {
             var errorBody = await response.Content.ReadAsStringAsync(ct);
             _logger.LogError("Soniox API Error ({StatusCode}): {Body}", response.StatusCode, errorBody);
             response.EnsureSuccessStatusCode();
        }
        
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return result.GetProperty("api_key").GetString()!;
    }
}

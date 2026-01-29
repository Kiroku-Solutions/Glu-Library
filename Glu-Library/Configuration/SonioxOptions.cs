namespace Glu_Library.Configuration;

/// <summary>
/// Strongly-typed configuration options for Glu.
/// </summary>
public class SonioxOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json (e.g., "Soniox").
    /// </summary>
    public const string SectionName = "Soniox";

    /// <summary>
    /// Secret Soniox API Key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The base URL for the API (defaults to production).
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.soniox.com/";

    /// <summary>
    /// Default language model to use (e.g., "en_v2").
    /// </summary>
    public string DefaultModel { get; set; } = "en_v2";
}
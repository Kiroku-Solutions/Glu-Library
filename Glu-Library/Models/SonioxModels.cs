
namespace Glu_Library.Models;

/// <summary>
/// Constants for available Soniox models.
/// </summary>
public static class SonioxModels
{
    // Real-time Models
    
    /// <summary>
    /// Next-generation real-time speech recognition model.
    /// Recommended for all new real-time applications.
    /// </summary>
    public const string RealTimeV4 = "stt-rt-v4";

    /// <summary>
    /// Legacy real-time model.
    /// Active until Feb 28, 2026.
    /// </summary>
    public const string RealTimeV3 = "stt-rt-v3";

    // Async Models
    
    /// <summary>
    /// Latest generation of async speech recognition model.
    /// Recommended for all new async applications.
    /// </summary>
    public const string AsyncV4 = "stt-async-v4";

    /// <summary>
    /// Legacy async model.
    /// Active until Feb 28, 2026.
    /// </summary>
    public const string AsyncV3 = "stt-async-v3";

    /// <summary>
    /// Gets all available Real-time models.
    /// </summary>
    public static readonly IReadOnlyList<string> AllRealTime = new List<string>
    {
        RealTimeV4,
        RealTimeV3
    };

    /// <summary>
    /// Gets all available Async models.
    /// </summary>
    public static readonly IReadOnlyList<string> AllAsync = new List<string>
    {
        AsyncV4,
        AsyncV3
    };
}

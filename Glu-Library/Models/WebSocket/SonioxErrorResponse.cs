namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Represents an error payload received from the Soniox WebSocket.
/// This message is usually sent when the server cannot process
/// a request or when the session fails.
/// </summary>
public class SonioxErrorResponse
{
    /// <summary>
    /// Short error identifier or type.
    /// Example: "authentication_error", "invalid_request".
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Human-readable description of the error.
    /// Useful for logging and debugging purposes.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Optional numeric error code provided by Soniox.
    /// Can be used to map specific error handling logic.
    /// </summary>
    public int? Code { get; set; }
}
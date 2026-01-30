namespace Glu_Library.Models.WebSocket;

/// <summary>
/// Represents an error payload received from the Soniox WebSocket API.
/// This message is sent when the server cannot process a request, 
/// encounters a validation issue, or when the session fails unexpectedly.
/// </summary>
public class SonioxErrorResponse
{
    /// <summary>
    /// Short identifier or type of the error.
    /// Example: "authentication_error", "invalid_request".
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// A human-readable description of the error providing details on what went wrong.
    /// Useful for logging and debugging purposes.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Optional numeric error code provided by Soniox.
    /// Can be used to map specific error handling logic in the client.
    /// </summary>
    public int? Code { get; set; }
}
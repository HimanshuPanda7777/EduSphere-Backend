namespace SharedKernel;

/// <summary>
/// Standard error response shape used across all EduSphere services.
/// Every error response from any service returns this same JSON structure.
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}

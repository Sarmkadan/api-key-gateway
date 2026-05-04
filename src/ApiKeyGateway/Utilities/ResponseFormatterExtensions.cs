// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Extension methods for formatting HTTP responses.
/// Provides consistent response structure across the API.
/// All API responses follow a standard envelope with data, metadata, and errors.
/// </summary>
public static class ResponseFormatterExtensions
{
    /// <summary>
    /// Creates a successful response envelope.
    /// </summary>
    public static ApiResponse<T> Success<T>(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = 200,
            Data = data,
            Message = message ?? "Operation successful"
        };
    }

    /// <summary>
    /// Creates an error response envelope.
    /// </summary>
    public static ApiResponse<T> Error<T>(
        int statusCode,
        string message,
        string? errorCode = null,
        object? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            ErrorCode = errorCode,
            Details = details
        };
    }

    /// <summary>
    /// Creates a paginated response for list endpoints.
    /// </summary>
    public static PaginatedResponse<T> Paginated<T>(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResponse<T>
        {
            Items = items.ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasMore = pageNumber < totalPages
        };
    }
}

/// <summary>
/// Standard API response envelope used for all responses.
/// </summary>
public record ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public object? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response for paginated list endpoints.
/// </summary>
public record PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasMore { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

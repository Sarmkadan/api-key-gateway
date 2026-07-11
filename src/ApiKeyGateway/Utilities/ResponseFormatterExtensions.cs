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
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="data">The response data payload.</param>
    /// <param name="message">Optional success message. Defaults to "Operation successful".</param>
    /// <returns>A configured <see cref="ApiResponse{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null and T is a non-nullable reference type.</exception>
    public static ApiResponse<T> Success<T>(T data, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(data);

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
    /// <typeparam name="T">The type of data expected in a successful response.</typeparam>
    /// <param name="statusCode">HTTP status code for the error.</param>
    /// <param name="message">Human-readable error message.</param>
    /// <param name="errorCode">Optional machine-readable error code.</param>
    /// <param name="details">Optional additional error details.</param>
    /// <returns>A configured <see cref="ApiResponse{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="statusCode"/> is not a valid HTTP status code.</exception>
    public static ApiResponse<T> Error<T>(
        int statusCode,
        string message,
        string? errorCode = null,
        object? details = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (statusCode < 400 || statusCode >= 600)
        {
            throw new ArgumentOutOfRangeException(nameof(statusCode), statusCode, "Status code must be a valid HTTP error code (400-599).");
        }

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
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="items">The collection of items to paginate.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <returns>A configured <see cref="PaginatedResponse{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="pageNumber"/> or <paramref name="pageSize"/> are less than 1, or when <paramref name="totalCount"/> is negative.</exception>
    public static PaginatedResponse<T> Paginated<T>(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "Page number must be 1 or greater.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be 1 or greater.");
        }

        if (totalCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalCount), totalCount, "Total count cannot be negative.");
        }

        var totalPages = (int)Math.Max(0, Math.Ceiling(totalCount / (double)pageSize));

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
/// <typeparam name="T">The type of data contained in the response.</typeparam>
public sealed record ApiResponse<T>
{
    /// <summary>
    /// Gets or sets whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the response data payload.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the human-readable message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the machine-readable error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets additional error details.
    /// </summary>
    public object? Details { get; set; }

    /// <summary>
    /// Gets the timestamp when the response was created (UTC).
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Response for paginated list endpoints.
/// </summary>
/// <typeparam name="T">The type of items in the paginated collection.</typeparam>
public sealed record PaginatedResponse<T>
{
    /// <summary>
    /// Gets the list of items for the current page.
    /// </summary>
    public List<T> Items { get; init; } = new();

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Gets whether there are more pages available.
    /// </summary>
    public bool HasMore { get; init; }

    /// <summary>
    /// Gets the timestamp when the response was created (UTC).
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Fluent builder for constructing API responses.
/// Ensures consistent response structure across all endpoints.
/// Makes it easy to add metadata, pagination, or custom fields to responses.
/// </summary>
public sealed class ApiResponseBuilder<T>
{
    private T? _data;
    private bool _success = true;
    private int _statusCode = 200;
    private string? _message;
    private string? _errorCode;
    private Dictionary<string, object>? _metadata;
    private List<string>? _errors;

    /// <summary>
    /// Sets the response data.
    /// </summary>
    public ApiResponseBuilder<T> WithData(T? data)
    {
        _data = data;
        return this;
    }

    /// <summary>
    /// Marks response as success with optional message.
    /// </summary>
    public ApiResponseBuilder<T> Success(string? message = null)
    {
        _success = true;
        _statusCode = 200;
        _message = message ?? "Success";
        return this;
    }

    /// <summary>
    /// Marks response as error with code and message.
    /// </summary>
    public ApiResponseBuilder<T> Error(int statusCode, string message, string? errorCode = null)
    {
        _success = false;
        _statusCode = statusCode;
        _message = message;
        _errorCode = errorCode;
        return this;
    }

    /// <summary>
    /// Adds metadata to response.
    /// </summary>
    public ApiResponseBuilder<T> WithMetadata(string key, object value)
    {
        _metadata ??= new();
        _metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Adds error message to collection.
    /// </summary>
    public ApiResponseBuilder<T> AddError(string error)
    {
        _errors ??= new();
        _errors.Add(error);
        return this;
    }

    /// <summary>
    /// Builds the final response object.
    /// </summary>
    public object Build()
    {
        var response = new
        {
            success = _success,
            statusCode = _statusCode,
            message = _message,
            errorCode = _errorCode,
            data = _data,
            errors = _errors,
            metadata = _metadata,
            timestamp = DateTime.UtcNow
        };

        return response;
    }
}

/// <summary>
/// Factory for creating API response builders.
/// </summary>
public static class ApiResponseBuilderFactory
{
    /// <summary>
    /// Creates a builder for a successful response.
    /// </summary>
    public static ApiResponseBuilder<T> Success<T>(T data, string? message = null) =>
        new ApiResponseBuilder<T>()
            .WithData(data)
            .Success(message);

    /// <summary>
    /// Creates a builder for an error response.
    /// </summary>
    public static ApiResponseBuilder<T> Error<T>(int statusCode, string message, string? errorCode = null) =>
        new ApiResponseBuilder<T>()
            .Error(statusCode, message, errorCode);

    /// <summary>
    /// Creates a 404 Not Found response.
    /// </summary>
    public static object NotFound(string resource = "Resource") =>
        Error<object?>(404, $"{resource} not found", "NOT_FOUND").Build();

    /// <summary>
    /// Creates a 400 Bad Request response.
    /// </summary>
    public static object BadRequest(string message, params string[] errors)
    {
        var builder = Error<object?>(400, message, "BAD_REQUEST");
        foreach (var error in errors)
        {
            builder.AddError(error);
        }
        return builder.Build();
    }

    /// <summary>
    /// Creates a 401 Unauthorized response.
    /// </summary>
    public static object Unauthorized(string message = "Unauthorized") =>
        Error<object?>(401, message, "UNAUTHORIZED").Build();

    /// <summary>
    /// Creates a 403 Forbidden response.
    /// </summary>
    public static object Forbidden(string message = "Access denied") =>
        Error<object?>(403, message, "FORBIDDEN").Build();

    /// <summary>
    /// Creates a 429 Too Many Requests response.
    /// </summary>
    public static object TooManyRequests(string message = "Rate limit exceeded") =>
        Error<object?>(429, message, "RATE_LIMIT_EXCEEDED").Build();

    /// <summary>
    /// Creates a 500 Internal Server Error response.
    /// </summary>
    public static object InternalServerError(string message = "An unexpected error occurred") =>
        Error<object?>(500, message, "INTERNAL_SERVER_ERROR").Build();
}

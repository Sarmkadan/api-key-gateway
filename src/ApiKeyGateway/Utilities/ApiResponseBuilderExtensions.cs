// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Extension methods for ApiResponseBuilder<T>
// =============================================================================

using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Provides additional fluent helpers for <see cref="ApiResponseBuilder{T}"/>.
/// </summary>
public static class ApiResponseBuilderExtensions
{
    /// <summary>
    /// Adds a collection of metadata entries to the response.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="builder">The builder instance to extend.</param>
    /// <param name="metadata">A dictionary containing metadata key/value pairs.</param>
    /// <returns>The same <see cref="ApiResponseBuilder{T}"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="metadata"/> is <c>null</c>.</exception>
    public static ApiResponseBuilder<T> WithMetadata<T>(this ApiResponseBuilder<T> builder, IDictionary<string, object> metadata)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(metadata);

        foreach (var kvp in metadata)
        {
            builder.WithMetadata(kvp.Key, kvp.Value);
        }

        return builder;
    }

    /// <summary>
    /// Adds multiple error messages to the response.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="builder">The builder instance to extend.</param>
    /// <param name="errors">A collection of error strings.</param>
    /// <returns>The same <see cref="ApiResponseBuilder{T}"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="errors"/> is <c>null</c>.</exception>
    public static ApiResponseBuilder<T> AddErrors<T>(this ApiResponseBuilder<T> builder, IEnumerable<string> errors)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(errors);

        foreach (var error in errors)
        {
            // Guard against null or whitespace error messages.
            ArgumentException.ThrowIfNullOrEmpty(error);
            builder.AddError(error);
        }

        return builder;
    }

    /// <summary>
    /// Adds standard pagination metadata (page number, page size, total count) to the response.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="builder">The builder instance to extend.</param>
    /// <param name="pageNumber">The current page number (1‑based).</param>
    /// <param name="pageSize">The size of each page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <returns>The same <see cref="ApiResponseBuilder{T}"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any pagination argument is less than zero.</exception>
    public static ApiResponseBuilder<T> WithPagination<T>(this ApiResponseBuilder<T> builder, int pageNumber, int pageSize, int totalCount)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
        if (pageSize < 0)   throw new ArgumentOutOfRangeException(nameof(pageSize),   "Page size cannot be negative.");
        if (totalCount < 0) throw new ArgumentOutOfRangeException(nameof(totalCount), "Total count cannot be negative.");

        builder
            .WithMetadata("pageNumber", pageNumber)
            .WithMetadata("pageSize", pageSize)
            .WithMetadata("totalCount", totalCount);

        return builder;
    }

    /// <summary>
    /// Marks the response as successful and optionally sets a custom success message.
    /// This is a thin wrapper around <see cref="ApiResponseBuilder{T}.Success(string?)"/> that
    /// provides a more expressive method name for fluent usage.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="builder">The builder instance to extend.</param>
    /// <param name="message">An optional success message.</param>
    /// <returns>The same <see cref="ApiResponseBuilder{T}"/> instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public static ApiResponseBuilder<T> AsSuccess<T>(this ApiResponseBuilder<T> builder, string? message = null) =>
        (builder ?? throw new ArgumentNullException(nameof(builder))).Success(message);
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Provides extension methods for the <see cref="ApiKey"/> model.
/// </summary>
public static class ApiKeyExtensions
{
    /// <summary>
    /// Checks if the API key will expire within the specified duration from now.
    /// </summary>
    /// <param name="apiKey">The API key to check.</param>
    /// <param name="duration">The time span to check against.</param>
    /// <returns>True if the API key has an expiration date and it falls within the specified duration; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="apiKey"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="duration"/> is negative.</exception>
    public static bool IsExpiringWithin(this ApiKey apiKey, TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentOutOfRangeException.ThrowIfLessThan(duration, TimeSpan.Zero);

        return apiKey.ExpiresAt.HasValue
            && apiKey.ExpiresAt.Value > DateTime.UtcNow
            && (apiKey.ExpiresAt.Value - DateTime.UtcNow) <= duration;
    }

    /// <summary>
    /// Safely retrieves a value from the API key's metadata dictionary.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <param name="key">The metadata key to retrieve.</param>
    /// <returns>The value associated with the key, or null if the key does not exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="apiKey"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    public static string? GetMetadataValue(this ApiKey apiKey, string key)
    {
        ArgumentNullException.ThrowIfNull(apiKey);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return apiKey.Metadata.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Gets the list of allowed scopes as a collection of strings.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <returns>An enumerable collection of allowed scope strings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="apiKey"/> is null.</exception>
    public static IReadOnlyList<string> GetScopes(this ApiKey apiKey)
    {
        ArgumentNullException.ThrowIfNull(apiKey);

        return string.IsNullOrWhiteSpace(apiKey.AllowedScopes)
            ? Array.Empty<string>()
            : apiKey.AllowedScopes
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList()
                .AsReadOnly();
    }
}
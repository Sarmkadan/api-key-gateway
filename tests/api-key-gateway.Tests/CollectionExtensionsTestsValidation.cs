using System;
using System.Collections.Generic;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides validation extension methods for <see cref="CollectionExtensionsTests"/> instances.
/// </summary>
public static class CollectionExtensionsTestsValidation
{
    /// <summary>
    /// Validates the <see cref="CollectionExtensionsTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this CollectionExtensionsTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if the <see cref="CollectionExtensionsTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this CollectionExtensionsTests value) =>
        value is not null;

    /// <summary>
    /// Ensures that the <see cref="CollectionExtensionsTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this CollectionExtensionsTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
    }
}

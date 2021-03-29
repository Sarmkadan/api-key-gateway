using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ApiKeyGateway.Tests;

public static class CollectionExtensionsTestsValidation
{
    /// <summary>
    /// Validates the <see cref="CollectionExtensionsTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    public static IReadOnlyList<string> Validate(this CollectionExtensionsTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // No specific properties to validate, as CollectionExtensionsTests seems to be a test class with methods only.

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the <see cref="CollectionExtensionsTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>true if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this CollectionExtensionsTests value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="CollectionExtensionsTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
    public static void EnsureValid(this CollectionExtensionsTests value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid CollectionExtensionsTests: {string.Join(", ", problems)}", nameof(value));
        }
    }
}

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Validation helpers for <see cref="IpWhitelistTests"/> test class.
/// </summary>
public static class IpWhitelistTestsValidation
{
    /// <summary>
    /// Validates the <see cref="IpWhitelistTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this IpWhitelistTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // No additional validation needed for the test class itself
        // as it only contains test methods and mock dependencies

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="IpWhitelistTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this IpWhitelistTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="IpWhitelistTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this IpWhitelistTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Validation failed for {nameof(IpWhitelistTests)}.{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}
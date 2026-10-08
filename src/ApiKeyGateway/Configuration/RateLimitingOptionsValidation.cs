// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Validation rules for rate limit policies and per-key overrides. Shared by the startup
/// validator (<see cref="RateLimitingOptionsValidator"/>) and the admin override endpoint so
/// both enforce identical rules.
/// </summary>
public static class RateLimitingOptionsValidation
{
    /// <summary>Smallest allowed rate limit window, in seconds.</summary>
    public const int MinWindowSeconds = 1;

    /// <summary>Largest allowed rate limit window, in seconds (24 hours).</summary>
    public const int MaxWindowSeconds = 86400;

    /// <summary>
    /// Validates all policies and per-key overrides in <paramref name="options"/>.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <returns>A read-only list of error messages; empty when the options are valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
    public static IReadOnlyList<string> Validate(RateLimitingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = new List<string>();

        foreach (var (name, policy) in options.Policies)
        {
            errors.AddRange(ValidatePolicy(name, policy));
        }

        foreach (var (apiKeyId, keyOverride) in options.KeyOverrides)
        {
            errors.AddRange(ValidateKeyOverride(apiKeyId, keyOverride, options.Policies));
        }

        return errors;
    }

    /// <summary>
    /// Validates a single named policy: positive request count and a supported window.
    /// </summary>
    /// <param name="name">The policy name, used in error messages.</param>
    /// <param name="policy">The policy to validate.</param>
    /// <returns>A read-only list of error messages; empty when the policy is valid.</returns>
    public static IReadOnlyList<string> ValidatePolicy(string name, RateLimitPolicyOptions? policy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ["Rate limit policy names must not be empty."];
        }

        if (policy is null)
        {
            return [$"Rate limit policy '{name}' is not configured."];
        }

        var errors = new List<string>();

        if (policy.RequestsPerUnit <= 0)
        {
            errors.Add($"Rate limit policy '{name}': RequestsPerUnit must be positive.");
        }

        if (!IsWindowInRange(policy.WindowSeconds))
        {
            errors.Add($"Rate limit policy '{name}': WindowSeconds must be between {MinWindowSeconds} and {MaxWindowSeconds}.");
        }
        else if (!TryGetUnit(policy.WindowSeconds, out _))
        {
            errors.Add($"Rate limit policy '{name}': WindowSeconds must be 1, 60, 3600 or 86400 to match a supported rate limit unit.");
        }

        return errors;
    }

    /// <summary>
    /// Validates a per-key override: the referenced policy must exist and any explicit values
    /// must be positive or within the window range.
    /// </summary>
    /// <param name="apiKeyId">The API key the override applies to, used in error messages.</param>
    /// <param name="keyOverride">The override to validate.</param>
    /// <param name="policies">The configured policies that the override may reference.</param>
    /// <returns>A read-only list of error messages; empty when the override is valid.</returns>
    public static IReadOnlyList<string> ValidateKeyOverride(
        string apiKeyId,
        RateLimitKeyOverrideOptions? keyOverride,
        IReadOnlyDictionary<string, RateLimitPolicyOptions> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        if (string.IsNullOrWhiteSpace(apiKeyId))
        {
            return ["Rate limit override key id must not be empty."];
        }

        if (keyOverride is null)
        {
            return [$"Rate limit override for key '{apiKeyId}' must not be null."];
        }

        if (string.IsNullOrWhiteSpace(keyOverride.Policy))
        {
            return [$"Rate limit override for key '{apiKeyId}' must reference a policy name."];
        }

        var errors = new List<string>();

        if (!policies.ContainsKey(keyOverride.Policy))
        {
            errors.Add($"Rate limit policy '{keyOverride.Policy}' referenced by key '{apiKeyId}' is not defined.");
        }

        if (keyOverride.RequestsPerUnit is { } requests && requests <= 0)
        {
            errors.Add($"Rate limit override for key '{apiKeyId}': RequestsPerUnit must be positive.");
        }

        if (keyOverride.WindowSeconds is { } window)
        {
            if (!IsWindowInRange(window))
            {
                errors.Add($"Rate limit override for key '{apiKeyId}': WindowSeconds must be between {MinWindowSeconds} and {MaxWindowSeconds}.");
            }
            else if (!TryGetUnit(window, out _))
            {
                errors.Add($"Rate limit override for key '{apiKeyId}': WindowSeconds must be 1, 60, 3600 or 86400 to match a supported rate limit unit.");
            }
        }

        return errors;
    }

    /// <summary>
    /// Maps a window in seconds to the rate limit unit it represents.
    /// </summary>
    /// <param name="windowSeconds">The window length in seconds.</param>
    /// <param name="unit">The matching unit when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> when the window equals one of the supported units; otherwise <see langword="false"/>.</returns>
    public static bool TryGetUnit(int windowSeconds, out RateLimitUnit unit)
    {
        switch (windowSeconds)
        {
            case 1:
                unit = RateLimitUnit.Second;
                return true;
            case 60:
                unit = RateLimitUnit.Minute;
                return true;
            case 3600:
                unit = RateLimitUnit.Hour;
                return true;
            case 86400:
                unit = RateLimitUnit.Day;
                return true;
            default:
                unit = default;
                return false;
        }
    }

    private static bool IsWindowInRange(int windowSeconds) =>
        windowSeconds >= MinWindowSeconds && windowSeconds <= MaxWindowSeconds;
}

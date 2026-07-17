// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Extension methods for <see cref="ApiKeyConsumer"/> providing common operations and validations.
/// This class is static and sealed to prevent inheritance and instantiation.
/// </summary>
public static class ApiKeyConsumerExtensions
{
    /// <summary>
    /// Determines if the consumer is currently active based on both <see cref="ApiKeyConsumer.IsActive"/> flag and <see cref="ApiKeyConsumer.InactiveSince"/> timestamp.
    /// A consumer is considered active only when <see cref="ApiKeyConsumer.IsActive"/> is <see langword="true"/> and <see cref="ApiKeyConsumer.InactiveSince"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="consumer">The consumer to check. Must not be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the consumer is active; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="consumer"/> is <see langword="null"/>.</exception>
    public static bool IsCurrentlyActive(this ApiKeyConsumer consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        return consumer.IsActive && consumer.InactiveSince == null;
    }

    /// <summary>
    /// Gets the consumer's tier level as an enum for type-safe comparisons.
    /// </summary>
    /// <param name="consumer">The consumer to check. Must not be <see langword="null"/>.</param>
    /// <returns>The tier enum value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="consumer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the tier value is not recognized.</exception>
    public static ApiKeyTier GetTier(this ApiKeyConsumer consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);

        return consumer.Tier switch
        {
            "free" => ApiKeyTier.Free,
            "basic" => ApiKeyTier.Basic,
            "pro" => ApiKeyTier.Pro,
            "enterprise" => ApiKeyTier.Enterprise,
            _ => throw new ArgumentException($"Unknown tier value: {consumer.Tier}", nameof(consumer))
        };
    }

    /// <summary>
    /// Determines if the consumer is eligible for a specific tier upgrade.
    /// </summary>
    /// <param name="consumer">The consumer to check. Must not be <see langword="null"/>.</param>
    /// <param name="targetTier">The target tier to check eligibility for.</param>
    /// <returns><see langword="true"/> if the consumer can be upgraded to the target tier; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="consumer"/> is <see langword="null"/>.</exception>
    public static bool CanUpgradeTo(this ApiKeyConsumer consumer, ApiKeyTier targetTier)
    {
        ArgumentNullException.ThrowIfNull(consumer);

        var currentTier = consumer.GetTier();
        return targetTier > currentTier;
    }

    /// <summary>
    /// Gets the consumer's organization domain for email-based routing.
    /// </summary>
    /// <param name="consumer">The consumer to check. Must not be <see langword="null"/>.</param>
    /// <returns>The organization domain if available; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="consumer"/> is <see langword="null"/>.</exception>
    public static string? GetOrganizationDomain(this ApiKeyConsumer consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);

        if (string.IsNullOrWhiteSpace(consumer.Email) || !consumer.Email.Contains('@'))
        {
            return null;
        }

        var parts = consumer.Email.Split('@');
        return parts.Length == 2 ? parts[1] : null;
    }

    /// <summary>
    /// Determines if the consumer has been inactive for a specified period.
    /// </summary>
    /// <param name="consumer">The consumer to check. Must not be <see langword="null"/>.</param>
    /// <param name="daysThreshold">Number of days of inactivity to check for. Must be greater than 0.</param>
    /// <returns><see langword="true"/> if the consumer has been inactive for at least the specified days; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="consumer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="daysThreshold"/> is less than or equal to 0.</exception>
    public static bool IsInactiveForDays(this ApiKeyConsumer consumer, int daysThreshold)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(daysThreshold, 0);

        return consumer.LastActivityAt.HasValue &&
               DateTime.UtcNow.Subtract(consumer.LastActivityAt.Value).TotalDays >= daysThreshold;
    }

    /// <summary>
    /// Safely gets a custom property value by key, returning a default value if not found.
    /// </summary>
    /// <param name="consumer">The consumer to check. Must not be <see langword="null"/>.</param>
    /// <param name="propertyKey">The custom property key. Must not be <see langword="null"/>.</param>
    /// <param name="defaultValue">The default value to return if property doesn't exist.</param>
    /// <returns>The property value or default value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="consumer"/> or <paramref name="propertyKey"/> is <see langword="null"/>.</exception>
    public static string GetCustomProperty(this ApiKeyConsumer consumer, string propertyKey, string defaultValue = "")
    {
        ArgumentNullException.ThrowIfNull(consumer);
        ArgumentNullException.ThrowIfNull(propertyKey);

        return consumer.CustomProperties.TryGetValue(propertyKey, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Safely gets a custom property value as a specific type, with type conversion.
    /// </summary>
    /// <param name="consumer">The consumer to check. Must not be <see langword="null"/>.</param>
    /// <param name="propertyKey">The custom property key. Must not be <see langword="null"/>.</param>
    /// <param name="defaultValue">The default value to return if property doesn't exist or conversion fails.</param>
    /// <returns>The converted property value or default value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="consumer"/> or <paramref name="propertyKey"/> is <see langword="null"/>.</exception>
    public static int GetCustomPropertyAsInt(this ApiKeyConsumer consumer, string propertyKey, int defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        ArgumentNullException.ThrowIfNull(propertyKey);

        if (consumer.CustomProperties.TryGetValue(propertyKey, out var value) &&
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return defaultValue;
    }
}

/// <summary>
/// Enum representing API key consumer tiers
/// </summary>
public enum ApiKeyTier
{
    /// <summary>Free tier with basic API key management</summary>
    Free = 0,

    /// <summary>Basic tier with additional features</summary>
    Basic = 1,

    /// <summary>Pro tier with advanced rate limiting and quotas</summary>
    Pro = 2,

    /// <summary>Enterprise tier with all features and support</summary>
    Enterprise = 3
}
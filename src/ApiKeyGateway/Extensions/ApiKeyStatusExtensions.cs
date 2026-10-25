// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Enums;

namespace ApiKeyGateway.Extensions;

/// <summary>
/// Extension methods for <see cref="ApiKeyStatus"/>.
/// Provides lifecycle checks and display formatting for API key statuses.
/// </summary>
public static class ApiKeyStatusExtensions
{
    /// <summary>
    /// Determines whether the status is <see cref="ApiKeyStatus.Active"/>.
    /// This checks the status only; it does not evaluate the key's expiration date.
    /// </summary>
    /// <param name="status">The status to check.</param>
    /// <returns><see langword="true"/> if the status is Active; otherwise, <see langword="false"/>.</returns>
    public static bool IsActive(this ApiKeyStatus status) =>
        status == ApiKeyStatus.Active;

    /// <summary>
    /// Determines whether the status is terminal, meaning the key can never become usable again.
    /// <see cref="ApiKeyStatus.Expired"/> and <see cref="ApiKeyStatus.Revoked"/> are terminal.
    /// <see cref="ApiKeyStatus.Disabled"/> and <see cref="ApiKeyStatus.Suspended"/> are not.
    /// </summary>
    /// <param name="status">The status to check.</param>
    /// <returns><see langword="true"/> if the status is terminal; otherwise, <see langword="false"/>.</returns>
    public static bool IsTerminal(this ApiKeyStatus status) =>
        status is ApiKeyStatus.Expired or ApiKeyStatus.Revoked;

    /// <summary>
    /// Returns a human-readable name for the status, suitable for display in dashboards and logs.
    /// </summary>
    /// <param name="status">The status to format.</param>
    /// <returns>The display string for the status.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="status"/> is not a defined value.</exception>
    public static string ToDisplayString(this ApiKeyStatus status) => status switch
    {
        ApiKeyStatus.Active => "Active",
        ApiKeyStatus.Disabled => "Disabled",
        ApiKeyStatus.Expired => "Expired",
        ApiKeyStatus.Revoked => "Revoked",
        ApiKeyStatus.Suspended => "Suspended",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Undefined ApiKeyStatus value.")
    };
}

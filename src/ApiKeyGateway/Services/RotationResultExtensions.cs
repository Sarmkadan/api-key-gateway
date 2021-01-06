// =============================================================================
// Author: [Your Name]
// =============================================================================

namespace ApiKeyGateway.Services;

/// <summary>
/// Extension methods for <see cref="RotationResult"/> to enhance key rotation operations.
/// </summary>
public static class RotationResultExtensions
{
    /// <summary>
    /// Determines if the rotation result indicates a successful key rotation.
    /// </summary>
    /// <param name="result">The rotation result.</param>
    /// <returns>True if the rotation was successful; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static bool IsSuccess(this RotationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Success;
    }

    /// <summary>
    /// Gets a human-readable description of the rotation result.
    /// </summary>
    /// <param name="result">The rotation result.</param>
    /// <returns>A formatted description string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static string GetDescription(this RotationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var keyIdText = string.IsNullOrEmpty(result.NewKeyId)
            ? "no new key generated"
            : $"new key {result.NewKeyId}";

        if (result.Success)
        {
            return $"Rotated key {result.OldKeyId} → {keyIdText} for consumer {result.ConsumerId}";
        }

        return $"Failed to rotate key {result.OldKeyId}: {result.FailureReason}";
    }

    /// <summary>
    /// Determines if the rotation result has a failure reason.
    /// </summary>
    /// <param name="result">The rotation result.</param>
    /// <returns>True if the rotation failed with a reason; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static bool HasFailureReason(this RotationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return !string.IsNullOrEmpty(result.FailureReason);
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Provides extension methods for <see cref="ApiKeysController"/> to simplify bulk operations on API keys.
/// </summary>
public static class ApiKeysControllerExtensions
{
    /// <summary>
    /// Toggles the status of an API key by enabling or disabling it.
    /// </summary>
    /// <param name="controller">The controller instance. Must not be <see langword="null"/>.</param>
    /// <param name="keyId">The ID of the key to toggle. Must not be <see langword="null"/>, <see langword="string.Empty"/>, or whitespace.</param>
    /// <param name="enable"><see langword="true"/> to enable the key; <see langword="false"/> to disable it.</param>
    /// <returns>An <see cref="ActionResult{TValue}"/> indicating success or failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="controller"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="keyId"/> is <see langword="null"/>, <see langword="string.Empty"/>, or consists only of whitespace.</exception>
    public static async Task<ActionResult<object>> ToggleKeyStatusAsync(this ApiKeysController controller, string keyId, bool enable)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(keyId);

        return enable
            ? await controller.EnableKey(keyId)
            : await controller.DisableKey(keyId);
    }

    /// <summary>
    /// Revokes multiple API keys in a single batch operation.
    /// </summary>
    /// <param name="controller">The controller instance. Must not be <see langword="null"/>.</param>
    /// <param name="keyIds">The IDs of the keys to revoke. Must not be <see langword="null"/>.</param>
    /// <returns>A read-only list of <see cref="ActionResult{TValue}"/> for each revocation attempt.</returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>Thrown if <paramref name="controller"/> is <see langword="null"/>.</para>
    ///   <para>Thrown if <paramref name="keyIds"/> is <see langword="null"/>.</para>
    /// </exception>
    public static async Task<IReadOnlyList<ActionResult<object>>> RevokeMultipleKeysAsync(this ApiKeysController controller, IEnumerable<string> keyIds)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(keyIds);

        var results = new List<ActionResult<object>>();
        foreach (var keyId in keyIds)
        {
            results.Add(await controller.RevokeKey(keyId));
        }

        return results.AsReadOnly();
    }
}

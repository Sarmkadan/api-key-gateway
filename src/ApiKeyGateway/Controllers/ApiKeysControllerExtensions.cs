using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Provides extension methods for <see cref="ApiKeysController"/>.
/// </summary>
public static class ApiKeysControllerExtensions
{
    /// <summary>
    /// Toggles the status of an API key.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="keyId">The ID of the key to toggle.</param>
    /// <param name="enable">Whether to enable or disable the key.</param>
    /// <returns>The action result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller is null.</exception>
    /// <exception cref="ArgumentException">Thrown if keyId is null or empty.</exception>
    public static async Task<ActionResult<object>> ToggleKeyStatusAsync(this ApiKeysController controller, string keyId, bool enable)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(keyId);

        return enable ? await controller.EnableKey(keyId) : await controller.DisableKey(keyId);
    }

    /// <summary>
    /// Revokes multiple API keys.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <param name="keyIds">The IDs of the keys to revoke.</param>
    /// <returns>A list of action results for each revocation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller or keyIds is null.</exception>
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

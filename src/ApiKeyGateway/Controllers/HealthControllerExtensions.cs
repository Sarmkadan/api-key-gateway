using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Provides extension methods for <see cref="HealthController"/>.
/// </summary>
public static class HealthControllerExtensions
{
    /// <summary>
    /// Checks if the request comes from the local network.
    /// Useful for restricting administrative health checks to local network only.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <returns>True if the request is from the local network or loopback address, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="controller"/> is null.</exception>
    public static bool IsLocalRequest(this HealthController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var connection = controller.HttpContext.Connection;
        var remoteIp = connection.RemoteIpAddress;
        var localIp = connection.LocalIpAddress;

        if (remoteIp is null || localIp is null)
        {
            return false;
        }

        // Check for loopback addresses
        if (IPAddress.IsLoopback(remoteIp) || IPAddress.IsLoopback(localIp))
        {
            return true;
        }

        // Check for exact match
        if (remoteIp.Equals(localIp))
        {
            return true;
        }

        // Check for IPv4-mapped IPv6 addresses
        if (remoteIp.IsIPv4MappedToIPv6)
        {
            return remoteIp.MapToIPv4().Equals(localIp);
        }

        return false;
    }

    /// <summary>
    /// Gets the current timestamp in ISO 8601 format for health logs.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <returns>A string representation of the current UTC time in ISO 8601 format.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="controller"/> is null.</exception>
    public static string GetCurrentHealthTimestamp(this HealthController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);

        return DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
    }
}

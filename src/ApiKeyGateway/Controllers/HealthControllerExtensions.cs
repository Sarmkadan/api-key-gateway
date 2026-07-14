using Microsoft.AspNetCore.Mvc;
using System;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Provides extension methods for <see cref="HealthController"/>.
/// </summary>
public static class HealthControllerExtensions
{
    /// <summary>
    /// Checks if the request comes from the local network.
    /// Useful for restricting administrative health checks.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <returns>True if the request is from the local network, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller is null.</exception>
    public static bool IsLocalRequest(this HealthController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        
        var connection = controller.HttpContext.Connection;
        return connection.RemoteIpAddress?.Equals(connection.LocalIpAddress) == true || 
               connection.RemoteIpAddress?.IsIPv4MappedToIPv6 == true && connection.RemoteIpAddress.MapToIPv4().Equals(connection.LocalIpAddress);
    }

    /// <summary>
    /// Gets the current timestamp in ISO 8601 format for health logs.
    /// </summary>
    /// <param name="controller">The controller instance.</param>
    /// <returns>A string representation of the current UTC time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if controller is null.</exception>
    public static string GetCurrentHealthTimestamp(this HealthController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        
        return DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
    }
}

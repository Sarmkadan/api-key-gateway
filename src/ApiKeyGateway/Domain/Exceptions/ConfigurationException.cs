// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Thrown when gateway configuration is invalid or incomplete
// =============================================================================

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Thrown when gateway configuration is invalid or incomplete
/// </summary>
public class ConfigurationException : ApiKeyGatewayException
{
    public string? Setting { get; init; }

    public ConfigurationException(string message) : base(message) { }

    public ConfigurationException(string message, string setting) : base(message)
    {
        Setting = setting;
    }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }

    public ConfigurationException(string message, string setting, Exception innerException) : base(message, innerException)
    {
        Setting = setting;
    }
}
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
    /// <summary>Name of the configuration setting that caused the error</summary>
    public string? Setting { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="ConfigurationException"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    public ConfigurationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="ConfigurationException"/> with setting name
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="setting">Name of the configuration setting that caused the error.</param>
    public ConfigurationException(string message, string setting) : base(message)
    {
        Setting = setting;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ConfigurationException"/> with inner exception
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of <see cref="ConfigurationException"/> with setting name and inner exception
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="setting">Name of the configuration setting that caused the error.</param>
    /// <param name="innerException">The inner exception.</param>
    public ConfigurationException(string message, string setting, Exception innerException) : base(message, innerException)
    {
        Setting = setting;
    }
}

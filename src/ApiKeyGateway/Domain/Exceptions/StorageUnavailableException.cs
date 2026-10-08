// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;

namespace ApiKeyGateway.Domain.Exceptions;

/// <summary>
/// Thrown when the usage storage provider cannot be reached at startup.
/// </summary>
public class StorageUnavailableException : ApiKeyGatewayException
{
    /// <summary>Name of the storage provider that could not be reached (e.g. "SqlServer")</summary>
    public string? Provider { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="StorageUnavailableException"/>
    /// </summary>
    /// <param name="message">The error message.</param>
    [JsonConstructor]
    public StorageUnavailableException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of <see cref="StorageUnavailableException"/> with provider name
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="provider">Name of the storage provider that could not be reached.</param>
    public StorageUnavailableException(string message, string provider) : base(message)
    {
        Provider = provider;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StorageUnavailableException"/> with provider name and inner exception
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="provider">Name of the storage provider that could not be reached.</param>
    /// <param name="innerException">The exception raised while pinging the store.</param>
    public StorageUnavailableException(string message, string provider, Exception innerException) : base(message, innerException)
    {
        Provider = provider;
    }
}

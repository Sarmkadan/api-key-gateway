// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Data;
using System.Data.Common;

namespace ApiKeyGateway.Repositories;

/// <summary>
/// Provides validation helpers for <see cref="UsageRepository"/> instances.
/// </summary>
public static class UsageRepositoryValidation
{
    /// <summary>
    /// Validates the <see cref="UsageRepository"/> instance for logical consistency.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static IReadOnlyList<string> Validate(this UsageRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate connection state
        if (value.GetConnectionState() != System.Data.ConnectionState.Closed)
        {
            problems.Add("Database connection is not closed. Ensure proper connection lifecycle management.");
        }

        // Validate logger
        if (value.Logger == null)
        {
            problems.Add("Logger dependency is null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="UsageRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    public static bool IsValid(this UsageRepository value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="UsageRepository"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with a detailed message if it is not.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the repository is invalid.</exception>
    public static void EnsureValid(this UsageRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"UsageRepository is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }

    /// <summary>
    /// Gets the database connection associated with this repository.
    /// </summary>
    internal static IDbConnection Connection(this UsageRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        // Use reflection to access the private connection field since we can't modify the class
        var connectionField = typeof(UsageRepository).GetField("_connection",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (connectionField == null)
        {
            throw new InvalidOperationException("Failed to access connection field via reflection.");
        }

        return (IDbConnection)connectionField.GetValue(repository) ?? throw new InvalidOperationException("Connection field is null.");
    }

    /// <summary>
    /// Gets the logger associated with this repository.
    /// </summary>
    internal static ILogger<UsageRepository> Logger(this UsageRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        // Use reflection to access the private logger field since we can't modify the class
        var loggerField = typeof(UsageRepository).GetField("_logger",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (loggerField == null)
        {
            throw new InvalidOperationException("Failed to access logger field via reflection.");
        }

        return (ILogger<UsageRepository>)loggerField.GetValue(repository) ?? throw new InvalidOperationException("Logger field is null.");
    }

    /// <summary>
    /// Gets the current connection state of the repository's database connection.
    /// </summary>
    internal static System.Data.ConnectionState GetConnectionState(this UsageRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        try
        {
            return repository.Connection().State;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            return System.Data.ConnectionState.Broken;
        }
    }
}
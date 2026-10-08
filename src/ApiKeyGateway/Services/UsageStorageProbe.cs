// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data;
using ApiKeyGateway.Configuration;

namespace ApiKeyGateway.Services;

/// <summary>
/// Checks that the usage storage provider is reachable without touching usage data.
/// </summary>
public interface IUsageStorageProbe
{
    /// <summary>Name of the storage provider being probed (e.g. "SqlServer").</summary>
    string ProviderName { get; }

    /// <summary>
    /// Opens a connection to the store and runs a trivial round-trip.
    /// </summary>
    /// <param name="cancellationToken">Token to abort the ping.</param>
    /// <exception cref="Exception">Any error raised by the underlying provider when the store is unreachable.</exception>
    Task PingAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// SQL Server implementation of <see cref="IUsageStorageProbe"/> that runs <c>SELECT 1</c>.
/// </summary>
public sealed class SqlServerUsageStorageProbe : IUsageStorageProbe
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of <see cref="SqlServerUsageStorageProbe"/>.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string used for usage records.</param>
    /// <remarks>
    /// Blank strings are accepted here so construction never pre-empts options validation;
    /// <see cref="Configuration.UsageTrackingOptionsValidator"/> reports them at startup.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is null.</exception>
    public SqlServerUsageStorageProbe(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <inheritdoc />
    public string ProviderName => "SqlServer";

    /// <inheritdoc />
    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new System.Data.SqlClient.SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1";
        command.CommandType = CommandType.Text;
        await command.ExecuteScalarAsync(cancellationToken);
    }
}

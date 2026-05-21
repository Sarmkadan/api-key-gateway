// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data;

namespace ApiKeyGateway.Data;

/// <summary>
/// Database connection abstraction for dependency injection
/// </summary>
public interface IDbConnection : IDisposable
{
    Task OpenAsync();
    Task CloseAsync();
    DbCommand CreateCommand();
    DbParameter CreateParameter();
}

/// <summary>
/// SQL Server connection implementation
/// </summary>
public class SqlServerConnection : IDbConnection
{
    private System.Data.SqlClient.SqlConnection? _connection;
    private readonly string _connectionString;
    private readonly ILogger<SqlServerConnection> _logger;

    public SqlServerConnection(string connectionString, ILogger<SqlServerConnection> logger)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));

        _connectionString = connectionString;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connection = new System.Data.SqlClient.SqlConnection(_connectionString);
    }

    public async Task OpenAsync()
    {
        try
        {
            if (_connection?.State != ConnectionState.Open)
            {
                await _connection!.OpenAsync().ConfigureAwait(false);
                _logger.LogDebug("Database connection opened");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open database connection");
            throw new DataAccessException("Failed to open database connection", ex);
        }
    }

    public async Task CloseAsync()
    {
        try
        {
            if (_connection?.State == ConnectionState.Open)
            {
                await _connection.CloseAsync().ConfigureAwait(false);
                _logger.LogDebug("Database connection closed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close database connection");
        }
    }

    public DbCommand CreateCommand()
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection is not initialized");

        return _connection.CreateCommand();
    }

    public DbParameter CreateParameter()
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection is not initialized");

        return _connection.CreateParameter();
    }

    public void Dispose()
    {
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Exception for database operations
/// </summary>
public class DataAccessException : Exception
{
    public DataAccessException(string message) : base(message) { }
    public DataAccessException(string message, Exception innerException) : base(message, innerException) { }
}

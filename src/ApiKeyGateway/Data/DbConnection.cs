// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging; // Added for structured logging

namespace ApiKeyGateway.Data;

/// <summary>
/// Database connection abstraction for dependency injection
/// </summary>
public interface IDbConnection : IDisposable
{
    /// <summary>
    /// Opens the database connection if it is not already open.
    /// </summary>
    Task OpenAsync();

    /// <summary>
    /// Closes the database connection if it is open.
    /// </summary>
    Task CloseAsync();

    /// <summary>
    /// Creates a new command associated with this connection.
    /// </summary>
    /// <returns>A new <see cref="DbCommand"/>.</returns>
    DbCommand CreateCommand();

    /// <summary>
    /// Creates a new parameter for use with a command on this connection.
    /// </summary>
    /// <returns>A new <see cref="DbParameter"/>.</returns>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerConnection"/> class.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="logger">The logger.</param>
    public SqlServerConnection(string connectionString, ILogger<SqlServerConnection> logger)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));
        }

        _connectionString = connectionString;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connection = new System.Data.SqlClient.SqlConnection(_connectionString);
    }

    /// <summary>
    /// Opens the database connection if it is not already open.
    /// </summary>
    public async Task OpenAsync()
    {
        _logger.LogDebug("Opening database connection");
        try
        {
            if (_connection?.State != ConnectionState.Open)
            {
                await _connection!.OpenAsync();
                _logger.LogInformation("Database connection opened");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open database connection");
            throw new DbConnectionException("Failed to open database connection", ex);
        }
    }

    /// <summary>
    /// Closes the database connection if it is open.
    /// </summary>
    public async Task CloseAsync()
    {
        _logger.LogDebug("Closing database connection");
        try
        {
            if (_connection?.State == ConnectionState.Open)
            {
                await _connection.CloseAsync();
                _logger.LogInformation("Database connection closed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close database connection");
        }
    }

    /// <summary>
    /// Creates a new command associated with this connection.
    /// </summary>
    /// <returns>A new <see cref="DbCommand"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the underlying connection is not initialized.</exception>
    public DbCommand CreateCommand()
    {
        _logger.LogDebug("Creating DB command");
        if (_connection == null)
        {
            throw new InvalidOperationException("Connection is not initialized");
        }

        return _connection.CreateCommand();
    }

    /// <summary>
    /// Creates a new parameter for use with a command on this connection.
    /// </summary>
    /// <returns>A new <see cref="DbParameter"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the underlying connection is not initialized.</exception>
    public DbParameter CreateParameter()
    {
        _logger.LogDebug("Creating DB parameter");
        if (_connection == null)
        {
            throw new InvalidOperationException("Connection is not initialized");
        }

        return _connection.CreateCommand().CreateParameter();
    }

    /// <summary>
    /// Releases the underlying SQL connection.
    /// </summary>
    public void Dispose()
    {
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Exception for database operations
/// </summary>
public class DbConnectionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DbConnectionException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DbConnectionException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbConnectionException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    public DbConnectionException(string message, Exception innerException) : base(message, innerException) { }
}

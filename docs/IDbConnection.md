# IDbConnection

`IDbConnection` is a class that manages a connection to a SQL Server database within the api-key-gateway project. It provides asynchronous methods to open and close the connection, synchronous factory methods for creating commands and parameters, and a `Dispose` method for resource cleanup. The class also defines a nested exception type, `DbConnectionException`, for reporting connection‑specific errors.

## API

### SqlServerConnection
- **Type:** Property (read‑only)
- **Purpose:** Gets the underlying `SqlServerConnection` instance that this object wraps.
- **Returns:** The `SqlServerConnection` object used for the actual database communication.
- **Throws:** Nothing.

### OpenAsync
- **Signature:** `public async Task OpenAsync()`
- **Purpose:** Asynchronously opens the database connection. The connection must be in the closed state before calling this method.
- **Returns:** A `Task` representing the asynchronous open operation.
- **Throws:** `DbConnectionException` if the connection cannot be opened (e.g., network failure, invalid credentials).

### CloseAsync
- **Signature:** `public async Task CloseAsync()`
- **Purpose:** Asynchronously closes the database connection. Any pending transactions are rolled back.
- **Returns:** A `Task` representing the asynchronous close operation.
- **Throws:** `DbConnectionException` if an error occurs while closing the connection.

### CreateCommand
- **Signature:** `public DbCommand CreateCommand()`
- **Purpose:** Creates a new `DbCommand` object that is associated with this connection. The command is not executed; it must be configured with a command text and parameters.
- **Returns:** A new `DbCommand` instance.
- **Throws:** `DbConnectionException` if the connection is not in the open state.

### CreateParameter
- **Signature:** `public DbParameter CreateParameter()`
- **Purpose:** Creates a new `DbParameter` object that can be added to a `DbCommand`’s parameter collection.
- **Returns:** A new `DbParameter` instance.
- **Throws:** Nothing.

### Dispose
- **Signature:** `public void Dispose()`
- **Purpose:** Releases all managed and unmanaged resources used by the connection. After calling `Dispose`, the connection cannot be reopened.
- **Returns:** Nothing.
- **Throws:** Nothing.

### DbConnectionException (nested class)
- **Constructor:** `public DbConnectionException(string message) : base(message)`
  - **Purpose:** Initializes a new instance of `DbConnectionException` with a specified error message.
  - **Parameters:** `message` – a human‑readable description of the error.
- **Constructor:** `public DbConnectionException(string message, Exception innerException) : base(message, innerException)`
  - **Purpose:** Initializes a new instance of `DbConnectionException` with a specified error message and a reference to the inner exception that caused this exception.
  - **Parameters:** `message` – a human‑readable description of the error; `innerException` – the exception that is the cause of the current exception.

## Usage

### Example 1: Basic open, query, and close

```csharp
using var connection = new IDbConnection();

await connection.OpenAsync();

var command = connection.CreateCommand();
command.CommandText = "SELECT COUNT(*) FROM ApiKeys";
var count = (int)await command.ExecuteScalarAsync();

Console.WriteLine($"Total API keys: {count}");

await connection.CloseAsync();
```

### Example 2: Using `Dispose` and handling exceptions

```csharp
IDbConnection connection = null;
try
{
    connection = new IDbConnection();
    await connection.OpenAsync();

    var param = connection.CreateParameter();
    param.ParameterName = "@key";
    param.Value = "abc123";

    var cmd = connection.CreateCommand();
    cmd.CommandText = "SELECT * FROM ApiKeys WHERE Key = @key";
    cmd.Parameters.Add(param);

    using var reader = await cmd.ExecuteReaderAsync();
    // Process results...
}
catch (DbConnectionException ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
}
finally
{
    connection?.Dispose();
}
```

## Notes

- **Edge cases:**  
  - Calling `OpenAsync` on an already open connection throws `DbConnectionException`.  
  - Calling `CloseAsync` on an already closed connection is a no‑op (no exception).  
  - `CreateCommand` throws if the connection is not open; always ensure `OpenAsync` completes successfully before creating commands.  
  - `Dispose` can be called multiple times safely; subsequent calls have no effect.

- **Thread safety:**  
  `IDbConnection` is not thread‑safe. Concurrent calls to `OpenAsync`, `CloseAsync`, `CreateCommand`, or `Dispose` from multiple threads may result in undefined behavior or data corruption. Use external synchronization (e.g., a lock) if the same instance must be accessed from multiple threads.

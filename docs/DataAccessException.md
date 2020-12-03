# DataAccessException

The `DataAccessException` class represents a custom exception thrown when an error occurs during data access operations within the `api-key-gateway` service. It extends the standard exception hierarchy to provide specific context regarding the failed operation and the target entity, facilitating more precise error handling, logging, and debugging in scenarios involving database interactions or repository calls.

## API

### `Operation`
```csharp
public string? Operation { get; }
```
Gets the name or description of the data access operation that failed (e.g., "Insert", "Update", "FindByKey"). This property may be `null` if the operation was not specified during exception construction.

### `Entity`
```csharp
public string? Entity { get; }
```
Gets the name of the data entity or table involved in the failed operation (e.g., "ApiKeys", "Users"). This property may be `null` if the specific entity was not identified at the time of the exception.

### `DataAccessException(string message)`
```csharp
public DataAccessException(string message) : base(message)
```
Initializes a new instance of the `DataAccessException` class with a specified error message.

*   **Parameters**:
    *   `message`: A string describing the error condition.
*   **Return Value**: A new `DataAccessException` instance.
*   **Throws**: `ArgumentNullException` if `message` is `null`.
*   **Remarks**: In this overload, both `Operation` and `Entity` properties are initialized to `null`.

### `DataAccessException(string message, string operation)`
```csharp
public DataAccessException(string message, string operation) : base(message)
```
Initializes a new instance of the `DataAccessException` class with a specified error message and the name of the failed operation.

*   **Parameters**:
    *   `message`: A string describing the error condition.
    *   `operation`: A string identifying the specific data access operation that failed.
*   **Return Value**: A new `DataAccessException` instance.
*   **Throws**: `ArgumentNullException` if `message` or `operation` is `null`.
*   **Remarks**: In this overload, the `Operation` property is set to the provided value, while the `Entity` property remains `null`.

### `DataAccessException` (Additional Overloads)
The class includes additional constructors to support standard exception serialization and inner exception chaining, consistent with .NET exception patterns. These allow the exception to be wrapped around lower-level data access errors or serialized across application domains.

*   **Purpose**: To preserve stack traces and inner exception details when catching and rethrowing data access errors, or to support serialization frameworks.
*   **Parameters**: Varies by overload (typically includes `string message`, `Exception innerException`, or serialization `StreamingContext` data).
*   **Return Value**: A new `DataAccessException` instance.
*   **Throws**: Depends on specific parameter validation within the base `Exception` class.

## Usage

### Example 1: Throwing with Operation Context
This example demonstrates throwing the exception when a specific repository method fails, providing the operation name for clearer logging.

```csharp
public async Task<ApiKey> GetApiKeyAsync(string keyId)
{
    try
    {
        return await _repository.GetByIdAsync(keyId);
    }
    catch (SqlException ex)
    {
        // Wrap the low-level SQL exception with context about the operation
        throw new DataAccessException(
            $"Failed to retrieve API key with ID {keyId}", 
            "GetById"
        );
    }
}
```

### Example 2: Handling and Inspecting Properties
This example shows how a global error handler might inspect the `Operation` and `Entity` properties to determine the appropriate HTTP response code or log category.

```csharp
public IActionResult HandleException(Exception ex)
{
    if (ex is DataAccessException dataEx)
    {
        _logger.LogWarning(
            "Data access failed during {Operation} on {Entity}: {Message}",
            dataEx.Operation ?? "UnknownOperation",
            dataEx.Entity ?? "UnknownEntity",
            ex.Message
        );

        // Return a generic 500 error to avoid leaking internal details
        return StatusCode(500, "A database error occurred.");
    }

    // Fallback for other exceptions
    throw;
}
```

## Notes

*   **Nullability**: The `Operation` and `Entity` properties are nullable (`string?`). Consumers must perform null checks or use the null-coalescing operator (`??`) before accessing these values, particularly when the exception is instantiated using the single-argument constructor `DataAccessException(string message)`.
*   **Immutability**: Once instantiated, the `Operation` and `Entity` properties are read-only. Their values are determined strictly at construction time and cannot be modified afterward.
*   **Thread Safety**: As with standard .NET exceptions, `DataAccessException` is thread-safe for reading properties after construction. However, the exception object itself should not be mutated (though its design prevents this) or shared across threads for modification purposes. It is safe to throw and catch this exception across thread boundaries.
*   **Serialization**: When implementing custom constructors for serialization (as hinted by the additional overloads), ensure that the `Operation` and `Entity` values are preserved in the `GetObjectData` method and restored in the special deserialization constructor to maintain context across app domains or network boundaries.

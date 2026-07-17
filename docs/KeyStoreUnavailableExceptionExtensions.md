# KeyStoreUnavailableExceptionExtensions

The `KeyStoreUnavailableExceptionExtensions` class provides a set of static extension methods for the `KeyStoreUnavailableException` type. These methods enable fluent construction of exception instances with additional context (operation name, cache miss indicator, arbitrary context data), inspection of stored operations, transient-fault classification, and generation of a diagnostic string. The class is designed to simplify error handling and logging within the `api-key-gateway` project by centralizing common exception manipulation patterns.

## API

### `public static KeyStoreUnavailableException WithOperation(this KeyStoreUnavailableException exception, string operation)`

Creates a new `KeyStoreUnavailableException` that includes the specified operation name.

- **Parameters**  
  `exception` – The original exception to extend.  
  `operation` – The name of the operation that was unavailable (e.g., "GetKey", "ValidateToken").

- **Returns**  
  A new `KeyStoreUnavailableException` instance with the operation appended to its internal operation list.

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.  
  `ArgumentException` if `operation` is `null` or empty.

### `public static KeyStoreUnavailableException WithCacheMiss(this KeyStoreUnavailableException exception)`

Creates a new `KeyStoreUnavailableException` that marks the failure as a cache miss.

- **Parameters**  
  `exception` – The original exception to extend.

- **Returns**  
  A new `KeyStoreUnavailableException` instance with the cache-miss flag set.

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.

### `public static KeyStoreUnavailableException WithContext(this KeyStoreUnavailableException exception, string context)`

Creates a new `KeyStoreUnavailableException` that includes an arbitrary context string (e.g., a correlation ID or endpoint name).

- **Parameters**  
  `exception` – The original exception to extend.  
  `context` – A string describing the context in which the exception occurred.

- **Returns**  
  A new `KeyStoreUnavailableException` instance with the context stored.

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.  
  `ArgumentException` if `context` is `null` or empty.

### `public static IEnumerable<string> GetAllOperations(this KeyStoreUnavailableException exception)`

Returns all operation names that have been recorded on the exception via `WithOperation` calls.

- **Parameters**  
  `exception` – The exception from which to retrieve operations.

- **Returns**  
  An `IEnumerable<string>` containing the operation names in the order they were added. May be empty if no operations were recorded.

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.

### `public static bool IsLikelyTransient(this KeyStoreUnavailableException exception)`

Determines whether the exception is likely to represent a transient fault (e.g., a temporary network issue) rather than a permanent failure.

- **Parameters**  
  `exception` – The exception to evaluate.

- **Returns**  
  `true` if the exception is considered transient; otherwise `false`. The heuristic is based on the exception’s internal state (e.g., presence of a cache miss flag or certain operation names).

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.

### `public static string ToDiagnosticString(this KeyStoreUnavailableException exception)`

Produces a human-readable diagnostic string that summarizes the exception’s context, operations, and transient status.

- **Parameters**  
  `exception` – The exception to format.

- **Returns**  
  A formatted string suitable for logging or debugging.

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.

## Usage

### Example 1: Fluent construction and logging

```csharp
var ex = new KeyStoreUnavailableException("Key store is unreachable")
    .WithOperation("GetApiKey")
    .WithCacheMiss()
    .WithContext("RequestId=abc-123");

if (ex.IsLikelyTransient())
{
    logger.LogWarning("Transient key store failure: {Diagnostic}", ex.ToDiagnosticString());
    // Retry logic...
}
else
{
    logger.LogError("Permanent key store failure: {Diagnostic}", ex.ToDiagnosticString());
}
```

### Example 2: Inspecting recorded operations

```csharp
var ex = new KeyStoreUnavailableException("Timeout")
    .WithOperation("ValidateToken")
    .WithOperation("FetchPolicy");

IEnumerable<string> ops = ex.GetAllOperations();
Console.WriteLine($"Operations affected: {string.Join(", ", ops)}");
// Output: Operations affected: ValidateToken, FetchPolicy
```

## Notes

- All methods are static extension methods and operate on the provided `KeyStoreUnavailableException` instance. They do not modify the original exception; instead, they return a new instance with the additional data. This makes the class inherently thread-safe because no shared mutable state is altered.
- Passing a `null` exception argument to any method will throw an `ArgumentNullException`. String parameters (`operation`, `context`) must be non-null and non-empty; otherwise an `ArgumentException` is thrown.
- The `IsLikelyTransient` method uses internal heuristics that may change between versions. It should not be relied upon for security-critical decisions.
- `GetAllOperations` returns a snapshot of the operation list at the time of the call; subsequent calls to `WithOperation` on the same exception instance will not affect the returned enumerable.

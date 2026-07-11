# UnauthorizedAccessException

`UnauthorizedAccessException` is a custom exception type used in the `api-key-gateway` project to indicate that an API request was rejected due to insufficient permissions or invalid access credentials. It extends the base `System.Exception` class and includes additional context about the rejection reason and the source IP address of the request.

## API

### Constructors

#### `UnauthorizedAccessException(string message)`
Initializes a new instance of the `UnauthorizedAccessException` class with a specified error message.

- **Parameters**:
  - `message` (string): A message describing the exception.
- **Return value**: None.
- **Throws**: No exceptions are thrown by this constructor.

#### `UnauthorizedAccessException(string message, string reason)`
Initializes a new instance of the `UnauthorizedAccessException` class with a specified error message and a reason for the access denial.

- **Parameters**:
  - `message` (string): A message describing the exception.
  - `reason` (string): A string providing additional context about why access was denied.
- **Return value**: None.
- **Throws**: No exceptions are thrown by this constructor.

### Properties

#### `Reason`
Gets the reason why access was denied, if provided.

- **Type**: `string?`
- **Access**: Public get-only property.
- **Usage**: Use this property to retrieve the contextual reason for the access denial when handling the exception.

#### `SourceIp`
Gets the IP address of the source request that triggered the exception.

- **Type**: `string?`
- **Access**: Public get-only property.
- **Usage**: Use this property to log or audit the source of the unauthorized request.

## Usage

### Example 1: Basic Usage
```csharp
try
{
    await _apiKeyValidator.ValidateAsync(apiKey, requestContext);
}
catch (UnauthorizedAccessException ex)
{
    _logger.LogWarning(ex, "Access denied: {Reason}", ex.Reason);
    return Unauthorized(ex.Message);
}
```

### Example 2: Including Reason and Source IP
```csharp
if (!apiKey.IsValid)
{
    throw new UnauthorizedAccessException(
        "Invalid API key",
        "The provided API key does not match any active keys");
}

if (!requestContext.IsAllowedSourceIp)
{
    throw new UnauthorizedAccessException(
        "Access denied from this IP",
        "Source IP is not in the allowed list") { SourceIp = requestContext.ClientIp };
}
```

## Notes

- The `Reason` and `SourceIp` properties are nullable, so they may be `null` if not explicitly set during construction. Always check for `null` when using these properties.
- This exception is designed to be thrown synchronously and is not intended for asynchronous scenarios. Ensure proper exception handling in async contexts.
- Thread safety is guaranteed as the exception does not expose any mutable state. Instances of this exception are immutable after construction.

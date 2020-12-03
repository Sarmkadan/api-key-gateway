# ValidationException

`ValidationException` is a specialized exception type used to signal validation failures in the `api-key-gateway` project. It carries additional context about the validation error, including the parameter name, attempted value, and a collection of validation error messages, to aid in debugging and client feedback.

## API

### `ParameterName`
- **Purpose**: Identifies the name of the parameter that failed validation.
- **Type**: `string?`
- **Usage**: Useful when the validation failure is tied to a specific input parameter in an API request or method call.

### `AttemptedValue`
- **Purpose**: Contains the value that was provided and failed validation.
- **Type**: `object?`
- **Usage**: Helps diagnose why validation failed by showing the exact input that caused the error.

### `ValidationErrors`
- **Purpose**: Provides a collection of error messages describing why validation failed.
- **Type**: `IEnumerable<string>?`
- **Usage**: Useful when multiple validation rules were violated, allowing clients to receive detailed feedback.

### `ValidationException(string message)`
- **Purpose**: Constructs a `ValidationException` with a custom error message.
- **Parameters**:
  - `message` (`string`): The error message describing the validation failure.
- **Base Call**: Invokes the base `Exception` constructor with `message`.

### `ValidationException(string message, string parameterName, object? attemptedValue)`
- **Purpose**: Constructs a `ValidationException` with a message, parameter name, and attempted value.
- **Parameters**:
  - `message` (`string`): The error message describing the validation failure.
  - `parameterName` (`string`): The name of the parameter that failed validation.
  - `attemptedValue` (`object?`): The value that was provided and failed validation.
- **Base Call**: Invokes the base `Exception` constructor with `message`.

### `ValidationException(string message, IEnumerable<string> validationErrors)`
- **Purpose**: Constructs a `ValidationException` with a message and a collection of validation errors.
- **Parameters**:
  - `message` (`string`): The error message describing the validation failure.
  - `validationErrors` (`IEnumerable<string>`): A collection of error messages describing why validation failed.
- **Base Call**: Invokes the base `Exception` constructor with `message`.

### `ValidationException(string message, Exception innerException)`
- **Purpose**: Constructs a `ValidationException` with a message and an inner exception.
- **Parameters**:
  - `message` (`string`): The error message describing the validation failure.
  - `innerException` (`Exception`): The inner exception that caused this validation failure.
- **Base Call**: Invokes the base `Exception` constructor with `message`.

## Usage

### Example 1: Basic Validation Failure
```csharp
if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new ValidationException(
        "API key must not be null or whitespace.",
        nameof(apiKey),
        apiKey);
}
```

### Example 2: Multiple Validation Errors
```csharp
var errors = new List<string>
{
    "API key must be at least 32 characters long.",
    "API key must contain only alphanumeric characters."
};

if (!IsValidApiKeyFormat(apiKey))
{
    throw new ValidationException(
        "API key validation failed.",
        errors);
}
```

## Notes
- **Thread Safety**: This type is safe for concurrent use, as it does not expose mutable state and its members are read-only after construction.
- **Edge Cases**:
  - If `ParameterName` is `null` or empty, the exception still functions but lacks parameter context.
  - If `ValidationErrors` is an empty collection, the exception is still valid but provides no detailed error messages.
  - `AttemptedValue` may be `null`, which is acceptable when the validation failure is not tied to a specific value.

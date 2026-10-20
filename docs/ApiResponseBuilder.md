# ApiResponseBuilder

`ApiResponseBuilder<T>` is a generic, fluent builder for constructing standardized API response objects. It provides a consistent way to assemble success and error payloads, attach metadata key-value pairs, and accumulate multiple error messages before building the final response.

`ApiResponseBuilderFactory` is a companion static factory that offers shorthand methods for common HTTP status responses (404, 400, 401, 403, 429, 500).

## Namespace

`ApiKeyGateway.Utilities`

## ApiResponseBuilder\<T\>

### `WithData(T? data)`
Sets the response data payload.
- **Parameters:** `data` - the payload to include.
- **Returns:** The current builder instance for chaining.

### `Success(string? message = null)`
Marks the response as successful (status code 200). Sets the message to `"Success"` when none is provided.
- **Parameters:** `message` - optional success message.
- **Returns:** The current builder instance for chaining.

### `Error(int statusCode, string message, string? errorCode = null)`
Marks the response as an error with the given HTTP status code and message.
- **Parameters:**
  - `statusCode` - HTTP status code.
  - `message` - human-readable error description.
  - `errorCode` - optional machine-readable error code.
- **Returns:** The current builder instance for chaining.

### `WithMetadata(string key, object value)`
Adds a single key-value metadata entry to the response. Can be called multiple times to accumulate entries.
- **Parameters:**
  - `key` - metadata key (must not be null or empty).
  - `value` - metadata value (must not be null).
- **Returns:** The current builder instance for chaining.
- **Throws:** `ArgumentException` if `key` is null or empty; `ArgumentNullException` if `value` is null.

### `AddError(string error)`
Appends an error message string to the errors collection. Can be called multiple times.
- **Parameters:** `error` - error message (must not be null or empty).
- **Returns:** The current builder instance for chaining.
- **Throws:** `ArgumentException` if `error` is null or empty.

### `Build()`
Finalizes and returns the constructed response as an `ExpandoObject` with the following fields:
- `success` (bool)
- `statusCode` (int)
- `message` (string?)
- `errorCode` (string?)
- `data` (T?)
- `errors` (List\<string\>?)
- `metadata` (Dictionary\<string, object\>?)
- `timestamp` (DateTime, UTC)

- **Returns:** An `object` ready for JSON serialization.

## ApiResponseBuilderFactory

Static factory methods that produce pre-configured builders or pre-built response objects.

### `Success<T>(T data, string? message = null)`
Creates a builder pre-configured as a success response with the given data.
- **Returns:** `ApiResponseBuilder<T>` in success state.

### `Error<T>(int statusCode, string message, string? errorCode = null)`
Creates a builder pre-configured as an error response.
- **Returns:** `ApiResponseBuilder<T>` in error state.

### `NotFound(string resource = "Resource")`
Returns a pre-built 404 response with error code `NOT_FOUND`.
- **Parameters:** `resource` - name of the missing resource for the message.
- **Returns:** `object` ready for serialization.

### `BadRequest(string message, params string[] errors)`
Returns a pre-built 400 response with error code `BAD_REQUEST` and accumulated error messages.
- **Parameters:**
  - `message` - primary error description (must not be null or empty).
  - `errors` - additional error detail strings (must not be null).
- **Returns:** `object` ready for serialization.

### `Unauthorized(string message = "Unauthorized")`
Returns a pre-built 401 response with error code `UNAUTHORIZED`.

### `Forbidden(string message = "Access denied")`
Returns a pre-built 403 response with error code `FORBIDDEN`.

### `TooManyRequests(string message = "Rate limit exceeded")`
Returns a pre-built 429 response with error code `RATE_LIMIT_EXCEEDED`.

### `InternalServerError(string message = "An unexpected error occurred")`
Returns a pre-built 500 response with error code `INTERNAL_SERVER_ERROR`.

## Usage

### Building a successful response with metadata
```csharp
var response = new ApiResponseBuilder<OrderDto>()
    .WithData(order)
    .Success("Order retrieved")
    .WithMetadata("page", 1)
    .WithMetadata("totalPages", 5)
    .Build();

return Ok(response);
```

### Using the factory for a quick error response
```csharp
return BadRequest(ApiResponseBuilderFactory.BadRequest(
    "Validation failed",
    "Name is required",
    "Email format is invalid"));
```

### Building a custom error with the factory
```csharp
var response = ApiResponseBuilderFactory.Error<object?>(422, "Unprocessable entity", "VALIDATION_ERROR")
    .AddError("Field 'amount' must be positive")
    .AddError("Field 'currency' is required")
    .Build();
```

## Notes

- The static convenience methods on `ApiResponseBuilderFactory` (`NotFound`, `Unauthorized`, `Forbidden`, `TooManyRequests`, `InternalServerError`) return pre-built objects. They do not support further chaining.
- `BadRequest` is the exception among convenience methods: it accepts a `params string[]` of errors and builds internally.
- The builder is not thread-safe. Use it within a single execution context.
- The response object is an `ExpandoObject` cast to `object`; serialization produces the standard JSON envelope.

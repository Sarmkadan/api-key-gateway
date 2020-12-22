# ApiResponseBuilder

`ApiResponseBuilder` is a generic, fluent builder for constructing standardized API response objects within the `api-key-gateway` project. It provides a consistent way to assemble success and error payloads, attach metadata, and accumulate multiple errors before building the final response. Static convenience methods offer quick access to common HTTP status responses without requiring a generic type parameter.

## API

### `public ApiResponseBuilder<T> WithData(T data)`
Sets the data payload for a successful response.
- **Parameters:** `data` (type `T`) – the payload to include in the response.
- **Returns:** The current `ApiResponseBuilder<T>` instance for chaining.
- **Throws:** Nothing documented.

### `public ApiResponseBuilder<T> Success()`
Marks the response as successful. Typically called after setting data or metadata.
- **Parameters:** None.
- **Returns:** The current `ApiResponseBuilder<T>` instance for chaining.
- **Throws:** Nothing documented.

### `public ApiResponseBuilder<T> Error()`
Marks the response as an error. Call before or after adding specific error details with `AddError`.
- **Parameters:** None.
- **Returns:** The current `ApiResponseBuilder<T>` instance for chaining.
- **Throws:** Nothing documented.

### `public ApiResponseBuilder<T> WithMetadata(object metadata)`
Attaches arbitrary metadata to the response (e.g., pagination info, request IDs, timestamps).
- **Parameters:** `metadata` (type `object`) – the metadata object to attach.
- **Returns:** The current `ApiResponseBuilder<T>` instance for chaining.
- **Throws:** Nothing documented.

### `public ApiResponseBuilder<T> AddError(string code, string message)`
Appends an error entry to the response. Multiple calls accumulate errors. Implicitly marks the response as an error if not already set.
- **Parameters:**
  - `code` (type `string`) – a machine-readable error code.
  - `message` (type `string`) – a human-readable error description.
- **Returns:** The current `ApiResponseBuilder<T>` instance for chaining.
- **Throws:** Nothing documented.

### `public object Build()`
Finalizes and returns the constructed response object. The concrete type is an internal representation that serializes to the standard API envelope.
- **Parameters:** None.
- **Returns:** An `object` representing the complete API response.
- **Throws:** May throw if the builder state is inconsistent (e.g., both success and error flags set simultaneously without resolution).

### `public static ApiResponseBuilder<T> Success<T>(T data)`
Creates a new builder pre-configured as a success response with the given data.
- **Parameters:** `data` (type `T`) – the payload.
- **Returns:** A new `ApiResponseBuilder<T>` instance in success state with data set.
- **Throws:** Nothing documented.

### `public static ApiResponseBuilder<T> Error<T>(string code, string message)`
Creates a new builder pre-configured as an error response with a single error entry.
- **Parameters:**
  - `code` (type `string`) – error code.
  - `message` (type `string`) – error description.
- **Returns:** A new `ApiResponseBuilder<T>` instance in error state with one error.
- **Throws:** Nothing documented.

### `public static object NotFound()`
Returns a pre-built response representing HTTP 404 Not Found.
- **Parameters:** None.
- **Returns:** An `object` ready for serialization as a 404 response.
- **Throws:** Nothing documented.

### `public static object BadRequest()`
Returns a pre-built response representing HTTP 400 Bad Request.
- **Parameters:** None.
- **Returns:** An `object` ready for serialization as a 400 response.
- **Throws:** Nothing documented.

### `public static object Unauthorized()`
Returns a pre-built response representing HTTP 401 Unauthorized.
- **Parameters:** None.
- **Returns:** An `object` ready for serialization as a 401 response.
- **Throws:** Nothing documented.

### `public static object Forbidden()`
Returns a pre-built response representing HTTP 403 Forbidden.
- **Parameters:** None.
- **Returns:** An `object` ready for serialization as a 403 response.
- **Throws:** Nothing documented.

### `public static object TooManyRequests()`
Returns a pre-built response representing HTTP 429 Too Many Requests.
- **Parameters:** None.
- **Returns:** An `object` ready for serialization as a 429 response.
- **Throws:** Nothing documented.

### `public static object InternalServerError()`
Returns a pre-built response representing HTTP 500 Internal Server Error.
- **Parameters:** None.
- **Returns:** An `object` ready for serialization as a 500 response.
- **Throws:** Nothing documented.

## Usage

### Example 1: Building a successful response with data and metadata
```csharp
var response = ApiResponseBuilder<OrderDto>
    .Success(order)
    .WithMetadata(new { Page = 1, TotalPages = 5, RequestId = Guid.NewGuid() })
    .Build();

return Ok(response);
```

### Example 2: Accumulating multiple validation errors
```csharp
var builder = ApiResponseBuilder<object>.Error();

foreach (var failure in validationFailures)
{
    builder.AddError(failure.Code, failure.Message);
}

var response = builder
    .WithMetadata(new { Timestamp = DateTime.UtcNow })
    .Build();

return BadRequest(response);
```

## Notes

- The static convenience methods (`NotFound`, `BadRequest`, `Unauthorized`, `Forbidden`, `TooManyRequests`, `InternalServerError`) return pre-built objects that do not support further chaining. They are intended for immediate return from controller actions.
- Calling `Success()` after `Error()` (or vice versa) on the same builder instance produces undefined behavior; `Build()` may throw to prevent ambiguous responses.
- `AddError` can be called multiple times to accumulate errors. The first call implicitly sets the error state if neither `Success()` nor `Error()` has been called.
- The builder is not thread-safe. Instances should be used within a single execution context and not shared across threads without external synchronization.
- The concrete type returned by `Build()` is an internal implementation detail. Consumers should treat it as `object` and rely on the gateway's serialization layer to produce the correct JSON envelope structure.

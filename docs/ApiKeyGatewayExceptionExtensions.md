# ApiKeyGatewayExceptionExtensions

Static extension methods for `ApiKeyGatewayException` (`ApiKeyGateway.Domain.Exceptions`) that create copies of an exception with one property replaced and check its error code. The class lives in `src/ApiKeyGateway/Domain/Exceptions/ApiKeyGatewayExceptionExtensions.cs`.

The `with*` methods never modify the original exception. Each returns a new `ApiKeyGatewayException` with a fresh `OccurredAt` timestamp, so the copy's timestamp is the time of the call, not the time of the original error. The methods are declared on the base type, so they also accept derived exception types, but they always return a plain `ApiKeyGatewayException`.

Argument checks use `ArgumentNullException.ThrowIfNull` and `ArgumentException.ThrowIfNullOrEmpty`. A `null` argument throws `ArgumentNullException`; an empty string throws `ArgumentException`. Whitespace-only strings pass the check.

## API

### `WithMessage(this ApiKeyGatewayException exception, string message)` → `ApiKeyGatewayException`

Returns a new exception with `message` as its message. The error code is kept when the original has one.

The original exception is set as the **inner exception** of the new one. The original's own inner exception is not carried over, so the chain is one level longer than the source. This differs from `WithErrorCode` and `WithInnerException`, which keep the original inner exception.

- **Throws** `ArgumentNullException` if `exception` or `message` is `null`.
- **Throws** `ArgumentException` if `message` is an empty string.

```csharp
using ApiKeyGateway.Domain.Exceptions;

var original = new ApiKeyGatewayException("Quota exceeded", "QUOTA_EXCEEDED");

var renamed = original.WithMessage("Monthly quota exceeded for consumer-42");

// renamed.Message     == "Monthly quota exceeded for consumer-42"
// renamed.ErrorCode   == "QUOTA_EXCEEDED"
// renamed.InnerException == original
```

### `WithErrorCode(this ApiKeyGatewayException exception, string errorCode)` → `ApiKeyGatewayException`

Returns a new exception with `errorCode` as its error code. The message and the inner exception are copied from the original (the inner exception is `null` when the original has none).

- **Throws** `ArgumentNullException` if `exception` or `errorCode` is `null`.
- **Throws** `ArgumentException` if `errorCode` is an empty string.

```csharp
var original = new ApiKeyGatewayException("Key revoked");

var coded = original.WithErrorCode("KEY_REVOKED");

// coded.Message   == "Key revoked"
// coded.ErrorCode == "KEY_REVOKED"
```

### `WithInnerException(this ApiKeyGatewayException exception, Exception innerException)` → `ApiKeyGatewayException`

Returns a new exception with `innerException` as its inner exception. The message and the error code are copied from the original. Any inner exception the original had is replaced, not chained.

- **Throws** `ArgumentNullException` if `exception` or `innerException` is `null`.

```csharp
var original = new ApiKeyGatewayException("Usage store unavailable", "USAGE_STORE_DOWN");

try
{
    // ... database call ...
}
catch (Exception ex)
{
    throw original.WithInnerException(ex);
}
```

### `HasErrorCode(this ApiKeyGatewayException exception, string errorCode)` → bool

Returns `true` when `ErrorCode` equals `errorCode` using an ordinal, case-sensitive comparison. It returns `false` when the exception has no error code (`ErrorCode` is `null`).

- **Throws** `ArgumentNullException` if `exception` or `errorCode` is `null`.
- **Throws** `ArgumentException` if `errorCode` is an empty string.

```csharp
var ex = new ApiKeyGatewayException("Rate limit hit", "RATE_LIMITED");

ex.HasErrorCode("RATE_LIMITED"); // true
ex.HasErrorCode("rate_limited"); // false (case-sensitive)

var plain = new ApiKeyGatewayException("Something failed");
plain.HasErrorCode("RATE_LIMITED"); // false (no error code)
```

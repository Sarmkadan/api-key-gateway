# RequestContextHelper

Utility class providing helper methods to extract common request-scoped values from the current HTTP context in ASP.NET Core applications. Designed for use within the `api-key-gateway` project to centralize request context extraction logic and ensure consistent behavior across endpoints.

## API

### `ExtractApiKey(HttpContext context)`

Extracts the API key from the request using the configured header name. Returns `null` if the header is missing or empty.

- **Parameters**
  - `context` – The current `HttpContext` instance.
- **Returns**
  - `string?` – The API key value, or `null` if not present.
- **Exceptions**
  - Throws `ArgumentNullException` if `context` is `null`.

### `GetOrCreateCorrelationId(HttpContext context)`

Gets the correlation ID from the request headers (if provided) or generates a new one if missing. The generated ID is stored in the response headers for traceability.

- **Parameters**
  - `context` – The current `HttpContext` instance.
- **Returns**
  - `string` – A correlation ID, guaranteed non-empty.
- **Exceptions**
  - Throws `ArgumentNullException` if `context` is `null`.

### `ExtractPaginationParams(HttpContext context)`

Extracts pagination parameters (`pageNumber` and `pageSize`) from query string values. Defaults to `pageNumber = 1` and `pageSize = 20` if values are missing or invalid.

- **Parameters**
  - `context` – The current `HttpContext` instance.
- **Returns**
  - `(int pageNumber, int pageSize)` – A tuple containing the parsed pagination parameters.
- **Exceptions**
  - Throws `ArgumentNullException` if `context` is `null`.
  - Throws `FormatException` if `pageNumber` or `pageSize` cannot be parsed as integers.

### `GetClientIpAddress(HttpContext context)`

Extracts the client IP address from the request, considering forwarded headers (e.g., `X-Forwarded-For`) when behind a proxy.

- **Parameters**
  - `context` – The current `HttpContext` instance.
- **Returns**
  - `string` – The client IP address as a string.
- **Exceptions**
  - Throws `ArgumentNullException` if `context` is `null`.

### `GetRequestScope(HttpContext context)`

Retrieves the current request scope identifier from the request headers or context items. Useful for logging and auditing.

- **Parameters**
  - `context` – The current `HttpContext` instance.
- **Returns**
  - `string` – The request scope identifier, or an empty string if not set.
- **Exceptions**
  - Throws `ArgumentNullException` if `context` is `null`.

### `AcceptsJson(HttpContext context)`

Determines whether the client indicates preference for JSON responses via the `Accept` header.

- **Parameters**
  - `context` – The current `HttpContext` instance.
- **Returns**
  - `bool` – `true` if the `Accept` header includes `application/json`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `context` is `null`.

## Usage

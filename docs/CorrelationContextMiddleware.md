# CorrelationContextMiddleware

Middleware component that enriches the HTTP context with correlation identifiers and API key metadata for request tracing and auditing. It extracts values from headers or generates them when absent, making them available downstream via static accessors.

## API

### `CorrelationContextMiddleware`

Constructor that enables the middleware to participate in the ASP.NET Core pipeline. No parameters are required as the component relies on injected services and configuration.

### `async Task InvokeAsync(HttpContext context, RequestDelegate next)`

Invokes the middleware, enriching the context with correlation identifiers and API key metadata before passing control to the next middleware in the pipeline.

- **context** – The `HttpContext` for the current request.
- **next** – The `RequestDelegate` representing the next middleware in the pipeline.
- **Return value** – A `Task` that completes when the middleware has finished processing.
- **Exceptions** – Throws `ArgumentNullException` if `context` or `next` is `null`.

### `public static string GetCorrelationId(HttpContext context)`

Retrieves the correlation identifier for the given HTTP context.

- **context** – The `HttpContext` containing the correlation identifier.
- **Return value** – The correlation identifier string, or `null` if not present.
- **Exceptions** – Throws `ArgumentNullException` if `context` is `null`.

### `public static string GetApiKeyId(HttpContext context)`

Retrieves the API key identifier associated with the current request.

- **context** – The `HttpContext` containing the API key metadata.
- **Return value** – The API key identifier string, or `null` if not present.
- **Exceptions** – Throws `ArgumentNullException` if `context` is `null`.

### `public static string GetClientIp(HttpContext context)`

Retrieves the client IP address from the HTTP context.

- **context** – The `HttpContext` containing the client IP metadata.
- **Return value** – The client IP address string, or `null` if not present or determinable.
- **Exceptions** – Throws `ArgumentNullException` if `context` is `null`.

## Usage

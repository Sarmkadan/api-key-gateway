# ApiKeyAuthenticationMiddleware

`src/ApiKeyGateway/Middleware/ApiKeyAuthenticationMiddleware.cs` is the request pipeline step that authenticates API keys. For each request it extracts a key, authenticates it, enforces rate limits, route scopes and usage quotas, and then either passes the request to the next middleware or writes an RFC 7807 `application/problem+json` response.

It is registered in `Program.cs` through the `UseApiKeyAuthentication()` extension method:

```csharp
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowAll");
app.UseApiKeyAuthentication();
app.UseRequestTransformation();
app.MapControllers();
```

It runs after `ErrorHandlingMiddleware` and CORS, and before `RequestTransformationMiddleware`, which reads the identity this middleware writes to `HttpContext.Items`.

## Constructor

```csharp
public ApiKeyAuthenticationMiddleware(
    RequestDelegate next,
    ILogger<ApiKeyAuthenticationMiddleware> logger,
    Configuration.GatewayConfiguration? gatewayConfig = null)
```

| Parameter | Required | Purpose |
|-----------|----------|---------|
| `next` | yes | Next delegate in the pipeline. Throws `ArgumentNullException` if null. |
| `logger` | yes | Logs authentication events. Throws `ArgumentNullException` if null. |
| `gatewayConfig` | no | Bound from the `Gateway` configuration section. Only `FailOpenOnKeyStoreUnavailable` is read; see [Configuration](#configuration). |

`GatewayConfiguration` is registered as a singleton by `AddGatewayServices`, so it is injected in normal use. If it is not registered, the optional parameter is `null` and the fail-open flag defaults to `false`.

## InvokeAsync

```csharp
public async Task InvokeAsync(
    HttpContext context,
    IAuthenticationService authenticationService,
    IUsageTrackingService usageTrackingService,
    IRateLimitingService rateLimitingService,
    IUsageQuotaService usageQuotaService,
    IQuotaAlertEvaluator quotaAlertEvaluator)
```

The services are method-injected, not constructor-injected. Conventional middleware is built once from the root provider, but these services are scoped. Resolving them per request gives each request its own scope and repository or connection.

## Request flow

For each request, `InvokeAsync` does the following:

1. **Extract the key** from the first source that has a non-blank value (see [Key sources](#key-sources)). Remember which source was used for logging.
2. **No key supplied:** skip authentication and call `_next(context)`. The middleware does not reject anonymous requests. Protection of individual endpoints is up to the endpoint (for example, `[Authorize]` on `AdminController` and `StatsController`).
3. **Key supplied:** resolve the client IP (see [Client IP](#client-ip)) and call `IAuthenticationService.AuthenticateAsync(apiKey, clientIp)`.
4. **Authentication succeeded:**
   1. Call `IRateLimitingService.CheckLimitAsync(keyId)`. It throws `RateLimitExceededException` when the limit is exceeded.
   2. Read the current limit with `GetLimitAsync(keyId)`. If the limit is enabled and not `Unlimited`, add the `X-RateLimit-*` headers.
   3. Check the route scope with `ApiKey.IsScopeAllowed(path)`. On failure, return `403` (route scope restricted).
   4. Call `IUsageQuotaService.CheckAndRecordAsync(keyId)`. If the quota limit is not `long.MaxValue`, add the `X-RateLimit-Quota-*` headers. If the quota is exceeded, return `429` (quota exceeded).
   5. Record the metric `RequestsAllowed`, and set `context.Items["ApiKey"]` and `context.Items["ConsumerId"]`.
5. **Authentication failed:** map the failure reason to a response (see [Failure responses](#failure-responses)). The request ends here.
6. **Call `_next(context)`** for requests that passed the checks.
7. **After the downstream pipeline returns**, `RecordUsageAsync` writes a `UsageRecord` through `IUsageTrackingService` and calls `IRateLimitingService.RecordRequestAsync`. Then `EvaluateQuotaAlertsAsync` runs quota alerting for the consumer. Both steps only run when the key was authenticated. They are best-effort: failures are logged and do not change the response.

Usage recording happens only if the downstream pipeline returns normally. If a downstream component throws, the exception goes to the `catch` blocks below and no usage record is written.

## Key sources

`TryExtractApiKey` checks these sources in order. The first non-blank value wins.

| Order | Source | Format | `source` value in logs |
|-------|--------|--------|------------------------|
| 1 | `X-API-Key` header | raw key | `X-API-Key` |
| 2 | `Authorization` header | `Bearer <key>` (prefix matched case-insensitively, token trimmed) | `Authorization Bearer` |
| 3 | `api_key` query string parameter | raw key | `api_key` |

An `Authorization` header that is not a `Bearer` scheme (for example `Basic`) is ignored, and extraction continues to the query string.

Keys in the query string appear in server and proxy access logs and in browser history. Prefer the header sources.

## Client IP

`ExtractClientIp` is used for IP whitelisting during authentication and for the `SourceIp` field of usage records:

1. If an `X-Forwarded-For` header is present, the first comma-separated entry, trimmed, is used.
2. Otherwise, `Connection.RemoteIpAddress` is used.
3. If neither is available, the value is `"unknown"`.

The middleware trusts `X-Forwarded-For` unconditionally and does not check that the request came through a trusted proxy. A client connecting directly can set this header to any address, which defeats IP whitelisting. Only expose the gateway behind a proxy that overwrites this header.

## Failure responses

When `AuthenticateAsync` returns a failure, the middleware maps `AuthenticationResult.FailureReason` as follows. Every response is written with `GatewayProblemDetailsFactory` and `WriteProblemAsync`.

| Failure reason | HTTP status | Factory method (problem title) | Notes |
|----------------|-------------|--------------------------------|-------|
| `InvalidApiKeyFormat` | 401 | `CreateInvalidKeyProblem` ("Invalid API Key") | Also returned for keys that are not found. |
| `ApiKeyNotFound` | 401 | `CreateInvalidKeyProblem` ("Invalid API Key") | |
| `MissingApiKey` | 401 | `CreateInvalidKeyProblem` ("Invalid API Key") | Not reached in practice, because blank keys are never extracted. |
| `ApiKeyExpired` | 401 | `CreateKeyExpiredProblem` ("API Key Expired") | |
| `ApiKeyDisabled` | 401 | `CreateKeyDisabledProblem` ("API Key Disabled") | |
| `IpNotWhitelisted` | 401 | `CreateIpNotWhitelistedProblem` ("IP Address Not Whitelisted") | |
| `ServiceUnavailable` | 503 | `CreateServiceUnavailableProblem` | The key store could not be reached, or an unexpected error occurred during authentication. Not counted as unauthorized. |

Route scope and quota failures are returned from the authenticated path:

| Condition | HTTP status | Factory method |
|-----------|-------------|----------------|
| Path not in `ApiKey.AllowedScopes` | 403 | `CreateRouteScopeRestrictedProblem` ("Route Scope Restricted") |
| Usage quota exceeded | 429 | `CreateQuotaExceededProblem` ("Usage Quota Exceeded"), includes the period end |

Failures that escape the authenticated path are caught by the middleware's own `try`/`catch`:

| Exception | HTTP status | Behavior |
|-----------|-------------|----------|
| `RateLimitExceededException` | 429 | `RequestsRateLimited` metric. The problem includes `RetryAfter` (or the window length if it is null), the limit, and the window. `Retry-After` is set from that value. |
| `KeyStoreUnavailableException` | 503 | Raised by the rate limiter when the rate-limit store is down and the policy is `FailClosed`. |
| Any other `Exception` | 500 | Logged as `Unexpected error in authentication middleware`. |

Exceptions thrown by downstream middleware and controllers are not caught here. They propagate up to `ErrorHandlingMiddleware`, which `Program.cs` registers ahead of this middleware.

## Response headers

Successful requests with a rate limit that is enabled and not `Unlimited` get:

| Header | Value |
|--------|-------|
| `X-RateLimit-Limit` | `RequestsPerUnit` |
| `X-RateLimit-Remaining` | `RemainingRequests` |
| `X-RateLimit-Reset` | Unix time of `(LastResetAt ?? CreatedAt) + window length` |

When the usage quota is limited (not `long.MaxValue`):

| Header | Value |
|--------|-------|
| `X-RateLimit-Quota-Limit` | Quota limit |
| `X-RateLimit-Quota-Remaining` | Remaining quota |
| `X-RateLimit-Quota-Reset` | Unix time of the quota period end |

The rate-limit headers are added before the scope and quota checks run, so a `403` or quota-`429` response can also carry them.

## Metrics

Metrics are defined in `Diagnostics/GatewayMetrics.cs`:

| Metric | Recorded when |
|--------|---------------|
| `RequestsAllowed` | An authenticated request passes the rate-limit, scope and quota checks. |
| `RequestsRateLimited` | `RateLimitExceededException` is caught. |
| `RecordUnauthorized(reason)` | Authentication fails for any reason other than `ServiceUnavailable`. |

## Configuration

The middleware reads one setting directly. Rate-limit behavior is configured elsewhere but affects this middleware's responses.

### `Gateway` section

Bound to `GatewayConfiguration` (`Configuration/ServiceCollectionExtensions.cs`):

| Key | Type | Default | Effect on this middleware |
|-----|------|---------|---------------------------|
| `Gateway:FailOpenOnKeyStoreUnavailable` | `bool` | `false` | Copied into a private field in the constructor. **The field is never read, so the setting currently has no effect.** Authentication store outages always return `503`. |

### `RateLimiting` section

Bound to `RateLimitingOptions` (`RateLimitingOptions.SectionName = "RateLimiting"`). The middleware does not read these values. `RateLimitingService` applies them, and they determine whether `CheckLimitAsync` throws:

| Key | Type | Default | Effect |
|-----|------|---------|--------|
| `RateLimiting:FailurePolicy` | `FailOpen` / `FailClosed` | `FailClosed` | When the rate-limit store fails, `FailClosed` throws `KeyStoreUnavailableException`, which becomes a `503`. `FailOpen` allows the request and publishes a warning event. |

Example:

```json
{
  "Gateway": {
    "RequireSsl": true,
    "LogAllRequests": true
  },
  "RateLimiting": {
    "FailurePolicy": "FailClosed"
  }
}
```

## Logging

| Level | Message | When |
|-------|---------|------|
| Debug | `API key supplied via {ApiKeySource}` | A key was extracted. The key value is never logged. |
| Warning | `API key {ApiKeyId} does not have scope permission for path {Path}` | Scope check failed. |
| Warning | `Usage quota exceeded for API key {ApiKeyId}` | Quota exceeded. |
| Warning | `Rate limit exceeded for key {ApiKeyId}` | Rate limit exceeded. |
| Warning | `Rate limit store unavailable; rejecting request with 503` | `KeyStoreUnavailableException` caught. |
| Error | `Unexpected error in authentication middleware` | Any other exception. |
| Error | `Failed to record usage` | Usage recording failed (best-effort). |
| Error | `Quota alert evaluation failed` | Alert evaluation failed (best-effort). |

Authentication attempts are logged separately by `AuthenticationService`.

## Security notes

- **Anonymous requests pass through.** Without a key, the request continues to the next middleware. Anything that needs authentication must enforce it at the endpoint.
- **`X-Forwarded-For` is trusted without validation.** It is used for IP whitelisting. See [Client IP](#client-ip).
- **Query-string keys are logged by intermediaries.** Use the header sources in production.
- **`FailOpenOnKeyStoreUnavailable` is not implemented.** Setting it to `true` does not allow requests through when the key store is down. If fail-open behavior is needed, it must be implemented in this middleware first.

## Related types

- `Services/AuthenticationService.cs`: `AuthenticateAsync` and the mapping from exceptions to `AuthenticationFailureReason`.
- `Services/RateLimitingService.cs`: `CheckLimitAsync`, `GetLimitAsync`, `RecordRequestAsync`, and the failure policy.
- `Services/UsageQuotaService.cs`: `CheckAndRecordAsync` and the quota result.
- `Middleware/GatewayProblemDetailsFactory.cs`: builds the RFC 7807 problem details.
- `Middleware/ErrorHandlingMiddleware.cs`: the outer error boundary.
- `Middleware/RequestTransformationMiddleware.cs`: reads `HttpContext.Items["ApiKey"]` and `["ConsumerId"]`.
- `Diagnostics/GatewayMetrics.cs`: metric definitions.

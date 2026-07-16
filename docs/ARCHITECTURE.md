# API Key Gateway - Architecture

This document describes the system as it exists in `src/ApiKeyGateway`. Every
component named here is present in the code; where the design has gaps or
sharp edges, they are called out explicitly in
[Known limitations](#known-limitations).

## Overview

API Key Gateway is a single ASP.NET Core application (net10.0, see
`global.json` / `Directory.Build.props`) that sits in front of self-hosted
services and provides:

- API key issuance, validation, rotation and revocation
- Per-key rate limiting and usage quotas (daily/monthly hard caps)
- Usage tracking, analytics and data export (CSV/XML helpers)
- Audit logging with retention-based cleanup
- Optional request transformation via a Lua scripting pipeline (MoonSharp)
- Webhook/event publishing for key lifecycle events

Persistence is plain ADO.NET against SQL Server through a thin custom
abstraction (`Data/DbConnection.cs`: `IDbConnection` /
`SqlServerConnection`) - there is no EF Core or Dapper. Caching is in-memory
only (`Caching/CacheProvider.cs`).

## Project layout

```
src/ApiKeyGateway/
  Program.cs                  Minimal hosting: config, DI, pipeline
  Configuration/              DI registration + bound options
    ServiceRegistrationExtensions.cs   AddGatewayServices (composition root)
    ServiceCollectionExtensions.cs     AddGatewayCoreServices, GatewayConfiguration
    CachingConfiguration.cs            AddGatewayCaching (IMemoryCache, 100 MB cap)
    MiddlewareConfiguration.cs         AddGatewayMiddleware / UseGatewayMiddleware
    RequestCoalescingExtensions.cs     Singleton request coalescing
    TransformationOptions.cs           AddRequestTransformationPipeline
    EventConfiguration.cs              Event publishing registration
  Controllers/                ApiKeys, Usage, Analytics, Stats, Admin, Health
  Middleware/                 ErrorHandling, ApiKeyAuthentication,
                              RequestTransformation, CorrelationContext,
                              RequestLogging, RequestValidation,
                              PerformanceMonitoring
  Services/                   ApiKeyService, AuthenticationService,
                              RateLimitingService, UsageTracking/Analytics,
                              UsageQuotaService, AuditLogService,
                              ApiKeyRotationService, DataExportService,
                              MetricsCollectionService, RequestCoalescingService
  Repositories/               One repository per aggregate (ADO.NET SQL)
  Domain/                     Models, Enums, Exceptions, Constants
  Caching/                    ICacheProvider, InMemoryCacheProvider,
                              CacheKeyGenerator
  BackgroundWorkers/          4 hosted services (see below)
  Transformation/             ITransformationPipeline, LuaScriptExecutor
  Events/                     EventPublisher, handlers, event models
  Integration/                WebhookManager/Handler, ExternalApiClient,
                              BatchOperationHandler
  Utilities/                  CryptoHelpers, RetryPolicyBuilder,
                              CircuitBreakerPattern, export helpers, etc.
tests/api-key-gateway.Tests/  xUnit unit + integration tests
benchmarks/                   BenchmarkDotNet benchmarks
```

Many types have sibling `*Validation.cs` / `*Extensions.cs` /
`*JsonExtensions.cs` files: generated-style companions providing argument
validation, convenience extensions and System.Text.Json round-tripping for
the primary type in the same file group.

## Startup and dependency injection

`Program.cs` is deliberately small. All registration is funneled through one
composition root, `ServiceRegistrationExtensions.AddGatewayServices`, which
in turn calls the focused registrars:

1. `AddGatewayCoreServices` - scoped `IDbConnection` (one SQL connection
   scope per request), all repositories and domain services (scoped), and a
   singleton `GatewayConfiguration` bound from the `Gateway` section of
   `appsettings.json` (startup throws if the section is missing).
2. `AddGatewayCaching` - `IMemoryCache` with a 100 MB size limit and a
   singleton `ICacheProvider` (`InMemoryCacheProvider`). Redis is sketched in
   comments but not implemented.
3. `AddEventPublishing`, webhook/batch/export registrations.
4. `IMetricsCollectionService` as a singleton - it only holds
   `ConcurrentDictionary` counters, so the singleton lifetime is safe (no
   scoped dependencies are captured).
5. Four `IHostedService` background workers.
6. `AddRequestTransformationPipeline` for the Lua transformation pipeline.

Key lifetime decisions and why:

- **Repositories/services are scoped**, matching the scoped `IDbConnection`,
  so each HTTP request gets its own connection scope.
- **`IRequestCoalescingService` is a singleton** by necessity: it keeps a
  shared dictionary of in-flight tasks so concurrent identical lookups are
  deduplicated; a scoped registration would defeat coalescing entirely
  (documented in `RequestCoalescingExtensions`).
- **`ApiKeyAuthenticationMiddleware` uses method injection** in
  `InvokeAsync` for `IAuthenticationService`, `IRateLimitingService`,
  `IUsageTrackingService` and `IUsageQuotaService`. Conventional middleware
  is constructed once from the root provider; constructor-injecting these
  scoped services would create captive dependencies. Method injection
  resolves them per request from the request scope.

## Request pipeline (as actually wired in Program.cs)

```
Client
  -> ErrorHandlingMiddleware      (converts exceptions to structured JSON)
  -> HTTPS redirection            (only when Gateway:RequireSsl = true)
  -> CORS ("AllowAll" policy)
  -> ApiKeyAuthenticationMiddleware
  -> RequestTransformationMiddleware (Lua pipeline, when rules match)
  -> Controllers / MapHealthChecks("/health")
```

Note: `MiddlewareConfiguration.UseGatewayMiddleware` defines a richer
pipeline (request logging, request validation, performance monitoring), but
`Program.cs` does not call it - those middleware classes exist and are
tested, yet are opt-in. If you enable it, be aware it calls
`MapControllers()` itself.

### Authentication flow

`ApiKeyAuthenticationMiddleware`:

1. Extracts the key from the `X-API-Key` header or the `api_key` query
   parameter.
2. `AuthenticationService.AuthenticateAsync(key, clientIp)` validates the
   hash, status, expiry and per-key IP whitelist.
3. `RateLimitingService.CheckLimitAsync` enforces the key's rate limit
   (throws `RateLimitExceededException` -> 429 with `Retry-After`).
4. Route-scope check (`ApiKey.IsScopeAllowed(path)`) -> 403 on mismatch.
5. `UsageQuotaService.CheckAndRecordAsync` enforces period quotas and emits
   `X-RateLimit-Quota-*` headers -> 429 when exceeded.
6. On success the authenticated key and consumer id are stored in
   `HttpContext.Items` for controllers; usage is recorded after the
   downstream pipeline completes.

Failure behavior when the key store is down is configurable:
`Gateway:FailOpenOnKeyStoreUnavailable` (default `false` = fail-closed,
503).

**Important:** requests *without* any API key are not rejected by the
middleware - they pass through unauthenticated and it is up to individual
controllers to check `HttpContext.Items`. See Known limitations.

## Background workers

All four are standard `IHostedService` registrations:

| Worker | Purpose |
|---|---|
| `UsageAggregationWorker` | Periodically rolls raw usage events into aggregates |
| `RateLimitResetScheduler` | Resets expired rate-limit windows |
| `AuditLogCleanupWorker` | Purges audit logs older than `Gateway:AuditLogRetentionDays` |
| `KeyRotationScheduler` | Drives scheduled key rotation via `ApiKeyRotationService` |

Because hosted services live outside the request scope, they create their own
DI scopes to consume scoped repositories.

## Configuration

`GatewayConfiguration` (in `Configuration/ServiceCollectionExtensions.cs`)
is bound once from the `Gateway` section and registered as a singleton.
Notable knobs: `RequireSsl`, `MinKeyLength`/`MaxKeyLength`,
`DefaultKeyExpirationDays`, `AuditLogRetentionDays`, `EnableRateLimiting`,
`DefaultRateLimitPerHour`, `ClockSkewToleranceSeconds` (rate-limit window
tolerance fed into `RateLimitingService`), and
`FailOpenOnKeyStoreUnavailable`.

There is a *second*, unrelated `GatewayConfiguration` class in
`Domain/Models/GatewayConfiguration.cs`. It is not the type registered in
DI; anything resolving `GatewayConfiguration` gets the
`ApiKeyGateway.Configuration` one. Prefer the Configuration-namespace type;
the Domain.Models copy is a candidate for consolidation.

## Extension points

- **Cache backend**: implement `ICacheProvider` and swap the singleton
  registration in `CachingConfiguration` (Redis is the intended second
  implementation).
- **Transformation rules**: `ITransformationPipeline` +
  `DatabaseTransformationRuleRepository`; rules can carry Lua scripts run by
  `LuaScriptExecutor` (MoonSharp, sandboxed interpreter).
- **Events/webhooks**: implement handlers registered via
  `EventConfiguration` / `IWebhookHandler`.
- **Resilience**: `RetryPolicyBuilder` and `CircuitBreakerPattern` in
  `Utilities/` are self-contained and reusable by new outbound integrations.

## Known limitations

- **No default deny**: as noted above, keyless requests flow through to
  controllers; the gateway is not a hard perimeter unless every endpoint
  validates `HttpContext.Items` or `UseGatewayMiddleware`-style enforcement
  is added.
- **CORS is `AllowAll`** (any origin/method/header) and unconditional.
  Acceptable for a pure machine-to-machine gateway, wrong if browsers ever
  call it directly with credentials.
- **Single-node state**: rate limiting counters, quotas cache, request
  coalescing and `IMemoryCache` are all per-process. Running multiple
  instances behind a load balancer weakens rate-limit and quota accuracy
  until a distributed cache is implemented.
- **SQL Server only**: `SqlServerConnection` is the sole `IDbConnection`
  implementation and the connection string is required at startup.
- Docs in `docs/*.md` (per-class pages) were written at different times;
  when they disagree with the code, the code wins. `docs/architecture.md`
  is the older narrative version of this document.

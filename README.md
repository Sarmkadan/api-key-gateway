# ApiKeyGateway

A self-hosted ASP.NET Core API key authentication gateway with rate limiting, usage quotas, audit logging, and request transformation. Backed by SQL Server via raw ADO.NET. Also packaged as NuGet `Zaiets.api.key.gateway`.

## Getting Started

### Program.cs

```csharp
using ApiKeyGateway;
using ApiKeyGateway.Configuration;
using ApiKeyGateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add ApiKeyGateway services
builder.Services.AddGatewayCoreServices(builder.Configuration);
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add ApiKeyGateway middleware in the correct pipeline order
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### appsettings.json

```json
{
  "Gateway": {
    "ApiKeyHeaderName": "X-Api-Key", // Header name for API key
    "AdminCredentials": {
      "Username": "admin", // Admin username for management endpoints
      "Password": "securepassword" // Admin password for management endpoints
    },
    "RateLimiting": {
      "Enabled": true, // Enable rate limiting
      "DefaultLimit": 100, // Default requests per minute
      "WindowInSeconds": 60 // Rate limit window in seconds
    },
    "UsageQuotas": {
      "Enabled": true, // Enable usage quotas
      "DefaultQuota": 1000, // Default quota limit
      "Period": "Monthly" // Quota period (Daily, Weekly, Monthly)
    },
    "KeyRotation": {
      "GracePeriodDays": 7, // Days to allow old keys to work after rotation
      "AutoRotate": true // Automatically rotate keys when they expire
    },
    "AuditLogging": {
      "Enabled": true, // Enable audit logging
      "RetentionDays": 30 // Days to retain audit logs
    }
  }
}
```

### cURL Examples

#### Successful Request

```bash
curl -X GET "https://api.example.com/endpoint" \
  -H "X-Api-Key: sk_live_abc123xyz789" \
  -H "X-Correlation-ID: abc123"
```

#### 401 Unauthorized

```bash
curl -X GET "https://api.example.com/endpoint" \
  -H "X-Api-Key: invalid_key" \
  -H "X-Correlation-ID: def456"

# Response headers:
# X-Correlation-ID: def456
# X-RateLimit-Limit: 100
# X-RateLimit-Remaining: 99
# X-RateLimit-Reset: 60
```

#### 429 Too Many Requests

```bash
curl -X GET "https://api.example.com/endpoint" \
  -H "X-Api-Key: sk_live_abc123xyz789" \
  -H "X-Correlation-ID: ghi789"

# Response headers:
# X-Correlation-ID: ghi789
# X-RateLimit-Limit: 100
# X-RateLimit-Remaining: 0
# X-RateLimit-Reset: 60
# Retry-After: 60
```

## Managing Keys

The AdminController provides endpoints for managing API keys, monitoring usage, and configuring the gateway. All administrative endpoints require authentication.

### Authentication

Administrative endpoints use standard ASP.NET Core authentication. Include the Authorization header with a valid token:

```bash
curl -X GET "https://gateway.example.com/api/admin/stats" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Or using basic credentials (if configured):

```bash
curl -X GET "https://gateway.example.com/api/admin/stats" \
  -u "admin:securepassword"
```

### Key Management Examples

#### Create a New API Key

```bash
curl -X POST "https://gateway.example.com/api/apikeys" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -d '{
    "consumerId": "customer123",
    "name": "Production API Key",
    "expirationDays": 365
  }'
```

Response:
```json
{
  "keyId": "ak_live_1a2b3c4d5e6f7g8h9i0j",
  "consumerId": "customer123",
  "name": "Production API Key",
  "expiresAt": "2027-10-03T14:30:00Z",
  "createdAt": "2026-10-03T14:30:00Z"
}
```

#### List API Keys for a Consumer

```bash
curl -X GET "https://gateway.example.com/api/apikeys/consumer/customer123" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
[
  {
    "keyId": "ak_live_1a2b3c4d5e6f7g8h9i0j",
    "consumerId": "customer123",
    "name": "Production API Key",
    "status": "Active",
    "createdAt": "2026-10-03T14:30:00Z",
    "expiresAt": "2027-10-03T14:30:00Z",
    "lastUsedAt": "2026-10-03T10:15:00Z",
    "requestCount": 12450,
    "isActive": true
  }
]
```

#### Get API Key Details

```bash
curl -X GET "https://gateway.example.com/api/apikeys/ak_live_1a2b3c4d5e6f7g8h9i0j" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "keyId": "ak_live_1a2b3c4d5e6f7g8h9i0j",
  "consumerId": "customer123",
  "name": "Production API Key",
  "status": "Active",
  "createdAt": "2026-10-03T14:30:00Z",
  "expiresAt": "2027-10-03T14:30:00Z",
  "lastUsedAt": "2026-10-03T10:15:00Z",
  "requestCount": 12450,
  "isActive": true
}
```

#### Disable an API Key

```bash
curl -X PUT "https://gateway.example.com/api/apikeys/ak_live_1a2b3c4d5e6f7g8h9i0j/disable" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "message": "API key disabled successfully"
}
```

#### Enable an API Key

```bash
curl -X PUT "https://gateway.example.com/api/apikeys/ak_live_1a2b3c4d5e6f7g8h9i0j/enable" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "message": "API key enabled successfully"
}
```

#### Revoke an API Key (Permanent)

```bash
curl -X PUT "https://gateway.example.com/api/apikeys/ak_live_1a2b3c4d5e6f7g8h9i0j/revoke" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "message": "API key revoked successfully"
}
```

#### Rotate an API Key

```bash
curl -X POST "https://gateway.example.com/api/apikeys/ak_live_1a2b3c4d5e6f7g8h9i0j/rotate" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -d '{
    "newExpirationDays": 365
  }'
```

Response:
```json
{
  "oldKeyId": "ak_live_1a2b3c4d5e6f7g8h9i0j",
  "newKeyId": "ak_live_2b3c4d5e6f7g8h9i0j1k",
  "consumerId": "customer123",
  "newKeyExpiresAt": "2027-10-03T14:30:00Z"
}
```

### Administrative Endpoints

#### Get System Statistics

```bash
curl -X GET "https://gateway.example.com/api/admin/stats" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "totalApiKeys": 150,
  "activeApiKeys": 135,
  "disabledApiKeys": 10,
  "totalRequests": 1250000,
  "requestsToday": 8450,
  "rateLimitEvents": 25,
  "rateLimitEventsToday": 3,
  "averageResponseTimeMs": 45.2,
  "errorRate": 0.02,
  "uptime": "45.00:00:00"
}
```

#### Export Usage Data

```bash
curl -X GET "https://gateway.example.com/api/admin/export/usage?format=csv&startDate=2026-09-01&endDate=2026-09-30" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -o usage_september_2026.csv
```

For XML format:
```bash
curl -X GET "https://gateway.example.com/api/admin/export/usage?format=xml" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -o usage.xml
```

#### Get Gateway Configuration

```bash
curl -X GET "https://gateway.example.com/api/admin/config" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "maxApiKeys": 1000,
  "maxRequestsPerHour": 10000,
  "auditLogRetentionDays": 90,
  "webhookDeliveryTimeout": 30,
  "webhookMaxRetries": 3,
  "cacheEnabled": true,
  "cacheTtlSeconds": 3600
}
```

#### Run System Diagnostics

```bash
curl -X POST "https://gateway.example.com/api/admin/diagnose" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "timestamp": "2026-10-03T14:30:00Z",
  "tests": {
    "database": { "status": "ok", "latencyMs": 12 },
    "cache": { "status": "ok", "latencyMs": 3 },
    "externalApi": { "status": "ok", "latencyMs": 156 },
    "diskSpace": { "status": "ok", "availableMb": 4560 },
    "memory": { "status": "ok", "usagePercent": 67.5 }
  },
  "overallStatus": "healthy"
}
```

#### Reset Rate Limits (Emergency Operation)

```bash
curl -X POST "https://gateway.example.com/api/admin/reset-limits" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "message": "Rate limits have been reset for all API keys"
}
```

#### Search Audit Logs

```bash
curl -X GET "https://gateway.example.com/api/admin/audit/search?action=KeyCreated&fromUtc=2026-10-01T00:00:00Z&toUtc=2026-10-03T23:59:59Z&limit=50" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
[
  {
    "id": "audit_1a2b3c4d5e6f7g8h9i0j",
    "resourceId": "ak_live_1a2b3c4d5e6f7g8h9i0j",
    "resourceType": "ApiKeysController",
    "action": "KeyCreated",
    "performedBy": "admin",
    "performedAt": "2026-10-03T14:30:00Z",
    "isSuccess": true,
    "changes": [
      {
        "fieldName": "consumerId",
        "oldValue": null,
        "newValue": "customer123"
      },
      {
        "fieldName": "name",
        "oldValue": null,
        "newValue": "Production API Key"
      }
    ]
  }
]
```

#### Export Audit Logs by Resource

```bash
curl -X GET "https://gateway.example.com/api/admin/audit/export/resource/ak_live_1a2b3c4d5e6f7g8h9i0j?limit=100" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -o audit_logs_key.xml
```

#### Export Audit Logs by Period

```bash
curl -X GET "https://gateway.example.com/api/admin/audit/export/period?startDate=2026-10-01&endDate=2026-10-03&limit=1000" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -o audit_logs_october_2026.xml
```

### Key Rotation Explained

Key rotation is a security practice that involves replacing an API key with a new one while maintaining a grace period where both keys are valid. This allows for smooth transitions without disrupting services.

In ApiKeyGateway, key rotation works as follows:

1. **Rotation Request**: When you call the rotate endpoint (`POST /api/apikeys/{id}/rotate`), the system:
   - Creates a new API key with the same consumer ID and properties
   - Optionally sets a new expiration period (if specified in the request)
   - Marks the original key as rotated (but not immediately revoked)

2. **Grace Period**: Both the old and new keys remain valid during the grace period configured in `appsettings.json`:
   ```json
   "KeyRotation": {
     "GracePeriodDays": 7, // Days to allow old keys to work after rotation
     "AutoRotate": true
   }
   ```

3. **Usage During Transition**: 
   - Applications can start using the new key immediately
   - Existing applications continue to work with the old key during the grace period
   - Monitor usage of both keys to ensure a smooth transition

4. **After Grace Period**: 
   - After the grace period ends, the old key is automatically revoked (if auto-rotation is enabled)
   - Or manually revoked by calling the revoke endpoint
   - Only the new key remains valid

**Example Rotation Timeline**:
- Day 1: Original key created (expires in 365 days)
- Day 100: Key rotated - new key issued, both keys valid
- Day 100-107: Grace period - both keys work
- Day 108: Old key automatically revoked (if auto-rotate enabled), only new key valid
- Day 465: New key expires (365 days from rotation date)

### Usage Tracking

ApiKeyGateway provides comprehensive usage tracking capabilities to monitor API consumption and identify trends.

#### Reading Usage Statistics

Get detailed statistics for a specific API key:

```bash
curl -X GET "https://gateway.example.com/api/usage/keys/ak_live_1a2b3c4d5e6f7g8h9i0j/statistics?startDate=2026-09-01&endDate=2026-09-30" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "apiKeyId": "ak_live_1a2b3c4d5e6f7g8h9i0j",
  "startDate": "2026-09-01T00:00:00Z",
  "endDate": "2026-09-30T23:59:59Z",
  "totalRequests": 12450,
  "successfulRequests": 12200,
  "failedRequests": 250,
  "successRate": 97.99,
  "totalBytesTransferred": 452301000,
  "averageResponseTimeMs": 45.2,
  "uniqueEndpoints": 15
}
```

#### Getting Usage Records

Retrieve detailed usage records (individual requests):

```bash
curl -X GET "https://gateway.example.com/api/usage/keys/ak_live_1a2b3c4d5e6f7g8h9i0j/records?startDate=2026-10-01&endDate=2026-10-03&limit=100" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
[
  {
    "id": "record_1a2b3c4d5e6f7g8h9i0j",
    "recordedAt": "2026-10-03T10:15:00Z",
    "endpoint": "/api/users",
    "method": "GET",
    "statusCode": 200,
    "requestBytes": 1200,
    "responseBytes": 3400,
    "responseTimeMs": 32,
    "sourceIp": "203.0.113.45"
  }
]
```

#### Consumer-Level Usage

Get aggregated usage for all keys belonging to a consumer:

```bash
curl -X GET "https://gateway.example.com/api/usage/consumers/customer123/total?startDate=2026-09-01&endDate=2026-09-30" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "consumerId": "customer123",
  "startDate": "2026-09-01T00:00:00Z",
  "endDate": "2026-09-30T23:59:59Z",
  "totalBytesTransferred": 452301000,
  "totalGBTransferred": 0.42
}
```

#### Quota Consumption

Check current quota usage for an API key:

```bash
curl -X GET "https://gateway.example.com/api/usage/quota/ak_live_1a2b3c4d5e6f7g8h9i0j" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

Response:
```json
{
  "keyId": "ak_live_1a2b3c4d5e6f7g8h9i0j",
  "used": 12450,
  "limit": 100000,
  "percentUsed": 12.45,
  "isExceeded": false,
  "period": "Monthly",
  "periodStart": "2026-10-01T00:00:00Z",
  "periodEnd": "2026-10-31T23:59:59Z",
  "remaining": 87550
}
```

### Diagnostics and Observability

### Diagnostic output

Models print safe summaries in the debugger and in logs. `ApiKey` shows the public prefix and a mask (for example `sk_live****`). The stored hash is never printed. `RateLimitDecision`, `UsageRecord`, `UsageSnapshot` and the rate limit policy options also override `ToString()` and expose a `DebuggerDisplay`. `UsageRecord` omits client IP and user agent.

```csharp
var key = await apiKeyService.GetByIdAsync(id);
logger.LogInformation("Loaded {Key}", key);   // ApiKey { Id = ..., Key = sk_live****, Status = Active, ... }
```

### Health checks

Register the gateway check on the health checks builder:

```csharp
builder.Services.AddHealthChecks()
    .AddApiKeyGatewayHealthCheck(options =>
    {
        options.UsageFlushQueueDegradedThreshold = 10_000; // default
    });
```

The check is named `api-key-gateway` and tagged `ready`. It reports:

| Data key | Values | Meaning |
|---|---|---|
| `store` | `reachable`, `unreachable` | Opens and closes a connection to the API key store. Failure makes the check **Unhealthy**. |
| `usageTracker` | `ok`, `backlogged`, `not_registered` | State of the usage flush queue. `backlogged` makes the check **Degraded**. |
| `usageFlushPendingRequests` | number | Requests counted in memory and not yet flushed. |
| `usageFlushQueueThreshold` | number | The configured threshold. |
| `rateLimiterBackend` | `in_memory`, `not_registered` | Backend of the in-memory `IRateLimiter`. |

`usageTracker` and `rateLimiterBackend` only report live state when the host registers `IUsageTracker` and `IRateLimiter`. The default `AddGatewayServices` registration does not register either, so both report `not_registered` and the flush-queue threshold never applies. The store probe is always active.

Endpoints:

- `GET /health` runs every registered check. **Behaviour change:** it now returns `503` when the key store is unreachable. Before this change it always returned `200` because no checks were registered. Load balancers that poll `/health` will see the instance as down during a database outage.
- `GET /health/checks` runs only checks tagged `ready`. `/health/ready` is already used by `HealthController`, so this endpoint uses a different path.

Error messages from the store are written to the log, not to the response, because health endpoints are anonymous.

### Metrics

Counters are published on the meter `ApiKeyGateway` (`GatewayMetrics.MeterName`), using `System.Diagnostics.Metrics`:

| Instrument | Incremented when |
|---|---|
| `requests_allowed` | A request passes authentication, rate limiting, route scope and quota checks. |
| `requests_rate_limited` | A request is rejected with `429` because the key exceeded its limit. |
| `requests_unauthorized` | A supplied key is rejected. Tagged `reason` with the `AuthenticationFailureReason` name, for example `ApiKeyExpired`. Store outages (`503`) are not counted. |

The counters carry no API key identifiers, so their cardinality stays bounded. The host does not export them by default. Register the meter with an exporter, for example OpenTelemetry:

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddMeter("ApiKeyGateway"));
```

## C# HttpClient Examples

All examples assume you have configured an `HttpClient` instance with the base address and authentication.

#### Creating an HttpClient Instance

```csharp
var client = new HttpClient
{
    BaseAddress = new Uri("https://gateway.example.com/")
};

// Add authentication (Bearer token)
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", "YOUR_ADMIN_TOKEN");

// Or basic authentication
var byteArray = Encoding.ASCII.GetBytes("admin:securepassword");
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Basic", 
        Convert.ToBase64String(byteArray));
```

#### Create a New API Key

```csharp
var request = new CreateKeyRequest
{
    ConsumerId = "customer123",
    Name = "Production API Key",
    ExpirationDays = 365
};

var response = await client.PostAsJsonAsync("/api/apikeys", request);
response.EnsureSuccessStatusCode();

var result = await response.Content.ReadFromJsonAsync<CreateKeyResponse>();
Console.WriteLine($"Created key: {result.KeyId}");
```

#### Get API Key Statistics

```csharp
var response = await client.GetFromJsonAsync<UsageStatisticsResponse>(
    $"/api/usage/keys/{keyId}/statistics?startDate={startDate:O}&endDate={endDate:O}");

if (response != null)
{
    Console.WriteLine($"Total requests: {response.TotalRequests}");
    Console.WriteLine($"Success rate: {response.SuccessRate}%");
    Console.WriteLine($"Average response time: {response.AverageResponseTimeMs}ms");
}
```

#### Export Usage Data

```csharp
var response = await client.GetAsync(
    $"/api/admin/export/usage?format=csv&startDate={startDate:O}&endDate={endDate:O}");

response.EnsureSuccessStatusCode();

var csvData = await response.Content.ReadAsStringAsync();
await File.WriteAllTextAsync("usage_report.csv", csvData);
```

#### Rotate an API Key

```csharp
var request = new RotateKeyRequest { NewExpirationDays = 365 };

var response = await client.PostAsJsonAsync(
    $"/api/apikeys/{keyId}/rotate", request);

response.EnsureSuccessStatusCode();

var result = await response.Content.ReadFromJsonAsync<RotateKeyResponse>();
Console.WriteLine($"Rotated key {result.OldKeyId} to {result.NewKeyId}");
```

#### Search Audit Logs

```csharp
var response = await client.GetFromJsonAsync<AuditLogEntry[]>(
    $"/api/admin/audit/search?action={action}&fromUtc={fromUtc:O}&toUtc={toUtc:O}&limit={limit}");

if (response != null)
{
    foreach (var log in response)
    {
        Console.WriteLine($"{log.PerformedAt}: {log.Action} by {log.PerformedBy}");
    }
}
```

## Project overview

ApiKeyGateway - a self-hosted ASP.NET Core (.NET 10) API key authentication gateway with rate limiting, usage quotas, audit logging and request transformation, backed by SQL Server via raw ADO.NET. Also packaged as NuGet `Zaiets.api.key.gateway`.

## Build

SDK pinned in `global.json` (10.0.100, rollForward latestMinor). Solution: `api-key-gateway.sln` (3 projects: app, tests, benchmarks).

```bash
dotnet restore
dotnet build --no-restore --configuration Release        # whole solution (what CI does)
dotnet build src/ApiKeyGateway --configuration Release   # app only (make build)
make build                                               # restore + build Release
dotnet watch run --project src/ApiKeyGateway             # dev server (make dev)
```

Docker: `Dockerfile` + `docker-compose.yml` (`make docker-build`, `make docker-compose-up`).

## Test

xUnit 2.9 + FluentAssertions 7 + Moq 4.20 + `Microsoft.AspNetCore.Mvc.Testing` (WebApplicationFactory<Program>). Single test project: `tests/api-key-gateway.Tests/` (~1500 Fact/Theory cases).

```bash
dotnet test                                                        # all tests
dotnet test --no-build --verbosity normal                          # CI form
dotnet test tests/api-key-gateway.Tests --filter "FullyQualifiedName~ApiKeyServiceTests"
make test-coverage                                                 # with coverage
```

Test conventions:
- Namespace `ApiKeyGateway.Tests`; one class per SUT, file named `<Type>Tests.cs`. Extra files split by concern: `<Type>ValidationTests.cs`, `<Type>JsonExtensionsTests.cs`, `<Type>ExtensionsTests.cs`, `<Type>UnitTests.cs`, partials like `UsageQuotaServiceTests.Comprehensive.cs`.
- Method names `Method_Scenario_ExpectedResult`; `// Arrange / Act / Assert` comments; SUT field `_sut`, mocks `_xxxMock`.
- Dependencies mocked with Moq (`Mock<IRepository>`, `Mock<ILogger<T>>`); assertions via FluentAssertions (`act.Should().ThrowAsync<...>()`).
- Integration tests use `WebApplicationFactory<Program>` (`Program` is exposed as `public partial class Program`).
- Note: a stray `src/tests/api-key-gateway.Tests/` folder exists but is not in the solution; the real test project is `tests/`.

## Lint / Format

`.editorconfig` is the source of truth (4-space indent, LF, Allman braces, `_camelCase` private fields, `I`-prefixed interfaces, PascalCase types).

```bash
dotnet format                       # apply
dotnet format --verify-no-changes   # check (make lint also builds with /p:TreatWarningsAsErrors=true)
```

`Directory.Build.props`: `Nullable` and `ImplicitUsings` enabled, `LangVersion latest`, `TreatWarningsAsErrors=false`. `GenerateDocumentationFile=true` on the app - public members need XML doc comments or CS1591 warnings appear.

## Architecture

Entry point: `src/ApiKeyGateway/Program.cs` (minimal hosting). Pipeline order: `ErrorHandlingMiddleware` -> CORS -> `UseApiKeyAuthentication()` -> `UseRequestTransformation()` -> controllers; `/health` mapped; Swagger at `/docs` in Development.

Layers under `src/ApiKeyGateway/`:
- `Controllers/` - `AdminController`, `ApiKeysController`, `UsageController`, `AnalyticsController`, `StatsController`, `HealthController`.
- `Middleware/` - auth, correlation, error handling (`GatewayProblemDetailsFactory` -> RFC 7807 `application/problem+json`), logging, perf, request validation/transformation.
- `Services/` - business logic (`ApiKeyService`, `RateLimitingService`, `UsageQuotaService`, `UsageTrackingService` + `BufferedUsageTrackingService` decorator, `ApiKeyRotationService`, `AuditLogService`, `DataExportService`, `QuotaAlertEvaluator`, `RequestCoalescingService`).
- `Repositories/` - data access over `Data/DbConnection.cs` (`IDbConnection` / `SqlServerConnection`, `System.Data.SqlClient`, hand-written SQL, no EF).
- `Domain/` - `Models/`, `Enums/`, `Constants/ErrorMessages.cs`, `Exceptions/` (all derive from `ApiKeyGatewayException`).
- `Events/` - `IEventPublisher`, `RetryingEventPublisher`, dead-letter queue, typed events.
- `Transformation/` - `ITransformationPipeline`, `LuaScriptExecutor` (MoonSharp).
- `BackgroundWorkers/` - `BackgroundServiceBase` + cleanup/rotation/aggregation workers.
- `Configuration/` - DI registration (`ServiceRegistrationExtensions.AddGatewayServices`, `ServiceCollectionExtensions.AddGatewayCoreServices`) and `*Options` classes bound from `appsettings.json` (`Gateway:*`, `RateLimiting`, `QuotaAlerts`).
- `Caching/`, `Integration/` (webhooks, external API client, batch handler), `Utilities/`, `Validation/`, `Extensions/`.

Other: `benchmarks/api-key-gateway.Benchmarks/` (BenchmarkDotNet), `examples/` (multi-language client samples), `docs/` (per-type markdown), `.github/workflows/` (ci, build, codeql, docker, nuget-publish, release).

## Conventions

- Every `.cs` file starts with the `// Author: Vladyslav Zaiets | https://sarmkadan.com` banner. Keep it on new files.
- File-scoped namespaces matching folder (`ApiKeyGateway.Services` etc.). One primary type per file.
- Companion-file pattern: for a type `Foo`, related static helpers live in `FooExtensions.cs`, `FooJsonExtensions.cs` (System.Text.Json serialization) and `FooValidation.cs` (validation rules). Follow this when adding helpers rather than growing the main file.
- Interfaces `IFoo` are usually declared in the same file as the implementation; register both in `Configuration/ServiceCollectionExtensions.cs` (constructor injection, mostly `AddScoped`; singletons for caches/state).
- Guard clauses: `ArgumentNullException.ThrowIfNull(x)`; input problems throw `Domain.Exceptions.ValidationException`; DB failures wrap into `DataAccessException`; auth into `InvalidApiKeyException` / `UnauthorizedAccessException` (the project's own type - alias it when `System.UnauthorizedAccessException` is in scope); `RateLimitExceededException` carries `RetryAfter`/`Limit`/`WindowInSeconds`. Controllers do not catch - `ErrorHandlingMiddleware` maps exceptions to ProblemDetails.
- Logging via `ILogger<T>` structured templates; `DateTime.UtcNow` everywhere; `CancellationToken` as last parameter on async methods; async methods suffixed `Async`.
- Classes that are not meant for inheritance are `sealed`; models use `init` setters.
- Do not commit `.aider*`, `bin/`, `obj/`, `appsettings.Development.json` (gitignored). `CHANGELOG.md` exists but history lives in git.

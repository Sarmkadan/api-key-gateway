# API Key Gateway Architecture

## System Overview

API Key Gateway follows a layered architecture with clear separation of concerns, designed for scalability, testability, and maintainability.

### Architectural Layers

```
┌─────────────────────────────────────────────┐
│         Web/Presentation Layer              │
│    (Controllers, REST API, HTTP Handlers)   │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│         Business Logic Layer                │
│  (Services, Domain Logic, Validation)       │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│     Data Access Layer                       │
│  (Repositories, Entity Models, Queries)     │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│         Infrastructure Layer                │
│  (Database, Caching, External Services)     │
└─────────────────────────────────────────────┘
```

## Core Components

### 1. Controllers (Web Layer)

**Responsibility**: Handle HTTP requests/responses, route to services

**Key Files**:
- `ApiKeysController.cs` - API key CRUD operations
- `UsageController.cs` - Usage statistics and analytics
- `StatsController.cs` - Gateway metrics
- `AdminController.cs` - Administrative operations
- `HealthController.cs` - Health checks for monitoring

**Design Pattern**: RESTful API with standard HTTP methods

```csharp
[ApiController]
[Route("api/[controller]")]
public class ApiKeysController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiKeyResponse>> CreateKey(
        [FromBody] CreateApiKeyRequest request)
    {
        var result = await _apiKeyService.CreateKeyAsync(request);
        return CreatedAtAction(nameof(GetKey), new { id = result.Id }, result);
    }
}
```

### 2. Services (Business Logic Layer)

**Responsibility**: Business logic, orchestration, validation

**Key Services**:

#### ApiKeyService
- Create, read, update, delete API keys
- Key validation and rotation
- Status management (active, disabled, revoked)

#### RateLimitingService
- Enforce rate limits per key
- Track request counts in time windows
- Handle limit exceeded scenarios

#### UsageTrackingService
- Record each API request
- Calculate statistics
- Aggregate usage data

#### AuditLogService
- Log all administrative actions
- Track API usage for compliance
- Archive old logs based on retention policy

#### AuthenticationService
- Validate API keys
- Check expiration and IP whitelist
- Return authenticated consumer context

**Design Pattern**: Single Responsibility, Dependency Injection

```csharp
public class ApiKeyService
{
    private readonly IApiKeyRepository _repository;
    private readonly ICacheProvider _cache;
    private readonly IAuditLogService _auditLog;

    public async Task<ApiKeyResponse> CreateKeyAsync(CreateApiKeyRequest request)
    {
        // Validate request
        ValidateKeyRequest(request);
        
        // Create domain model
        var apiKey = new ApiKey(request.ConsumerId, request.Name);
        
        // Persist
        await _repository.InsertAsync(apiKey);
        
        // Cache
        await _cache.SetAsync(apiKey.Id, apiKey);
        
        // Audit
        await _auditLog.LogAsync(AuditAction.KeyCreated, apiKey.Id);
        
        return new ApiKeyResponse(apiKey);
    }
}
```

### 3. Middleware (Request Pipeline)

**Responsibility**: Cross-cutting concerns, request interception

**Processing order actually wired in `Program.cs`**:

1. **ErrorHandlingMiddleware** - Catch and format errors (must be first)
2. **CORS** (`AllowAll` policy)
3. **ApiKeyAuthenticationMiddleware** - Extract and validate API key,
   rate limit, scope and quota checks
4. **RequestTransformationMiddleware** - Lua transformation pipeline
5. **Controllers** - Route to appropriate handler

`CorrelationContextMiddleware`, `RequestLoggingMiddleware`,
`RequestValidationMiddleware` and `PerformanceMonitoringMiddleware` exist in
`Middleware/` and are exercised by tests, but they are opt-in: `Program.cs`
does not add them. `MiddlewareConfiguration.UseGatewayMiddleware` wires
several of them if you choose to call it. See
[ARCHITECTURE.md](ARCHITECTURE.md) for the current, code-accurate pipeline.

**Example: ApiKeyAuthenticationMiddleware**

```csharp
public class ApiKeyAuthenticationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var apiKey = ExtractApiKey(context);
        
        if (string.IsNullOrEmpty(apiKey))
        {
            context.Response.StatusCode = 401;
            return;
        }
        
        var consumer = await _authService.ValidateKeyAsync(apiKey);
        if (consumer == null)
        {
            context.Response.StatusCode = 401;
            return;
        }
        
        context.Items["ConsumerId"] = consumer.Id;
        context.Items["ApiKeyId"] = consumer.KeyId;
        
        await _next(context);
    }
}
```

### 4. Repositories (Data Access Layer)

**Responsibility**: Persist and retrieve domain objects

**Key Repositories**:

- `IApiKeyRepository` - API key storage
- `IUsageRepository` - Usage record persistence
- `IRateLimitRepository` - Rate limit tracking
- `IAuditLogRepository` - Audit log storage

**Design Pattern**: Repository Pattern with async operations

```csharp
public interface IApiKeyRepository
{
    Task<ApiKey> GetByIdAsync(string id);
    Task<IEnumerable<ApiKey>> GetByConsumerAsync(string consumerId);
    Task InsertAsync(ApiKey key);
    Task UpdateAsync(ApiKey key);
    Task DeleteAsync(string id);
}
```

### 5. Domain Models

**Responsibility**: Core business entities with business logic

**Key Models**:

#### ApiKey
- Identifier, hash, consumer ID
- Expiration, status, metadata
- Business rules: validation, rotation

#### RateLimit
- Window configuration (second, minute, hour, day)
- Request counts within windows
- Burst size handling

#### UsageRecord
- Request metadata (timestamp, endpoint, status)
- Performance metrics (latency, bandwidth)
- Consumer attribution

#### AuditLog
- Action type (created, disabled, revoked)
- Actor and affected resource
- Timestamp and details

```csharp
public class ApiKey
{
    public string Id { get; set; }
    public string ConsumerId { get; set; }
    public string KeyHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ApiKeyStatus Status { get; set; }
    
    public bool IsValid() => 
        Status == ApiKeyStatus.Active && 
        DateTime.UtcNow < ExpiresAt;
    
    public void Disable() => Status = ApiKeyStatus.Disabled;
    public void Revoke() => Status = ApiKeyStatus.Revoked;
}
```

## Data Flow

### Request Processing Flow

```
Client Request
     │
     ▼
┌─────────────────────────────────────────┐
│ Middleware Pipeline                     │
│ 1. Correlation ID                       │
│ 2. Request Validation                   │
│ 3. API Key Extraction                   │
│ 4. Authentication                       │
│ 5. Rate Limit Check                     │
│ 6. Performance Monitoring                │
└──────────────┬──────────────────────────┘
               │
      ┌────────▼────────┐
      │ Authorized?     │
      └────┬────────┬───┘
           │        │
          YES      NO
           │        │
           ▼        ▼
       ┌─────┐  ┌──────────┐
       │Route│  │401 Error │
       └──┬──┘  └──────────┘
          │
          ▼
    ┌──────────────────┐
    │ Service Layer    │
    │ Business Logic   │
    └────────┬─────────┘
             │
             ▼
    ┌──────────────────┐
    │ Data Access      │
    │ Repositories     │
    └────────┬─────────┘
             │
             ▼
    ┌──────────────────┐
    │ Database/Cache   │
    │ SQL Server       │
    └────────┬─────────┘
             │
             ▼
    ┌──────────────────┐
    │ Response         │
    │ Formatting       │
    └────────┬─────────┘
             │
             ▼
         Client
```

### API Key Validation Flow

```
Request with API Key
        │
        ▼
  ┌──────────────┐
  │ Check Cache  │
  └──┬──────┬────┘
     │      │
  HIT│      │MISS
     │      │
     ▼      ▼
  Cache  Query DB
    │       │
    └───┬───┘
        │
        ▼
  ┌──────────────┐
  │ Hash Match?  │
  └──┬──────┬────┘
     │      │
   YES     NO
     │      │
     ▼      ▼
  ┌────┐  ┌──────────┐
  │OK  │  │401 Error │
  └─┬──┘  └──────────┘
    │
    ▼
┌──────────────┐
│ Check Status │
│ (Active?)    │
└──┬──────┬────┘
   │      │
  YES     NO
   │      │
   ▼      ▼
┌────┐  ┌──────────────┐
│OK  │  │403 Forbidden │
└─┬──┘  └──────────────┘
  │
  ▼
┌──────────────┐
│ Check Expiry │
└──┬──────┬────┘
   │      │
 VALID  EXPIRED
   │      │
   ▼      ▼
┌────┐  ┌──────────────┐
│OK  │  │401 Error     │
└─┬──┘  └──────────────┘
  │
  ▼
┌──────────────────┐
│ Check IP         │
│ Whitelist (if)   │
└──┬──────────┬────┘
   │          │
  YES        NO
   │          │
   ▼          ▼
┌────┐  ┌──────────────┐
│OK  │  │403 Forbidden │
└─┬──┘  └──────────────┘
  │
  ▼
Proceed with Request
```

### Rate Limiting Implementation

**Strategy**: Sliding Window Counter

```
Requests:  X  X  X  X  X  |  X  X  X  X
Timeline:  |-----1 minute---------|
           
Limit: 10 requests/minute

Current Time:
- Count requests in past 60 seconds: 9
- New request: allowed (9 < 10)
- Increment counter: 10

Next Request:
- Count requests in past 60 seconds: 10
- New request: rejected (10 >= 10)
- Return 429 Too Many Requests
```

**Implementation**:

```csharp
public class RateLimitingService
{
    private readonly ICacheProvider _cache;
    
    public async Task<bool> IsAllowedAsync(string apiKeyId, RateLimit limit)
    {
        var windowKey = $"ratelimit:{apiKeyId}:{DateTime.UtcNow.Minute}";
        var currentCount = await _cache.IncrementAsync(windowKey);
        
        if (currentCount == 1)
        {
            // First request in window, set expiry
            await _cache.ExpireAsync(windowKey, TimeSpan.FromMinutes(1));
        }
        
        return currentCount <= limit.RequestsPerMinute;
    }
}
```

## Caching Strategy

### Cache Layers

1. **L1: In-Memory Cache** (ASP.NET Core IMemoryCache)
   - API key validation (30-minute TTL)
   - Consumer details
   - Rate limit counters

2. **L2: Distributed Cache** (Redis, optional)
   - Shared state across instances
   - Rate limit sync
   - Consumer data

### Cache Key Patterns

```
apikey:{keyId} -> ApiKey object
ratelimit:{keyId}:{minute} -> request count
consumer:{consumerId} -> Consumer object
stats:{keyId}:{date} -> Usage statistics
```

## Security Architecture

### Authentication

- **Key Validation**: SHA-256 hash comparison (constant-time)
- **Extraction Points**: Header (`X-API-Key`), Query parameter, Body
- **Expiration**: Automatic rejection of expired keys
- **Status Check**: Disabled/revoked keys rejected

### Authorization

- **Role-based** (future): Admin, consumer, read-only
- **IP Whitelisting**: Per-key IP restrictions
- **Rate Limiting**: Per-key usage quotas

### Data Protection

- **In Transit**: HTTPS/TLS enforcement (configurable)
- **At Rest**: Database encryption (configurable)
- **Audit Trail**: Immutable audit logs
- **Key Storage**: Never store plaintext keys

## Performance Considerations

### Optimization Techniques

1. **Caching**: 30-min TTL on API keys reduces DB queries 95%
2. **Async/Await**: Non-blocking I/O throughout
3. **Connection Pooling**: SQL Server pooling enabled
4. **Batch Operations**: Bulk insert/update for scale
5. **Indexing**: Optimized database indexes on hot columns
6. **Compression**: Request/response gzip when appropriate

### Scalability

**Horizontal Scaling**:
- Deploy multiple gateway instances
- Use load balancer (nginx, Azure LB)
- Share distributed cache (Redis)
- Scale database independently

**Database Scaling**:
- Read replicas for analytics
- Sharding by consumer ID (future)
- Archive old audit logs

## Error Handling

### Error Response Format

```json
{
  "error": {
    "code": "INVALID_API_KEY",
    "message": "The provided API key is invalid or expired",
    "details": {
      "requestId": "req_abc123",
      "timestamp": "2026-05-04T15:30:00Z"
    }
  }
}
```

### Error Categories

| Code | HTTP Status | Meaning | Retry |
|------|-------------|---------|-------|
| INVALID_API_KEY | 401 | Key not found or hash mismatch | No |
| API_KEY_EXPIRED | 401 | Key expiration date passed | No |
| API_KEY_DISABLED | 403 | Key explicitly disabled | No |
| RATE_LIMIT_EXCEEDED | 429 | Quota exceeded | Yes (after window reset) |
| INVALID_REQUEST | 400 | Malformed request | No |
| INTERNAL_ERROR | 500 | Server error | Yes (with backoff) |

## Deployment Topology

### Single-Instance (Dev)

```
Client -> Gateway (w/ SQL Server)
```

### Multi-Instance (Prod)

```
Clients
  │
  ├─────────────────────────────┐
  │                             │
  ▼                             ▼
Load Balancer (nginx/HAProxy)
  │
  ├─────────────────────────────┐
  │                             │
  ▼                             ▼
Gateway-1                    Gateway-2
  │                             │
  └──────────────┬──────────────┘
                 │
        ┌────────┴────────┐
        │                 │
        ▼                 ▼
   SQL Server        Redis Cache
     (Primary)       (Distributed)
        │
        ▼
  SQL Replicas (Read-only)
```

## Testing Architecture

### Unit Tests
- Service logic in isolation
- Mock repositories
- Test business rules

### Integration Tests
- Full request pipeline
- Real or in-memory database
- Controller → Service → Repository

### Load Tests
- Gateway performance under load
- Rate limiting accuracy
- Cache effectiveness

## Monitoring & Observability

### Key Metrics

- **Response Time**: 50th, 95th, 99th percentiles
- **Throughput**: Requests per second
- **Error Rate**: Failed requests percentage
- **Cache Hit Rate**: % of requests served from cache
- **Database Connections**: Active and queued

### Health Checks

```
GET /health
{
  "status": "Healthy",
  "database": "Connected",
  "cache": "Operational",
  "uptime": "72:15:30",
  "checks": {
    "database": { "status": "OK", "responseTime": 5 },
    "cache": { "status": "OK", "responseTime": 2 }
  }
}
```

## Future Architectural Enhancements

1. **Microservices**: Separate audit, analytics services
2. **Event-Driven**: Kafka for audit events
3. **GraphQL**: Complementary query language
4. **Multi-Tenant**: Isolated data per customer
5. **Machine Learning**: Anomaly detection for API usage

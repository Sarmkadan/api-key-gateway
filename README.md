// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

## ApiKeyRepository

The `ApiKeyRepository` class is a concrete implementation of the `IApiKeyRepository` interface, providing data access and persistence for API keys using an ADO.NET connection with an in-memory write-through cache. It supports CRUD operations on API keys, including creating, retrieving, updating, and deleting keys.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;

// Create a new API key repository instance
var connection = new SqlServerConnection("Server=localhost;Database=api_key_gateway;User Id=sa;Password=your_password;");
var logger = new Logger<ApiKeyRepository>(new LoggerFactory());
var repository = new ApiKeyRepository(connection, logger);

// Create a new API key
var newApiKey = new ApiKey
{
    Id = "key_001",
    ConsumerId = "consumer_001",
    Name = "Test API Key",
    KeyHash = "hashed_value_here",
    Prefix = "test_abc",
    Status = ApiKeyStatus.Active,
    CreatedAt = DateTime.UtcNow,
    ExpiresAt = DateTime.UtcNow.AddYears(1),
    Description = "Test API key for development"
};

var createdKey = await repository.CreateAsync(newApiKey);
Console.WriteLine($"Created API key: {createdKey.Id}");

// Retrieve an API key by ID
var retrievedKey = await repository.GetByIdAsync("key_001");
if (retrievedKey != null)
{
    Console.WriteLine($"Retrieved key: {retrievedKey.Name}");
}

// Update the API key
retrievedKey.Name = "Updated Test API Key";
await repository.UpdateAsync(retrievedKey);

// Delete the API key
await repository.DeleteAsync("key_001");
```

## IAuditLogRepository

The `IAuditLogRepository` interface defines methods for creating, querying, and managing audit log entries in the system. It supports creating logs for resource actions, retrieving logs by resource ID or date range, and purging old logs.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;

// Create an audit log repository instance
var connection = new SqlServerConnection("Server=localhost;Database=api_key_gateway;User Id=sa;Password=your_password;");
var logger = new Logger<AuditLogRepository>(new LoggerFactory());
var auditRepo = new AuditLogRepository(connection, logger);

// Create a new audit log entry
var auditLog = new AuditLog
{
    Id = "audit_001",
    ResourceId = "key_001",
    ResourceType = "ApiKey",
    Action = Domain.Enums.AuditAction.KeyCreated,
    PerformedBy = "admin_user",
    PerformedAt = DateTime.UtcNow,
    HttpStatusCode = 201,
    SourceIp = "192.168.1.1",
    Reason = "Initial creation",
    IsSuccess = true
};

await auditRepo.CreateAsync(auditLog);
Console.WriteLine("Audit log created for API key creation");

// Retrieve logs for a specific resource
var logs = await auditRepo.GetByResourceIdAsync("key_001", limit: 50);
foreach (var log in logs)
{
    Console.WriteLine($"Action: {log.GetActionDescription()} at {log.PerformedAt}");
}

// Retrieve logs for a date range
var recentLogs = await auditRepo.GetByDateRangeAsync(
    startDate: DateTime.UtcNow.AddDays(-7),
    endDate: DateTime.UtcNow);

// Delete logs older than 30 days
var deletedCount = await auditRepo.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-30));
Console.WriteLine($"Deleted {deletedCount} old audit logs");
```

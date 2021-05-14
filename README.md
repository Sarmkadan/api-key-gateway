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

## RateLimitRepository

The `RateLimitRepository` class provides data access and persistence for rate limit configurations. It supports CRUD operations on rate limits.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;

// Create a rate limit repository instance
var connection = new SqlServerConnection("Server=localhost;Database=api_key_gateway;User Id=sa;Password=your_password;");
var logger = new Logger<RateLimitRepository>(new LoggerFactory());
var rateLimitRepo = new RateLimitRepository(connection, logger);

// Create a new rate limit
var newRateLimit = new RateLimit
{
    Id = "rl_001",
    ApiKeyId = "key_001",
    RequestsPerUnit = 100,
    Unit = Domain.Enums.RateLimitUnit.Minute,
    IsEnabled = true,
    CreatedAt = DateTime.UtcNow,
    LastResetAt = DateTime.UtcNow,
    CurrentRequestCount = 0
};

var createdRateLimit = await rateLimitRepo.CreateAsync(newRateLimit);
Console.WriteLine($"Created rate limit: {createdRateLimit.Id}");

// Retrieve a rate limit by API key ID
var retrievedRateLimit = await rateLimitRepo.GetByApiKeyIdAsync("key_001");
if (retrievedRateLimit != null)
{
    Console.WriteLine($"Retrieved rate limit: {retrievedRateLimit.RequestsPerUnit} requests per {retrievedRateLimit.Unit}");
}

// Update the rate limit
retrievedRateLimit.RequestsPerUnit = 200;
await rateLimitRepo.UpdateAsync(retrievedRateLimit);

// Delete the rate limit
await rateLimitRepo.DeleteAsync("rl_001");
```

## UsageQuotaRepository

The `UsageQuotaRepository` class provides data access and persistence for usage quotas. It supports CRUD operations on usage quotas.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;

// Create a usage quota repository instance
var connection = new SqlServerConnection("Server=localhost;Database=api_key_gateway;User Id=sa;Password=your_password;");
var logger = new Logger<UsageQuotaRepository>(new LoggerFactory());
var usageQuotaRepo = new UsageQuotaRepository(connection, logger);

// Create a new usage quota
var newUsageQuota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_001",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = Domain.Enums.QuotaPeriod.Day,
    CreatedAt = DateTime.UtcNow,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

var createdUsageQuota = await usageQuotaRepo.CreateAsync(newUsageQuota);
Console.WriteLine($"Created usage quota: {createdUsageQuota.Id}");

// Retrieve a usage quota by API key ID
var retrievedUsageQuota = await usageQuotaRepo.GetByApiKeyIdAsync("key_001");
if (retrievedUsageQuota != null)
{
    Console.WriteLine($"Retrieved usage quota: {retrievedUsageQuota.QuotaLimit} for {retrievedUsageQuota.Period}");
}

// Update the usage quota
retrievedUsageQuota.CurrentUsage = 500;
await usageQuotaRepo.UpdateAsync(retrievedUsageQuota);

// Delete the usage quota
await usageQuotaRepo.DeleteAsync("quota_001");
```

## RequestValidatorTests

The `RequestValidatorTests` class provides unit tests for the `RequestValidator` class, covering validation methods for email addresses, URLs, IP addresses, string lengths, numeric ranges, and GUIDs. These tests ensure that the request validation logic correctly handles various input scenarios and edge cases.

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using Xunit;

// Create test instance
var validatorTests = new RequestValidatorTests();

// Test email validation with valid and invalid inputs
var emailResult = validatorTests.ValidateEmail_VariousInputs_ReturnsExpectedResult(
    "test@example.com", true);
Assert.True(emailResult);

// Test URL validation with HTTPS requirement
var urlResult = validatorTests.ValidateUrl_VariousInputs_ReturnsExpectedResult(
    "https://example.com", true);
Assert.True(urlResult);

// Test IP address validation
var ipResult = validatorTests.ValidateIpAddress_VariousInputs_ReturnsExpectedResult(
    "192.168.1.1", true);
Assert.True(ipResult);

// Test string length validation
var lengthValidResult = validatorTests.ValidateLength_ValidLength_ReturnsTrue();
Assert.True(lengthValidResult);

// Test numeric range validation
var rangeValidResult = validatorTests.ValidateRange_ValidValue_ReturnsTrue();
Assert.True(rangeValidResult);

// Test GUID validation
var guidValidResult = validatorTests.ValidateGuid_ValidGuid_ReturnsTrue();
Assert.True(guidValidResult);
```

## UsageQuotaServiceTests

The `UsageQuotaServiceTests` class provides unit tests for the `UsageQuotaService` class, covering quota checking, recording, and management functionality. It ensures features like quota limits, period rollover, concurrent access safety, and exceeded quota handling work correctly.

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Services;
using Xunit;

// Create test instance
var quotaServiceTests = new UsageQuotaServiceTests();

// Test checking and recording quota usage
var quota = new UsageQuota
{
    ApiKeyId = "key_001",
    QuotaLimit = 1000,
    Period = Domain.Enums.QuotaPeriod.Day,
    CurrentUsage = 500,
    IsEnabled = true,
    PeriodStartAt = DateTime.UtcNow.Date
};

var result = await quotaServiceTests.CheckAndRecordAsync(quota);
Assert.False(result.IsExceeded);
Assert.Equal(500, result.Remaining);
Assert.Equal(1000, result.Limit);
```

## ApiKeyModelTests

The `ApiKeyModelTests` class provides unit tests for the `ApiKey` model class, covering key status checks, usage tracking, and IP whitelist validation. It tests the functionality of key status, usage recording, and IP address validation. 

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Domain.Models;
using Xunit;

// Create test instance
var apiKeyModelTests = new ApiKeyModelTests();

// Test active non-expired key
apiKeyModelTests.CanBeUsed_ActiveNonExpiredKey_ReturnsTrue();

// Test inactive status
apiKeyModelTests.CanBeUsed_InactiveStatus_ReturnsFalse(ApiKeyStatus.Disabled);

// Test expired key
var expiredKey = new ApiKey { Status = ApiKeyStatus.Active, ExpiresAt = DateTime.UtcNow.AddDays(-1) };
apiKeyModelTests.CanBeUsed_ExpiredKey_ReturnsFalse();

// Test recording usage
var key = new ApiKey { Status = ApiKeyStatus.Active };
apiKeyModelTests.RecordUsage_Called_IncrementsRequestCountAndUpdatesLastUsed();

// Test disabling active key
var activeKey = new ApiKey { Status = ApiKeyStatus.Active };
apiKeyModelTests.Disable_ActiveKey_SetsDisabledStatusAndTimestamp();

// Test enabling disabled key
var disabledKey = new ApiKey { Status = ApiKeyStatus.Disabled };
apiKeyModelTests.Enable_DisabledKey_RestoresActiveStatusAndClearsTimestamp();

// Test IP allowed with null whitelist
var nullWhitelistKey = new ApiKey { IpWhitelist = null };
apiKeyModelTests.IsIpAllowed_NullWhitelist_AllowsAnyIp();

// Test IP allowed in comma-delimited whitelist
var whitelistKey = new ApiKey { IpWhitelist = "10.0.0.1, 192.168.1.50, 172.16.0.1" };
apiKeyModelTests.IsIpAllowed_IpInCommaDelimitedWhitelist_ReturnsTrue();

// Test IP not allowed in whitelist
var notInWhitelistKey = new ApiKey { IpWhitelist = "10.0.0.1,10.0.0.2" };
apiKeyModelTests.IsIpAllowed_IpNotInWhitelist_ReturnsFalse();
```

## CollectionExtensionsTests

The `CollectionExtensionsTests` class provides unit tests for the collection extension methods in `ApiKeyGateway.Extensions.CollectionExtensions`. It tests functionality for pagination, collection state checking, batching, and other collection operations. 

### Example Usage

```csharp
using ApiKeyGateway.Extensions;
using System.Linq;

// Test pagination
var items = Enumerable.Range(1, 20);
var paginatedItems = items.Paginate(1, 5);
Console.WriteLine(string.Join(", ", paginatedItems)); // Output: 1, 2, 3, 4, 5

// Test checking if a collection is empty
var emptyCollection = Enumerable.Empty<int>();
Console.WriteLine(emptyCollection.IsEmpty()); // Output: True

// Test counting items by a key
var itemsToCount = new[] { "a", "b", "a", "c", "b", "a" };
var countedItems = itemsToCount.CountBy(x => x);
Console.WriteLine(string.Join(", ", countedItems.Select(x => $"{x.Key}: {x.Value}"))); // Output: a: 3, b: 2, c: 1

// Test batching
var itemsToBatch = Enumerable.Range(1, 7);
var batches = itemsToBatch.Batch(3).Select(b => string.Join(", ", b));
Console.WriteLine(string.Join(", ", batches)); // Output: 1, 2, 3, 4, 5, 6, 7
```

## StringExtensionsTests

The `StringExtensionsTests` class provides unit tests for the string extension methods in `ApiKeyGateway.Extensions.StringExtensions`. It tests functionality for string truncation, ellipsis truncation, string containment, prefix matching, slug creation, and more. 

### Example Usage

```csharp
using ApiKeyGateway.Extensions;

// Test truncation
var originalString = "This is a very long string that needs to be truncated.";
var truncatedString = originalString.Truncate(20);
Console.WriteLine(truncatedString); // Output: "This is a very long..."

// Test ellipsis truncation
var ellipsisString = originalString.TruncateWithEllipsis(20);
Console.WriteLine(ellipsisString); // Output: "This is a very..."

// Test string containment
var containsString = "Hello World";
var containsResult = containsString.ContainsAny("World", "Universe");
Console.WriteLine(containsResult); // Output: True

// Test prefix matching
var prefixString = "sk_abc123";
var prefixResult = prefixString.StartsWithAny("pk_", "sk_");
Console.WriteLine(prefixResult); // Output: True

// Test slug creation
var slugString = "Hello World Test";
var slugResult = slugString.ToSlug();
Console.WriteLine(slugResult); // Output: "hello-world-test"
```

## AuthenticationServiceTests

The `AuthenticationServiceTests` class provides unit tests for the `AuthenticationService` class, covering authentication scenarios, IP whitelisting, and error handling. It tests the functionality of authenticating API keys, validating IP addresses, and logging authentication attempts.

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Services;
using Xunit;

// Create test instance
var authenticationServiceTests = new AuthenticationServiceTests();

// Test authentication with a valid API key
var key = new ApiKey { Id = "key-123", ConsumerId = "consumer-abc", Status = ApiKeyStatus.Active };
var result = await authenticationServiceTests.AuthenticateAsync_ValidKey_ReturnsKeyAndLogsSuccess();
Assert.NotNull(result);

// Test authentication with an invalid API key
var invalidKeyResult = await authenticationServiceTests.AuthenticateAsync_InvalidKey_ThrowsUnauthorizedExceptionAndLogsFailure();
Assert.Throws<UnauthorizedAccessException>(() => invalidKeyResult);

// Test IP whitelisting
var whitelistedKey = new ApiKey { Id = "key-456", IpWhitelist = "10.0.0.1, 10.0.0.2" };
var whitelistedResult = await authenticationServiceTests.AuthenticateAsync_IpWhitelisted_AllowsAuthentication();
Assert.NotNull(whitelistedResult);
```
```
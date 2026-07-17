// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

## Architecture

For the system-level picture - project layout, DI composition root, the
actual request pipeline, background workers, extension points and known
limitations - see [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md). The sections
below document individual classes.

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

## ApiKeyGatewayExample

The `ApiKeyGatewayExample` class demonstrates bulk API key management operations using the ApiKey Gateway API. It provides examples for creating, listing, updating, rotating, and reporting on API keys in batch operations, making it ideal for administrative tools and automation scenarios.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Create an instance of the example
var example = new ApiKeyGatewayExample(
    baseUrl: "http://localhost:5000",
    adminKey: "admin_key_example"
);

// Run the example
await example.RunAsync();

// Or use individual methods
var consumerId = "consumer_123";

// Create a new API key
var newKey = await example.CreateKeyAsync(consumerId, "Production API Key");
Console.WriteLine($"Created key: {newKey.Id}");

// List all keys for a consumer
var keys = await example.ListConsumerKeysAsync(consumerId);
foreach (var key in keys)
{
    Console.WriteLine($"Key: {key.Name}, Status: {key.Status}");
}

// Update multiple keys at once
var updatedCount = await example.UpdateKeysAsync(
    new List<string> { "key_001", "key_002" },
    new Dictionary<string, object> { { "rateLimit", new { requestsPerSecond = 200 } } }
);

// Rotate keys for a consumer (creates new keys, keeps old ones during grace period)
await example.RotateConsumerKeysAsync(consumerId, gracePeriodHours: 24);

// Generate a consumer report
await example.GenerateConsumerReportAsync(consumerId);
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

## CacheKeyGeneratorTests

The `CacheKeyGeneratorTests` class provides unit tests for the `CacheKeyGenerator` class, covering cache key generation for various scenarios. It tests the functionality of generating cache keys for API keys, rate limits, usage statistics, quotas, and webhook deliveries. 

### Example Usage

```csharp
using ApiKeyGateway.Caching;
using Xunit;

// Test generating cache key for API key
var apiKeyKey = CacheKeyGenerator.GetApiKeyKey("key-001");
Assert.Equal("apigw:apikey:key-001", apiKeyKey);

// Test generating cache key for rate limit
var rateLimitKey = CacheKeyGenerator.GetRateLimitKey("key-001", "/api/users");
Assert.Equal("apigw:ratelimit:key-001:/api/users", rateLimitKey);

// Test generating cache key for usage statistics
var usageStatsKey = CacheKeyGenerator.GetUsageStatsKey("key-001", DateTime.UtcNow);
Assert.StartsWith("apigw:usage:key-001:", usageStatsKey);
```

## RateLimitingServiceTests

The `RateLimitingServiceTests` class provides unit tests for the `RateLimitingService` class, covering rate limiting scenarios, including checking limits, recording requests, updating limits, and resetting windows. It tests the functionality of rate limiting, ensuring that the service correctly handles various input scenarios and edge cases.

### Example Usage

```csharp
using ApiKeyGateway.Services;
using ApiKeyGateway.Domain.Models;
using Xunit;

// Create test instance
var rateLimitingServiceTests = new RateLimitingServiceTests();

// Test checking limit with a valid API key
var rateLimit = new RateLimit
{
    ApiKeyId = "key-001",
    RequestsPerUnit = 100,
    Unit = Domain.Enums.RateLimitUnit.Minute,
    CurrentRequestCount = 50,
    LastResetAt = DateTime.UtcNow
};
var result = await rateLimitingServiceTests.CheckLimitAsync_BelowLimit_ReturnsTrue();
Assert.True(result);

// Test recording a request with a valid API key
var requestResult = await rateLimitingServiceTests.RecordRequestAsync_ValidKey_IncrementsCountAndPersists();
Assert.NotNull(requestResult);

// Test updating a limit with a valid API key
var updateResult = await rateLimitingServiceTests.UpdateLimitAsync_ExistingKey_UpdatesAndReturnsTrue();
Assert.True(updateResult);
```

## ApiKeyRotationServiceTests

The `ApiKeyRotationServiceTests` class provides unit tests for the `ApiKeyRotationService` class, covering key rotation scenarios, including rotating active keys, handling inactive or non-existent keys, and preserving IP whitelists. It tests the functionality of key rotation, ensuring that the service correctly handles various input scenarios and edge cases.

### Example Usage

```csharp
using ApiKeyGateway.Services;
using ApiKeyGateway.Domain.Models;
using Xunit;

// Create test instance
var apiKeyRotationServiceTests = new ApiKeyRotationServiceTests();

// Test rotating an active key
var activeKey = new ApiKey { Id = "key-001", ConsumerId = "consumer-abc", Status = ApiKeyStatus.Active };
var result = await apiKeyRotationServiceTests.RotateKeyAsync_ActiveKey_CreatesNewKeyAndRevokesOld();
Assert.True(result.Success);

// Test rotating a non-existent key
var nonExistentKeyResult = await apiKeyRotationServiceTests.RotateKeyAsync_KeyNotFound_ReturnsFailureResult();
Assert.False(nonExistentKeyResult.Success);

// Test rotating an inactive key
var inactiveKeyResult = await apiKeyRotationServiceTests.RotateKeyAsync_InactiveKey_ReturnsFailureResult();
Assert.False(inactiveKeyResult.Success);
```

## StringExtensionsJsonExtensions

The `StringExtensionsJsonExtensions` class provides JSON serialization and deserialization capabilities for `StringExtensions` type metadata. Since `StringExtensions` is a static class, this class serializes metadata about the extension methods rather than instances of `StringExtensions` itself. It includes methods for converting metadata to JSON strings, parsing JSON strings back to metadata objects, and safely attempting deserialization.

### Example Usage

```csharp
using ApiKeyGateway.Extensions;
using System;

// Serialize StringExtensions metadata to JSON
var json = StringExtensionsJsonExtensions.ToJson();
Console.WriteLine(json);

// Serialize with indentation for readability
var prettyJson = StringExtensionsJsonExtensions.ToJson(indented: true);
Console.WriteLine(prettyJson);

// Deserialize JSON back to metadata
var metadata = StringExtensionsJsonExtensions.FromJson(json);
if (metadata != null)
{
    Console.WriteLine($"Type: {metadata.TypeName}");
    Console.WriteLine($"Methods: {string.Join(", ", metadata.Methods ?? Array.Empty<string>())}");
}

// Safely attempt deserialization
if (StringExtensionsJsonExtensions.TryFromJson(json, out var safeMetadata))
{
    Console.WriteLine("Deserialization succeeded!");
}

// Handle invalid JSON safely
var invalidJson = "{ invalid: json";
if (!StringExtensionsJsonExtensions.TryFromJson(invalidJson, out var errorMetadata))
{
    Console.WriteLine("Failed to deserialize invalid JSON");
}
```

## CollectionExtensionsValidation

The `CollectionExtensionsValidation` class provides a set of helper methods that validate common collection‑related parameters and state before they are used by extension methods. It ensures arguments such as pagination values, batch sizes, key selectors, and actions are correct, and it offers utilities to check or enforce collection validity.

### Example Usage

```csharp
using ApiKeyGateway.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

class ValidationExample
{
    public void Run()
    {
        // Validate pagination parameters (page number must be >= 1, page size >= 1)
        IReadOnlyList<string> paginationProblems =
            CollectionExtensionsValidation.ValidatePaginationParameters(pageNumber: 1, pageSize: 20);

        // Validate batch size
        IReadOnlyList<string> batchProblems =
            CollectionExtensionsValidation.ValidateBatchParameters(batchSize: 5);

        // Validate a key selector function
        Func<string, int> keySelector = s => s.Length;
        IReadOnlyList<string> keySelectorProblems =
            CollectionExtensionsValidation.ValidateKeySelector<string, int>(keySelector);

        // Validate an action for ForEachSafe
        Action<string> action = s => Console.WriteLine(s);
        IReadOnlyList<string> actionProblems =
            CollectionExtensionsValidation.ValidateForEachAction(action);

        // Validate a collection itself
        IEnumerable<int> numbers = new List<int> { 1, 2, 3 };
        IReadOnlyList<string> collectionProblems = CollectionExtensionsValidation.Validate(numbers);

        // Quick validity check
        bool isValid = numbers.IsValid();

        // Ensure the collection is not null (throws ArgumentNullException if it is)
        numbers.EnsureValid();
    }
}
```

## StringExtensionsValidation

The `StringExtensionsValidation` class provides validation helpers for string operation parameters from `StringExtensions`. It validates inputs before they are used by extension methods like `Truncate`, `TruncateWithEllipsis`, `ContainsAny`, `StartsWithAny`, and `ToList`, ensuring parameters meet expected constraints. The class offers both validation methods that return lists of problems and convenience methods that throw exceptions when validation fails.

### Example Usage

```csharp
using ApiKeyGateway.Extensions;
using System;

class ValidationExample
{
    public void Run()
    {
        // Validate Truncate parameters (maxLength must be > 0)
        var truncateProblems = StringExtensionsValidation.ValidateTruncateParameters(25);
        if (truncateProblems.Count == 0)
        {
            Console.WriteLine("Truncate parameters are valid");
        }

        // Validate TruncateWithEllipsis parameters (maxLength must be >= 3 to fit ellipsis)
        var ellipsisProblems = StringExtensionsValidation.ValidateTruncateWithEllipsisParameters(10);
        if (ellipsisProblems.Count == 0)
        {
            Console.WriteLine("TruncateWithEllipsis parameters are valid");
        }

        // Validate ContainsAny parameters (search strings must not be null/empty/whitespace)
        var containsProblems = StringExtensionsValidation.ValidateContainsAnyParameters("test", "example");
        if (containsProblems.Count == 0)
        {
            Console.WriteLine("ContainsAny parameters are valid");
        }

        // Validate StartsWithAny parameters (prefixes must not be null/empty/whitespace)
        var startsWithProblems = StringExtensionsValidation.ValidateStartsWithAnyParameters("pk_", "sk_");
        if (startsWithProblems.Count == 0)
        {
            Console.WriteLine("StartsWithAny parameters are valid");
        }

        // Validate ToList parameters (delimiter must not be a control character)
        var toListProblems = StringExtensionsValidation.ValidateToListParameters(',');
        if (toListProblems.Count == 0)
        {
            Console.WriteLine("ToList parameters are valid");
        }

        // Quick validity checks (returns bool instead of problems list)
        bool isValidTruncate = StringExtensionsValidation.IsValidTruncateParameters(25);
        bool isValidContains = StringExtensionsValidation.IsValidContainsAnyParameters("test");

        // Ensure parameters are valid (throws ArgumentException if not)
        StringExtensionsValidation.EnsureValidTruncateParameters(25);
        StringExtensionsValidation.EnsureValidContainsAnyParameters("pk_", "sk_");
    }
}
```

## KeyStoreUnavailableExceptionExtensions

The `KeyStoreUnavailableExceptionExtensions` class provides extension methods for `KeyStoreUnavailableException` that enable fluent exception enrichment, diagnostic analysis, and retry decision-making. It offers methods to create enriched exceptions with operation context, cache miss details, and custom messages, as well as utilities to analyze exception chains and determine if failures are likely transient.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Exceptions;
using System;

class KeyStoreExample
{
    public void Run()
    {
        try
        {
            // Simulate a key store operation that fails
            throw new KeyStoreUnavailableException("Key store connection failed");
        }
        catch (KeyStoreUnavailableException ex)
        {
            // Enrich exception with operation context
            var enrichedWithOperation = ex.WithOperation("GetApiKeyAsync");
            Console.WriteLine(enrichedWithOperation.Message);
            
            // Enrich with cache miss details
            var enrichedWithCacheMiss = ex.WithCacheMiss("key_12345");
            Console.WriteLine(enrichedWithCacheMiss.Message);
            
            // Enrich with custom context
            var enrichedWithContext = ex.WithContext("Redis connection timeout after 5 seconds");
            Console.WriteLine(enrichedWithContext.Message);
            
            // Analyze all operations in the exception chain
            var allOperations = ex.GetAllOperations();
            foreach (var operation in allOperations)
            {
                Console.WriteLine($"Failed operation: {operation}");
            }
            
            // Determine if the failure is likely transient (good for retry logic)
            bool isTransient = ex.IsLikelyTransient();
            Console.WriteLine($"Is transient: {isTransient}");
            
            // Generate a diagnostic report for logging
            string diagnosticReport = ex.ToDiagnosticString();
            Console.WriteLine(diagnosticReport);
            
            // Chain multiple enrichments for detailed error reporting
            var detailedException = new KeyStoreUnavailableException("Database unavailable")
                .WithOperation("ValidateKeyAsync")
                .WithContext("PostgreSQL connection pool exhausted")
                .WithCacheMiss("consumer_api_key_abc123");
            
            Console.WriteLine($"Detailed message: {detailedException.Message}");
            Console.WriteLine($"Is transient: {detailedException.IsLikelyTransient()}");
        }
    }
}
```

## UnauthorizedAccessExceptionExtensions

The `UnauthorizedAccessExceptionExtensions` class provides extension methods for `UnauthorizedAccessException` to enable fluent exception enrichment with source IP addresses, reasons, and custom messages. It offers utilities for creating enriched exceptions, checking exception reasons, and generating formatted diagnostic strings for logging and debugging purposes.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Exceptions;
using System;

class UnauthorizedAccessExample
{
public void Run()
{
try
{
// Simulate an unauthorized access scenario
throw new UnauthorizedAccessException("Access denied", "Invalid API key provided", "192.168.1.100");
}
catch (UnauthorizedAccessException ex)
{
// Enrich exception with source IP address
var enrichedWithIp = ex.WithSourceIp("10.0.0.50");
Console.WriteLine(enrichedWithIp.Message); // Original message with new source IP

// Enrich exception with a reason
var enrichedWithReason = ex.WithReason("Rate limit exceeded");
Console.WriteLine(enrichedWithReason.Message);

// Enrich exception with a custom message
var enrichedWithMessage = ex.WithMessage("API key authentication failed: rate limit exceeded");
Console.WriteLine(enrichedWithMessage.Message);

// Check if exception has a specific reason
bool hasReason = ex.HasReason("Invalid API key provided");
Console.WriteLine($"Has reason: {hasReason}");

// Generate a formatted string for logging
string formatted = ex.ToFormattedString();
Console.WriteLine(formatted); // "Access denied | Reason: Invalid API key provided | Source IP: 192.168.1.100"

// Chain multiple enrichments for detailed error reporting
var detailedException = new UnauthorizedAccessException("Access denied", "Invalid API key", "192.168.1.100")
.WithSourceIp("10.0.0.50")
.WithReason("Authentication failed");

Console.WriteLine($"Detailed message: {detailedException.Message}");
Console.WriteLine($"Formatted: {detailedException.ToFormattedString()}");
}
}
}
```

## InvalidApiKeyExceptionExtensions

The `InvalidApiKeyExceptionExtensions` class provides extension methods for `InvalidApiKeyException` to simplify common operations like checking if a key is expired, retrieving the API key hash, getting the timestamp when the exception occurred, and formatting exception details for logging purposes.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Exceptions;
using System;

class InvalidApiKeyExample
{
public void Run()
{
try
{
// Simulate an invalid API key scenario
throw new InvalidApiKeyException("API key validation failed", "sk_test_abc123xyz", isExpired: true);
}
catch (InvalidApiKeyException ex)
{
// Check if the key is expired
bool isExpired = ex.IsKeyExpired();
Console.WriteLine($"Is key expired: {isExpired}");

// Get the API key hash
string? keyHash = ex.GetApiKeyHash();
Console.WriteLine($"API key hash: {keyHash}");

// Get when the exception occurred
DateTime occurredAt = ex.GetOccurredAt();
Console.WriteLine($"Exception occurred at: {occurredAt:yyyy-MM-dd HH:mm:ss}");

// Check if the key is disabled (not expired)
bool isDisabled = ex.IsKeyDisabled();
Console.WriteLine($"Is key disabled: {isDisabled}");

// Format the exception for logging
string logMessage = ex.FormatForLogging();
Console.WriteLine(logMessage);
// Output: "InvalidApiKeyException: API key validation failed | ApiKeyHash: sk_test_abc123xyz | IsExpired: True | OccurredAt: 2026-07-19 14:30:00 UTC"

// Chain multiple checks for detailed error handling
if (ex.IsKeyExpired())
{
Console.WriteLine("Key has expired - consider renewing the API key");
}
else if (ex.IsKeyDisabled())
{
Console.WriteLine("Key is disabled - check key status in the admin portal");
}
}
}
}
```

## ApiKeyConsumerExtensions

The `ApiKeyConsumerExtensions` class provides extension methods for `ApiKeyConsumer` that simplify common operations for checking consumer status, tier management, organization domain extraction, and custom property access. These methods handle null checks and provide type-safe operations for consumer management.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using System;
using System.Collections.Generic;

class ApiKeyConsumerExample
{
public void Run()
{
// Create a consumer with custom properties
var consumer = new ApiKeyConsumer
{
Id = "consumer_001",
Name = "Acme Corporation",
Email = "admin@acme.com",
IsActive = true,
Tier = "pro",
CustomProperties = new Dictionary<string, string>
{
{ "max_requests_per_minute", "1000" },
{ "support_tier", "priority" },
{ "billing_id", "billing_12345" }
},
LastActivityAt = DateTime.UtcNow.AddDays(-5),
InactiveSince = null
};

// Check if consumer is currently active
bool isActive = consumer.IsCurrentlyActive();
Console.WriteLine($"Consumer is active: {isActive}");

// Get the consumer's tier as an enum
ApiKeyTier tier = consumer.GetTier();
Console.WriteLine($"Consumer tier: {tier}");

// Check if consumer can upgrade to enterprise tier
bool canUpgrade = consumer.CanUpgradeTo(ApiKeyTier.Enterprise);
Console.WriteLine($"Can upgrade to Enterprise: {canUpgrade}");

// Get the organization domain from email
string? organizationDomain = consumer.GetOrganizationDomain();
Console.WriteLine($"Organization domain: {organizationDomain}");

// Check if consumer has been inactive for more than 30 days
bool isInactive = consumer.IsInactiveForDays(30);
Console.WriteLine($"Is inactive for 30+ days: {isInactive}");

// Get a custom property value
string maxRequests = consumer.GetCustomProperty("max_requests_per_minute", "500");
Console.WriteLine($"Max requests per minute: {maxRequests}");

// Get a custom property as an integer
int maxRequestsInt = consumer.GetCustomPropertyAsInt("max_requests_per_minute", 500);
Console.WriteLine($"Max requests per minute (int): {maxRequestsInt}");

// Get a non-existent property with default value
string nonExistent = consumer.GetCustomProperty("non_existent_key", "default_value");
Console.WriteLine($"Non-existent property: {nonExistent}");

// Get a non-existent integer property with default value
int nonExistentInt = consumer.GetCustomPropertyAsInt("non_existent_key", 0);
Console.WriteLine($"Non-existent int property: {nonExistentInt}");
}
}
```

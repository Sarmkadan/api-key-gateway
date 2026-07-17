try
{
  ApiKeyValidatorValidation.EnsureValidQuotaLimit(-1);
  Console.WriteLine("Quota limit validation passed.");
}
catch (ArgumentException ex)
{
  Console.WriteLine($"Quota limit validation failed: {ex.Message}");
}

## StringExtensionsTestsExtensions

The `StringExtensionsTestsExtensions` class provides extension methods for `StringExtensionsTests` that offer additional test utilities for string manipulation scenarios commonly encountered in API key gateway testing. These extensions handle edge cases, null values, and provide deterministic test data generation for comprehensive test coverage.

## RateLimitingServiceTestsExtensions

The `RateLimitingServiceTestsExtensions` class provides extension methods for `RateLimitingServiceTests` that offer reusable test utilities for rate limiting service scenarios. It includes methods for creating configured service instances, generating rate limit configurations, executing concurrent requests, and verifying rate limit behavior through assertions.

### Public Members

- `CreateService(this RateLimitingServiceTests tests)` - Creates a configured `RateLimitingService` instance with default mocks
- `CreateRateLimit(this RateLimitingServiceTests tests, string apiKeyId, int requestsPerUnit, RateLimitUnit unit, int currentCount = 0)` - Creates a rate limit configuration for testing purposes
- `ExecuteConcurrentRequestsAsync(this RateLimitingService service, string keyId, int requestCount)` - Executes multiple concurrent requests against the rate limiting service and returns the results with exception tracking
- `ShouldAllThrowRateLimitExceededAsync(this Task<ConcurrentBag<RateLimitResult>> resultsTask, int expectedCount)` - Verifies that all requests in a collection resulted in rate limit exceptions
- `ShouldAllSucceedAsync(this Task<ConcurrentBag<RateLimitResult>> resultsTask, int expectedCount)` - Verifies that all requests in a collection succeeded
- `RateLimitResult(bool Success, Exception? Exception)` - Record that tracks the result of rate limit test requests

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;

// Create a test instance
var testInstance = new RateLimitingServiceTests();

// Create a configured rate limiting service with default mocks
var service = testInstance.CreateService();

// Create a rate limit configuration for testing
var rateLimit = testInstance.CreateRateLimit(
    apiKeyId: "test-api-key-123",
    requestsPerUnit: 5,
    unit: RateLimitUnit.Minute
);

// Test successful requests within rate limit
var successfulResults = await service.ExecuteConcurrentRequestsAsync("test-api-key-123", 3);
await successfulResults.ShouldAllSucceedAsync(3);

// Test rate limit exceeded scenario
var exceededResults = await service.ExecuteConcurrentRequestsAsync("test-api-key-123", 10);
await exceededResults.ShouldAllThrowRateLimitExceededAsync(10);

// Verify result properties
foreach (var result in successfulResults)
{
    result.Success.Should().BeTrue();
    result.Exception.Should().BeNull();
}

foreach (var result in exceededResults)
{
    result.Success.Should().BeFalse();
    result.Exception.Should().BeOfType<RateLimitExceededException>();
}
```

### Public Members

- `ContainsAny(this string source, params string[] values)` - Determines whether the string contains any of the specified substrings, ignoring case and culture
- `StartsWithAny(this string source, params string[] prefixes)` - Determines whether the string starts with any of the specified prefixes, ignoring case and culture
- `ToSlug(this string source)` - Converts the string to a URL-safe slug format for test assertions
- `Truncate(this string? source, int maxLength)` - Truncates the string to the specified maximum length, returning null if the input is null
- `TruncateWithEllipsis(this string? source, int maxLength)` - Truncates the string to the specified maximum length and appends an ellipsis if truncated, returning null if the input is null
- `CreateTestString(int length, int? seed = null)` - Creates a test string with controlled content for deterministic test scenarios
- `RepeatPattern(this string pattern, int repeatCount)` - Generates a test string with repeated pattern for consistency testing
- `CreateEdgeCaseString()` - Creates a string with all possible edge case characters for comprehensive testing

## RequestValidatorTestsExtensions

The `RequestValidatorTestsExtensions` class provides extension methods for `RequestValidatorTests` that offer reusable test utilities for validating various request parameters commonly encountered in API key gateway scenarios. These extensions generate comprehensive test cases for email validation, URL validation, IP address validation, length validation, range validation, and GUID validation to ensure robust parameter validation in the API gateway.

### Public Members

- `CreateEmailValidationTestCases()` - Creates a collection of test cases for email validation with expected boolean results
- `CreateUrlValidationTestCases()` - Creates a collection of test cases for URL validation with expected boolean results
- `CreateIpAddressValidationTestCases()` - Creates a collection of test cases for IP address validation with expected boolean results
- `CreateLengthValidationTestCases()` - Creates a collection of test cases for string length validation with minimum and maximum length constraints
- `CreateRangeValidationTestCases()` - Creates a collection of test cases for numeric range validation with minimum and maximum values
- `CreateGuidValidationTestCases()` - Creates a collection of test cases for GUID validation with expected boolean results
- `ShouldBeValid(this RequestValidatorTests tests)` - Asserts that a validation result indicates success
- `ShouldBeInvalid(this RequestValidatorTests tests)` - Asserts that a validation result indicates failure
- `CreateValidationResult(this RequestValidatorTests tests, bool isValid, string? errorMessage = null)` - Creates a validation result for testing purposes

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;

// Create a test instance
var testInstance = new RequestValidatorTests();

// Test email validation scenarios
var emailTestCases = testInstance.CreateEmailValidationTestCases();
emailTestCases.Should().NotBeEmpty();

foreach (var (email, expected) in emailTestCases)
{
    var validationResult = RequestValidator.ValidateEmail(email);
    if (expected)
    {
        validationResult.ShouldBeValid(testInstance);
    }
    else
    {
        validationResult.ShouldBeInvalid(testInstance);
    }
}

// Test URL validation scenarios
var urlTestCases = testInstance.CreateUrlValidationTestCases();
urlTestCases.Should().NotBeEmpty();

foreach (var (url, expected) in urlTestCases)
{
    var validationResult = RequestValidator.ValidateUrl(url);
    if (expected)
    {
        validationResult.ShouldBeValid(testInstance);
    }
    else
    {
        validationResult.ShouldBeInvalid(testInstance);
    }
}

// Test IP address validation scenarios
var ipTestCases = testInstance.CreateIpAddressValidationTestCases();
ipTestCases.Should().NotBeEmpty();

foreach (var (ipAddress, expected) in ipTestCases)
{
    var validationResult = RequestValidator.ValidateIpAddress(ipAddress);
    if (expected)
    {
        validationResult.ShouldBeValid(testInstance);
    }
    else
    {
        validationResult.ShouldBeInvalid(testInstance);
    }
}

// Test length validation scenarios
var lengthTestCases = testInstance.CreateLengthValidationTestCases();
lengthTestCases.Should().NotBeEmpty();

foreach (var (value, minLength, maxLength, expected) in lengthTestCases)
{
    var validationResult = RequestValidator.ValidateLength(value, minLength, maxLength);
    if (expected)
    {
        validationResult.ShouldBeValid(testInstance);
    }
    else
    {
        validationResult.ShouldBeInvalid(testInstance);
    }
}

// Test range validation scenarios
var rangeTestCases = testInstance.CreateRangeValidationTestCases();
rangeTestCases.Should().NotBeEmpty();

foreach (var (value, minimum, maximum, expected) in rangeTestCases)
{
    var validationResult = RequestValidator.ValidateRange(value, minimum, maximum);
    if (expected)
    {
        validationResult.ShouldBeValid(testInstance);
    }
    else
    {
        validationResult.ShouldBeInvalid(testInstance);
    }
}

// Test GUID validation scenarios
var guidTestCases = testInstance.CreateGuidValidationTestCases();
guidTestCases.Should().NotBeEmpty();

foreach (var (guid, expected) in guidTestCases)
{
    var validationResult = RequestValidator.ValidateGuid(guid);
    if (expected)
    {
        validationResult.ShouldBeValid(testInstance);
    }
    else
    {
        validationResult.ShouldBeInvalid(testInstance);
    }
}

// Create custom validation results for specific test scenarios
var customResult = testInstance.CreateValidationResult(true, null);
testInstance.ShouldBeValid(customResult);

var errorResult = testInstance.CreateValidationResult(false, "Invalid parameter format");
testInstance.ShouldBeInvalid(errorResult);
```

## ApiKeyModelTestsExtensions

The `ApiKeyModelTestsExtensions` class provides extension methods for `ApiKeyModelTests` that offer reusable test utilities for creating and asserting API key scenarios. These extensions simplify the setup of test API keys with various statuses, IP whitelists, and expiration dates, and provide fluent assertions for verifying API key state and behavior.

### Public Members

- `WithDefaultValues(this ApiKeyModelTests tests, int expirationDays = 30)` - Creates a new active API key with default test-friendly values including 30-day expiration
- `WithStatus(this ApiKeyModelTests tests, ApiKeyStatus status, int expirationDays = 30)` - Creates an API key with the specified status and expiration
- `WithIpWhitelist(this ApiKeyModelTests tests, string ipWhitelist, ApiKeyStatus status = ApiKeyStatus.Active)` - Creates an API key with the specified IP whitelist and optional status
- `ShouldBeUsable(this ApiKey key, bool expected)` - Asserts that the API key can or cannot be used based on its status and expiration
- `ShouldHaveUsage(this ApiKey key, int expectedCount, long expectedBytes)` - Asserts that the API key has the expected request count and bytes transferred
- `ShouldHaveLastUsedAt(this ApiKey key, DateTime? expected)` - Asserts that the API key has the expected last used timestamp
- `ShouldHaveDisabledAt(this ApiKey key, DateTime? expected)` - Asserts that the API key has the expected disabled timestamp
- `ShouldAllowIp(this ApiKey key, string ipAddress, bool expected)` - Asserts that the API key allows or denies the specified IP address
- `DisableAndAssert(this ApiKey key, DateTime? before = null)` - Disables the API key and asserts the operation was successful
- `EnableAndAssert(this ApiKey key)` - Enables the API key and asserts the operation was successful
- `RecordUsageAndAssert(this ApiKey key, long bytes = 0, DateTime? before = null)` - Records usage on the API key and asserts the operation was successful

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;

// Create a test instance
var testInstance = new ApiKeyModelTests();

// Create an active API key with default values (30-day expiration)
var activeKey = testInstance.WithDefaultValues();
activeKey.ShouldBeUsable(true);
activeKey.ShouldHaveUsage(0, 0);
activeKey.ShouldHaveLastUsedAt(null);
activeKey.ShouldHaveDisabledAt(null);

// Create an API key with specific status and expiration
var expiredKey = testInstance.WithStatus(ApiKeyStatus.Active, expirationDays: 0);
// Set expiration to past date for testing
expiredKey.ExpiresAt = DateTime.UtcNow.AddDays(-1);
expiredKey.ShouldBeUsable(false);

// Create an API key with IP whitelist
var ipRestrictedKey = testInstance.WithIpWhitelist("192.168.1.1, 10.0.0.1, 172.16.0.1");
ipRestrictedKey.ShouldAllowIp("192.168.1.1", true);
ipRestrictedKey.ShouldAllowIp("8.8.8.8", false);

// Test disabling an API key
var keyToDisable = testInstance.WithDefaultValues();
var disabledKey = keyToDisable.DisableAndAssert();
disabledKey.Status.Should().Be(ApiKeyStatus.Disabled);

// Test enabling an API key
var keyToEnable = testInstance.WithStatus(ApiKeyStatus.Disabled);
var enabledKey = keyToEnable.EnableAndAssert();
enabledKey.Status.Should().Be(ApiKeyStatus.Active);

// Test recording usage
var keyForUsage = testInstance.WithDefaultValues();
var usedKey = keyForUsage.RecordUsageAndAssert(bytes: 1024);
usedKey.ShouldHaveUsage(1, 1024);
usedKey.ShouldHaveLastUsedAt(usedKey.LastUsedAt);

// Test multiple usage recordings
var multiUsageKey = testInstance.WithDefaultValues();
var finalKey = multiUsageKey
    .RecordUsageAndAssert(bytes: 512)
    .RecordUsageAndAssert(bytes: 2048);
finalKey.ShouldHaveUsage(2, 2560); // 512 + 2048 = 2560
```

## CacheKeyGeneratorTestsExtensions

The `CacheKeyGeneratorTestsExtensions` class provides extension methods for `CacheKeyGeneratorTests` that offer reusable assertions and helper methods for testing cache key generation scenarios. These extensions validate cache key formats, parameter handling, and hash generation for various API gateway caching use cases including API keys, rate limits, usage statistics, quotas, webhook deliveries, and external API calls.

## AuditLogServiceTestsExtensions

The `AuditLogServiceTestsExtensions` class provides extension methods for `AuditLogServiceTests` that offer reusable test utilities for audit logging scenarios. It includes methods for creating test audit logs, setting up mock repository behaviors, verifying log creation and logging calls, and asserting on log collections with fluent assertions.

### Public Members

- `CreateTestAuditLog(this string resourceId, AuditAction action, bool isSuccess = true, string? performedBy = null, string resourceType = "ApiKey")` - Creates a test audit log with the specified parameters
- `VerifyLogCreated(this AuditLogServiceTests test, AuditLog expectedLog)` - Verifies that the repository received a call to create the specified log
- `VerifyInformationLogForAction(this AuditLogServiceTests test, AuditAction expectedAction)` - Verifies that the logger received an information-level log containing the specified action
- `SetupGetLogsAsync(this AuditLogServiceTests test, string resourceId, List<AuditLog> logs, int limit = 100)` - Sets up the repository to return a specific list of logs for the given resource ID
- `SetupGetLogsForPeriodAsync(this AuditLogServiceTests test, DateTime startDate, DateTime endDate, List<AuditLog> logs)` - Sets up the repository to return a specific list of logs for the given date range
- `SetupCleanupOldLogsAsync(this AuditLogServiceTests test, int retentionDays, int deletedCount)` - Sets up the repository to return a specific count when deleting old logs
- `GetMockRepository(this AuditLogServiceTests test)` - Gets the mock repository from the test instance
- `GetMockLogger(this AuditLogServiceTests test)` - Gets the mock logger from the test instance
- `GetServiceUnderTest(this AuditLogServiceTests test)` - Gets the service under test from the test instance
- `ContainOnlyActions(this List<AuditLog> logs, params AuditAction[] expectedActions)` - Asserts that a collection of logs contains only the expected actions
- `ContainOnlySuccessfulOperations(this List<AuditLog> logs)` - Asserts that a collection of logs contains only successful operations
- `ContainOnlyFailedOperations(this List<AuditLog> logs)` - Asserts that a collection of logs contains only failed operations

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;
using FluentAssertions;

// Create a test instance
var testInstance = new AuditLogServiceTests();

// Create a test audit log
var auditLog = "api-key-123".CreateTestAuditLog(
    action: AuditAction.Create,
    isSuccess: true,
    performedBy: "admin@example.com",
    resourceType: "ApiKey"
);

// Verify log creation
var service = testInstance.GetServiceUnderTest();
testInstance.VerifyLogCreated(auditLog);

// Setup repository to return specific logs for a resource
var logs = new List<AuditLog>
{
    auditLog,
    "api-key-123".CreateTestAuditLog(AuditAction.Update, true, "admin@example.com")
};
testInstance.SetupGetLogsAsync("api-key-123", logs, limit: 50);

// Test log retrieval
var retrievedLogs = await service.GetByResourceIdAsync("api-key-123", 50);
retrievedLogs.Should().HaveCount(2);

// Verify information log was created for the action
var expectedAction = AuditAction.Create;
testInstance.VerifyInformationLogForAction(expectedAction);

// Assert on log collection
logs.ContainOnlyActions(AuditAction.Create, AuditAction.Update);
logs.ContainOnlySuccessfulOperations();
```

### Public Members

- `ShouldHaveApiKeyFormat(this CacheKeyGeneratorTests test, string apiKey, string expectedKey)` - Asserts that a cache key follows the expected format pattern for API keys
- `ShouldHaveApiKeyMetadataFormat(this CacheKeyGeneratorTests test, string apiKey, string expectedKey)` - Asserts that a cache key follows the expected format pattern for API key metadata
- `ShouldHaveRateLimitKey(this CacheKeyGeneratorTests test, string apiKey, string? endpoint, string expectedKey)` - Asserts that a rate limit cache key includes the expected components
- `ShouldHaveUsageStatsKey(this CacheKeyGeneratorTests test, string apiKey, DateTime date, string expectedKey)` - Asserts that a usage statistics cache key formats the date correctly
- `ShouldHaveQuotaKey(this CacheKeyGeneratorTests test, string apiKey, string expectedKey)` - Asserts that a quota cache key follows the expected format pattern
- `ShouldHaveWebhookDeliveryKey(this CacheKeyGeneratorTests test, Guid eventId, string expectedKey)` - Asserts that a webhook delivery cache key uses the expected GUID format
- `ShouldHaveExternalApiCacheKey(this CacheKeyGeneratorTests test, string provider, string endpoint, Dictionary<string, string>? parameters, string expectedKey)` - Asserts that an external API cache key follows the expected format
- `ShouldIncludeHash(this CacheKeyGeneratorTests test, string key)` - Asserts that an external API cache key includes a hash when parameters are provided
- `ShouldBeHashOrderInvariant(this CacheKeyGeneratorTests test, string key1, string key2)` - Asserts that two cache keys are identical regardless of parameter dictionary order
- `ShouldHaveApiKeyInvalidationPattern(this CacheKeyGeneratorTests test, string apiKey, string expectedPattern)` - Asserts that a cache key follows the expected format pattern for API key invalidation
- `ShouldHaveRateLimitInvalidationPattern(this CacheKeyGeneratorTests test, string expectedPattern)` - Asserts that a rate limit invalidation pattern matches all rate limit keys
- `CreateParameterDictionary(this CacheKeyGeneratorTests test, params (string Key, string Value)[] parameters)` - Creates a dictionary of query parameters for testing external API cache keys
- `CreateDate(this CacheKeyGeneratorTests test, int year, int month, int day)` - Creates a date for testing usage statistics cache keys

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Caching;
using FluentAssertions;

// Create a test instance
var testInstance = new CacheKeyGeneratorTests();

// Test API key format assertions
var apiKey = "test-api-key-12345";
testInstance.ShouldHaveApiKeyFormat(apiKey, "api-key:test-api-key-12345");
testInstance.ShouldHaveApiKeyMetadataFormat(apiKey, "api-key:test-api-key-12345:metadata");

// Test rate limit key generation
testInstance.ShouldHaveRateLimitKey(apiKey, null, "rate-limit:*:test-api-key-12345");
testInstance.ShouldHaveRateLimitKey(apiKey, "/api/v1/users", "rate-limit:/api/v1/users:test-api-key-12345");

// Test usage statistics key generation
var testDate = testInstance.CreateDate(2024, 6, 15);
testInstance.ShouldHaveUsageStatsKey(apiKey, testDate, "usage-stats:2024-06-15:test-api-key-12345");

// Test quota key generation
testInstance.ShouldHaveQuotaKey(apiKey, "quota:test-api-key-12345");

// Test webhook delivery key generation
var eventId = Guid.Parse("12345678-1234-5678-1234-567812345678");
testInstance.ShouldHaveWebhookDeliveryKey(eventId, "webhook-delivery:12345678-1234-5678-1234-567812345678");

// Test external API cache key generation with parameters
var parameters = testInstance.CreateParameterDictionary(
    ("limit", "100"),
    ("offset", "50"),
    ("sort", "date")
);
testInstance.ShouldHaveExternalApiCacheKey(
    "stripe",
    "/v1/customers",
    parameters,
    "external-api:stripe:/v1/customers:limit:100:offset:50:sort:date:hash"
);

// Test hash inclusion assertion
testInstance.ShouldIncludeHash("external-api:stripe:/v1/customers:limit:100:hash");

// Test hash order invariance
var key1 = "external-api:stripe:/v1/customers:limit:100:offset:50:hash";
var key2 = "external-api:stripe:/v1/customers:offset:50:limit:100:hash";
testInstance.ShouldBeHashOrderInvariant(key1, key2);

// Test invalidation patterns
testInstance.ShouldHaveApiKeyInvalidationPattern(apiKey, "api-key:test-api-key-12345:*");
testInstance.ShouldHaveRateLimitInvalidationPattern("rate-limit:*:*");
```

```csharp
using ApiKeyGateway.Tests;
using FluentAssertions;

// Test ContainsAny extension method
string testString = "Production API Key for Service A";
bool containsProduction = testString.ContainsAny("production", "dev", "staging");
containsProduction.Should().BeTrue();

bool containsDev = testString.ContainsAny("dev", "staging");
containsDev.Should().BeFalse();

// Test StartsWithAny extension method
string apiKeyName = "prod_api_key_12345";
bool startsWithProd = apiKeyName.StartsWithAny("prod_", "dev_", "test_");
startsWithProd.Should().BeTrue();

// Test ToSlug extension method
string slugInput = "Production API Key - Service A!";
string slug = slugInput.ToSlug();
slug.Should().Be("production-api-key---service-a-");

// Test Truncate extension method
string longString = "This is a very long string that needs to be truncated";
string truncated = longString.Truncate(10);
truncated.Should().Be("This is a ");

// Test Truncate with null input
string? nullString = null;
string? truncatedNull = nullString.Truncate(10);
truncatedNull.Should().BeNull();

// Test TruncateWithEllipsis extension method
string mediumString = "Medium length string";
string truncatedWithEllipsis = mediumString.TruncateWithEllipsis(10);
truncatedWithEllipsis.Should().Be("Medium l...");

// Test CreateTestString for deterministic test data
string testData1 = StringExtensionsTestsExtensions.CreateTestString(20, seed: 42);
string testData2 = StringExtensionsTestsExtensions.CreateTestString(20, seed: 42);
testData1.Should().Be(testData2); // Same seed produces same result

// Test RepeatPattern extension method
string pattern = "abc";
string repeated = pattern.RepeatPattern(3);
repeated.Should().Be("abcabcabc");

// Test CreateEdgeCaseString for comprehensive testing
string edgeCases = StringExtensionsTestsExtensions.CreateEdgeCaseString();
edgeCases.Should().NotBeNullOrEmpty();
edgeCases.Should().Contain("!@#$%^&*()");
edgeCases.Should().Contain("\t\n");
```
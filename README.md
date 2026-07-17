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

### Example Usage

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
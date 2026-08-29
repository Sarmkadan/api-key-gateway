try
{
  // QuotaLimit.Unlimited (-1) is a valid sentinel meaning "no quota enforced"
  ApiKeyValidatorValidation.EnsureValidQuotaLimit(QuotaLimit.Unlimited);
  Console.WriteLine("Quota limit validation passed.");

  // Zero and other negative values are rejected
  ApiKeyValidatorValidation.EnsureValidQuotaLimit(-5);
}
catch (ArgumentOutOfRangeException ex)
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

## ValidationHelpersTestsExtensions

The `ValidationHelpersTestsExtensions` class provides extension methods for `ValidationHelpersTests` that offer reusable assertion helpers for validating various input types commonly encountered in API key gateway testing scenarios. These extensions simplify test assertions for email addresses, API keys, IP addresses, GUIDs, URLs, and input sanitization by wrapping the underlying `ValidationHelpers` methods with fluent assertions.

### Public Members

- `AssertEmailValidity(this ValidationHelpersTests test, string email, bool expected)` - Asserts that the result of `ValidationHelpers.IsValidEmail` matches the expected value
- `AssertApiKeyFormat(this ValidationHelpersTests test, string apiKey, bool expected)` - Asserts that the result of `ValidationHelpers.IsValidApiKeyFormat` matches the expected value
- `AssertIpAddressValidity(this ValidationHelpersTests test, string ipAddress, bool expected)` - Asserts that the result of `ValidationHelpers.IsValidIpAddress` matches the expected value
- `AssertGuidValidity(this ValidationHelpersTests test, string guidValue, bool expected)` - Asserts that the result of `ValidationHelpers.IsValidGuid` matches the expected value
- `AssertUrlValidity(this ValidationHelpersTests test, string url, bool expected)` - Asserts that the result of `ValidationHelpers.IsValidUrl` matches the expected value
- `AssertSanitizedInput(this ValidationHelpersTests test, string input, int maxLength, string expected)` - Asserts that `ValidationHelpers.SanitizeInput` returns the expected sanitized string

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using FluentAssertions;

// Create a test instance
var testInstance = new ValidationHelpersTests();

// Test email validation
var validEmail = "user@example.com";
testInstance.AssertEmailValidity(validEmail, true);
testInstance.AssertEmailValidity("invalid-email", false);

// Test API key format validation
testInstance.AssertApiKeyFormat("sk_live_abc123xyz789", true);
testInstance.AssertApiKeyFormat("invalid-key", false);

// Test IP address validation
testInstance.AssertIpAddressValidity("192.168.1.1", true);
testInstance.AssertIpAddressValidity("999.999.999.999", false);

// Test GUID validation
testInstance.AssertGuidValidity("123e4567-e89b-12d3-a456-426614174000", true);
testInstance.AssertGuidValidity("not-a-guid", false);

// Test URL validation
testInstance.AssertUrlValidity("https://api.example.com/v1/users", true);
testInstance.AssertUrlValidity("not-a-url", false);

// Test input sanitization
var sanitizedInput = "<script>alert('xss')</script>";
testInstance.AssertSanitizedInput(sanitizedInput, 100, "scriptalertxssscript");
```

## CollectionExtensionsTestsExtensions

The `CollectionExtensionsTestsExtensions` class provides extension methods for `CollectionExtensionsTests` that offer reusable test utilities for common collection operations in test scenarios. These extensions simplify assertions for collection state, provide dictionary conversion utilities, and offer first/last element accessors with default values, making test code more readable and maintainable.

### Public Members

- `ShouldBeEmpty<T>(this IEnumerable<T> collection, string paramName = null)` - Asserts that the collection is empty, throwing an exception if it contains any elements
- `ShouldNotBeEmpty<T>(this IEnumerable<T> collection, string paramName = null)` - Asserts that the collection is not empty, throwing an exception if it's empty
- `ShouldHaveCount<T>(this IEnumerable<T> collection, int expectedCount, string paramName = null)` - Asserts that the collection has the expected count of items
- `ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)` - Creates a dictionary from the collection using key selector and element selector functions
- `FirstOrDefault<T>(this IEnumerable<T> source, T defaultValue = default)` - Gets the first element of a sequence, or a default value if the sequence is empty
- `LastOrDefault<T>(this IEnumerable<T> source, T defaultValue = default)` - Gets the last element of a sequence, or a default value if the sequence is empty
- `All<T>(this IEnumerable<T> source, Func<T, bool> predicate)` - Determines whether all elements of a sequence satisfy a condition
- `Any<T>(this IEnumerable<T> source, Func<T, bool> predicate)` - Determines whether any element of a sequence satisfies a condition
- `Append<T>(this IEnumerable<T> source, T item)` - Returns a new sequence with the specified item added to the end
- `Prepend<T>(this IEnumerable<T> source, T item)` - Returns a new sequence with the specified item added to the beginning

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using FluentAssertions;
using System.Collections.Generic;

// Create test collections
var emptyCollection = new List<int>();
var singleItemCollection = new List<int> { 42 };
var multipleItemsCollection = new List<int> { 1, 2, 3, 4, 5 };

// Test empty collection assertion
emptyCollection.ShouldBeEmpty();

// Test non-empty collection assertion  
singleItemCollection.ShouldNotBeEmpty();
multipleItemsCollection.ShouldNotBeEmpty();

// Test collection count assertion
multipleItemsCollection.ShouldHaveCount(5);

// Test first/last element accessors
var firstItem = singleItemCollection.FirstOrDefault();
firstItem.Should().Be(42);

var lastItem = multipleItemsCollection.LastOrDefault();
lastItem.Should().Be(5);

// Test first/last with default values
var emptyFirst = emptyCollection.FirstOrDefault(defaultValue: -1);
emptyFirst.Should().Be(-1);

var emptyLast = emptyCollection.LastOrDefault(defaultValue: -1);
emptyLast.Should().Be(-1);

// Test dictionary conversion
var keyValuePairs = new List<KeyValuePair<string, int>>
{
    new KeyValuePair<string, int>("first", 1),
    new KeyValuePair<string, int>("second", 2),
    new KeyValuePair<string, int>("third", 3)
};

var dictionary = keyValuePairs.ToDictionary(
    kvp => kvp.Key,
    kvp => kvp.Value
);
dictionary.Should().HaveCount(3);
dictionary["first"].Should().Be(1);

// Test predicate-based operations
var hasEvenNumbers = multipleItemsCollection.Any(x => x % 2 == 0);
hasEvenNumbers.Should().BeTrue();

var allPositive = multipleItemsCollection.All(x => x > 0);
allPositive.Should().BeTrue();

// Test sequence manipulation
var appendedCollection = multipleItemsCollection.Append(6);
appendedCollection.Should().HaveCount(6);
appendedCollection.Last().Should().Be(6);

var prependedCollection = multipleItemsCollection.Prepend(0);
prependedCollection.Should().HaveCount(6);
prependedCollection.First().Should().Be(0);
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

## StringExtensionsTestsJsonExtensions

The `StringExtensionsTestsJsonExtensions` class provides JSON serialization and deserialization utilities for `StringExtensions` type metadata. This enables test scenarios that require serializing extension method information to JSON for logging, caching, or API responses, and deserializing it back for validation or comparison purposes.

### Public Members

- `TypeName` (string?) - Gets or sets the type name for serialization
- `Methods` (IReadOnlyList<string>?) - Gets or sets the list of public extension method names
- `ToJson(bool indented = false)` (static string) - Serializes StringExtensions type metadata to a JSON string
- `FromJson(string json)` (static StringExtensionsMetadata?) - Deserializes a JSON string to StringExtensions type metadata
- `TryFromJson(string json, out StringExtensionsMetadata? value)` (static bool) - Attempts to deserialize a JSON string to StringExtensions type metadata

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using System.Text.Json;
using FluentAssertions;

// Serialize StringExtensions metadata to compact JSON
var compactJson = StringExtensionsTestsJsonExtensions.ToJson();
compactJson.Should().NotBeNullOrEmpty();

// Serialize with indentation for readability
var indentedJson = StringExtensionsTestsJsonExtensions.ToJson(indented: true);
indentedJson.Should().Contain("\n"); // Should have newlines for indentation

// Deserialize the JSON back to metadata
var metadata = StringExtensionsTestsJsonExtensions.FromJson(compactJson);
metadata.Should().NotBeNull();
metadata.TypeName.Should().Be("StringExtensions");
metadata.Methods.Should().NotBeNull();
metadata.Methods.Should().NotBeEmpty();

// Test TryFromJson with valid JSON
bool parseSuccess = StringExtensionsTestsJsonExtensions.TryFromJson(compactJson, out var parsedMetadata);
parseSuccess.Should().BeTrue();
parsedMetadata.Should().NotBeNull();

// Test TryFromJson with invalid JSON (should return false)
bool invalidParseSuccess = StringExtensionsTestsJsonExtensions.TryFromJson("invalid json", out var invalidMetadata);
invalidParseSuccess.Should().BeFalse();
invalidMetadata.Should().BeNull();

// Test FromJson with null input (throws ArgumentNullException)
Action act = () => StringExtensionsTestsJsonExtensions.FromJson(null);
act.Should().Throw<ArgumentNullException>();

// Test FromJson with empty string (returns null)
var emptyResult = StringExtensionsTestsJsonExtensions.FromJson("   ");
emptyResult.Should().BeNull();
```

## UsageQuotaServiceTestsExtensions

The `UsageQuotaServiceTestsExtensions` class provides extension methods for `UsageQuotaServiceTests` that offer reusable test utilities for managing and verifying API key usage quotas. It includes methods for setting up mock quota repositories, creating quota services with configured limits, parsing quota limits from strings, and verifying quota operations through assertions.

### Public Members

- `SetupQuotaRepository(this UsageQuotaServiceTests tests, Dictionary<string, long>? quotas = null)` - Sets up the mock quota repository with predefined quota values for testing
- `CreateQuotaService(this UsageQuotaServiceTests tests, Dictionary<string, long>? quotas = null)` - Creates a configured `UsageQuotaService` instance with mock repository and optional predefined quotas
- `VerifyQuotaSet(this UsageQuotaServiceTests tests, IUsageQuotaRepository repository, string apiKey, long expectedLimit)` - Verifies that the repository received a call to set the expected quota limit
- `VerifyQuotaGet(this UsageQuotaServiceTests tests, IUsageQuotaRepository repository, string apiKey, long expectedUsage)` - Verifies that the repository received a call to get the quota usage for the specified API key
- `CreateQuotaKeys(this UsageQuotaServiceTests tests, params string[] apiKeys)` - Creates a dictionary of quota keys for testing purposes
- `ParseQuotaLimit(this UsageQuotaServiceTests tests, string limitString)` - Parses a quota limit string into a long value, returning null if parsing fails
- `CreateCapturingLogger(this UsageQuotaServiceTests tests)` - Creates a mock logger that captures log messages for verification

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Domain.Interfaces;
using ApiKeyGateway.Services;
using FluentAssertions;
using Moq;

// Create a test instance
var testInstance = new UsageQuotaServiceTests();

// Setup quota keys for testing
var quotaKeys = testInstance.CreateQuotaKeys(
    "test-api-key-123",
    "prod-api-key-456",
    "dev-api-key-789"
);

// Create a quota service with predefined limits
var quotas = new Dictionary<string, long>
{
    { "test-api-key-123", 1000 },
    { "prod-api-key-456", 5000 },
    { "dev-api-key-789", 100 }
};

var service = testInstance.CreateQuotaService(quotas);

// Test setting a quota limit
var mockRepository = new Mock<IUsageQuotaRepository>();
testInstance.VerifyQuotaSet(mockRepository, "new-api-key-abc", 5000);

// Test getting quota usage
var usage = 42L;
testInstance.VerifyQuotaGet(mockRepository, "test-api-key-123", usage);

// Create a capturing logger for testing
var (mockLogger, logMessages) = testInstance.CreateCapturingLogger();

// Parse quota limit from string
var parsedLimit = testInstance.ParseQuotaLimit("1000");
parsedLimit.Should().Be(1000L);

var invalidLimit = testInstance.ParseQuotaLimit("invalid");
invalidLimit.Should().BeNull();

// Verify quota keys were created correctly
quotaKeys.Should().HaveCount(3);
quotaKeys.Should().ContainKey("test-api-key-123");
quotaKeys["test-api-key-123"].Should().Be("quota:test-api-key-123");
```

## ApiKeyRotationServiceTestsValidation

The `ApiKeyRotationServiceTestsValidation` class provides validation helpers for `ApiKeyRotationServiceTests` instances. It includes methods for validating test setup, key rotation results, and API key properties to ensure test instances are properly configured and rotation operations produce expected outcomes.

### Public Members

- `Validate(this ApiKeyRotationServiceTests value)` - Validates the test instance and its mock dependencies, returning a list of validation problems
- `IsValid(this ApiKeyRotationServiceTests value)` - Determines whether the test instance is valid by checking if the validation list is empty
- `EnsureValid(this ApiKeyRotationServiceTests value)` - Ensures that the test instance is valid, throwing an exception if validation fails
- `ValidateKeyRotationResult(this ApiKeyRotationServiceTests _, RotationResult result, string expectedOldKeyId, string expectedNewKeyId, string expectedConsumerId)` - Validates that a key rotation result contains expected values including success status and key identifiers
- `ValidateApiKey(this ApiKeyRotationServiceTests _, ApiKey key, string expectedConsumerId, ApiKeyStatus expectedStatus, string? expectedIpWhitelist = null)` - Validates that an API key has expected properties including consumer ID, status, and optional IP whitelist

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Services;
using FluentAssertions;

// Create a test instance
var testInstance = new ApiKeyRotationServiceTests();

// Validate the test instance setup
var validationProblems = testInstance.Validate();
testInstance.IsValid().Should().BeTrue();

// Test successful validation
var validResult = new ApiKeyRotationServiceTests();
validResult.EnsureValid(); // Throws if invalid

// Test key rotation result validation
var rotationResult = new RotationResult
{
    Success = true,
    OldKeyId = "old-key-123",
    NewKeyId = "new-key-456",
    ConsumerId = "consumer-789",
    FailureReason = null
};

var rotationProblems = testInstance.ValidateKeyRotationResult(
    rotationResult,
    expectedOldKeyId: "old-key-123",
    expectedNewKeyId: "new-key-456",
    expectedConsumerId: "consumer-789"
);
rotationProblems.Should().BeEmpty();

// Test API key validation
var apiKey = new ApiKey
{
    Id = "key-789",
    ConsumerId = "consumer-789",
    Status = ApiKeyStatus.Active,
    CreatedAt = DateTime.UtcNow,
    IpWhitelist = "192.168.1.1,10.0.0.1"
};

var keyProblems = testInstance.ValidateApiKey(
    apiKey,
    expectedConsumerId: "consumer-789",
    expectedStatus: ApiKeyStatus.Active,
    expectedIpWhitelist: "192.168.1.1,10.0.0.1"
);
keyProblems.Should().BeEmpty();

// Test validation with mismatched values
var invalidRotationResult = new RotationResult
{
    Success = false,
    OldKeyId = "wrong-old-key",
    NewKeyId = "wrong-new-key",
    ConsumerId = "wrong-consumer",
    FailureReason = "Invalid key format"
};

var invalidRotationProblems = testInstance.ValidateKeyRotationResult(
    invalidRotationResult,
    expectedOldKeyId: "old-key-123",
    expectedNewKeyId: "new-key-456",
    expectedConsumerId: "consumer-789"
);
invalidRotationProblems.Should().HaveCount(3); // Success, OldKeyId, NewKeyId, ConsumerId mismatches

// Test validation with invalid API key
var invalidApiKey = new ApiKey
{
    Id = null,
    ConsumerId = "wrong-consumer",
    Status = ApiKeyStatus.Disabled,
    CreatedAt = default,
    IpWhitelist = null
};

var invalidKeyProblems = testInstance.ValidateApiKey(
    invalidApiKey,
    expectedConsumerId: "consumer-789",
    expectedStatus: ApiKeyStatus.Active
);
invalidKeyProblems.Should().HaveCount(4); // ConsumerId, Status, CreatedAt, Id, IpWhitelist issues
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
## AuditLogEventHandlerExtensionsTests

The `AuditLogEventHandlerExtensionsTests` class contains unit tests for the `AuditLogEventHandlerExtensions` extension methods, verifying proper handling of API key lifecycle events (creation, rotation, and disabling) through bulk operations and event delegation. These tests ensure that events are correctly processed, null arguments are properly validated, and unsupported event types are safely ignored.

### Public Members

- `AuditLogEventHandlerExtensionsTests()` - Constructor for the test class.
- `HandleApiKeyCreatedAsync_SingleEvent_DelegatesToHandler()` - Tests that HandleApiKeyCreatedAsync properly handles a single event.
- `HandleApiKeyCreatedAsync_MultipleEvents_ProcessesAll()` - Tests that HandleApiKeyCreatedAsync handles multiple events in sequence.
- `HandleApiKeyCreatedAsync_NullHandler_ThrowsArgumentNullException()` - Tests that HandleApiKeyCreatedAsync throws ArgumentNullException when handler is null.
- `HandleApiKeyCreatedAsync_NullEvents_ThrowsArgumentNullException()` - Tests that HandleApiKeyCreatedAsync throws ArgumentNullException when events collection is null.
- `HandleApiKeyCreatedAsync_EmptyEventsCollection_NoException()` - Tests that HandleApiKeyCreatedAsync does not throw when events collection is empty.
- `HandleApiKeyRotatedAsync_SingleEvent_DelegatesToHandler()` - Tests that HandleApiKeyRotatedAsync properly handles a single event.
- `HandleApiKeyRotatedAsync_MultipleEvents_ProcessesAll()` - Tests that HandleApiKeyRotatedAsync handles multiple events in sequence.
- `HandleApiKeyRotatedAsync_NullHandler_ThrowsArgumentNullException()` - Tests that HandleApiKeyRotatedAsync throws ArgumentNullException when handler is null.
- `HandleApiKeyRotatedAsync_NullEvents_ThrowsArgumentNullException()` - Tests that HandleApiKeyRotatedAsync throws ArgumentNullException when events collection is null.
- `HandleApiKeyDisabledAsync_SingleEvent_DelegatesToHandler()` - Tests that HandleApiKeyDisabledAsync properly handles a single event.
- `HandleApiKeyDisabledAsync_MultipleEvents_ProcessesAll()` - Tests that HandleApiKeyDisabledAsync handles multiple events in sequence.
- `HandleApiKeyDisabledAsync_NullHandler_ThrowsArgumentNullException()` - Tests that HandleApiKeyDisabledAsync throws ArgumentNullException when handler is null.
- `HandleApiKeyDisabledAsync_NullEvents_ThrowsArgumentNullException()` - Tests that HandleApiKeyDisabledAsync throws ArgumentNullException when events collection is null.
- `CreateEventDelegate_ReturnsNonNullDelegate()` - Tests that CreateEventDelegate returns a non-null delegate.
- `CreateEventDelegate_NullHandler_ThrowsArgumentNullException()` - Tests that CreateEventDelegate throws ArgumentNullException when handler is null.
- `CreateEventDelegate_HandlesApiKeyCreatedEvent()` - Tests that CreateEventDelegate properly handles ApiKeyCreatedEvent.
- `CreateEventDelegate_HandlesApiKeyRotatedEvent()` - Tests that CreateEventDelegate properly handles ApiKeyRotatedEvent.
- `CreateEventDelegate_HandlesApiKeyDisabledEvent()` - Tests that CreateEventDelegate properly handles ApiKeyDisabledEvent.
- `CreateEventDelegate_IgnoresUnsupportedEventType()` - Tests that CreateEventDelegate ignores unsupported event types.

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Events;
using ApiKeyGateway.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

// Setup mocks (typically done in test setup)
var scopeFactoryMock = new Mock<IServiceScopeFactory>();
var loggerMock = new Mock<ILogger<AuditLogEventHandler>>();
var handler = new AuditLogEventHandler(scopeFactoryMock.Object, loggerMock.Object);

// Setup scope and repository mocks
var scopeMock = new Mock<IServiceScope>();
var repositoryMock = new Mock<IAuditLogRepository>();
scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
scopeMock.Setup(x => x.ServiceProvider).Returns(() => new ServiceCollection()
    .AddSingleton(repositoryMock.Object)
    .BuildServiceProvider());

// Test handling a single API key created event
var @event = new ApiKeyCreatedEvent
{
    ApiKeyId = Guid.NewGuid().ToString(),
    Name = "Test Key",
    CreatedBy = "test-user"
};

repositoryMock.Setup(x => x.CreateAsync(It.IsAny<AuditLog>()))
    .Returns(Task.CompletedTask);

// Call the extension method being tested
await handler.HandleApiKeyCreatedAsync(new[] { @event });

// Verify the repository was called correctly
repositoryMock.Verify(
    x => x.CreateAsync(It.Is<AuditLog>(log =>
        log.ResourceId == @event.ApiKeyId &&
        log.Action == AuditAction.KeyCreated &&
        log.PerformedBy == @event.CreatedBy &&
        log.Reason == $"API key '{@event.Name}' created")),
    Times.Once);
```
## AuditLogServiceTestsValidation

The `AuditLogServiceTestsValidation` class provides validation helpers for `AuditLogServiceTests` instances. It includes methods for validating test setup, audit log creation, and audit log properties to ensure test instances are properly configured and audit logging operations produce expected outcomes.

### Public Members

- `Validate(this AuditLogServiceTests value)` - Validates the test instance and its mock dependencies, returning a list of validation problems
- `IsValid(this AuditLogServiceTests value)` - Determines whether the test instance is valid by checking if the validation list is empty
- `EnsureValid(this AuditLogServiceTests value)` - Ensures that the test instance is valid, throwing an exception if validation fails
- `ValidateAuditLog(this AuditLogServiceTests _, AuditLog log, string expectedResourceId, AuditAction expectedAction, bool expectedIsSuccess, string? expectedPerformedBy = null, string expectedResourceType = "ApiKey")` - Validates that an audit log contains expected values including resource ID, action, success status, performer, and resource type
- `ValidateAuditLogCollection(this AuditLogServiceTests _, List<AuditLog> logs, params (string ResourceId, AuditAction Action, bool IsSuccess)[] expectedItems)` - Validates that a collection of audit logs contains expected items with specific resource IDs, actions, and success statuses

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;
using FluentAssertions;

// Create a test instance
var testInstance = new AuditLogServiceTestsValidation();

// Validate the test instance setup
var validationProblems = testInstance.Validate();
testInstance.IsValid().Should().BeTrue();

// Test successful validation
var validResult = new AuditLogServiceTestsValidation();
validResult.EnsureValid(); // Throws if invalid

// Test audit log validation
var auditLog = new AuditLog
{
    Id = Guid.NewGuid(),
    ResourceId = "api-key-123",
    Action = AuditAction.Create,
    IsSuccess = true,
    PerformedBy = "admin@example.com",
    ResourceType = "ApiKey",
    Timestamp = DateTime.UtcNow,
    Metadata = "{\"ipAddress\":\"192.168.1.1\"}"
};

var logProblems = testInstance.ValidateAuditLog(
    auditLog,
    expectedResourceId: "api-key-123",
    expectedAction: AuditAction.Create,
    expectedIsSuccess: true,
    expectedPerformedBy: "admin@example.com",
    expectedResourceType: "ApiKey"
);
logProblems.Should().BeEmpty();

// Test audit log collection validation
var logCollection = new List<AuditLog>
{
    new AuditLog
    {
        Id = Guid.NewGuid(),
        ResourceId = "api-key-1",
        Action = AuditAction.Create,
        IsSuccess = true,
        PerformedBy = "user1@example.com",
        ResourceType = "ApiKey",
        Timestamp = DateTime.UtcNow.AddMinutes(-10)
    },
    new AuditLog
    {
        Id = Guid.NewGuid(),
        ResourceId = "api-key-2",
        Action = AuditAction.Update,
        IsSuccess = false,
        PerformedBy = "user2@example.com",
        ResourceType = "ApiKey",
        Timestamp = DateTime.UtcNow.AddMinutes(-5)
    },
    new AuditLog
    {
        Id = Guid.NewGuid(),
        ResourceId = "api-key-3",
        Action = AuditAction.Delete,
        IsSuccess = true,
        PerformedBy = "admin@example.com",
        ResourceType = "ApiKey",
        Timestamp = DateTime.UtcNow
    }
};

var collectionProblems = testInstance.ValidateAuditLogCollection(
    logCollection,
    ("api-key-1", AuditAction.Create, true),
    ("api-key-2", AuditAction.Update, false),
    ("api-key-3", AuditAction.Delete, true)
);
collectionProblems.Should().BeEmpty();

// Test validation with mismatched values
var invalidLog = new AuditLog
{
    Id = Guid.Empty,
    ResourceId = null,
    Action = (AuditAction)999, // Invalid action
    IsSuccess = false,
    PerformedBy = "",
    ResourceType = null,
    Timestamp = DateTime.MinValue,
    Metadata = null
};

var invalidLogProblems = testInstance.ValidateAuditLog(
    invalidLog,
    expectedResourceId: "api-key-123",
    expectedAction: AuditAction.Create,
    expectedIsSuccess: true,
    expectedPerformedBy: "admin@example.com",
    expectedResourceType: "ApiKey"
);
invalidLogProblems.Should().HaveCount(6); // Multiple validation failures
```

## WebhookManagerTests

The `WebhookManagerTests` class contains unit tests for the `WebhookManager` class, verifying the behavior of webhook registration, retrieval, and validation. It tests scenarios such as successful registration, duplicate registration, invalid URLs, and null or empty parameters.

### Example Usage

```csharp
using api_key_gateway.Tests;
using FluentAssertions;
using System.Threading.Tasks;

// Create a test instance
var testInstance = new WebhookManagerTests();

// Test registering a webhook with valid parameters
await testInstance.RegisterWebhookAsync_Valid_ReturnsIdAndStoresSubscription();

// Test that getting webhooks for a key with no webhooks returns empty
await testInstance.GetWebhooksForKeyAsync_NoWebhooks_ReturnsEmpty();

// Test duplicate registration returns distinct IDs
await testInstance.RegisterWebhookAsync_DuplicateRegistration_ReturnsDistinctIds();
```

## AuthenticationResult

The `AuthenticationResult` record (in `ApiKeyGateway.Domain.Models`) represents the outcome of an authentication attempt against the API key gateway. It exposes a `Success` flag, an optional `AuthenticationFailureReason` populated when authentication fails, and the authenticated `ApiKey` when it succeeds. Static factory methods `SuccessResult` and `FailureResult` provide a clean way to construct results without manually setting properties.

### Public Members

- `Success` (bool) - Indicates whether authentication was successful
- `FailureReason` (AuthenticationFailureReason?) - The failure reason when `Success` is false
- `ApiKey` (ApiKey?) - The authenticated API key when `Success` is true
- `SuccessResult(ApiKey apiKey)` (static) - Creates a successful authentication result with the given API key
- `FailureResult(AuthenticationFailureReason failureReason)` (static) - Creates a failed authentication result with a specific reason

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Build a successful result from an authenticated API key
var apiKey = new ApiKey
{
    Id = "key-123",
    ConsumerId = "consumer-789",
    Name = "Production Key",
    Status = ApiKeyStatus.Active
};

AuthenticationResult success = AuthenticationResult.SuccessResult(apiKey);
if (success.Success)
{
    Console.WriteLine($"Authenticated as {success.ApiKey!.ConsumerId}");
}

// Build a failure result with a specific reason
AuthenticationResult failure = AuthenticationResult.FailureResult(AuthenticationFailureReason.ApiKeyExpired);
if (!failure.Success)
{
    Console.WriteLine($"Authentication failed: {failure.FailureReason}");
}
```

## CollectionExtensionsJsonExtensionsTests

The `CollectionExtensionsJsonExtensionsTests` class contains unit tests for the `CollectionExtensionsJsonExtensions` class, which provides JSON serialization and deserialization extension methods for collections. These tests verify correct behavior for various scenarios including non-empty and empty collections, null inputs, indentation, complex objects, and error conditions such as invalid JSON.

### Public Members

- `CollectionExtensionsJsonExtensionsTests()` - Constructor for the test class.
- `Id` - Gets or sets the identifier for the test instance.
- `Name` - Gets or sets the name for the test instance.
- `ToJson_WithNonEmptyCollection_ReturnsValidJsonString()` - Tests that converting a non-empty collection to JSON produces a valid JSON string containing all items.
- `ToJson_WithEmptyCollection_ReturnsEmptyArrayJson()` - Tests that converting an empty collection to JSON produces an empty JSON array.
- `ToJson_WithIndentedTrue_ReturnsFormattedJson()` - Tests that converting a collection to JSON with indentation produces formatted JSON containing newlines.
- `ToJson_WithNullCollection_ThrowsArgumentNullException()` - Tests that converting a null collection throws an `ArgumentNullException`.
- `ToJson_WithComplexObjectCollection_SerializesCorrectly()` - Tests that converting a collection of complex objects serializes correctly with camelCase property names.
- `FromJson_WithValidJson_ReturnsDeserializedCollection()` - Tests that deserializing a valid JSON array produces a collection equivalent to the expected values.
- `FromJson_WithEmptyArray_ReturnsEmptyCollection()` - Tests that deserializing an empty JSON array produces an empty collection.
- `FromJson_WithNullOrWhitespaceJson_ReturnsNull()` - Tests that deserializing null, empty, or whitespace-only JSON returns null.
- `FromJson_WithInvalidJson_ThrowsJsonException()` - Tests that deserializing invalid JSON throws a `JsonException`.
- `FromJson_WithComplexObjectCollection_DeserializesCorrectly()` - Tests that deserializing a JSON array of complex objects produces the correct collection with expected property values.
- `FromJson_WithEmptyString_ReturnsNull()` - Tests that deserializing an empty string returns null.
- `TryFromJson_WithValidJson_ReturnsTrueAndDeserializesCollection()` - Tests that trying to deserialize a valid JSON array returns true and produces the expected collection.
- `TryFromJson_WithEmptyArray_ReturnsTrueAndEmptyCollection()` - Tests that trying to deserialize an empty JSON array returns true and produces an empty collection.
- `TryFromJson_WithNullJson_ThrowsArgumentNullException()` - Tests that trying to deserialize null JSON throws an `ArgumentNullException`.
- `TryFromJson_WithWhitespaceJson_ReturnsTrueAndNull()` - Tests that trying to deserialize whitespace-only JSON returns true and produces null.
- `TryFromJson_WithEmptyString_ReturnsTrueAndNull()` - Tests that trying to deserialize an empty string returns true and produces null.
- `TryFromJson_WithInvalidJson_ReturnsFalseAndNull()` - Tests that trying to deserialize invalid JSON returns false and produces null.
- `TryFromJson_WithComplexObjectCollection_DeserializesCorrectly()` - Tests that trying to deserialize a JSON array of complex objects returns true and produces the correct collection.

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using FluentAssertions;

var test = new CollectionExtensionsJsonExtensionsTests();
test.Id = 1;
test.Name = "Sample Test";

// Test serializing a non-empty collection to JSON
test.ToJson_WithNonEmptyCollection_ReturnsValidJsonString();

// Test deserializing valid JSON
test.FromJson_WithValidJson_ReturnsDeserializedCollection();

// Test trying to deserialize valid JSON
test.TryFromJson_WithValidJson_ReturnsTrueAndDeserializesCollection();
```

## RetryPolicyBuilderUnitTests

The `RetryPolicyBuilderUnitTests` class contains unit tests for the `RetryPolicyBuilder` class, verifying retry policy construction and behavior. These tests cover builder configuration, retry behavior on various exception types, and edge cases like zero initial delay and maximum retry limits.

### Public Members

- `DefaultValues_WhenNotConfigured_ReturnsExpectedDefaults` - Tests that the default RetryPolicyBuilder has expected initial values.
- `WithMaxRetries_SetsCorrectValue` - Tests that WithMaxRetries sets the correct value.
- `WithInitialDelay_SetsCorrectValue` - Tests that WithInitialDelay sets the correct value.
- `WithBackoffMultiplier_SetsCorrectValue` - Tests that WithBackoffMultiplier sets the correct value.
- `WithMaxDelay_SetsCorrectValue` - Tests that WithMaxDelay sets the correct value.
- `RetryOn_AddsExceptionTypeToRetryList` - Tests that RetryOn adds exception types to the retry list.
- `Build_ReturnsNonNullRetryPolicyFunction` - Tests that Build returns a non-null retry policy function.
- `Build_Policy_SucceedsOnFirstAttemptWithoutRetry` - Tests that the retry policy succeeds on first attempt without retry.
- `Build_Policy_RetriesOnHttpRequestException` - Tests that the retry policy retries on transient exceptions (HttpRequestException).
- `Build_Policy_RetriesOnTimeoutException` - Tests that the retry policy retries on transient exceptions (TimeoutException).
- `Build_Policy_RetriesOnInvalidOperationException` - Tests that the retry policy retries on InvalidOperationException.
- `Build_Policy_RespectsMaxRetriesLimit` - Tests that the retry policy respects MaxRetries limit.
- `Build_Policy_RespectsMaxDelay` - Tests that the retry policy respects MaxDelay when backoff would exceed it.
- `Build_Policy_UsesCustomRetryExceptionTypes` - Tests that custom retry exception types are used when configured.
- `Build_Policy_DoesNotRetryNonRetryableExceptions` - Tests that non-retryable exceptions are not retried.
- `Build_Policy_WithMultipleRetryExceptionTypes` - Tests that RetryOn with multiple exception types works correctly.
- `Build_Policy_WorksWithDifferentReturnTypes` - Tests that the retry policy works with different return types.
- `Build_Policy_WithZeroInitialDelay_WorksCorrectly` - Tests that WithInitialDelay of 0 works correctly.
- `CustomRetryException(string message) : base(message)` - Custom exception type for testing retry behavior.

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using FluentAssertions;

// Create a test instance
var testInstance = new RetryPolicyBuilderUnitTests();

// Test default values
testInstance.DefaultValues_WhenNotConfigured_ReturnsExpectedDefaults();

// Test configuring max retries
testInstance.WithMaxRetries_SetsCorrectValue(5);

// Test building a policy that succeeds on first attempt
await testInstance.Build_Policy_SucceedsOnFirstAttemptWithoutRetry();

// Test building a policy that retries on HTTP request exceptions
await testInstance.Build_Policy_RetriesOnHttpRequestException();

// Test building a policy that respects max retries limit
await testInstance.Build_Policy_RespectsMaxRetriesLimit();
```

## RateLimitCalculationHelperUnitTests

The `RateLimitCalculationHelperUnitTests` class contains unit tests for the `RateLimitCalculationHelper` utility class, which provides core rate limit calculation logic for the API key gateway. These tests verify correct behavior for time window calculations, quota percentage computations, and limit warning thresholds across different time units.

### Public Members

- `GetWindowEnd_SecondUnit_AddsOneSecond` - Tests that getting window end for second unit adds exactly one second
- `GetWindowEnd_MinuteUnit_RoundsUpToNextMinute` - Tests that getting window end for minute unit rounds up to the next minute boundary
- `GetWindowEnd_HourUnit_RoundsUpToNextHour` - Tests that getting window end for hour unit rounds up to the next hour boundary
- `GetWindowEnd_DayUnit_RoundsUpToNextDay` - Tests that getting window end for day unit rounds up to the next day boundary
- `GetWindowEnd_MonthUnit_RoundsUpToNextMonth` - Tests that getting window end for month unit rounds up to the next month boundary
- `GetWindowStart_SecondUnit_SubtractsOneSecond` - Tests that getting window start for second unit subtracts exactly one second
- `GetWindowStart_MinuteUnit_RoundsDownToStartOfMinute` - Tests that getting window start for minute unit rounds down to the start of the minute
- `GetWindowStart_HourUnit_RoundsDownToStartOfHour` - Tests that getting window start for hour unit rounds down to the start of the hour
- `GetWindowStart_DayUnit_RoundsDownToStartOfDay` - Tests that getting window start for day unit rounds down to the start of the day
- `GetWindowStart_MonthUnit_RoundsDownToStartOfMonth` - Tests that getting window start for month unit rounds down to the start of the month
- `GetSecondsUntilAllowed_UnderLimit_ReturnsZero` - Tests that getting seconds until allowed returns zero when under the rate limit
- `GetSecondsUntilAllowed_AtLimit_ReturnsPositiveValue` - Tests that getting seconds until allowed returns a positive value when at the rate limit
- `GetSecondsUntilAllowed_OverLimit_ReturnsPositiveValue` - Tests that getting seconds until allowed returns a positive value when over the rate limit
- `CalculateQuotagePercentage_NormalValues_ReturnsCorrectPercentage` - Tests that calculating quota percentage returns the correct value for normal inputs
- `CalculateQuotagePercentage_UsageEqualsLimit_Returns100` - Tests that calculating quota percentage returns 100 when usage equals the limit
- `CalculateQuotagePercentage_UsageExceedsLimit_CapsAt100` - Tests that calculating quota percentage caps at 100 when usage exceeds the limit
- `CalculateQuotagePercentage_ZeroLimit_ReturnsZero` - Tests that calculating quota percentage returns zero when the limit is zero
- `CalculateQuotagePercentage_NegativeLimit_ReturnsZero` - Tests that calculating quota percentage returns zero when the limit is negative
- `ShouldWarnAboutLimit_Below80_ReturnsFalse` - Tests that the limit warning check returns false when usage is below 80% of the limit
- `ShouldWarnAboutLimit_At80_ReturnsTrue` - Tests that the limit warning check returns true when usage is exactly at 80% of the limit

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using ApiKeyGateway.Domain.Enums;
using System;
using FluentAssertions;

// Create a test instance
var testInstance = new RateLimitCalculationHelperUnitTests();

// Test second unit window calculations
testInstance.GetWindowEnd_SecondUnit_AddsOneSecond();
testInstance.GetWindowStart_SecondUnit_SubtractsOneSecond();

// Test minute unit window calculations
testInstance.GetWindowEnd_MinuteUnit_RoundsUpToNextMinute();
testInstance.GetWindowStart_MinuteUnit_RoundsDownToStartOfMinute();

// Test hour unit window calculations
testInstance.GetWindowEnd_HourUnit_RoundsUpToNextHour();
testInstance.GetWindowStart_HourUnit_RoundsDownToStartOfHour();

// Test day unit window calculations
testInstance.GetWindowEnd_DayUnit_RoundsUpToNextDay();
testInstance.GetWindowStart_DayUnit_RoundsDownToStartOfDay();

// Test month unit window calculations
testInstance.GetWindowEnd_MonthUnit_RoundsUpToNextMonth();
testInstance.GetWindowStart_MonthUnit_RoundsDownToStartOfMonth();

// Test quota percentage calculations
testInstance.CalculateQuotagePercentage_NormalValues_ReturnsCorrectPercentage();
testInstance.CalculateQuotagePercentage_UsageEqualsLimit_Returns100();
testInstance.CalculateQuotagePercentage_UsageExceedsLimit_CapsAt100();
testInstance.CalculateQuotagePercentage_ZeroLimit_ReturnsZero();
testInstance.CalculateQuotagePercentage_NegativeLimit_ReturnsZero();

// Test seconds until allowed calculations
testInstance.GetSecondsUntilAllowed_UnderLimit_ReturnsZero();
testInstance.GetSecondsUntilAllowed_AtLimit_ReturnsPositiveValue();
testInstance.GetSecondsUntilAllowed_OverLimit_ReturnsPositiveValue();

// Test limit warning calculations
testInstance.ShouldWarnAboutLimit_Below80_ReturnsFalse();
testInstance.ShouldWarnAboutLimit_At80_ReturnsTrue();
```

## AuditLogEventHandlerTests

The `AuditLogEventHandlerTests` class contains unit tests for the `AuditLogEventHandler`, `UsageTrackingEventHandler`, and `RateLimitEventHandler` classes. It verifies that events such as API key creation, rotation, disabling, usage, quota exhaustion, and rate limit violations are properly logged and persisted.

### Example Usage

```csharp
using ApiKeyGateway.Tests;
using System.Threading.Tasks;

// Create a test instance
var testInstance = new AuditLogEventHandlerTests();

// Test handling an API key creation event
await testInstance.HandleApiKeyCreatedAsync_HappyPath_LogsAndPersists();

// Test handling a null event for API key creation (expects exception)
await Assert.ThrowsAsync<ArgumentNullException>(
    () => testInstance.HandleApiKeyCreatedAsync_NullEvent_ThrowsArgumentNullException());
```

## StringExtensionsJsonExtensionsTests

The `StringExtensionsJsonExtensionsTests` class contains unit tests for the JSON serialization and deserialization extensions of the `StringExtensions` class. These tests verify the behavior of the `ToJson`, `FromJson`, and `TryFromJson` methods under various conditions, including handling of null, empty, whitespace, and invalid JSON inputs, as well as checking the serialized metadata properties.

### Example Usage

```csharp
using ApiKeyGateway.Tests;

// Create a test instance
var test = new StringExtensionsJsonExtensionsTests();

// Test serializing to compact JSON (default)
test.ToJson_WithDefaultIndentedFalse_ReturnsCompactJson();

// Test serializing to formatted JSON
test.ToJson_WithIndentedTrue_ReturnsFormattedJson();

// Test deserializing valid JSON
test.FromJson_WithValidJson_ReturnsDeserializedMetadata();

// Test deserializing camelCase JSON
test.FromJson_WithCamelCaseJson_ReturnsDeserializedMetadata();

// Test TryFromJson with valid JSON
test.TryFromJson_WithValidJson_ReturnsTrueAndDeserializedMetadata();

// Test TypeName property
test.TypeName_Property_ReturnsCorrectTypeName();

// Test Methods property returns list
test.Methods_Property_ReturnsListOfExtensionMethods();

// Test Methods property returns read-only list
test.Methods_Property_ReturnsReadOnlyList();
```

## BatchOperationHandlerTests

The `BatchOperationHandlerTests` class contains unit tests for the `BatchOperationHandler` class, verifying the behavior of batch operations such as enabling, disabling, rotating, and setting quotas on API keys. It tests scenarios such as empty batches, null operations, single and multiple item batches, batches larger than chunk size, set-quota operations with and without parameters, invalid quota parameters, unknown operation types, mixed success and failure items, exceptions in single items, all items failing, result properties, and item results containing correct API key IDs.

### Public Members

- `BatchOperationHandlerTests()` - Constructor for the test class.
- `ExecuteAsync_EmptyBatch_ThrowsValidationException` - Verifies that executing an empty batch operation throws a validation exception.
- `ExecuteAsync_NullOperation_ThrowsArgumentNullException` - Verifies that executing a null operation throws an argument null exception.
- `ExecuteAsync_SingleItemBatch_ProcessesSuccessfully` - Verifies that a single item batch is processed successfully.
- `ExecuteAsync_MultipleItemsBatch_ProcessesAllSuccessfully` - Verifies that a multiple item batch is processed successfully.
- `ExecuteAsync_BatchLargerThanChunkSize_ProcessesAllItems` - Verifies that a batch larger than the chunk size is processed completely.
- `ExecuteAsync_SetQuotaOperation_ProcessesWithParameters` - Verifies that a set-quota operation with parameters is processed successfully.
- `ExecuteAsync_SetQuotaWithoutParameters_ReturnsFailureForAllItems` - Verifies that a set-quota operation without parameters results in failure for all items.
- `ExecuteAsync_InvalidQuotaParameter_ReturnsFailureForAllItems` - Verifies that a set-quota operation with an invalid quota parameter results in failure for all items.
- `ExecuteAsync_UnknownOperationType_ReturnsFailureForAllItems` - Verifies that an unknown operation type results in failure for all items.
- `ExecuteAsync_MixedSuccessAndFailureItems_ReportsCorrectCounts` - Verifies that a batch with mixed success and failure items reports correct counts.
- `ExecuteAsync_ExceptionInSingleItem_ContinuesProcessingAndReportsError` - Verifies that when an exception occurs in a single item, processing continues and the error is reported.
- `ExecuteAsync_AllItemsFail_ReportsAllFailures` - Verifies that when all items fail, all failures are reported.
- `ExecuteAsync_ResultPropertiesAreCorrectlySet` - Verifies that the result properties (like OperationId, CompletedAt) are correctly set.
- `ExecuteAsync_ItemResultsContainCorrectApiKeyIds` - Verifies that the item results contain the correct API key IDs.

### Example Usage

```csharp
using ApiKeyGateway.Tests.Integration;
using ApiKeyGateway.Domain.Models;
using FluentAssertions;
using System.Threading.Tasks;

// Create a test instance
var testInstance = new BatchOperationHandlerTests();

// Test executing an empty batch operation (expects validation exception)
await Assert.ThrowsAsync<ValidationException>(
    async () => await testInstance.ExecuteAsync_EmptyBatch_ThrowsValidationException());

// Test executing a null operation (expects argument null exception)
await Assert.ThrowsAsync<ArgumentNullException>(
    async () => await testInstance.ExecuteAsync_NullOperation_ThrowsArgumentNullException());

// Test single item batch processes successfully
await testInstance.ExecuteAsync_SingleItemBatch_ProcessesSuccessfully();

// Test multiple items batch processes all successfully
await testInstance.ExecuteAsync_MultipleItemsBatch_ProcessesAllSuccessfully();

// Test batch larger than chunk size processes all items
await testInstance.ExecuteAsync_BatchLargerThanChunkSize_ProcessesAllItems();

// Test set-quota operation with parameters processes successfully
await testInstance.ExecuteAsync_SetQuotaOperation_ProcessesWithParameters();

// Test set-quota operation without parameters returns failure for all items
await testInstance.ExecuteAsync_SetQuotaWithoutParameters_ReturnsFailureForAllItems();

// Test set-quota operation with invalid quota parameter returns failure for all items
await testInstance.ExecuteAsync_InvalidQuotaParameter_ReturnsFailureForAllItems();

// Test unknown operation type returns failure for all items
await testInstance.ExecuteAsync_UnknownOperationType_ReturnsFailureForAllItems();

// Test mixed success and failure items reports correct counts
await testInstance.ExecuteAsync_MixedSuccessAndFailureItems_ReportsCorrectCounts();

// Test exception in single item continues processing and reports error
await testInstance.ExecuteAsync_ExceptionInSingleItem_ContinuesProcessingAndReportsError();

// Test all items fail reports all failures
await testInstance.ExecuteAsync_AllItemsFail_ReportsAllFailures();

// Test result properties are correctly set
await testInstance.ExecuteAsync_ResultPropertiesAreCorrectlySet();

// Test item results contain correct API key IDs
await testInstance.ExecuteAsync_ItemResultsContainCorrectApiKeyIds();
```

## CsvExportHelperUnitTests

The `CsvExportHelperUnitTests` fixture verifies that `CsvExportHelper` handles null and empty inputs, optional headers, CSV quoting and escaping, invariant formatting, and asynchronous stream exports. Its scenarios exercise rows with `Id`, `Name`, nullable `Description`, `IsActive`, and `CreatedAt` fields, including commas, quotes, and newlines in text values.

### Public Members

- `ToCsv_NullInput_ReturnsEmptyString()` - Verifies that a null input produces an empty string.
- `ToCsv_EmptyCollection_ReturnsEmptyString()` - Verifies that an empty collection produces an empty string.
- `ToCsv_SimpleData_IncludesHeadersAndValues()` - Verifies that headers and values are included for ordinary rows.
- `ToCsv_IncludeHeadersFalse_ExcludesHeaders()` - Verifies that headers can be omitted.
- `ToCsv_ValuesWithCommas_AreProperlyQuoted()` - Verifies that values containing commas are quoted.
- `ToCsv_ValuesWithQuotes_AreProperlyEscaped()` - Verifies that embedded quotes are escaped.
- `ToCsv_ValuesWithNewlines_AreProperlyQuoted()` - Verifies that values containing newlines are quoted.
- `ToCsv_VariousDataTypes_UsesInvariantCulture()` - Verifies invariant formatting for integers, booleans, nullable strings, and dates.
- `ExportToCsvAsync_SimpleData_WritesCorrectCsvToStream()` - Verifies that asynchronous export writes headers and rows to a stream.
- `ExportToCsvAsync_EmptyCollection_WritesOnlyHeaders()` - Verifies that asynchronous export of an empty collection writes only headers.
- `ExportToCsvAsync_IncludeHeadersFalse_WritesNoHeaders()` - Verifies that asynchronous export can omit headers.

### Example Usage

```csharp
using ApiKeyGateway.Tests;

var tests = new CsvExportHelperUnitTests();

tests.ToCsv_SimpleData_IncludesHeadersAndValues();
tests.ToCsv_ValuesWithCommas_AreProperlyQuoted();
tests.ToCsv_ValuesWithQuotes_AreProperlyEscaped();
tests.ToCsv_VariousDataTypes_UsesInvariantCulture();

await tests.ExportToCsvAsync_SimpleData_WritesCorrectCsvToStream();
await tests.ExportToCsvAsync_IncludeHeadersFalse_WritesNoHeaders();
```

## XmlExportHelperUnitTests

The `XmlExportHelperUnitTests` fixture verifies that XML export handles single objects, null inputs, empty and populated collections, custom element names, escaped values, and valid XML property names. Its scenarios cover values represented by `Id`, `Name`, nullable `Description`, `IsActive`, `CreatedAt`, `UpdatedAt`, and `Price`, along with property names containing underscores, dots, dashes, and symbols.

### Example Usage

```csharp
using ApiKeyGateway.Tests;

var tests = new XmlExportHelperUnitTests();

tests.ToXml_SingleObject_ReturnsWellFormedXml();
tests.ToXml_SingleObjectWithCustomRootName_UsesCustomName();
tests.ToXml_NullInput_ReturnsEmptyString();
tests.ToXml_EmptyCollection_ReturnsWellFormedXmlWithEmptyRoot();
tests.ToXml_CollectionWithItems_ReturnsWellFormedXmlWithItems();
tests.ToXml_CollectionWithCustomNames_UsesCustomNames();
tests.ToXml_SpecialCharacters_AreProperlyEscaped();
tests.ToXml_PropertyNamesWithSpecialChars_AreConvertedToValidXmlNames();
```

## RequestCoalescingServiceUnitTests

The `RequestCoalescingServiceUnitTests` fixture verifies request-key and operation validation, result sharing for identical requests, independent handling of different keys, and exception and cancellation behavior. It also covers request metrics while work is idle, active, or completed, as well as cancellation of pending requests when the service is disposed.

### Example Usage

```csharp
using ApiKeyGateway.Tests;

var tests = new RequestCoalescingServiceUnitTests();

tests.ExecuteAsync_NullRequestKey_ThrowsArgumentException();
tests.ExecuteAsync_EmptyRequestKey_ThrowsArgumentException();
tests.ExecuteAsync_WhitespaceRequestKey_ThrowsArgumentException();
tests.ExecuteAsync_NullOperation_ThrowsArgumentNullException();
tests.GetMetrics_NoRequests_ReturnsZeroValues();

await tests.ExecuteAsync_NoCoalescing_ReturnsOperationResult();
await tests.ExecuteAsync_IdenticalRequests_CoalescesAndReturnsSameResult();
await tests.ExecuteAsync_OperationThrowsException_PropagatesToAllCallers();
await tests.ExecuteAsync_FollowerCancelled_DoesNotAffectLeader();
await tests.GetMetrics_AfterRequests_ReturnsCorrectValues();
await tests.GetMetrics_DuringRequests_ReturnsCorrectActiveCount();
await tests.Dispose_CancelsPendingRequests();
await tests.ExecuteAsync_MultipleDifferentKeys_HandlesIndependently();
```

## EventPublisherTests

The `EventPublisherTests` fixture verifies that `InMemoryEventPublisher` dispatches events to the correct subscribers, including multiple registrations and subscribers for different event types. It also checks that publishing without subscribers is safe and that one failing subscriber does not prevent later subscribers from running.

### Example Usage

```csharp
using ApiKeyGateway.Tests;

var tests = new EventPublisherTests();

await tests.PublishAsync_WithZeroSubscribers_DoesNotThrowAndLogsDebug();
await tests.PublishAsync_WithOneSubscriber_InvokesSubscriberOnce();
await tests.PublishAsync_WithMultipleSubscribers_AllSubscribersInvoked();
await tests.PublishAsync_SubscriberThrows_ContinuesToNextSubscriber();
await tests.PublishAsync_DifferentEventTypes_OnlySubscribersForThatTypeInvoked();
await tests.PublishAsync_SameSubscriberRegisteredMultipleTimes_InvokedMultipleTimes();
await tests.Subscribe_RegistersHandlerForEventType();
```

## DateTimeExtensionsTests

The `DateTimeExtensionsTests` fixture verifies the date-boundary helpers for the start and end of a day, week, and month, including leap-year and calendar-edge cases. It also checks whether past, future, and current dates are classified correctly by `IsInPast`.

### Example Usage

```csharp
using ApiKeyGateway.Tests;

var tests = new DateTimeExtensionsTests();

tests.StartOfDay_ShouldReturnMidnightOfSameDay();
tests.EndOfDay_ShouldReturnEndOfDay();
tests.StartOfWeek_ShouldReturnSundayForFriday();
tests.StartOfMonth_ShouldReturnFirstDayOfMonth();
tests.EndOfMonth_ShouldReturnLastMomentForFebruaryLeapYear();
tests.IsInPast_ShouldReturnTrueForPastDate();
tests.IsInPast_ShouldReturnFalseForFutureDate();
```

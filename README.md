// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

## ConfigurationException

Thrown when gateway configuration is invalid or incomplete. This exception provides information about the setting that caused the error.

### Example Usage

```csharp
try
{
  // Simulate a request with invalid configuration
  await DoSomethingAsync();
}
catch (ConfigurationException ex)
{
  Console.WriteLine($"Configuration error for setting {ex.Setting}.");
  Console.WriteLine($"Error message: {ex.Message}");
}
```

## UsageQuota

The `UsageQuota` class defines a hard usage quota for an API key that resets on a calendar basis (daily or monthly). Unlike rate limits, quotas enforce a total request cap over a billing-style period rather than a rolling window. It tracks request usage and provides methods to check quota status, reset periods, and record requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new usage quota for an API key
var quota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_123",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = QuotaPeriod.Daily,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

// Record a request
quota.RecordRequest();

// Check remaining requests
Console.WriteLine($"Remaining requests: {quota.RemainingRequests}"); // 999

// Check if quota is exceeded
if (quota.IsExceeded)
{
    Console.WriteLine("Quota exceeded!");
}

// Get the period end time
var periodEnd = quota.GetPeriodEndUtc();
Console.WriteLine($"Period ends at: {periodEnd}");

// Reset the period (e.g., at midnight)
quota.ResetPeriod(DateTime.UtcNow);

// Static method to get period start
var periodStart = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Monthly);
Console.WriteLine($"Current period started: {periodStart}");
```

## InvalidApiKeyException

Thrown when an API key is invalid, expired, or disabled. This exception provides information about the invalid API key, including its hash, the timestamp when the exception occurred, and whether the key was expired.

### Example Usage

```csharp
try
{
  // Simulate a request with an invalid API key
  await DoSomethingAsync();
}
catch (InvalidApiKeyException ex)
{
  Console.WriteLine($"Invalid API key: {ex.ApiKeyHash}");
  Console.WriteLine($"Exception occurred at: {ex.OccurredAt}");
  Console.WriteLine($"Key was expired: {ex.IsExpired}");
}
```

## UsageQuota

The `UsageQuota` class defines a hard usage quota for an API key that resets on a calendar basis (daily or monthly). Unlike rate limits, quotas enforce a total request cap over a billing-style period rather than a rolling window. It tracks request usage and provides methods to check quota status, reset periods, and record requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new usage quota for an API key
var quota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_123",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = QuotaPeriod.Daily,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

// Record a request
quota.RecordRequest();

// Check remaining requests
Console.WriteLine($"Remaining requests: {quota.RemainingRequests}"); // 999

// Check if quota is exceeded
if (quota.IsExceeded)
{
    Console.WriteLine("Quota exceeded!");
}

// Get the period end time
var periodEnd = quota.GetPeriodEndUtc();
Console.WriteLine($"Period ends at: {periodEnd}");

// Reset the period (e.g., at midnight)
quota.ResetPeriod(DateTime.UtcNow);

// Static method to get period start
var periodStart = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Monthly);
Console.WriteLine($"Current period started: {periodStart}");
```

## IWebhookHandler

The `IWebhookHandler` interface manages webhook subscriptions and event delivery with retry logic and HMAC signing. It supports registering webhooks for specific event types, tracking delivery statistics, and ensuring reliable delivery through exponential backoff.

### Example Usage

```csharp
using ApiKeyGateway.Integration;
using ApiKeyGateway.Events;

var webhookHandler = new WebhookHandler();

// Register a webhook subscription
var subscriptionId = await webhookHandler.RegisterWebhookAsync(
  url: "https://example.com/webhook-endpoint",
  eventTypes: new[] { "ApiKeyCreated", "QuotaExceeded" },
  secret: "my-webhook-secret");

// Create a sample event to deliver
var sampleEvent = new SampleEvent
{
  EventId = Guid.NewGuid(),
  Timestamp = DateTime.UtcNow,
  ApiKeyId = "key_123"
};

// Deliver the event to all matching subscriptions
await webhookHandler.DeliverWebhookAsync(sampleEvent);

// Access subscription metadata
var subscription = webhookHandler.GetSubscription(subscriptionId); // Hypothetical helper method
Console.WriteLine($"Webhook {subscription.Id} has {subscription.TotalDeliveries} successful deliveries");
```

Where `SampleEvent` is a custom event implementing `ApiKeyEvent`:

```csharp
public class SampleEvent : ApiKeyEvent
{
  public Guid EventId { get; set; }
  public DateTime Timestamp { get; set; }
  public string ApiKeyId { get; set; }
}
```

## UsageQuota

The `UsageQuota` class defines a hard usage quota for an API key that resets on a calendar basis (daily or monthly). Unlike rate limits, quotas enforce a total request cap over a billing-style period rather than a rolling window. It tracks request usage and provides methods to check quota status, reset periods, and record requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new usage quota for an API key
var quota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_123",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = QuotaPeriod.Daily,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

// Record a request
quota.RecordRequest();

// Check remaining requests
Console.WriteLine($"Remaining requests: {quota.RemainingRequests}"); // 999

// Check if quota is exceeded
if (quota.IsExceeded)
{
    Console.WriteLine("Quota exceeded!");
}

// Get the period end time
var periodEnd = quota.GetPeriodEndUtc();
Console.WriteLine($"Period ends at: {periodEnd}");

// Reset the period (e.g., at midnight)
quota.ResetPeriod(DateTime.UtcNow);

// Static method to get period start
var periodStart = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Monthly);
Console.WriteLine($"Current period started: {periodStart}");
```

## IBatchOperationHandler

The `IBatchOperationHandler` interface enables bulk management of API keys by executing operations like disabling/enabling keys, setting quotas, or rotating keys in a single transaction. It tracks success/failure counts and provides detailed per-key results.

### Example Usage

```csharp
using ApiKeyGateway.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

var batchHandler = new BatchOperationHandler();

// Create a batch operation to disable multiple API keys
var operation = new BatchOperation
{
  OperationType = "disable",
  ApiKeyIds = new List<string> { "key_001", "key_002", "key_003" }
};

// Execute the batch operation
var result = await batchHandler.ExecuteAsync(operation);

// Process results
Console.WriteLine($"Operation {result.OperationId} completed: {result.SuccessCount} succeeded, {result.FailureCount} failed");
foreach (var item in result.Items)
{
  Console.WriteLine($"Key {item.ApiKeyId}: {(item.Success ? "Success" : $"Error: {item.ErrorMessage}")}");
}
```

## UsageQuota

The `UsageQuota` class defines a hard usage quota for an API key that resets on a calendar basis (daily or monthly). Unlike rate limits, quotas enforce a total request cap over a billing-style period rather than a rolling window. It tracks request usage and provides methods to check quota status, reset periods, and record requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new usage quota for an API key
var quota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_123",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = QuotaPeriod.Daily,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

// Record a request
quota.RecordRequest();

// Check remaining requests
Console.WriteLine($"Remaining requests: {quota.RemainingRequests}"); // 999

// Check if quota is exceeded
if (quota.IsExceeded)
{
    Console.WriteLine("Quota exceeded!");
}

// Get the period end time
var periodEnd = quota.GetPeriodEndUtc();
Console.WriteLine($"Period ends at: {periodEnd}");

// Reset the period (e.g., at midnight)
quota.ResetPeriod(DateTime.UtcNow);

// Static method to get period start
var periodStart = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Monthly);
Console.WriteLine($"Current period started: {periodStart}");
```

## CollectionExtensions

The `CollectionExtensions` class provides a set of extension methods for common collection operations, such as pagination, grouping, and filtering.

### Example Usage

```csharp
using ApiKeyGateway.Extensions;

var numbers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// Paginate the collection
var paginatedNumbers = numbers.Paginate(2, 3);
foreach (var number in paginatedNumbers)
{
  Console.WriteLine(number);
}

// Check if the collection is empty
Console.WriteLine(numbers.IsEmpty()); // Output: False

// Count occurrences of each number
var counts = numbers.CountBy(x => x % 2);
foreach (var pair in counts)
{
  Console.WriteLine($"{pair.Key}: {pair.Value}");
}

// Batch the collection
var batchedNumbers = numbers.Batch(3);
foreach (var batch in batchedNumbers)
{
  Console.WriteLine(string.Join(", ", batch));
}

// Get distinct numbers
var distinctNumbers = numbers.DistinctBy(x => x);
foreach (var number in distinctNumbers)
{
  Console.WriteLine(number);
}

// Safely execute an action for each number
numbers.ForEachSafe(x => Console.WriteLine(x));
```

## StringExtensions

The `StringExtensions` class provides a set of extension methods for common string operations, such as truncation, slug generation, and validation.

### Example Usage

```csharp
using ApiKeyGateway.Extensions;

var originalString = "This is a very long string that needs to be truncated.";

// Truncate the string to a maximum length of 20 characters
var truncatedString = originalString.Truncate(20);
Console.WriteLine(truncatedString); // Output: "This is a very long..."

// Truncate the string with an ellipsis suffix
var truncatedWithEllipsis = originalString.TruncateWithEllipsis(20);
Console.WriteLine(truncatedWithEllipsis); // Output: "This is a very long... "

// Check if the string contains any of the provided values
var containsAny = originalString.ContainsAny("very", "long");
Console.WriteLine(containsAny); // Output: True

// Check if the string starts with any of the provided values
var startsWithAny = originalString.StartsWithAny("This", "is");
Console.WriteLine(startsWithAny); // Output: True

// Generate a slug from the original string
var slug = originalString.ToSlug();
Console.WriteLine(slug); // Output: "this-is-a-very-long-string-that-needs-to-be-truncated"

// Capitalize the first character of the string
var capitalizedString = originalString.CapitalizeFirst();
Console.WriteLine(capitalizedString); // Output: "This is a very long string that needs to be truncated."

// Convert the string to a list of substrings
var list = originalString.ToList(',');
Console.WriteLine(string.Join(", ", list)); // Output: "This, is, a, very, long, string, that, needs, to, be, truncated."

// Check if the string is numeric
var isNumeric = originalString.IsNumeric();
Console.WriteLine(isNumeric); // Output: False

// Safely parse the string to an integer
var parsedInt = originalString.TryParseInt();
Console.WriteLine(parsedInt); // Output: null

// Safely parse the string to a long
var parsedLong = originalString.TryParseLong();
Console.WriteLine(parsedLong); // Output: null
```

## RateLimit

The `RateLimit` class defines a rolling window rate limit for an API key that tracks request counts and enforces limits based on time windows. It supports different rate limit units (seconds, minutes, hours) and provides methods to check if requests can be processed, record requests, and reset the rate limit window.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new rate limit for an API key with 100 requests per minute
var rateLimit = new RateLimit
{
    Id = "rate_limit_prod_001",
    ApiKeyId = "key_prod_001",
    RequestsPerUnit = 100,
    Unit = RateLimitUnit.Minute,
    IsEnabled = true,
    CreatedAt = DateTime.UtcNow,
    LastResetAt = null,
    CurrentRequestCount = 0
};

// Check if a request can be processed
if (rateLimit.CanProcessRequest())
{
    Console.WriteLine("Request can be processed - rate limit not exceeded.");
    
    // Record the request
    rateLimit.RecordRequest();
    Console.WriteLine($"Current request count: {rateLimit.CurrentRequestCount}");
    Console.WriteLine($"Window in seconds: {rateLimit.GetWindowInSeconds()}");
}
else
{
    Console.WriteLine("Rate limit exceeded - cannot process request.");
}

// Check window information
Console.WriteLine($"Rate limit: {rateLimit.RequestsPerUnit} requests per {rateLimit.Unit}");
Console.WriteLine($"Current count: {rateLimit.CurrentRequestCount}");

// Reset the window (e.g., at the start of a new minute)
rateLimit.ResetWindow();
Console.WriteLine($"Window reset at: {rateLimit.LastResetAt}");
Console.WriteLine($"Current count after reset: {rateLimit.CurrentRequestCount}");

// Create a rate limit with 10 requests per second for high-frequency endpoints
var highFrequencyLimit = new RateLimit
{
    Id = "rate_limit_high_freq",
    ApiKeyId = "key_high_freq",
    RequestsPerUnit = 10,
    Unit = RateLimitUnit.Second,
    IsEnabled = true,
    CreatedAt = DateTime.UtcNow
};
```

## RateLimitExceededException

Thrown when a request exceeds the configured rate limit for an API key.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Exceptions;

try
{
  // Simulate a request that exceeds the rate limit
  await DoSomethingAsync();
}
catch (RateLimitExceededException ex)
{
  Console.WriteLine($"Rate limit exceeded for API key {ex.ApiKeyId} with limit {ex.Limit} and window {ex.WindowInSeconds} seconds.");
  Console.WriteLine($"Retry after {ex.RetryAfter?.ToString("yyyy-MM-dd HH:mm:ss") ?? "never"}");
}
```

## UsageQuota

The `UsageQuota` class defines a hard usage quota for an API key that resets on a calendar basis (daily or monthly). Unlike rate limits, quotas enforce a total request cap over a billing-style period rather than a rolling window. It tracks request usage and provides methods to check quota status, reset periods, and record requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new usage quota for an API key
var quota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_123",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = QuotaPeriod.Daily,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

// Record a request
quota.RecordRequest();

// Check remaining requests
Console.WriteLine($"Remaining requests: {quota.RemainingRequests}"); // 999

// Check if quota is exceeded
if (quota.IsExceeded)
{
    Console.WriteLine("Quota exceeded!");
}

// Get the period end time
var periodEnd = quota.GetPeriodEndUtc();
Console.WriteLine($"Period ends at: {periodEnd}");

// Reset the period (e.g., at midnight)
quota.ResetPeriod(DateTime.UtcNow);

// Static method to get period start
var periodStart = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Monthly);
Console.WriteLine($"Current period started: {periodStart}");
```

## DataAccessException

Thrown when database or repository operations fail.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Exceptions;

try
{
  // Simulate a database operation that fails
  await DatabaseOperationAsync();
}
catch (DataAccessException ex)
{
  Console.WriteLine($"Database operation failed: {ex.Message}");
  Console.WriteLine($"Operation: {ex.Operation}");
  Console.WriteLine($"Entity: {ex.Entity}");
}
```

## UsageQuota

The `UsageQuota` class defines a hard usage quota for an API key that resets on a calendar basis (daily or monthly). Unlike rate limits, quotas enforce a total request cap over a billing-style period rather than a rolling window. It tracks request usage and provides methods to check quota status, reset periods, and record requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new usage quota for an API key
var quota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_123",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = QuotaPeriod.Daily,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

// Record a request
quota.RecordRequest();

// Check remaining requests
Console.WriteLine($"Remaining requests: {quota.RemainingRequests}"); // 999

// Check if quota is exceeded
if (quota.IsExceeded)
{
    Console.WriteLine("Quota exceeded!");
}

// Get the period end time
var periodEnd = quota.GetPeriodEndUtc();
Console.WriteLine($"Period ends at: {periodEnd}");

// Reset the period (e.g., at midnight)
quota.ResetPeriod(DateTime.UtcNow);

// Static method to get period start
var periodStart = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Monthly);
Console.WriteLine($"Current period started: {periodStart}");
```

## UnauthorizedAccessException

Thrown when authentication fails or credentials are missing.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Exceptions;

try
{
  // Simulate a request with invalid credentials
  await AuthenticateAsync();
}
catch (UnauthorizedAccessException ex)
{
  Console.WriteLine($"Authentication failed: {ex.Message}");
  Console.WriteLine($"Reason: {ex.Reason}");
  Console.WriteLine($"Source IP: {ex.SourceIp}");
}
```

## UsageQuota

The `UsageQuota` class defines a hard usage quota for an API key that resets on a calendar basis (daily or monthly). Unlike rate limits, quotas enforce a total request cap over a billing-style period rather than a rolling window. It tracks request usage and provides methods to check quota status, reset periods, and record requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new usage quota for an API key
var quota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_123",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = QuotaPeriod.Daily,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

// Record a request
quota.RecordRequest();

// Check remaining requests
Console.WriteLine($"Remaining requests: {quota.RemainingRequests}"); // 999

// Check if quota is exceeded
if (quota.IsExceeded)
{
    Console.WriteLine("Quota exceeded!");
}

// Get the period end time
var periodEnd = quota.GetPeriodEndUtc();
Console.WriteLine($"Period ends at: {periodEnd}");

// Reset the period (e.g., at midnight)
quota.ResetPeriod(DateTime.UtcNow);

// Static method to get period start
var periodStart = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Monthly);
Console.WriteLine($"Current period started: {periodStart}");
```

## ValidationException
Thrown when validation of input parameters fails. This exception provides information about the failed validation, including the name of the parameter that failed, the attempted value, and a collection of validation error messages.

### Example Usage

```csharp
try
{
  // Simulate a request with invalid input parameters
  await DoSomethingAsync();
}
catch (ValidationException ex)
{
  Console.WriteLine($"Validation failed for parameter {ex.ParameterName} with attempted value {ex.AttemptedValue}.");
  Console.WriteLine($"Validation errors: {string.Join(", ", ex.ValidationErrors ?? new string[0])}");
}
```

## UsageQuota

The `UsageQuota` class defines a hard usage quota for an API key that resets on a calendar basis (daily or monthly). Unlike rate limits, quotas enforce a total request cap over a billing-style period rather than a rolling window. It tracks request usage and provides methods to check quota status, reset periods, and record requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new usage quota for an API key
var quota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_123",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = QuotaPeriod.Daily,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

// Record a request
quota.RecordRequest();

// Check remaining requests
Console.WriteLine($"Remaining requests: {quota.RemainingRequests}"); // 999

// Check if quota is exceeded
if (quota.IsExceeded)
{
    Console.WriteLine("Quota exceeded!");
}

// Get the period end time
var periodEnd = quota.GetPeriodEndUtc();
Console.WriteLine($"Period ends at: {periodEnd}");

// Reset the period (e.g., at midnight)
quota.ResetPeriod(DateTime.UtcNow);

// Static method to get period start
var periodStart = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Monthly);
Console.WriteLine($"Current period started: {periodStart}");
```

## ApiKeyGatewayException

Base exception class for all api-key-gateway specific exceptions. This exception provides a standardized way to handle gateway-specific errors with an optional error code and timestamp of when the exception occurred.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Exceptions;

try
{
  // Simulate a gateway operation that fails
  await GatewayOperationAsync();
}
catch (ApiKeyGatewayException ex)
{
  Console.WriteLine($"Gateway exception occurred at {ex.OccurredAt:yyyy-MM-dd HH:mm:ss}.");
  if (ex.ErrorCode != null)
  {
    Console.WriteLine($"Error code: {ex.ErrorCode}");
  }
  Console.WriteLine($"Message: {ex.Message}");
}

// Example with error code
try
{
  throw new ApiKeyGatewayException("Invalid gateway configuration", "GATEWAY_CONFIG_ERROR");
}
catch (ApiKeyGatewayException ex)
{
  Console.WriteLine($"Gateway error: {ex.ErrorCode} - {ex.Message}");
  Console.WriteLine($"Occurred at: {ex.OccurredAt}");
}

// Example wrapping another exception
try
{
  await GatewayOperationAsync();
}
catch (Exception ex)
{
  throw new ApiKeyGatewayException("Gateway operation failed", "GATEWAY_OPERATION_FAILED", ex);
}
```

## UsageQuota

The `UsageQuota` class defines a hard usage quota for an API key that resets on a calendar basis (daily or monthly). Unlike rate limits, quotas enforce a total request cap over a billing-style period rather than a rolling window. It tracks request usage and provides methods to check quota status, reset periods, and record requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new usage quota for an API key
var quota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_123",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = QuotaPeriod.Daily,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

// Record a request
quota.RecordRequest();

// Check remaining requests
Console.WriteLine($"Remaining requests: {quota.RemainingRequests}"); // 999

// Check if quota is exceeded
if (quota.IsExceeded)
{
    Console.WriteLine("Quota exceeded!");
}

// Get the period end time
var periodEnd = quota.GetPeriodEndUtc();
Console.WriteLine($"Period ends at: {periodEnd}");

// Reset the period (e.g., at midnight)
quota.ResetPeriod(DateTime.UtcNow);

// Static method to get period start
var periodStart = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Monthly);
Console.WriteLine($"Current period started: {periodStart}");
```

## KeyStoreUnavailableException

Thrown when the key store (database or cache) is temporarily unreachable and the gateway cannot verify API key authenticity. This exception provides information about the operation that failed, if available.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Exceptions;



try
{
    // Simulate a key verification operation
    await VerifyApiKeyAsync("test-key-123");
}
catch (KeyStoreUnavailableException ex)
{
    Console.WriteLine($"Key store unavailable: {ex.Message}");
    if (ex.Operation != null)
    {
        Console.WriteLine($"Failed operation: {ex.Operation}");
    }
}

// Example with operation context
try
{
    await LoadKeyFromStoreAsync("key_abc123");
}
catch (KeyStoreUnavailableException ex) when (ex.Operation == "LoadKeyFromStore")
{
    Console.WriteLine($"Failed to load key from store during {ex.Operation}: {ex.Message}");
    // Implement retry logic or fallback behavior
}

// Example wrapping the underlying exception
try
{
    await Database.GetApiKeyAsync("key_123");
}
catch (Exception ex)
{
    throw new KeyStoreUnavailableException(
        "Database connection failed while retrieving API key",
        "GetApiKeyAsync",
        ex
    );
}
```

## UsageQuota

The `UsageQuota` class defines a hard usage quota for an API key that resets on a calendar basis (daily or monthly). Unlike rate limits, quotas enforce a total request cap over a billing-style period rather than a rolling window. It tracks request usage and provides methods to check quota status, reset periods, and record requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new usage quota for an API key
var quota = new UsageQuota
{
    Id = "quota_001",
    ApiKeyId = "key_123",
    QuotaLimit = 1000,
    IsEnabled = true,
    Period = QuotaPeriod.Daily,
    PeriodStartAt = DateTime.UtcNow,
    CurrentUsage = 0
};

// Record a request
quota.RecordRequest();

// Check remaining requests
Console.WriteLine($"Remaining requests: {quota.RemainingRequests}"); // 999

// Check if quota is exceeded
if (quota.IsExceeded)
{
    Console.WriteLine("Quota exceeded!");
}

// Get the period end time
var periodEnd = quota.GetPeriodEndUtc();
Console.WriteLine($"Period ends at: {periodEnd}");

// Reset the period (e.g., at midnight)
quota.ResetPeriod(DateTime.UtcNow);

// Static method to get period start
var periodStart = UsageQuota.GetPeriodStart(DateTime.UtcNow, QuotaPeriod.Monthly);
Console.WriteLine($"Current period started: {periodStart}");
```

## ApiKey

The `ApiKey` class represents an API key entity used for authentication and authorization in the API key gateway. It tracks usage metrics, expiration status, IP restrictions, and provides methods to manage key lifecycle and validate access.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a new API key for a consumer
var apiKey = new ApiKey
{
    Id = "key_prod_001",
    ConsumerId = "consumer_001",
    Name = "Production API Key",
    KeyHash = "hashed_value_here",
    Prefix = "prod_abc123",
    Status = ApiKeyStatus.Active,
    ExpiresAt = DateTime.UtcNow.AddYears(1),
    Description = "Production API key for external partner",
    IpWhitelist = "192.168.1.100,192.168.1.101",
    RateLimitId = "rate_limit_prod",
    AllowedScopes = "/api/v1/metrics,/api/v1/stats",
    Metadata = new Dictionary<string, string>
    {
        ["partnerId"] = "partner_001",
        ["environment"] = "production"
    }
};

// Check if the key can be used
if (apiKey.CanBeUsed())
{
    Console.WriteLine("API key is valid and can be used for authentication");
}

// Record a successful usage
apiKey.RecordUsage(bytes: 1024);
Console.WriteLine($"Request count: {apiKey.RequestCount}");
Console.WriteLine($"Bytes transferred: {apiKey.BytesTransferred}");

// Check if IP is allowed
bool isIpAllowed = apiKey.IsIpAllowed("192.168.1.100");
Console.WriteLine($"IP allowed: {isIpAllowed}");

// Check if scope is allowed
bool isScopeAllowed = apiKey.IsScopeAllowed("/api/v1/metrics/usage");
Console.WriteLine($"Scope allowed: {isScopeAllowed}");

// Disable the key
apiKey.Disable();
Console.WriteLine($"Key disabled at: {apiKey.DisabledAt}");

// Create a minimal API key
var minimalKey = new ApiKey
{
    Id = "key_minimal",
    ConsumerId = "consumer_001",
    Name = "Minimal Key",
    KeyHash = "hashed_value",
    Prefix = "min_abc"
};
```

## ApiKeyConsumer

The `ApiKeyConsumer` class represents an API consumer - a user or service using the API key gateway. It tracks consumer metadata, organization details, tier information, and provides methods to manage consumer lifecycle and activity.



### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using System;
using System.Collections.Generic;

// Create a new API consumer
var consumer = new ApiKeyConsumer
{
    Id = "consumer_001",
    Name = "Acme Corporation",
    Email = "api@acme-corp.com",
    Organization = "Acme Corp",
    Tier = "enterprise",
    ContactPerson = "John Doe",
    Notes = "Premium customer with custom rate limits",
    WebhookUrl = "https://acme-corp.com/webhook/api-events",
    CustomProperties = new Dictionary<string, string>
    {
        ["department"] = "engineering",
        ["contractId"] = "CTR-2024-001",
        ["maxConcurrentRequests"] = "100"
    }
};

// Access consumer properties
Console.WriteLine($"Consumer: {consumer.Name} ({consumer.Email})");
Console.WriteLine($"Organization: {consumer.Organization}");
Console.WriteLine($"Tier: {consumer.Tier}");
Console.WriteLine($"Status: {(consumer.IsActive ? "Active" : "Inactive")}");
Console.WriteLine($"Created: {consumer.CreatedAt:yyyy-MM-dd}");
Console.WriteLine($"Total API keys: {consumer.TotalApiKeys}");

// Check if consumer is valid
if (consumer.IsValid())
{
    Console.WriteLine("Consumer information is valid.");
}

// Update last activity
consumer.UpdateLastActivity();
Console.WriteLine($"Last activity: {consumer.LastActivityAt}");

// Deactivate the consumer (e.g., when contract expires)
consumer.Deactivate();
Console.WriteLine($"Consumer deactivated at: {consumer.InactiveSince}");

// Reactivate the consumer (e.g., when contract renewed)
consumer.Activate();
Console.WriteLine($"Consumer reactivated. InactiveSince is now: {consumer.InactiveSince}");

// Access custom properties
var department = consumer.CustomProperties.GetValueOrDefault("department");
Console.WriteLine($"Department: {department}");

// Create a minimal consumer
var minimalConsumer = new ApiKeyConsumer
{
    Id = "consumer_minimal",
    Name = "Test User",
    Email = "test@example.com",
    Organization = "Test Org"
};
```

## ApiEndpoint

The `ApiEndpoint` class represents a configurable API endpoint that can be protected and routed through the gateway. It defines routing rules, access controls, caching behavior, and timeout settings for API requests. Endpoints can be configured to require API keys, restrict access to specific consumers, and apply custom headers or caching policies.



### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using System;
using System.Collections.Generic;

// Create a new API endpoint for a public service
var endpoint = new ApiEndpoint
{
    Id = "endpoint_public_api",
    Path = "/api/v1/public/data",
    Method = "GET",
    TargetUrl = "https://api.example.com/v1/data",
    Description = "Public data endpoint for external partners",
    RequireApiKey = true,
    TimeoutMs = 15000,
    MaxPayloadBytes = 1048576,
    CacheEnabled = true,
    CacheTtlSeconds = 600,
    AllowedConsumers = new List<string> { "consumer_001", "consumer_002" },
    Headers = new Dictionary<string, string>
    {
        ["X-Service-Version"] = "1.2.3",
        ["X-Environment"] = "production"
    }
};

// Check if endpoint is accessible
if (endpoint.IsAccessible())
{
    Console.WriteLine("Endpoint is accessible and ready to route requests.");
}

// Check if a specific consumer is allowed
bool isAllowed = endpoint.IsConsumerAllowed("consumer_001");
Console.WriteLine($"Consumer allowed: {isAllowed}");

// Validate payload size
bool isValidSize = endpoint.IsPayloadSizeValid(512000);
Console.WriteLine($"Payload size valid: {isValidSize}");

// Get the endpoint signature for logging
string signature = endpoint.GetEndpointSignature();
Console.WriteLine($"Endpoint signature: {signature}");

// Create a global endpoint without consumer restrictions
var globalEndpoint = new ApiEndpoint
{
    Id = "endpoint_global_health",
    Path = "/health",
    Method = "GET",
    TargetUrl = "https://status.example.com/health",
    Description = "Health check endpoint",
    RequireApiKey = false,
    TimeoutMs = 5000,
    MaxPayloadBytes = 10240,
    CacheEnabled = false
};

// Access endpoint properties
Console.WriteLine($"Endpoint ID: {endpoint.Id}");
Console.WriteLine($"Path: {endpoint.Path}");
Console.WriteLine($"Method: {endpoint.Method}");
Console.WriteLine($"Target URL: {endpoint.TargetUrl}");
Console.WriteLine($"Is Active: {endpoint.IsActive}");
Console.WriteLine($"Created At: {endpoint.CreatedAt:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Require API Key: {endpoint.RequireApiKey}");
Console.WriteLine($"Timeout (ms): {endpoint.TimeoutMs}");
Console.WriteLine($"Max Payload (bytes): {endpoint.MaxPayloadBytes}");
Console.WriteLine($"Description: {endpoint.Description}");
Console.WriteLine($"Cache Enabled: {endpoint.CacheEnabled}");
Console.WriteLine($"Cache TTL (seconds): {endpoint.CacheTtlSeconds}");
```

## TransformationRule

The `TransformationRule` class defines a single step in the API request transformation pipeline. Rules are evaluated in ascending `Priority` order and can mutate headers, query parameters, the request path, or body. Rules support two implementation types: built-in actions (like adding/removing headers or rewriting paths) or custom Lua scripts for advanced transformations. Rules can target specific API keys, specific consumers, or apply globally to all requests.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create a global transformation rule that adds a custom header to all requests
var globalRule = new TransformationRule
{
    Id = "rule_global_add_header",
    Name = "Add Global Header",
    Description = "Adds X-Request-Source header to all API requests",
    Scope = TransformationScope.Global,
    Type = TransformationRuleType.BuiltIn,
    Action = BuiltInAction.AddHeader,
    Parameters = new Dictionary<string, string>
    {
        ["HeaderName"] = "X-Request-Source",
        ["HeaderValue"] = "api-key-gateway"
    },
    Priority = 10,
    IsEnabled = true,
    CreatedBy = "admin@example.com"
};

// Create a consumer-specific rule that rewrites the request path
var consumerRule = new TransformationRule
{
    Id = "rule_consumer_rewrite_path",
    Name = "Rewrite Consumer Path",
    Description = "Rewrites path for consumer-specific API versioning",
    Scope = TransformationScope.Consumer,
    ConsumerId = "consumer_001",
    Type = TransformationRuleType.BuiltIn,
    Action = BuiltInAction.RewritePath,
    Parameters = new Dictionary<string, string>
    {
        ["PathTemplate"] = "/v2/{path}"
    },
    Priority = 20,
    IsEnabled = true
};

// Create an API key-specific rule that removes a query parameter
var apiKeyRule = new TransformationRule
{
    Id = "rule_apikey_remove_param",
    Name = "Remove Debug Parameter",
    Description = "Removes debug query parameter from production API keys",
    Scope = TransformationScope.ApiKey,
    ApiKeyId = "key_prod_001",
    Type = TransformationRuleType.BuiltIn,
    Action = BuiltInAction.RemoveQueryParam,
    Parameters = new Dictionary<string, string>
    {
        ["ParamName"] = "debug"
    },
    Priority = 30,
    IsEnabled = true
};

// Create a Lua script rule for advanced transformation
var luaRule = new TransformationRule
{
    Id = "rule_lua_custom_transform",
    Name = "Custom Lua Transformation",
    Description = "Applies custom transformation logic using Lua script",
    Scope = TransformationScope.Global,
    Type = TransformationRuleType.LuaScript,
    LuaScript = @"
        -- Add timestamp header
        context.Request.Headers["X-Request-Timestamp"] = os.date("%Y-%m-%dT%H:%M:%SZ")
        
        -- Log the transformation
        print("Applied custom transformation for request: " .. context.Request.Path)
    ",
    Priority = 40,
    IsEnabled = true
};
```

## ITransformationPipeline

The `ITransformationPipeline` interface orchestrates the complete request transformation workflow. It evaluates all enabled transformation rules in ascending priority order, applying header modifications, query parameter changes, path rewrites, body transformations, and Lua script execution to in-flight HTTP requests before they reach their destination handlers. The pipeline can block requests based on rule evaluation and provides detailed execution metrics.

### Example Usage

```csharp
using ApiKeyGateway.Transformation;
using ApiKeyGateway.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

// Create a transformation pipeline with required dependencies
var ruleRepository = new TransformationRuleRepository(); // Your implementation
var luaScriptExecutor = new LuaScriptExecutor(
    new LuaExecutionOptions { MaxExecutionMs = 100 },
    new Logger<LuaScriptExecutor>()
);
var pipeline = new TransformationPipeline(ruleRepository, luaScriptExecutor);

// Create a sample HTTP request context
var httpContext = new DefaultHttpContext();
httpContext.Request.Method = "GET";
httpContext.Request.Path = "/api/v1/users";
httpContext.Request.QueryString = new QueryString("?format=json&debug=true");
httpContext.Request.Headers["X-Forwarded-For"] = "192.168.1.100";
httpContext.Request.Headers["Accept"] = "application/json";

// Create transformation context from the HTTP request
var context = new TransformationContext(
    httpContext.Request,
    apiKeyId: "key_prod_001",
    consumerId: "consumer_001"
);

// Execute the transformation pipeline
var result = await pipeline.ApplyAsync(context);

// Process the transformation result
if (result.IsBlocked)
{
    Console.WriteLine($"Request blocked: {result.BlockReason}");
    return Results.Forbid();
}

if (!result.Success)
{
    Console.WriteLine("Transformation pipeline failed:");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- Rule {error.Key}: {error.Value}");
    }
    return Results.BadRequest("Request transformation failed");
}

Console.WriteLine($"Pipeline executed successfully in {result.Elapsed.TotalMilliseconds}ms");
Console.WriteLine($"Rules evaluated: {result.RulesEvaluated}, rules applied: {result.RulesApplied}");

// Access the transformed request data
Console.WriteLine($"Transformed method: {context.Method}");
Console.WriteLine($"Transformed path: {context.Path}");
Console.WriteLine("Modified headers:");
foreach (var header in context.Headers)
{
    Console.WriteLine($"  {header.Key}: {header.Value}");
}
Console.WriteLine("Modified query parameters:");
foreach (var param in context.QueryParameters)
{
    Console.WriteLine($"  {param.Key}: {param.Value}");
}
```

## ILuaScriptExecutor

The `ILuaScriptExecutor` interface executes sandboxed Lua scripts within the request transformation pipeline. Each invocation receives an isolated MoonSharp environment populated with the current `TransformationContext`; mutations made inside the script are reflected back into the context after execution completes. Scripts can modify headers, query parameters, request path, method, and body content. The executor runs scripts in a secure sandbox that prevents file-system access, process execution, and network I/O, and enforces configurable timeouts.

### Example Usage

```csharp
using ApiKeyGateway.Transformation;
using ApiKeyGateway.Domain.Models;
using System.Threading.Tasks;

// Create a Lua script executor with execution options
var options = new LuaExecutionOptions
{
    MaxExecutionMs = 100,
    MaxScriptSizeBytes = 1024
};
var logger = new Logger<LuaScriptExecutor>(); // Use actual logger in real code
var scriptExecutor = new LuaScriptExecutor(options, logger);

// Create a transformation context
var context = new TransformationContext
{
    Headers = new Dictionary<string, string>
    {
        ["X-Original-Header"] = "original-value"
    },
    QueryParameters = new Dictionary<string, string>
    {
        ["param1"] = "value1"
    },
    Body = "{\"test\": \"data\"}",
    Method = "GET",
    Path = "/api/v1/resource",
    SourceIp = "192.168.1.100",
    ConsumerId = "consumer_001",
    ApiKeyId = "key_001"
};

// Define a Lua script that adds a custom header
var luaScript = @"
-- Add custom header
request.headers["X-Custom-Header"] = "custom-value"

-- Modify query parameter
request.query.param1 = "modified-value"

-- Log the transformation
print("Transformed request for " .. request.path)
";

// Execute the script
bool shouldContinue = await scriptExecutor.ExecuteAsync(luaScript, context);

if (shouldContinue)
{
    Console.WriteLine("Script executed successfully");
    Console.WriteLine($"Modified headers: {string.Join(", ", context.Headers.Select(h => h.Key))}");
    Console.WriteLine($"Modified query params: {string.Join(", ", context.QueryParameters.Select(q => q.Key))}");
}
else
{
    Console.WriteLine("Script blocked the request");
}

// Validate a script before execution
var validationResult = scriptExecutor.Validate(luaScript);
if (validationResult.IsValid)
{
    Console.WriteLine("Script is valid and safe to execute");
}
else
{
    Console.WriteLine("Script validation failed:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

## UsageRecord

The `UsageRecord` class tracks detailed API usage metrics for billing, analytics, and monitoring purposes. It captures comprehensive information about each API request including timing, payload sizes, response status, and contextual metadata like source IP and user agent. The class provides static utility methods for aggregating usage statistics across collections of records.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using System.Linq;

// Create a usage record for a successful API request
var usageRecord = new UsageRecord
{
    Id = "record_001",
    ApiKeyId = "key_prod_001",
    ConsumerId = "consumer_001",
    Endpoint = "/api/v1/users",
    Method = "GET",
    ResponseStatusCode = 200,
    RequestBytes = 128,
    ResponseBytes = 2048,
    ResponseTimeMs = 45,
    SourceIp = "192.168.1.100",
    UserAgent = "MyApp/1.0",
    Tags = new Dictionary<string, string>
    {
        ["region"] = "us-east-1",
        ["environment"] = "production"
    }
};

// Access record properties
Console.WriteLine($"Request to {usageRecord.Endpoint} took {usageRecord.ResponseTimeMs}ms");
Console.WriteLine($"Transferred {usageRecord.TotalBytes} bytes");
Console.WriteLine($"Status: {(usageRecord.IsError ? "Error" : "Success")}");

// Create multiple usage records for analytics
var records = new List<UsageRecord>
{
    new UsageRecord
    {
        ApiKeyId = "key_001",
        ConsumerId = "consumer_001",
        Endpoint = "/api/v1/data",
        Method = "POST",
        ResponseStatusCode = 201,
        RequestBytes = 512,
        ResponseBytes = 1024,
        ResponseTimeMs = 89,
        Tags = new Dictionary<string, string> { ["operation"] = "create" }
    },
    new UsageRecord
    {
        ApiKeyId = "key_002",
        ConsumerId = "consumer_002",
        Endpoint = "/api/v1/data/123",
        Method = "GET",
        ResponseStatusCode = 200,
        RequestBytes = 64,
        ResponseBytes = 2048,
        ResponseTimeMs = 32,
        Tags = new Dictionary<string, string> { ["operation"] = "read" }
    },
    new UsageRecord
    {
        ApiKeyId = "key_001",
        ConsumerId = "consumer_001",
        Endpoint = "/api/v1/data/123",
        Method = "PUT",
        ResponseStatusCode = 400,
        RequestBytes = 256,
        ResponseBytes = 128,
        ResponseTimeMs = 22,
        ErrorCode = "VALIDATION_ERROR",
        Tags = new Dictionary<string, string> { ["operation"] = "update" }
    }
};

// Use static utility methods to aggregate data
var totalBytes = UsageRecord.CalculateTotalBytes(records);
var avgResponseTime = UsageRecord.CalculateAverageResponseTime(records);
var successfulRequests = UsageRecord.CountSuccessfulRequests(records);
var errorRequests = UsageRecord.CountErrorRequests(records);

Console.WriteLine($"Total bytes transferred: {totalBytes}");
Console.WriteLine($"Average response time: {avgResponseTime:F2}ms");
Console.WriteLine($"Successful requests: {successfulRequests}");
Console.WriteLine($"Error requests: {errorRequests}");
```

## AuditLog

The `AuditLog` class records security and administrative actions for compliance and debugging purposes. It captures details about who performed an action, when it occurred, which resource was affected, and what changes were made. Audit logs support tracking API key lifecycle events, configuration changes, and security-related operations.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;

// Create an audit log entry for a successful API key creation
var auditLog = new AuditLog
{
  Id = "log_001",
  ResourceId = "key_prod_001",
  ResourceType = "ApiKey",
  Action = AuditAction.KeyCreated,
  PerformedBy = "admin@example.com",
  PerformedAt = DateTime.UtcNow,
  HttpStatusCode = 201,
  SourceIp = "192.168.1.100",
  Reason = "Production API key created for external partner",
  IsSuccess = true
};

// Record changes made during the action
var oldKeyName = "Old Production Key";
auditLog.RecordChange("Name", oldKeyName, "Production Key v2");
auditLog.RecordChange("RateLimitPerHour", 1000, 5000);
auditLog.RecordChange("ExpirationDate", null, DateTime.UtcNow.AddYears(1));

// Add error information if the action failed
if (!auditLog.IsSuccess)
{
  auditLog.ErrorMessage = "Failed to update API key: key not found in database";
}

// Get a human-readable description of the action
Console.WriteLine($"Action: {auditLog.GetActionDescription()}"); // "API key created"

// Access the changes dictionary to review what was modified
foreach (var change in auditLog.Changes)
{
  Console.WriteLine($"{change.Key}: {change.Value}");
}

// Create an audit log for a failed authentication attempt
var failedAuthLog = new AuditLog
{
  Id = "log_002",
  ResourceId = "key_expired_001",
  ResourceType = "ApiKey",
  Action = AuditAction.UnauthorizedAttempt,
  PerformedBy = "unauthenticated_user",
  PerformedAt = DateTime.UtcNow,
  HttpStatusCode = 401,
  SourceIp = "203.0.113.45",
  Reason = "Expired API key used in request",
  IsSuccess = false,
  ErrorMessage = "API key has expired"
};
```

## GatewayConfiguration

The `GatewayConfiguration` class represents the central configuration for an API key gateway instance. It defines security policies, rate limiting, logging behavior, key generation rules, JWT signing, and database connectivity settings. This configuration is typically loaded at startup and controls all operational aspects of the gateway.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using System;
using System.Collections.Generic;

// Create a production-ready gateway configuration
var config = new GatewayConfiguration
{
    Id = "gateway_prod_001",
    RequireSsl = true,
    LogAllRequests = true,
    MaxKeyLength = 64,
    MinKeyLength = 32,
    DefaultKeyExpirationDays = 365,
    AuditLogRetentionDays = 90,
    EnableRateLimiting = true,
    DefaultRateLimitPerHour = 1000,
    EnableIpWhitelisting = true,
    MaxConcurrentRequests = 100,
    JwtSecret = "your-secure-jwt-secret-key-here-change-in-production",
    DatabaseConnectionString = "Host=localhost;Port=5432;Database=api_key_gateway;Username=gateway_user;Password=secure_password_123;",
    CustomSettings = new Dictionary<string, string>
    {
        ["CustomRateLimitHeader"] = "X-Custom-Rate-Limit",
        ["EnableCaching"] = "true",
        ["CacheDurationMinutes"] = "5"
    },
    UpdatedAt = DateTime.UtcNow,
    UpdatedBy = "admin@example.com",
    IsValid = true
};

// Access configuration properties
Console.WriteLine($"Gateway ID: {config.Id}");
Console.WriteLine($"SSL Required: {config.RequireSsl}");
Console.WriteLine($"Max Key Length: {config.MaxKeyLength}");
Console.WriteLine($"Rate Limiting Enabled: {config.EnableRateLimiting}");
Console.WriteLine($"Default Rate Limit: {config.DefaultRateLimitPerHour}/hour");

// Get a custom setting
var customRateLimitHeader = config.GetSetting("CustomRateLimitHeader");
Console.WriteLine($"Custom Rate Limit Header: {customRateLimitHeader}");

// Update a setting
config.SetSetting("EnableCaching", "false");
config.SetSetting("CacheDurationMinutes", "0");

// Check if configuration is valid
if (config.IsValid)
{
    Console.WriteLine("Configuration is valid and ready for use.");
}

// Example with minimal required settings
var minimalConfig = new GatewayConfiguration
{
    Id = "gateway_minimal",
    RequireSsl = false,
    LogAllRequests = false,
    MaxKeyLength = 48,
    MinKeyLength = 24,
    DefaultKeyExpirationDays = 90,
    AuditLogRetentionDays = 30,
    EnableRateLimiting = false,
    EnableIpWhitelisting = false,
    MaxConcurrentRequests = 50,
    JwtSecret = Guid.NewGuid().ToString("N"),
    DatabaseConnectionString = "Host=localhost;Database=test_db;",
    IsValid = true,
    UpdatedAt = DateTime.UtcNow
};
```

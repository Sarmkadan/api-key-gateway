// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
```
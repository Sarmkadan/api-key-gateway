// README.md
## CorrelationContextMiddlewareExtensions

The `CorrelationContextMiddlewareExtensions` class provides extension methods for accessing correlation context information from HTTP contexts. These methods enable extracting correlation IDs, API key IDs, client IPs, and other correlation context data from HTTP requests.

### Example Usage

```csharp
using ApiKeyGateway.Middleware;
using Microsoft.AspNetCore.Http;

// Assuming you have an HttpContext instance
HttpContext context = /* obtain HttpContext */;

// Get correlation ID from context
string? correlationId = context.GetCorrelationId();

// Get API key ID from context
string? apiKeyId = context.GetApiKeyId();

// Get client IP address from context
string? clientIp = context.GetClientIp();

// Get the entire correlation context dictionary
IReadOnlyDictionary<string, object?> correlationContext = context.GetCorrelationContext();

// Check if correlation context exists
bool hasContext = context.HasCorrelationContext();
```

## CacheKeyGeneratorJsonExtensions

The `CacheKeyGeneratorJsonExtensions` class provides extension methods for serializing and deserializing `CacheKeyGeneratorConfiguration` objects to/from JSON strings. This enables configuration persistence and retrieval for cache key generation settings.

### Example Usage

```csharp
using ApiKeyGateway.Caching;
using System;

// Create a CacheKeyGeneratorConfiguration instance
var config = new CacheKeyGeneratorConfiguration
{
    Prefix = "api",
    Separator = ':'
};

// Serialize the configuration to a JSON string
string json = CacheKeyGeneratorJsonExtensions.ToJson(config);

// Deserialize the JSON string back to a configuration object
CacheKeyGeneratorConfiguration? deserialized = CacheKeyGeneratorJsonExtensions.FromJson(json);

// Attempt to deserialize using TryFromJson method
bool success = CacheKeyGeneratorJsonExtensions.TryFromJson(json, out var parsedConfig);
if (success && parsedConfig != null)
{
    // Use parsed configuration
    string prefix = parsedConfig.Prefix;
    char separator = parsedConfig.Separator;
}

// Create configuration from an existing CacheKeyGenerator instance
var fromGenerator = CacheKeyGeneratorJsonExtensions.FromCacheKeyGenerator(config);
```

## CacheKeyGeneratorValidation

`CacheKeyGeneratorValidation` offers a set of helper methods to validate the parameters used by `CacheKeyGenerator` when constructing cache keys. It provides overloads for different key types, boolean checks, and methods that throw detailed exceptions when validation fails.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using ApiKeyGateway.Caching;

// Validate a simple API‑key‑related cache key
var apiKeyProblems = CacheKeyGeneratorValidation.Validate("my-api-key-id");
if (apiKeyProblems.Count == 0)
{
    // Parameters are valid
}

// Quick boolean validation
bool apiKeyIsValid = CacheKeyGeneratorValidation.IsValid("my-api-key-id");

// Throws ArgumentException if invalid
CacheKeyGeneratorValidation.EnsureValid("my-api-key-id");

// Validate a rate‑limit cache key (apiKeyId + endpoint)
var rateLimitProblems = CacheKeyGeneratorValidation.Validate("my-api-key-id", "/api/values");
bool rateLimitIsValid = CacheKeyGeneratorValidation.IsValid("my-api-key-id", "/api/values");

// Validate a usage‑statistics cache key (apiKeyId + date)
var usageProblems = CacheKeyGeneratorValidation.Validate("my-api-key-id", DateTime.UtcNow);
bool usageIsValid = CacheKeyGeneratorValidation.IsValid("my-api-key-id", DateTime.UtcNow);

// Validate a webhook‑delivery cache key (eventId)
var webhookProblems = CacheKeyGeneratorValidation.Validate(Guid.NewGuid());
bool webhookIsValid = CacheKeyGeneratorValidation.IsValid(Guid.NewGuid());

// Validate an external‑API cache key (apiName, endpoint, optional parameters)
var externalProblems = CacheKeyGeneratorValidation.Validate(
    apiName: "GitHub",
    endpoint: "/repos",
    parameters: new Dictionary<string, string> { ["owner"] = "octocat" });
bool externalIsValid = CacheKeyGeneratorValidation.IsValid("GitHub", "/repos");

// Ensure external‑API cache key parameters are valid (throws if not)
CacheKeyGeneratorValidation.EnsureValid("GitHub", "/repos");

// Validate the rate‑limit invalidation pattern (no parameters)
bool patternIsValid = CacheKeyGeneratorValidation.IsValid();
CacheKeyGeneratorValidation.EnsureValid();
```

## JsonSerializationHelperJsonExtensions

`JsonSerializationHelperJsonExtensions` provides JSON‑serialization helpers that work with the `JsonSerializationHelper` configuration. It defines a `JsonSerializationSettings` record exposing the naming policy, ignore condition, and indentation options, and offers methods to serialize these settings to JSON and deserialize them back safely.

### Example Usage

```csharp
using System.Text.Json;
using ApiKeyGateway.Utilities;

// Create a settings instance
var settings = new JsonSerializationHelperJsonExtensions.JsonSerializationSettings
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true
};

// Serialize the settings to a JSON string
string json = settings.ToJson(); // respects WriteIndented = true

// Deserialize back using the static FromJson method
var deserialized = JsonSerializationHelperJsonExtensions.FromJson(json);

// Try‑parse with TryFromJson
bool success = JsonSerializationHelperJsonExtensions.TryFromJson(json, out var parsedSettings);
if (success && parsedSettings != null)
{
    // Use parsedSettings...
}
```

## RateLimitRepositoryValidation

The `RateLimitRepositoryValidation` class provides static methods for validating rate limit repository configurations. It offers validation through multiple approaches: returning error lists, boolean checks, and exception-throwing validation.

### Example Usage

```csharp
using ApiKeyGateway.Repositories;

// Validate a rate limit repository configuration and get validation errors
IReadOnlyList<string> errors = RateLimitRepositoryValidation.Validate("your-repo-config");

// Quick boolean validation
bool isValid = RateLimitRepositoryValidation.IsValid("your-repo-config");

// Validate and throw ArgumentException if invalid
RateLimitRepositoryValidation.EnsureValid("your-repo-config");

// Validate with multiple repository configurations
var configs = new[] { "config1", "config2" };
IReadOnlyList<string> multiErrors = RateLimitRepositoryValidation.Validate(configs);
bool multiIsValid = RateLimitRepositoryValidation.IsValid(configs);
RateLimitRepositoryValidation.EnsureValid(configs);
```

## LoggerFactoryHelperJsonExtensions

`LoggerFactoryHelperJsonExtensions` provides System.Text.Json serialization support for logger factory configuration states. It enables converting between `LoggerFactoryConfiguration` objects and JSON strings, facilitating the persistence and retrieval of logging settings.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;

// Create a new configuration instance
var config = new LoggerFactoryHelperJsonExtensions.LoggerFactoryConfiguration
{
    DefaultLogLevel = "Debug",
    DebugEnabled = true,
    ConsoleEnabled = true
};

// Serialize the configuration to a JSON string
string json = config.ToJson(indented: true);

// Deserialize the JSON string back to a configuration object
var deserializedConfig = LoggerFactoryHelperJsonExtensions.FromJson(json);

// Attempt to deserialize using the TryFromJson method
if (LoggerFactoryHelperJsonExtensions.TryFromJson(json, out var parsedConfig))
{
    // Access properties
    bool isDebugEnabled = parsedConfig.DebugEnabled;
}
```

## JsonSerializationHelperValidation

`JsonSerializationHelperValidation` provides static methods to verify the behavior and accessibility of the `JsonSerializationHelper` utilities. It ensures that serialization, deserialization, and JSON validation logic function as expected by running internal tests.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;
using System;

// Validate the helper and retrieve any errors
IReadOnlyList<string> errors = JsonSerializationHelperValidation.Validate();
if (errors.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var error in errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Check if the helper is valid without retrieving errors
bool isValid = JsonSerializationHelperValidation.IsValid();

// Ensure validity; throws ArgumentException if validation fails
try
{
    JsonSerializationHelperValidation.EnsureValid();
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation exception: {ex.Message}");
}
```

## ResponseFormatterExtensionsValidation

`ResponseFormatterExtensionsValidation` provides extension methods for validating API responses created by `ResponseFormatterExtensions`. It offers validation for both `ApiResponse<T>` and `PaginatedResponse<T>` types through three approaches: returning error lists, boolean checks, and exception-throwing validation.

### Example Usage

```csharp
using System;
using System.Collections.Generic;
using ApiKeyGateway.Utilities;

// Create a valid API response
var validResponse = new ApiResponse<string>
{
    StatusCode = 200,
    Success = true,
    Message = "Request successful",
    Data = "api-key-123",
    ErrorCode = null,
    Timestamp = DateTime.UtcNow
};

// Validate the response and get validation errors
IReadOnlyList<string> errors = validResponse.Validate();
if (errors.Count == 0)
{
    Console.WriteLine("Response is valid!");
}

// Quick boolean validation
bool isValid = validResponse.IsValid();

// Validate and throw ArgumentException if invalid
validResponse.EnsureValid();

// Example with an invalid response
var invalidResponse = new ApiResponse<string>
{
    StatusCode = 200,
    Success = true,
    Message = "", // Missing message
    Data = "api-key-123",
    ErrorCode = null,
    Timestamp = DateTime.UtcNow
};

// This will return validation errors
IReadOnlyList<string> invalidErrors = invalidResponse.Validate();
// invalidErrors will contain: "Success is true but Message is null or empty."

// Validate a paginated response
var paginatedResponse = new PaginatedResponse<string>
{
    PageNumber = 1,
    PageSize = 10,
    TotalCount = 25,
    TotalPages = 3,
    Items = new List<string> { "item1", "item2" },
    Timestamp = DateTime.UtcNow
};

// Validate paginated response
IReadOnlyList<string> paginationErrors = paginatedResponse.Validate();
bool paginationIsValid = paginatedResponse.IsValid();
paginatedResponse.EnsureValid();
```

## ValidationHelpersJsonExtensions

`ValidationHelpersJsonExtensions` supplies JSON‑serialization utilities for the static `ValidationHelpers` class, allowing you to export metadata about its public validation methods and later reconstruct that information. It serializes a lightweight metadata model containing the type name and a list of method names.

### Example Usage

```csharp
using ApiKeyGateway.Utilities;

// Serialize the ValidationHelpers metadata to JSON (indented for readability)
string json = ValidationHelpersJsonExtensions.ToJson(indented: true);

// Deserialize the JSON back to a metadata object
var metadata = ValidationHelpersJsonExtensions.FromJson(json);
if (metadata != null)
{
    Console.WriteLine($"Type: {metadata.TypeName}");
    Console.WriteLine("Methods:");
    foreach (var method in metadata.Methods ?? Array.Empty<string>())
    {
        Console.WriteLine($" - {method}");
    }
}

// Try‑parse the JSON safely without throwing
bool ok = ValidationHelpersJsonExtensions.TryFromJson(json, out var parsedMetadata);
if (ok && parsedMetadata != null)
{
    // Use the parsed metadata
    var typeName = parsedMetadata.TypeName;
    var methods = parsedMetadata.Methods;
}
```

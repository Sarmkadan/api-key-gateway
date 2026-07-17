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

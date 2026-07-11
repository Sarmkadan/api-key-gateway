# ApiEndpoint
The `ApiEndpoint` type represents a single endpoint in the API key gateway, encapsulating its configuration and metadata. It provides properties for identifying the endpoint, specifying its behavior, and controlling access to it. This type is used throughout the API key gateway to manage and interact with individual endpoints.

## API
The `ApiEndpoint` type has the following public members:
* `Id`: A unique identifier for the endpoint.
* `Path`: The URL path of the endpoint.
* `Method`: The HTTP method supported by the endpoint (e.g., GET, POST, PUT, DELETE).
* `TargetUrl`: The URL of the target service that the endpoint forwards requests to.
* `IsActive`: A boolean indicating whether the endpoint is currently active.
* `CreatedAt`: The date and time when the endpoint was created.
* `RequireApiKey`: A boolean indicating whether an API key is required to access the endpoint.
* `TimeoutMs`: The timeout in milliseconds for requests to the endpoint.
* `MaxPayloadBytes`: The maximum allowed payload size in bytes for requests to the endpoint.
* `Description`: An optional description of the endpoint.
* `AllowedConsumers`: A list of allowed consumers that can access the endpoint.
* `Headers`: A dictionary of headers that are added to requests forwarded to the target service.
* `CacheEnabled`: A boolean indicating whether caching is enabled for the endpoint.
* `CacheTtlSeconds`: The time-to-live in seconds for cached responses.
* `IsAccessible`: A boolean indicating whether the endpoint is accessible based on its configuration.
* `IsConsumerAllowed`: A boolean indicating whether a specific consumer is allowed to access the endpoint.
* `IsPayloadSizeValid`: A boolean indicating whether the payload size of a request is valid based on the endpoint's configuration.
* `GetEndpointSignature`: A string representing the signature of the endpoint.

## Usage
Here are two examples of using the `ApiEndpoint` type:
```csharp
// Create a new ApiEndpoint instance
var endpoint = new ApiEndpoint
{
    Id = "example-endpoint",
    Path = "/example",
    Method = "GET",
    TargetUrl = "https://example.com",
    IsActive = true,
    RequireApiKey = true,
    TimeoutMs = 10000,
    MaxPayloadBytes = 1024,
    Description = "An example endpoint",
    AllowedConsumers = new List<string> { "consumer1", "consumer2" },
    Headers = new Dictionary<string, string> { { "X-Custom-Header", "custom-value" } }
};

// Check if a consumer is allowed to access the endpoint
var consumer = "consumer1";
var isAllowed = endpoint.AllowedConsumers.Contains(consumer);
Console.WriteLine($"Is {consumer} allowed to access the endpoint? {isAllowed}");
```

```csharp
// Create a new ApiEndpoint instance with caching enabled
var cachedEndpoint = new ApiEndpoint
{
    Id = "cached-example-endpoint",
    Path = "/cached-example",
    Method = "GET",
    TargetUrl = "https://example.com",
    IsActive = true,
    RequireApiKey = true,
    TimeoutMs = 10000,
    MaxPayloadBytes = 1024,
    CacheEnabled = true,
    CacheTtlSeconds = 3600
};

// Check if the endpoint is accessible and caching is enabled
var isAccessible = cachedEndpoint.IsAccessible;
var isCachingEnabled = cachedEndpoint.CacheEnabled;
Console.WriteLine($"Is the endpoint accessible and caching enabled? {isAccessible} and {isCachingEnabled}");
```

## Notes
When working with the `ApiEndpoint` type, consider the following edge cases and thread-safety remarks:
* The `GetEndpointSignature` property may throw an exception if the endpoint's configuration is invalid.
* The `IsAccessible` and `IsConsumerAllowed` properties may return false if the endpoint's configuration is invalid or if the consumer is not allowed to access the endpoint.
* The `IsPayloadSizeValid` property may return false if the payload size exceeds the maximum allowed size.
* The `CacheEnabled` and `CacheTtlSeconds` properties should be used carefully to avoid caching sensitive data or setting an excessively long cache expiration time.
* The `ApiEndpoint` type is not thread-safe, and concurrent access to its properties may result in inconsistent behavior. It is recommended to synchronize access to `ApiEndpoint` instances using locks or other concurrency control mechanisms.

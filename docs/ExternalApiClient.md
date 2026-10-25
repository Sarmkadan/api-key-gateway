# ExternalApiClient

`ExternalApiClient` is the gateway's wrapper around `HttpClient` for calling third-party HTTP APIs. It offers a GET with optional response caching, a JSON POST, and a generic send for custom `HttpRequestMessage` instances. Every failure that comes from the HTTP call or from deserialization is rethrown as a `KeyStoreUnavailableException`, so callers handle one exception type for external calls.

The interface `IExternalApiClient` and the implementation `HttpExternalApiClient` are defined in `src/ApiKeyGateway/Integration/ExternalApiClient.cs`.

> **Note:** the file header and the class summary say the client does "retries" and "circuit breaking". The code does neither. There is no retry loop, no backoff, and no circuit breaker. Each call makes one HTTP request. The only resilience behavior is the cache on `GetAsync`.

## Registration

The client is registered in `src/ApiKeyGateway/Configuration/ServiceRegistrationExtensions.cs` as a scoped service. Each scope gets an instance that uses an unnamed client from `IHttpClientFactory`, the shared `ICacheProvider`, and the logical API name `"ExternalApi"`:

```csharp
services.AddHttpClient();
services.AddScoped<IExternalApiClient>(sp =>
    new HttpExternalApiClient(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(),
        sp.GetRequiredService<ICacheProvider>(),
        sp.GetRequiredService<ILogger<HttpExternalApiClient>>(),
        "ExternalApi"));
```

The default registration does not set a `BaseAddress`. Because of that, the `endpoint` values passed to `GetAsync` and `PostAsync` must be absolute URIs, or the caller must supply an `HttpClient` that has a base address configured. A relative endpoint with no base address fails inside the `try` block and is wrapped in `KeyStoreUnavailableException`.

To use a different API, create the instance directly with its own `HttpClient`, cache, logger, and API name. The API name is part of every cache key, so two clients with different names never share cache entries.

## Types

### IExternalApiClient

The contract implemented by `HttpExternalApiClient`. Inject this interface rather than the concrete class.

| Member | Returns | Description |
|---|---|---|
| `GetAsync<T>(string endpoint, TimeSpan? cacheDuration = null)` | `Task<T>` | Sends a GET request, with optional caching of the deserialized response. |
| `PostAsync<T>(string endpoint, object payload)` | `Task<T>` | Sends a POST request with a compact JSON body. |
| `SendAsync<T>(HttpRequestMessage request)` | `Task<T>` | Sends a caller-built request and deserializes the response. |

All three methods have a constraint of `where T : class`.

### HttpExternalApiClient

`public sealed class HttpExternalApiClient : IExternalApiClient`

The class is sealed. It has no public members beyond its constructor and the interface methods.

**Constructor:**

```csharp
HttpExternalApiClient(HttpClient httpClient, ICacheProvider cache, ILogger<HttpExternalApiClient> logger, string apiName)
```

| Parameter | Description |
|---|---|
| `httpClient` | The `HttpClient` used for every request. |
| `cache` | The `ICacheProvider` used for GET responses. |
| `logger` | The logger for request diagnostics. |
| `apiName` | The logical name of the external API. It appears in log messages and in cache keys. |

Throws `ArgumentNullException` if any argument is `null`.

## Methods

### GetAsync

```csharp
Task<T> GetAsync<T>(string endpoint, TimeSpan? cacheDuration = null) where T : class
```

Performs a GET request to `endpoint` and deserializes the response body into `T`.

Behavior, in order:

1. `endpoint` is `null`, empty, or whitespace: `ArgumentException` (parameter `endpoint`). Nothing is sent.
2. The cache key is built with `CacheKeyGenerator.GetExternalApiCacheKey(apiName, endpoint)`.
3. If `cacheDuration` has a value, the cache is checked. A non-null cached value is returned without sending a request.
4. Otherwise the request is sent. A non-success status code is an error (`EnsureSuccessStatusCode`).
5. The body is deserialized with `JsonSerializationHelper.Deserialize<T>`. A `null` result throws `ConfigurationException`.
6. If `cacheDuration` has a value, the result is stored in the cache with that expiration.

The cache is used only when `cacheDuration` is provided. Passing `null` (the default) always sends a request and never reads or writes the cache.

Errors from steps 4 and 5 are handled as described in [Exceptions](#exceptions). A cache read failure in step 3 is not wrapped, because the cache lookup is outside the `try` block.

### PostAsync

```csharp
Task<T> PostAsync<T>(string endpoint, object payload) where T : class
```

Serializes `payload` to compact JSON with `JsonSerializationHelper.SerializeCompact` and sends it as a POST with `Content-Type: application/json; charset=utf-8`. The response body is deserialized into `T`.

- `endpoint` is empty or whitespace: `ArgumentException` (parameter `endpoint`).
- `payload` is `null`: `ArgumentNullException` (parameter `payload`).
- A `null` deserialized result throws `ConfigurationException`.

Responses are never cached, regardless of the method used.

### SendAsync

```csharp
Task<T> SendAsync<T>(HttpRequestMessage request) where T : class
```

Sends `request` as given through `HttpClient.SendAsync`, then deserializes the response body into `T`. The method does not set headers, change the method, or add a base address. Set those on the request before calling.

- `request` is `null`: `ArgumentNullException` (parameter `request`).
- A `null` deserialized result throws `ConfigurationException`.

Responses are never cached.

## Exceptions

| Situation | Exception thrown |
|---|---|
| Empty `endpoint` argument | `ArgumentException` (not wrapped) |
| `null` `payload` or `request` argument | `ArgumentNullException` (not wrapped) |
| Non-success HTTP status, network failure, or `HttpRequestException` | `KeyStoreUnavailableException` (wraps the original exception) |
| Any other failure in the request, including timeouts and JSON errors | `KeyStoreUnavailableException` (wraps the original exception) |
| Response deserializes to `null` | `ConfigurationException` (not wrapped) |

Any exception that already derives from `ApiKeyGatewayException` is rethrown as-is, not wrapped. `ConfigurationException` is the only one of these that the client raises itself.

Every wrapped exception is logged at `Error` level before it is thrown. The `Operation` property of the wrapped exception is `ExternalApiClient.GetAsync`, `ExternalApiClient.PostAsync`, or `ExternalApiClient.SendAsync`, depending on the method. The log message names the API and, for GET and POST, the endpoint.

When a `KeyStoreUnavailableException` reaches `ErrorHandlingMiddleware`, the response is a 503 Service Unavailable problem response. Callers that do not catch the exception themselves therefore return 503 to the client.

## Logging

| Level | When |
|---|---|
| `Debug` | At the start of each call, on a cache hit, and after a response is cached. |
| `Information` | After a successful GET, POST, or custom request. |
| `Error` | Before a request failure is wrapped and thrown. |

Log messages use structured parameters: `{ApiName}`, `{Endpoint}`, and `{Duration}`.

## Usage

The usual way to use the client is through DI. The example below calls a fictional exchange-rate API. The response model uses public settable properties, so the JSON deserializer can populate them.

```csharp
using ApiKeyGateway.Integration;

public sealed class ExchangeRate
{
    public string Currency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}

public sealed class ExchangeRateService
{
    private readonly IExternalApiClient _client;

    public ExchangeRateService(IExternalApiClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    // Cached for five minutes. Pass null as the duration to skip the cache.
    public Task<ExchangeRate> GetRateAsync(string currency) =>
        _client.GetAsync<ExchangeRate>(
            $"https://rates.example.com/v1/rates/{Uri.EscapeDataString(currency)}",
            TimeSpan.FromMinutes(5));

    public Task<ExchangeRate> RequestQuoteAsync(string currency, decimal amount) =>
        _client.PostAsync<ExchangeRate>(
            "https://rates.example.com/v1/quotes",
            new { currency, amount });
}
```

Handling failures at the call site:

```csharp
try
{
    var rate = await exchangeRates.GetRateAsync("EUR");
    Console.WriteLine($"{rate.Currency}: {rate.Rate}");
}
catch (KeyStoreUnavailableException ex)
{
    // The external API failed or timed out. The original error is in ex.InnerException.
    logger.LogWarning(ex, "Exchange rate lookup failed; using fallback");
}
```

A custom request, for example one with an authorization header:

```csharp
using var request = new HttpRequestMessage(HttpMethod.Get, "https://rates.example.com/v1/account");
request.Headers.Add("X-Api-Key", apiKey);

var account = await client.SendAsync<ExchangeRate>(request);
```

## Limitations

- **No retries or circuit breaking.** A transient failure surfaces on the first attempt. Add retry handling in the caller or in the `HttpClient` pipeline if it is needed.
- **Caching applies to GET only.** There is no cache for POST or `SendAsync`.
- **Cache entries are keyed by API name and the full `endpoint` string.** Include query parameters in `endpoint` so that different queries get different cache entries.
- **No cancellation token.** None of the methods accepts a `CancellationToken`. Limit the duration with the `HttpClient.Timeout` setting.
- **Relative endpoints need a base address.** See [Registration](#registration).
- **Response size is not limited by the client.** It reads the whole body with `ReadAsStringAsync` before deserializing.

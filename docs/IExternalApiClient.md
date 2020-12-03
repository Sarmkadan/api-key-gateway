# IExternalApiClient

The `IExternalApiClient` interface defines a contract for making HTTP requests to external APIs. It provides asynchronous methods for GET, POST, and generic send operations, along with a property that exposes the underlying `HttpExternalApiClient` instance. This interface is designed to be used throughout the `api-key-gateway` project for all outbound HTTP communication, ensuring consistent error handling, authentication, and serialization behavior.

## API

### `HttpExternalApiClient`

- **Purpose**: Gets the underlying `HttpExternalApiClient` instance used for HTTP communication. This property allows direct access to the client for advanced scenarios such as custom request configuration or inspection of client state.
- **Parameters**: None.
- **Return value**: An instance of `HttpExternalApiClient`.
- **Throws**: Never throws.

### `GetAsync<T>`

- **Purpose**: Sends an HTTP GET request to the specified endpoint and deserializes the response body into an object of type `T`.
- **Parameters**:
  - `url` (`string`) – The target endpoint URL.
  - `cancellationToken` (`CancellationToken`, optional) – A cancellation token to cancel the operation.
- **Return value**: A `Task<T>` that represents the asynchronous operation. The result is the deserialized response of type `T`.
- **Throws**:
  - `ArgumentNullException` – If `url` is `null`.
  - `HttpRequestException` – If the HTTP response status code indicates failure (e.g., 4xx or 5xx).
  - `TaskCanceledException` – If the operation is cancelled via the cancellation token.
  - `InvalidOperationException` – If deserialization of the response body fails.

### `PostAsync<T>`

- **Purpose**: Sends an HTTP POST request with a JSON-encoded body to the specified endpoint and deserializes the response body into an object of type `T`.
- **Parameters**:
  - `url` (`string`) – The target endpoint URL.
  - `body` (`object`) – The request body to be serialized as JSON.
  - `cancellationToken` (`CancellationToken`, optional) – A cancellation token to cancel the operation.
- **Return value**: A `Task<T>` that represents the asynchronous operation. The result is the deserialized response of type `T`.
- **Throws**:
  - `ArgumentNullException` – If `url` or `body` is `null`.
  - `HttpRequestException` – If the HTTP response status code indicates failure.
  - `TaskCanceledException` – If the operation is cancelled.
  - `InvalidOperationException` – If serialization of the body or deserialization of the response fails.

### `SendAsync<T>`

- **Purpose**: Sends an arbitrary HTTP request message and deserializes the response body into an object of type `T`. This method provides full control over the request, including headers, method, and content.
- **Parameters**:
  - `request` (`HttpRequestMessage`) – The HTTP request message to send.
  - `cancellationToken` (`CancellationToken`, optional) – A cancellation token to cancel the operation.
- **Return value**: A `Task<T>` that represents the asynchronous operation. The result is the deserialized response of type `T`.
- **Throws**:
  - `ArgumentNullException` – If `request` is `null`.
  - `HttpRequestException` – If the HTTP response status code indicates failure.
  - `TaskCanceledException` – If the operation is cancelled.
  - `InvalidOperationException` – If deserialization of the response body fails.

## Usage

### Example 1: GET request to retrieve a resource

```csharp
public class ExternalService
{
    private readonly IExternalApiClient _client;

    public ExternalService(IExternalApiClient client)
    {
        _client = client;
    }

    public async Task<User> GetUserAsync(int userId, CancellationToken ct)
    {
        string url = $"https://api.example.com/users/{userId}";
        return await _client.GetAsync<User>(url, ct);
    }
}
```

### Example 2: POST request with a JSON body

```csharp
public class DataPublisher
{
    private readonly IExternalApiClient _client;

    public DataPublisher(IExternalApiClient client)
    {
        _client = client;
    }

    public async Task<Confirmation> PublishEventAsync(EventData eventData, CancellationToken ct)
    {
        string url = "https://api.example.com/events";
        return await _client.PostAsync<Confirmation>(url, eventData, ct);
    }
}
```

## Notes

- **Thread safety**: All methods are safe for concurrent use. The underlying `HttpExternalApiClient` is designed to be reused across multiple requests. Do not dispose the client after each call; instead, rely on dependency injection to manage its lifetime.
- **Cancellation**: Always pass a `CancellationToken` to prevent resource leaks and ensure timely termination of long-running requests. The token is forwarded to the underlying HTTP client.
- **Error handling**: The interface throws `HttpRequestException` for non-success HTTP status codes. Implementations may include retry logic or custom error mapping; check the concrete `HttpExternalApiClient` documentation for details.
- **Serialization**: The request body for `PostAsync<T>` is serialized as JSON using the system’s default serializer settings. The response body is deserialized from JSON into the specified type `T`. Ensure that `T` has a parameterless constructor and properties with public setters.
- **Null arguments**: Passing `null` for `url`, `body`, or `request` will result in an `ArgumentNullException`. Validate inputs before calling these methods.
- **Property access**: The `HttpExternalApiClient` property should be used sparingly. Prefer the typed methods (`GetAsync<T>`, `PostAsync<T>`, `SendAsync<T>`) for standard operations to benefit from built-in serialization and error handling.

# IAuthenticationService

Provides core authentication functionality for the API‑Key Gateway, including validation of credentials, IP address checks, and logging of authentication attempts.

## API

### AuthenticationService
**Purpose**  
Exposes the underlying `AuthenticationService` implementation that performs the actual authentication logic.

**Parameters**  
None.

**Return Value**  
An instance of `AuthenticationService` used to invoke authentication‑related operations.

**Exceptions**  
- May throw `InvalidOperationException` if the service has not been properly initialized.

### AuthenticateAsync
**Purpose**  
Authenticates an incoming request and returns the associated API key if the credentials are valid.

**Parameters**  
None.

**Return Value**  
A `Task<ApiKey>` that completes with the authenticated `ApiKey` object, or `null` if authentication fails.

**Exceptions**  
- Throws `UnauthorizedAccessException` when the supplied credentials are invalid or missing.  
- Throws `ObjectDisposedException` if the service has been disposed.  
- May throw other unexpected exceptions originating from downstream stores (e.g., database connectivity issues).

### ValidateIpAsync
**Purpose**  
Determines whether the caller’s IP address is permitted to access the gateway.

**Parameters**  
None.

**Return Value**  
A `Task<bool>` that completes with `true` if the IP address is allowed, otherwise `false`.

**Exceptions**  
- Throws `InvalidOperationException` if the IP address cannot be determined from the request context.  
- May throw `ObjectDisposedException` if the service has been disposed.

### LogAuthenticationAttemptAsync
**Purpose**  
Records an authentication attempt (successful or failed) for auditing and monitoring purposes.

**Parameters**  
None.

**Return Value**  
A `Task` that completes when the log entry has been persisted.

**Exceptions**  
- Throws `ObjectDisposedException` if the service has been disposed.  
- May throw IOException‑derived exceptions if the underlying logging sink fails to write the entry.

## Usage

```csharp
// Example 1: Authenticating a request and using the resulting API key
public async Task<IActionResult> HandleRequest(HttpRequest request)
{
    var authService = _serviceProvider.GetRequiredService<IAuthenticationService>();
    
    // Validate the caller's IP first
    bool ipAllowed = await authService.ValidateIpAsync();
    if (!ipAllowed)
        return StatusCode(403, "IP address not allowed");

    // Attempt authentication
    ApiKey? apiKey = await authService.AuthenticateAsync();
    if (apiKey == null)
        return Unauthorized("Invalid credentials");

    // Log the attempt (successful)
    await authService.LogAuthenticationAttemptAsync();

    // Proceed with request processing using the authenticated API key
    return await _next.Invoke(request, apiKey);
}
```

```csharp
// Example 2: Decoupled usage via constructor injection
public class MyMiddleware
{
    private readonly IAuthenticationService _authService;

    public MyMiddleware(IAuthenticationService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Ensure the request originates from an allowed IP
        if (!await _authService.ValidateIpAsync())
        {
            context.Response.StatusCode = 403;
            return;
        }

        // Try to authenticate; if fails, log and abort
        ApiKey? key = await _authService.AuthenticateAsync();
        if (key == null)
        {
            await _authService.LogAuthenticationAttemptAsync();
            context.Response.StatusCode = 401;
            return;
        }

        // Successful authentication – log and continue pipeline
        await _authService.LogAuthenticationAttemptAsync();
        await _next(context);
    }
}
```

## Notes
- The interface itself does not enforce thread‑safety; concrete implementations should ensure that stateless methods (`AuthenticateAsync`, `ValidateIpAsync`, `LogAuthenticationAttemptAsync`) are safe to call concurrently from multiple threads.
- The `AuthenticationService` property typically returns a singleton or scoped instance; consumers should not assume ownership of the returned object and must not dispose it unless they are responsible for its lifetime.
- All asynchronous methods are expected to complete without blocking the calling thread; implementations should avoid synchronous work that could cause thread‑pool starvation.
- Error handling strategies (e.g., retry logic, fallback logging) are left to the implementing class; callers should be prepared to handle the documented exceptions as well as any unexpected exceptions that may arise from infrastructure dependencies.

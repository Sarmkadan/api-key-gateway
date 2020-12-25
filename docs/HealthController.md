# HealthController

The `HealthController` is an ASP.NET Core controller responsible for exposing health-check endpoints that allow external systems to verify the operational status of the API Key Gateway service. It provides endpoints to check basic liveness (`Live`), readiness for traffic (`Ready`), and a combined status view (`Status`).

## API

### `public HealthController()`

Initializes a new instance of the `HealthController` class.

### `public IActionResult Live()`

Returns a simple HTTP 200 OK response to indicate the application is running.

- **Return value**: `IActionResult` – HTTP 200 OK with no body.
- **Throws**: No exceptions.

### `public async Task<IActionResult> Ready()`

Performs asynchronous readiness checks and returns HTTP 200 OK if all checks pass.

- **Return value**: `Task<IActionResult>` – HTTP 200 OK if the service is ready to serve traffic; otherwise, HTTP 503 Service Unavailable.
- **Throws**: No exceptions. Any failure results in a non-200 response.

### `public IActionResult Status()`

Returns a combined health status including liveness and readiness indicators.

- **Return value**: `IActionResult` – HTTP 200 OK with a JSON body describing the status of liveness and readiness checks.
- **Throws**: No exceptions.

## Usage

# MiddlewareConfiguration

`src/ApiKeyGateway/Configuration/MiddlewareConfiguration.cs` is a static class with two extension methods. `AddGatewayMiddleware` registers the logging setup in the DI container. `UseGatewayMiddleware` builds a request pipeline in a fixed order. The class has no options type and no `*Options` binding. Its only configuration input is one key from `IConfiguration`, described in [Configuration properties](#configuration-properties).

## Members

| Member | Kind | Purpose |
|--------|------|---------|
| `AddGatewayMiddleware(IServiceCollection, IConfiguration)` | extension method on `IServiceCollection` | Configures logging: clears default providers, adds console and debug, and sets the minimum level. Returns the same `services` instance. |
| `UseGatewayMiddleware(WebApplication)` | extension method on `WebApplication` | Adds the gateway middleware in order, then routing, authentication, authorization and controller mapping. Returns `void`. |

Both methods throw `ArgumentNullException` when a required argument is `null`.

## Configuration properties

### Logging:LogLevel:Default

Read by `AddGatewayMiddleware`. This is the only configuration key the class reads.

| Property | Default | Notes |
|----------|---------|-------|
| `Logging:LogLevel:Default` | `Information` | Parsed with `Enum.TryParse<LogLevel>`. |

Behavior depends on the value:

- **Key missing.** The method uses `Information` and calls `SetMinimumLevel(LogLevel.Information)`.
- **Valid name.** The value must match a `LogLevel` member name exactly, such as `Debug`, `Warning` or `Error`. `TryParse` is case-sensitive by default, so `warning` does not match. Numeric strings such as `"2"` also parse.
- **Invalid value.** `SetMinimumLevel` is not called. The minimum level stays at whatever the host configured, and no error is raised.

Example `appsettings.json` fragment:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

The key is not under the `Gateway:` section, so it is not part of `GatewayConfiguration`. Category-specific keys such as `Logging:LogLevel:Microsoft` are not read here. They are only honored by the host's own logging configuration, which `ClearProviders()` does not remove.

## AddGatewayMiddleware

Registers logging. It runs as part of the service registration in `ServiceRegistrationExtensions.AddGatewayServices` (`src/ApiKeyGateway/Configuration/ServiceRegistrationExtensions.cs:79`).

Steps, in order:

1. Calls `services.AddLogging`.
2. Calls `ClearProviders()`. This removes every logger provider added earlier by the host, including ones configured in `Program.cs` or by the `WebApplicationBuilder` defaults.
3. Calls `AddConsole()` and `AddDebug()`.
4. Reads `Logging:LogLevel:Default` and calls `SetMinimumLevel` as described above.

Because `ClearProviders()` runs inside the registration, providers added before `AddGatewayMiddleware` runs are lost. Providers added after it are kept.

The method does not register any middleware types. The middleware classes are registered by `Program.cs` and by the other registration extensions.

## UseGatewayMiddleware

Adds the pipeline in this order:

| Step | Component | Why it is here |
|------|-----------|----------------|
| 1 | `ErrorHandlingMiddleware` | Catches exceptions from later steps. |
| 2 | `RequestLoggingMiddleware` | Logs every request, including ones that fail validation. |
| 3 | `RequestValidationMiddleware` | Rejects malformed requests before they reach handlers. |
| 4 | `PerformanceMonitoringMiddleware` | Measures response time around the remaining steps. |
| 5 | `UseRouting`, `UseAuthentication`, `UseAuthorization` | Standard ASP.NET Core stages. |
| 6 | `MapControllers` | Maps the controller endpoints. |

All four gateway middleware types are in `src/ApiKeyGateway/Middleware/`.

### Status: not called

Nothing in `src/` or the tests calls `UseGatewayMiddleware`. `Program.cs` builds its own pipeline instead. It uses `ErrorHandlingMiddleware`, then `UseCors`, `UseApiKeyAuthentication`, `UseRequestTransformation` and `MapControllers`. It does not use `RequestLoggingMiddleware`, `RequestValidationMiddleware` or `PerformanceMonitoringMiddleware`.

If you call `UseGatewayMiddleware` together with the `Program.cs` pipeline, `UseRouting`, `UseAuthentication`, `UseAuthorization` and `MapControllers` would run twice. The two pipelines also do not match in what they include. Pick one. Do not add both.

## Related

- [GatewayConfiguration](GatewayConfiguration.md)
- [CachingConfiguration](CachingConfiguration.md)
- [ApiKeyAuthenticationMiddleware](ApiKeyAuthenticationMiddleware.md)

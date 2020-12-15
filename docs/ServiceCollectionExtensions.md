# ServiceCollectionExtensions

Extension methods for registering API Key Gateway core services and configuration options with the Microsoft.Extensions.DependencyInjection `IServiceCollection`.

## API

### `AddGatewayCoreServices`

Registers the core services required for API Key Gateway functionality, including key validation, rate limiting, and request processing. This includes transient services for key management, rate limiting, audit logging, and request coalescing.

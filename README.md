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

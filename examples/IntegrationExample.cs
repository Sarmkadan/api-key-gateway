using Microsoft.Extensions.DependencyInjection;
using ApiKeyGateway.Configuration;
using Microsoft.Extensions.Configuration;

// Example of how to wire the gateway into an ASP.NET Core application
public class IntegrationExample
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // 1. Register the core gateway services
        // This adds repositories, services, and default configurations
        services.AddGatewayCoreServices(configuration);

        // 2. Register Swagger/OpenAPI documentation
        // This sets up the security definition for API Key authentication
        services.AddGatewayDocumentation();
        
        // At this point, the middleware can be added in the Program.cs or Startup.cs
        // app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
    }
}

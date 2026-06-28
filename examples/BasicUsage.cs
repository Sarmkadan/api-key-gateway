using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ApiKeyGateway.Services;

// Basic example of creating an API key using the IApiKeyService
public class BasicUsage
{
    public async Task RunExample(IServiceProvider serviceProvider)
    {
        // Obtain the service from the DI container
        var apiKeyService = serviceProvider.GetRequiredService<IApiKeyService>();

        // Create a new API key for a consumer
        // Returns an ApiKey object containing the Id, KeySecret (hash), ConsumerId, etc.
        var newKey = await apiKeyService.CreateKeyAsync(
            consumerId: "consumer_001",
            description: "Production-API-Key",
            expirationDays: 365
        );

        Console.WriteLine($"Successfully created API Key for {newKey.ConsumerId}.");
        Console.WriteLine($"Key ID: {newKey.Id}");
    }
}

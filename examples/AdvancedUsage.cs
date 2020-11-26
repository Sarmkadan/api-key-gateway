using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ApiKeyGateway.Services;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Exceptions;

// Advanced example showcasing error handling and service configuration usage
public class AdvancedUsage
{
    public async Task RunExample(IServiceProvider serviceProvider)
    {
        var apiKeyService = serviceProvider.GetRequiredService<IApiKeyService>();

        try
        {
            // Create a key with specific quota restrictions
            var apiKey = await apiKeyService.CreateKeyAsync("consumer_002", "RestrictedKey", 30);
            
            // Example: Revoking the key
            await apiKeyService.RevokeKeyAsync(apiKey.Id, "Policy-Violation");
            
            Console.WriteLine("Key successfully revoked due to policy violation.");
        }
        catch (InvalidApiKeyException ex)
        {
            // Specific exception handling for invalid keys
            Console.WriteLine($"Key error: {ex.Message}");
        }
        catch (Exception ex)
        {
            // General error handling for underlying data access issues
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}

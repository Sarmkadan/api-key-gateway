// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// C# example: Bulk API key management
/// Demonstrates batch operations, key rotation, and bulk updates
///
/// Usage: dotnet run
/// </summary>
public class ApiKeyGatewayExample
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _adminKey;

    public ApiKeyGatewayExample(
        string baseUrl = "http://localhost:5000",
        string adminKey = "admin_key_example")
    {
        _baseUrl = baseUrl;
        _adminKey = adminKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _adminKey);
    }

    // Create a single API key
    public async Task<ApiKeyResponse> CreateKeyAsync(string consumerId, string name)
    {
        var payload = new
        {
            consumerId,
            name,
            expirationDays = 365,
            rateLimit = new
            {
                requestsPerSecond = 100,
                requestsPerMinute = 5000,
                requestsPerHour = 100000
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/apikeys", content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var document = JsonDocument.Parse(responseJson);
        var dataElement = document.RootElement.GetProperty("data");

        return new ApiKeyResponse
        {
            Id = dataElement.GetProperty("id").GetString()!,
            DisplayKey = dataElement.GetProperty("displayKey").GetString()!,
            ConsumerId = dataElement.GetProperty("consumerId").GetString()!,
            CreatedAt = dataElement.GetProperty("createdAt").GetDateTime()
        };
    }

    // Get all keys for a consumer
    public async Task<List<ApiKeyInfo>> ListConsumerKeysAsync(string consumerId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/api/apikeys/consumer/{consumerId}").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var document = JsonDocument.Parse(responseJson);
        var dataArray = document.RootElement.GetProperty("data").EnumerateArray();

        var keys = new List<ApiKeyInfo>();
        foreach (var item in dataArray)
        {
            keys.Add(new ApiKeyInfo
            {
                Id = item.GetProperty("id").GetString()!,
                Name = item.GetProperty("name").GetString()!,
                Status = item.GetProperty("status").GetString()!
            });
        }

        return keys;
    }

    // Update multiple keys at once
    public async Task<int> UpdateKeysAsync(List<string> keyIds, Dictionary<string, object> updates)
    {
        int updated = 0;

        foreach (var keyId in keyIds)
        {
            try
            {
                var json = JsonSerializer.Serialize(updates);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}/api/apikeys/{keyId}", content).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                    updated++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️  Error updating {keyId}: {ex.Message}");
            }
        }

        return updated;
    }

    // Rotate API keys for a consumer
    public async Task RotateConsumerKeysAsync(string consumerId, int gracePeriodHours = 24)
    {
        Console.WriteLine($"\n🔄 Rotating API keys for consumer: {consumerId}");

        // Get existing keys
        var existingKeys = await ListConsumerKeysAsync(consumerId).ConfigureAwait(false);
        Console.WriteLine($"   Found {existingKeys.Count} existing keys");

        // Create new keys (one for each existing key)
        var newKeys = new List<ApiKeyResponse>();
        foreach (var oldKey in existingKeys)
        {
            var newKey = await CreateKeyAsync(consumerId, $"{oldKey.Name} (rotated)").ConfigureAwait(false);
            newKeys.Add(newKey);
            Console.WriteLine($"   ✅ Created new key: {newKey.Id}");
        }

        Console.WriteLine($"   ⏳ Grace period: {gracePeriodHours} hours");
        Console.WriteLine($"   📋 Clients should migrate from old to new keys during this period");

        // After grace period (simulated)
        Console.WriteLine($"   🗑️  After {gracePeriodHours} hours, old keys will be revoked:");
        foreach (var oldKey in existingKeys)
        {
            Console.WriteLine($"       - {oldKey.Id} ({oldKey.Name})");
        }
    }

    // Audit and report
    public async Task GenerateConsumerReportAsync(string consumerId)
    {
        Console.WriteLine($"\n📊 Consumer Report: {consumerId}");

        var keys = await ListConsumerKeysAsync(consumerId).ConfigureAwait(false);
        Console.WriteLine($"   📌 Active Keys: {keys.Count(k => k.Status == "Active")}");
        Console.WriteLine($"   🚫 Disabled Keys: {keys.Count(k => k.Status == "Disabled")}");
        Console.WriteLine($"   ❌ Revoked Keys: {keys.Count(k => k.Status == "Revoked")}");

        Console.WriteLine("\n   Key Details:");
        foreach (var key in keys)
        {
            Console.WriteLine($"   - {key.Id}");
            Console.WriteLine($"     Name: {key.Name}");
            Console.WriteLine($"     Status: {key.Status}");
        }
    }

    // Main execution
    public async Task RunAsync()
    {
        Console.WriteLine("🚀 API Key Gateway - C# Key Management Example\n");

        try
        {
            // Step 1: Create multiple keys for a consumer
            Console.WriteLine("1️⃣  Creating API keys for multiple consumers...");
            var consumerId = $"csharp_demo_{DateTime.Now.Ticks}";

            var key1 = await CreateKeyAsync(consumerId, "Production API Key").ConfigureAwait(false);
            Console.WriteLine($"✅ Key 1 created: {key1.Id}");

            var key2 = await CreateKeyAsync(consumerId, "Staging API Key").ConfigureAwait(false);
            Console.WriteLine($"✅ Key 2 created: {key2.Id}");

            var key3 = await CreateKeyAsync(consumerId, "Development API Key").ConfigureAwait(false);
            Console.WriteLine($"✅ Key 3 created: {key3.Id}\n");

            // Step 2: List all keys
            Console.WriteLine("2️⃣  Listing all keys for consumer...");
            var allKeys = await ListConsumerKeysAsync(consumerId).ConfigureAwait(false);
            Console.WriteLine($"✅ Found {allKeys.Count} keys");
            foreach (var key in allKeys)
            {
                Console.WriteLine($"   - {key.Name}: {key.Status}");
            }
            Console.WriteLine();

            // Step 3: Update rate limits
            Console.WriteLine("3️⃣  Updating rate limits...");
            var keyIds = allKeys.Select(k => k.Id).ToList();
            var updated = await UpdateKeysAsync(
                keyIds,
                new Dictionary<string, object>
                {
                    { "rateLimit", new { requestsPerSecond = 200 } }
                }
            );
            Console.WriteLine($"✅ Updated {updated} keys\n");

            // Step 4: Generate report
            await GenerateConsumerReportAsync(consumerId).ConfigureAwait(false);

            // Step 5: Demonstrate key rotation
            await RotateConsumerKeysAsync(consumerId, gracePeriodHours: 24).ConfigureAwait(false);

            Console.WriteLine("\n✨ Example completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
        }
    }

    public static async Task Main(string[] args)
    {
        var example = new ApiKeyGatewayExample();
        await example.RunAsync().ConfigureAwait(false);
    }
}

// Response models
public class ApiKeyResponse
{
    public string Id { get; set; } = string.Empty;
    public string DisplayKey { get; set; } = string.Empty;
    public string ConsumerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ApiKeyInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

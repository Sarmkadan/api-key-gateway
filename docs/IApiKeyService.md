# IApiKeyService
The `IApiKeyService` interface provides a set of methods for managing API keys, including creation, validation, and revocation. It allows for the retrieval of API keys by ID, updating of key metadata, and management of IP whitelists. This interface is designed to be used in the context of an API gateway, where API keys are used to authenticate and authorize incoming requests.

## API
* `CreateKeyAsync`: Creates a new API key. Returns an `ApiKey` object representing the newly created key.
* `GetByIdAsync`: Retrieves an API key by its ID. Returns an `ApiKey` object if the key exists, or `null` if it does not.
* `ValidateKeyAsync`: Validates an API key. Returns an `ApiKey` object if the key is valid, or `null` if it is not.
* `DisableKeyAsync`: Disables an API key. Returns `true` if the key was successfully disabled, or `false` if it was not.
* `EnableKeyAsync`: Enables an API key. Returns `true` if the key was successfully enabled, or `false` if it was not.
* `RevokeKeyAsync`: Revokes an API key. Returns `true` if the key was successfully revoked, or `false` if it was not.
* `GetConsumerKeysAsync`: Retrieves a list of API keys associated with a consumer. Returns a `List<ApiKey>` containing the API keys.
* `UpdateKeyMetadataAsync`: Updates the metadata associated with an API key. Returns `true` if the metadata was successfully updated, or `false` if it was not.
* `GetIpWhitelistAsync`: Retrieves the IP whitelist associated with an API key. Returns a `List<string>` containing the IP addresses in the whitelist.
* `SetIpWhitelistAsync`: Sets the IP whitelist associated with an API key. Returns `true` if the whitelist was successfully set, or `false` if it was not.
* `AddIpToWhitelistAsync`: Adds an IP address to the whitelist associated with an API key. Returns `true` if the IP address was successfully added, or `false` if it was not.
* `RemoveIpFromWhitelistAsync`: Removes an IP address from the whitelist associated with an API key. Returns `true` if the IP address was successfully removed, or `false` if it was not.

## Usage
```csharp
// Example 1: Creating and validating an API key
var apiKeyService = new ApiKeyService();
var apiKey = await apiKeyService.CreateKeyAsync();
if (apiKey != null)
{
    var validatedKey = await apiKeyService.ValidateKeyAsync(apiKey.Id);
    if (validatedKey != null)
    {
        Console.WriteLine("API key is valid");
    }
    else
    {
        Console.WriteLine("API key is not valid");
    }
}

// Example 2: Managing IP whitelists
var apiKeyService = new ApiKeyService();
var ipWhitelist = await apiKeyService.GetIpWhitelistAsync(apiKey.Id);
if (ipWhitelist != null)
{
    await apiKeyService.AddIpToWhitelistAsync(apiKey.Id, "192.168.1.100");
    ipWhitelist = await apiKeyService.GetIpWhitelistAsync(apiKey.Id);
    Console.WriteLine("IP whitelist: " + string.Join(", ", ipWhitelist));
}
```

## Notes
* The `IApiKeyService` interface is designed to be thread-safe, allowing for concurrent access to API key management functionality.
* When using the `CreateKeyAsync` method, the resulting `ApiKey` object will contain a unique ID that can be used to retrieve the key later using `GetByIdAsync`.
* The `ValidateKeyAsync` method will return `null` if the API key is not valid, or if the key does not exist.
* The `DisableKeyAsync`, `EnableKeyAsync`, and `RevokeKeyAsync` methods will return `false` if the API key does not exist, or if the operation fails for any other reason.
* The `GetConsumerKeysAsync` method will return an empty list if no API keys are associated with the consumer.
* The `UpdateKeyMetadataAsync` method will return `false` if the API key does not exist, or if the metadata update fails for any other reason.
* The `GetIpWhitelistAsync`, `SetIpWhitelistAsync`, `AddIpToWhitelistAsync`, and `RemoveIpFromWhitelistAsync` methods will return `false` if the API key does not exist, or if the IP whitelist operation fails for any other reason.

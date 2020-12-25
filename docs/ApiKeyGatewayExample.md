# ApiKeyGatewayExample
The `ApiKeyGatewayExample` type is designed to provide a comprehensive example of how to interact with API keys in a gateway context. It offers a range of methods for creating, listing, updating, and rotating API keys, as well as generating reports and running the gateway asynchronously. This type is intended to serve as a reference point for developers seeking to integrate API key management into their applications.

## API
### Constructors
* `public ApiKeyGatewayExample`: Initializes a new instance of the `ApiKeyGatewayExample` class.

### Methods
* `public async Task<ApiKeyResponse> CreateKeyAsync`: Creates a new API key asynchronously. Returns an `ApiKeyResponse` object containing information about the newly created key. Throws if the creation process fails.
* `public async Task<List<ApiKeyInfo>> ListConsumerKeysAsync`: Retrieves a list of API keys associated with a consumer asynchronously. Returns a list of `ApiKeyInfo` objects containing key information. Throws if the retrieval process fails.
* `public async Task<int> UpdateKeysAsync`: Updates API keys asynchronously. Returns the number of keys updated. Throws if the update process fails.
* `public async Task RotateConsumerKeysAsync`: Rotates API keys associated with a consumer asynchronously. Throws if the rotation process fails.
* `public async Task GenerateConsumerReportAsync`: Generates a report for a consumer asynchronously. Throws if the report generation process fails.
* `public async Task RunAsync`: Runs the gateway asynchronously. Throws if the execution process fails.
* `public static async Task Main`: The main entry point for the application.

### Properties
* `public string Id`: Gets the identifier of the API key gateway example.
* `public string DisplayKey`: Gets the display key of the API key gateway example.
* `public string ConsumerId`: Gets the identifier of the consumer associated with the API key gateway example.
* `public DateTime CreatedAt`: Gets the date and time when the API key gateway example was created.
* `public string Name`: Gets the name of the API key gateway example.
* `public string Status`: Gets the status of the API key gateway example.

## Usage
The following examples demonstrate how to use the `ApiKeyGatewayExample` type:
```csharp
// Create a new API key gateway example
var gatewayExample = new ApiKeyGatewayExample();

// Create a new API key
var apiKeyResponse = await gatewayExample.CreateKeyAsync();
Console.WriteLine($"API Key: {apiKeyResponse.Key}");

// List API keys associated with a consumer
var apiKeyInfos = await gatewayExample.ListConsumerKeysAsync();
foreach (var apiKeyInfo in apiKeyInfos)
{
    Console.WriteLine($"API Key: {apiKeyInfo.Key}");
}
```

## Notes
When using the `ApiKeyGatewayExample` type, consider the following edge cases and thread-safety remarks:
* The `CreateKeyAsync` method may throw if the creation process fails due to external factors such as network errors or database constraints.
* The `ListConsumerKeysAsync` method may return an empty list if no API keys are associated with the consumer.
* The `UpdateKeysAsync` method may throw if the update process fails due to external factors such as network errors or database constraints.
* The `RotateConsumerKeysAsync` method may throw if the rotation process fails due to external factors such as network errors or database constraints.
* The `GenerateConsumerReportAsync` method may throw if the report generation process fails due to external factors such as network errors or database constraints.
* The `RunAsync` method may throw if the execution process fails due to external factors such as network errors or database constraints.
* The `Main` method is the entry point for the application and should be called only once.
* The `ApiKeyGatewayExample` type is not thread-safe, and concurrent access to its methods and properties may result in unexpected behavior.

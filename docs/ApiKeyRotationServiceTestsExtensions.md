# ApiKeyRotationServiceTestsExtensions
The `ApiKeyRotationServiceTestsExtensions` class provides a set of extension methods to facilitate testing of the `ApiKeyRotationService` class. It offers a range of methods to set up mock repositories and services, create test API keys, and verify the behavior of the rotation service. These extensions aim to simplify the process of writing unit tests for the `ApiKeyRotationService` class.

## API
* `public static ApiKey WithTestValues`: Returns an `ApiKey` instance with test values. This method does not take any parameters and does not throw any exceptions.
* `public static Mock<IApiKeyRepository> SetupGetById`: Sets up a mock `IApiKeyRepository` to return a specific API key by ID. This method does not take any parameters and returns a `Mock<IApiKeyRepository>`. It does not throw any exceptions.
* `public static Mock<IApiKeyRepository> SetupGetKeys`: Sets up a mock `IApiKeyRepository` to return a collection of API keys. This method does not take any parameters and returns a `Mock<IApiKeyRepository>`. It does not throw any exceptions.
* `public static Mock<IApiKeyService> SetupCreateKey`: Sets up a mock `IApiKeyService` to create a new API key. This method does not take any parameters and returns a `Mock<IApiKeyService>`. It does not throw any exceptions.
* `public static Mock<IApiKeyService> SetupRevokeKey`: Sets up a mock `IApiKeyService` to revoke an existing API key. This method does not take any parameters and returns a `Mock<IApiKeyService>`. It does not throw any exceptions.
* `public static Mock<IApiKeyRepository> SetupUpdate`: Sets up a mock `IApiKeyRepository` to update an existing API key. This method does not take any parameters and returns a `Mock<IApiKeyRepository>`. It does not throw any exceptions.
* `public static void ShouldHaveRotated`: Verifies that the API key rotation was successful. This method does not take any parameters and does not return any value. It may throw an exception if the rotation was not successful.
* `public static void ShouldHaveFailed`: Verifies that the API key rotation failed. This method does not take any parameters and does not return any value. It may throw an exception if the rotation was successful.
* `public static ApiKeyRotationService BuildRotationService`: Builds an instance of the `ApiKeyRotationService` class. This method does not take any parameters and returns an `ApiKeyRotationService` instance. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `ApiKeyRotationServiceTestsExtensions` class:
```csharp
// Example 1: Verifying successful API key rotation
var apiKey = ApiKeyRotationServiceTestsExtensions.WithTestValues;
var apiKeyRepository = ApiKeyRotationServiceTestsExtensions.SetupGetById;
var apiKeyService = ApiKeyRotationServiceTestsExtensions.SetupCreateKey;
var rotationService = ApiKeyRotationServiceTestsExtensions.BuildRotationService;

// Rotate the API key
rotationService.RotateKey(apiKey);

// Verify that the rotation was successful
ApiKeyRotationServiceTestsExtensions.ShouldHaveRotated();
```

```csharp
// Example 2: Verifying failed API key rotation
var apiKey = ApiKeyRotationServiceTestsExtensions.WithTestValues;
var apiKeyRepository = ApiKeyRotationServiceTestsExtensions.SetupGetById;
var apiKeyService = ApiKeyRotationServiceTestsExtensions.SetupRevokeKey;
var rotationService = ApiKeyRotationServiceTestsExtensions.BuildRotationService;

// Attempt to rotate the API key
rotationService.RotateKey(apiKey);

// Verify that the rotation failed
ApiKeyRotationServiceTestsExtensions.ShouldHaveFailed();
```

## Notes
When using the `ApiKeyRotationServiceTestsExtensions` class, keep in mind the following edge cases and thread-safety considerations:
* The `WithTestValues` method returns a new `ApiKey` instance each time it is called, so it is safe to use in multi-threaded environments.
* The `SetupGetById`, `SetupGetKeys`, `SetupCreateKey`, `SetupRevokeKey`, and `SetupUpdate` methods return mock objects that can be used to set up test scenarios. These mock objects are not thread-safe and should not be shared across multiple threads.
* The `ShouldHaveRotated` and `ShouldHaveFailed` methods may throw exceptions if the rotation was not successful or failed, respectively. These exceptions should be caught and handled accordingly in the test code.
* The `BuildRotationService` method returns a new instance of the `ApiKeyRotationService` class each time it is called, so it is safe to use in multi-threaded environments. However, the instance returned by this method should not be shared across multiple threads, as it may not be thread-safe.

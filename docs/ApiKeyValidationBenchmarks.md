# ApiKeyValidationBenchmarks
The `ApiKeyValidationBenchmarks` type is designed to provide a set of benchmark tests for validating API keys. These tests cover various scenarios, including format validation, name validation, and quota validation, to ensure that API keys conform to the expected standards. By using these benchmarks, developers can verify that their API key validation logic is correct and robust.

## API
The `ApiKeyValidationBenchmarks` type exposes the following public members:
* `ValidateFormat_32Char_Valid`: Validates an API key with a 32-character format. This method returns a `ValidationResult` indicating whether the API key is valid.
* `ValidateFormat_64Char_Valid`: Validates an API key with a 64-character format. This method returns a `ValidationResult` indicating whether the API key is valid.
* `ValidateFormat_WeakEntropy`: Validates an API key with weak entropy. This method returns a `ValidationResult` indicating whether the API key is valid.
* `ValidateFormat_TooShort`: Validates an API key that is too short. This method returns a `ValidationResult` indicating whether the API key is valid.
* `ValidateName_Valid`: Validates an API key name that is valid. This method returns a `ValidationResult` indicating whether the API key name is valid.
* `ValidateName_TooLong`: Validates an API key name that is too long. This method returns a `ValidationResult` indicating whether the API key name is valid.
* `ValidateQuota_Valid`: Validates an API key quota that is valid. This method returns a `ValidationResult` indicating whether the API key quota is valid.

## Usage
Here are two examples of using the `ApiKeyValidationBenchmarks` type:
```csharp
// Example 1: Validating an API key format
var apiKey = "01234567890123456789012345678901";
var result = ApiKeyValidationBenchmarks.ValidateFormat_32Char_Valid;
if (result.IsValid)
{
    Console.WriteLine("API key format is valid");
}
else
{
    Console.WriteLine("API key format is invalid");
}

// Example 2: Validating an API key name
var apiKeyName = "MyApiKey";
var result = ApiKeyValidationBenchmarks.ValidateName_Valid;
if (result.IsValid)
{
    Console.WriteLine("API key name is valid");
}
else
{
    Console.WriteLine("API key name is invalid");
}
```

## Notes
When using the `ApiKeyValidationBenchmarks` type, note that the validation methods do not throw exceptions. Instead, they return a `ValidationResult` object indicating whether the API key is valid. Additionally, the validation methods are designed to be thread-safe, allowing them to be used concurrently in multi-threaded environments. However, it is still important to ensure that the API key validation logic is properly synchronized to avoid any potential issues. Edge cases, such as null or empty API key values, should be handled accordingly to prevent any unexpected behavior.

# ValidationHelpersTestsExtensions
The `ValidationHelpersTestsExtensions` class provides a set of static methods for validating various types of input data, such as email addresses, API keys, IP addresses, GUIDs, URLs, and sanitized input. These methods can be used in test scenarios to verify that the input data conforms to the expected formats, helping to ensure the correctness and robustness of the application.

## API
* `public static void AssertEmailValidity`: Verifies that the provided email address is valid. This method takes no parameters and does not return a value. It throws an exception if the email address is invalid.
* `public static void AssertApiKeyFormat`: Checks that the provided API key conforms to the expected format. This method takes no parameters and does not return a value. It throws an exception if the API key is malformed.
* `public static void AssertIpAddressValidity`: Validates the provided IP address. This method takes no parameters and does not return a value. It throws an exception if the IP address is invalid.
* `public static void AssertGuidValidity`: Verifies that the provided GUID is valid. This method takes no parameters and does not return a value. It throws an exception if the GUID is invalid.
* `public static void AssertUrlValidity`: Checks that the provided URL is valid. This method takes no parameters and does not return a value. It throws an exception if the URL is invalid.
* `public static void AssertSanitizedInput`: Validates that the provided input has been properly sanitized. This method takes no parameters and does not return a value. It throws an exception if the input is not sanitized.

## Usage
The following examples demonstrate how to use the `ValidationHelpersTestsExtensions` class in test scenarios:
```csharp
// Example 1: Validating an email address
try
{
    ValidationHelpersTestsExtensions.AssertEmailValidity();
    // Email address is valid
}
catch (Exception ex)
{
    // Email address is invalid
}

// Example 2: Validating an API key
try
{
    ValidationHelpersTestsExtensions.AssertApiKeyFormat();
    // API key is valid
}
catch (Exception ex)
{
    // API key is invalid
}
```

## Notes
When using the `ValidationHelpersTestsExtensions` class, consider the following edge cases and thread-safety remarks:
* The methods in this class are static, making them thread-safe for use in concurrent test scenarios.
* The methods do not perform any external validation (e.g., DNS lookups for email addresses or IP addresses), so they may not catch all possible invalid input cases.
* The `AssertSanitizedInput` method relies on the input being properly sanitized before calling this method, as it does not perform any sanitization itself.
* The methods in this class throw exceptions when the input is invalid, so it is essential to handle these exceptions properly in test code to avoid false positives or negatives.

# RequestValidatorValidation

The `RequestValidatorValidation` static class provides a set of validation utilities for common data types and constraints used in API request validation scenarios. It offers both validation methods that return detailed error lists and convenience methods that throw exceptions when validation fails, enabling flexible error handling in API gateway implementations.

## API

### Methods

#### `ValidateEmail(string email)`
Validates the format of an email address. Returns a list of validation error messages if the email is invalid, or an empty list if valid.
- **Parameters**: `email` – The email address string to validate.
- **Return value**: `IReadOnlyList<string>` – A list of error messages describing validation failures.
- **Throws**: Never throws exceptions; returns error list instead.

#### `ValidateUrl(string url)`
Validates the format of a URL string. Returns a list of validation error messages if the URL is invalid, or an empty list if valid.
- **Parameters**: `url` – The URL string to validate.
- **Return value**: `IReadOnlyList<string>` – A list of error messages describing validation failures.
- **Throws**: Never throws exceptions; returns error list instead.

#### `ValidateIpAddress(string ipAddress)`
Validates the format of an IP address (supports IPv4 and IPv6). Returns a list of validation error messages if the IP address is invalid, or an empty list if valid.
- **Parameters**: `ipAddress` – The IP address string to validate.
- **Return value**: `IReadOnlyList<string>` – A list of error messages describing validation failures.
- **Throws**: Never throws exceptions; returns error list instead.

#### `ValidateLength(string input, int minLength, int maxLength)`
Validates that the input string length falls within the specified inclusive range. Returns a list of validation error messages if the length is invalid, or an empty list if valid.
- **Parameters**:
  - `input` – The string to validate.
  - `minLength` – The minimum allowed length (inclusive).
  - `maxLength` – The maximum allowed length (inclusive).
- **Return value**: `IReadOnlyList<string>` – A list of error messages describing validation failures.
- **Throws**: Never throws exceptions; returns error list instead.

#### `ValidateRange(double value, double minValue, double maxValue)`
Validates that a numeric value falls within the specified inclusive range. Returns a list of validation error messages if the value is invalid, or an empty list if valid.
- **Parameters**:
  - `value` – The numeric value to validate.
  - `minValue` – The minimum allowed value (inclusive).
  - `maxValue` – The maximum allowed value (inclusive).
- **Return value**: `IReadOnlyList<string>` – A list of error messages describing validation failures.
- **Throws**: Never throws exceptions; returns error list instead.

#### `ValidateGuid(string guid)`
Validates the format of a GUID string. Returns a list of validation error messages if the GUID is invalid, or an empty list if valid.
- **Parameters**: `guid` – The GUID string to validate.
- **Return value**: `IReadOnlyList<string>` – A list of error messages describing validation failures.
- **Throws**: Never throws exceptions; returns error list instead.

#### `IsValidEmail(string email)`
Determines whether the specified email address is valid. Returns `true` if valid; otherwise, `false`.
- **Parameters**: `email` – The email address string to validate.
- **Return value**: `bool` – `true` if the email is valid; otherwise, `false`.
- **Throws**: Never throws exceptions.

#### `IsValidUrl(string url)`
Determines whether the specified URL string is valid. Returns `true` if valid; otherwise, `false`.
- **Parameters**: `url` – The URL string to validate.
- **Return value**: `bool` – `true` if the URL is valid; otherwise, `false`.
- **Throws**: Never throws exceptions.

#### `IsValidIpAddress(string ipAddress)`
Determines whether the specified IP address string is valid (supports IPv4 and IPv6). Returns `true` if valid; otherwise, `false`.
- **Parameters**: `ipAddress` – The IP address string to validate.
- **Return value**: `bool` – `true` if the IP address is valid; otherwise, `false`.
- **Throws**: Never throws exceptions.

#### `IsValidLength(string input, int minLength, int maxLength)`
Determines whether the length of the input string falls within the specified inclusive range. Returns `true` if valid; otherwise, `false`.
- **Parameters**:
  - `input` – The string to validate.
  - `minLength` – The minimum allowed length (inclusive).
  - `maxLength` – The maximum allowed length (inclusive).
- **Return value**: `bool` – `true` if the length is valid; otherwise, `false`.
- **Throws**: Never throws exceptions.

#### `IsValidRange(double value, double minValue, double maxValue)`
Determines whether the specified numeric value falls within the specified inclusive range. Returns `true` if valid; otherwise, `false`.
- **Parameters**:
  - `value` – The numeric value to validate.
  - `minValue` – The minimum allowed value (inclusive).
  - `maxValue` – The maximum allowed value (inclusive).
- **Return value**: `bool` – `true` if the value is valid; otherwise, `false`.
- **Throws**: Never throws exceptions.

#### `IsValidGuid(string guid)`
Determines whether the specified GUID string is valid. Returns `true` if valid; otherwise, `false`.
- **Parameters**: `guid` – The GUID string to validate.
- **Return value**: `bool` – `true` if the GUID is valid; otherwise, `false`.
- **Throws**: Never throws exceptions.

#### `EnsureValidEmail(string email)`
Validates the format of an email address. Throws an exception if the email is invalid.
- **Parameters**: `email` – The email address string to validate.
- **Throws**: Throws an exception (type not specified) if the email is invalid.

#### `EnsureValidUrl(string url)`
Validates the format of a URL string. Throws an exception if the URL is invalid.
- **Parameters**: `url` – The URL string to validate.
- **Throws**: Throws an exception (type not specified) if the URL is invalid.

#### `EnsureValidIpAddress(string ipAddress)`
Validates the format of an IP address (supports IPv4 and IPv6). Throws an exception if the IP address is invalid.
- **Parameters**: `ipAddress` – The IP address string to validate.
- **Throws**: Throws an exception (type not specified) if the IP address is invalid.

#### `EnsureValidLength(string input, int minLength, int maxLength)`
Validates that the input string length falls within the specified inclusive range. Throws an exception if the length is invalid.
- **Parameters**:
  - `input` – The string to validate.
  - `minLength` – The minimum allowed length (inclusive).
  - `maxLength` – The maximum allowed length (inclusive).
- **Throws**: Throws an exception (type not specified) if the length is invalid.

#### `EnsureValidRange(double value, double minValue, double maxValue)`
Validates that a numeric value falls within the specified inclusive range. Throws an exception if the value is invalid.
- **Parameters**:
  - `value` – The numeric value to validate.
  - `minValue` – The minimum allowed value (inclusive).
  - `maxValue` – The maximum allowed value (inclusive).
- **Throws**: Throws an exception (type not specified) if the value is invalid.

#### `EnsureValidGuid(string guid)`
Validates the format of a GUID string. Throws an exception if the GUID is invalid.
- **Parameters**: `guid` – The GUID string to validate.
- **Throws**: Throws an exception (type not specified) if the GUID is invalid.

## Usage

```csharp
// Example 1: Using validation methods to collect all errors
var email = "user@example.com";
var errors = RequestValidatorValidation.ValidateEmail(email);
if (errors.Count > 0)
{
    Console.WriteLine("Email validation failed:");
    foreach (var error in errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Example 2: Using Ensure methods for immediate failure
try
{
    var ip = "192.168.1.256";
    RequestValidatorValidation.EnsureValidIpAddress(ip);
}
catch (Exception ex)
{
    Console.WriteLine($"Invalid IP: {ex.Message}");
}
```

## Notes

- All validation methods are stateless and thread-safe; no shared mutable state is used.
- The `Validate*` methods return detailed error messages, suitable for API responses or logging.
- The `Ensure*` methods are convenience wrappers that throw exceptions on failure, useful for immediate validation in request pipelines.
- Edge cases such as null or empty strings are handled consistently: validation methods return appropriate error messages, while `IsValid*` methods return `false`.
- Range and length validations use inclusive bounds; ensure `minLength` ≤ `maxLength` and `minValue` ≤ `maxValue` to avoid logical errors.
- GUID validation is case-insensitive and supports both hyphenated and non-hyphenated formats.

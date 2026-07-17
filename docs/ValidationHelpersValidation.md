# ValidationHelpersValidation

Provides a centralized set of static validation utilities for common input types used throughout the API key gateway. Each validation target exposes three overloads: a method that returns a read-only list of validation error messages, a boolean property-style check, and a void method that throws an `ArgumentException` when validation fails. This design allows callers to choose between collecting multiple errors, performing a quick guard check, or enforcing strict preconditions.

## API

### ValidateEmail / IsValidEmail / EnsureValidEmail

- **`ValidateEmail(string input)`** – Validates the input as an email address. Returns a read-only list of error messages; an empty list indicates success.
- **`IsValidEmail(string input)`** – Returns `true` if the input is a valid email address; otherwise `false`.
- **`EnsureValidEmail(string input)`** – Throws `ArgumentException` if the input is not a valid email address.

### ValidateApiKey / IsValidApiKey / EnsureValidApiKey

- **`ValidateApiKey(string input)`** – Validates the input against the expected API key format. Returns a read-only list of error messages.
- **`IsValidApiKey(string input)`** – Returns `true` if the input matches the API key format; otherwise `false`.
- **`EnsureValidApiKey(string input)`** – Throws `ArgumentException` if the input is not a valid API key.

### ValidateIpAddress / IsValidIpAddress / EnsureValidIpAddress

- **`ValidateIpAddress(string input)`** – Validates the input as an IPv4 or IPv6 address. Returns a read-only list of error messages.
- **`IsValidIpAddress(string input)`** – Returns `true` if the input is a valid IP address; otherwise `false`.
- **`EnsureValidIpAddress(string input)`** – Throws `ArgumentException` if the input is not a valid IP address.

### ValidateGuid / IsValidGuid / EnsureValidGuid

- **`ValidateGuid(string input)`** – Validates the input as a GUID. Returns a read-only list of error messages.
- **`IsValidGuid(string input)`** – Returns `true` if the input is a valid GUID; otherwise `false`.
- **`EnsureValidGuid(string input)`** – Throws `ArgumentException` if the input is not a valid GUID.

### ValidateUrl / IsValidUrl / EnsureValidUrl

- **`ValidateUrl(string input)`** – Validates the input as an absolute URL. Returns a read-only list of error messages.
- **`IsValidUrl(string input)`** – Returns `true` if the input is a valid absolute URL; otherwise `false`.
- **`EnsureValidUrl(string input)`** – Throws `ArgumentException` if the input is not a valid absolute URL.

### ValidateSanitizeInput / IsValidSanitizeInput / EnsureValidSanitizeInput

- **`ValidateSanitizeInput(string input)`** – Validates that the input contains no potentially dangerous content (e.g., script tags, SQL fragments). Returns a read-only list of error messages.
- **`IsValidSanitizeInput(string input)`** – Returns `true` if the input passes sanitization checks; otherwise `false`.
- **`EnsureValidSanitizeInput(string input)`** – Throws `ArgumentException` if the input fails sanitization checks.

### Parameters

All methods accept a single `string input` parameter representing the value to validate. A `null` input is treated as invalid for all validators.

### Return Values

- **`Validate*` methods** return `IReadOnlyList<string>`. An empty list means the input is valid. Each element in a non-empty list describes a distinct validation failure.
- **`IsValid*` methods** return `bool`. `true` means valid; `false` means invalid.
- **`EnsureValid*` methods** return `void`. They throw `ArgumentException` with a message describing the first validation failure when the input is invalid.

### Exceptions

- All `EnsureValid*` methods throw `ArgumentException` when validation fails. The exception message includes the parameter name and the reason for failure.
- `Validate*` and `IsValid*` methods never throw exceptions; they always return a result.

## Usage

### Collecting multiple validation errors

```csharp
var errors = new List<string>();

errors.AddRange(ValidationHelpersValidation.ValidateEmail(userEmail));
errors.AddRange(ValidationHelpersValidation.ValidateApiKey(apiKey));
errors.AddRange(ValidationHelpersValidation.ValidateIpAddress(clientIp));

if (errors.Count > 0)
{
    throw new ValidationException(string.Join("; ", errors));
}
```

### Guard clause with immediate throw

```csharp
public void RegisterService(string serviceUrl, string apiKey)
{
    ValidationHelpersValidation.EnsureValidUrl(serviceUrl);
    ValidationHelpersValidation.EnsureValidApiKey(apiKey);

    // Proceed with registration
    _registry.Add(serviceUrl, apiKey);
}
```

## Notes

- All members are static and stateless, making them safe for concurrent access from multiple threads without any synchronization.
- The `Validate*` methods never return `null`; they return an empty list when validation passes.
- Inputs that are `null`, empty, or consist only of whitespace are treated as invalid for all validators.
- `ValidateIpAddress` accepts both IPv4 and IPv6 addresses; link-local and loopback addresses are considered valid unless explicitly excluded by the implementation.
- `ValidateUrl` requires an absolute URI; relative URLs and malformed URIs produce validation errors.
- `ValidateSanitizeInput` checks for common injection patterns but is not a substitute for proper output encoding or parameterized queries.

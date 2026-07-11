# ValidationHelpers

The `ValidationHelpers` static class provides a collection of utility methods for validating common data formats and sanitizing user input within the `api-key-gateway` project. Each method performs a single validation or sanitization task, returning a boolean result for checks or a sanitized string for input cleaning. These helpers are designed to be stateless and reusable across different components of the gateway.

## API

### `IsValidEmail`

```csharp
public static bool IsValidEmail(string email)
```

Validates whether the provided string is a syntactically correct email address according to standard email format rules.

- **Parameters**  
  `email`: The string to validate. Can be `null` or empty.
- **Returns**  
  `true` if the string is a non-null, non-empty, and valid email address; otherwise `false`.
- **Throws**  
  No exceptions are thrown. Invalid inputs (e.g., `null`, empty string, malformed addresses) simply return `false`.

### `IsValidApiKeyFormat`

```csharp
public static bool IsValidApiKeyFormat(string apiKey)
```

Checks whether the given string matches the expected API key format used by the gateway (e.g., a specific length, character set, or pattern).

- **Parameters**  
  `apiKey`: The string to validate. Can be `null` or empty.
- **Returns**  
  `true` if the string conforms to the defined API key format; otherwise `false`.
- **Throws**  
  No exceptions are thrown.

### `IsValidIpAddress`

```csharp
public static bool IsValidIpAddress(string ipAddress)
```

Determines whether the provided string is a valid IPv4 or IPv6 address.

- **Parameters**  
  `ipAddress`: The string to validate. Can be `null` or empty.
- **Returns**  
  `true` if the string is a valid IP address (either IPv4 or IPv6); otherwise `false`.
- **Throws**  
  No exceptions are thrown.

### `IsValidGuid`

```csharp
public static bool IsValidGuid(string guid)
```

Tests whether the given string can be parsed as a `System.Guid`.

- **Parameters**  
  `guid`: The string to validate. Can be `null` or empty.
- **Returns**  
  `true` if the string is a valid GUID representation (including hyphens and braces); otherwise `false`.
- **Throws**  
  No exceptions are thrown.

### `IsValidUrl`

```csharp
public static bool IsValidUrl(string url)
```

Validates whether the provided string is a well-formed absolute URL (e.g., `https://example.com/path`).

- **Parameters**  
  `url`: The string to validate. Can be `null` or empty.
- **Returns**  
  `true` if the string is a valid absolute URL; otherwise `false`.
- **Throws**  
  No exceptions are thrown.

### `SanitizeInput`

```csharp
public static string SanitizeInput(string input)
```

Removes or neutralizes potentially dangerous characters from the input string to prevent injection attacks (e.g., HTML/script tags, SQL metacharacters). The exact sanitization rules are implementation-defined but typically strip or encode characters such as `<`, `>`, `"`, `'`, `&`, and control characters.

- **Parameters**  
  `input`: The string to sanitize. Can be `null`.
- **Returns**  
  A sanitized version of the input string. If `input` is `null`, returns an empty string.
- **Throws**  
  No exceptions are thrown.

## Usage

### Example 1: Validating user registration input

```csharp
using ApiKeyGateway.Validation;

public class RegistrationService
{
    public bool TryRegister(string email, string apiKey, string ipAddress)
    {
        if (!ValidationHelpers.IsValidEmail(email))
            return false;

        if (!ValidationHelpers.IsValidApiKeyFormat(apiKey))
            return false;

        if (!ValidationHelpers.IsValidIpAddress(ipAddress))
            return false;

        // Proceed with registration logic...
        return true;
    }
}
```

### Example 2: Sanitizing and validating a URL from user input

```csharp
using ApiKeyGateway.Validation;

public class UrlProcessor
{
    public string ProcessUserUrl(string rawUrl)
    {
        string sanitized = ValidationHelpers.SanitizeInput(rawUrl);

        if (!ValidationHelpers.IsValidUrl(sanitized))
            throw new ArgumentException("The provided URL is not valid after sanitization.");

        return sanitized;
    }
}
```

## Notes

- **Edge cases**: All validation methods treat `null` and empty strings as invalid (return `false`). `SanitizeInput` returns an empty string for `null` input, which may then fail subsequent validation checks. This behavior is intentional to avoid null-reference exceptions and to force callers to handle missing input explicitly.
- **Thread safety**: Because `ValidationHelpers` contains only static methods and no mutable state, all members are inherently thread-safe. Multiple threads can call any method concurrently without synchronization.
- **Format specifics**: The exact patterns used by `IsValidApiKeyFormat` and the sanitization rules of `SanitizeInput` are defined by the gateway’s configuration. Refer to the project’s configuration documentation for details on the expected API key format and the list of sanitized characters.

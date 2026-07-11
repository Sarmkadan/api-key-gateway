# StringExtensions

The `StringExtensions` class provides a collection of static utility methods designed to extend the functionality of the standard .NET `string` type within the `api-key-gateway` project. These helpers address common string manipulation, validation, and parsing scenarios encountered in API key processing, slug generation, and input sanitization, offering a concise syntax for operations such as truncation, numeric validation, and list conversion without requiring external dependencies.

## API

### `Truncate`
Truncates a string to a specified maximum length. If the input string exceeds the defined length, it is cut off at that limit; otherwise, the original string is returned unchanged.
*   **Parameters**: `string value`, `int maxLength`
*   **Returns**: `string` – The truncated string or the original string if no truncation was necessary.
*   **Throws**: `ArgumentNullException` if `value` is null; `ArgumentOutOfRangeException` if `maxLength` is negative.

### `TruncateWithEllipsis`
Truncates a string to a specified maximum length and appends an ellipsis ("...") if truncation occurs. The returned string length will not exceed `maxLength`, meaning the content is shortened to accommodate the ellipsis characters if needed.
*   **Parameters**: `string value`, `int maxLength`
*   **Returns**: `string` – The truncated string with an ellipsis suffix, or the original string if it fits within the limit.
*   **Throws**: `ArgumentNullException` if `value` is null; `ArgumentOutOfRangeException` if `maxLength` is less than the length of the ellipsis string.

### `ContainsAny`
Determines whether the source string contains any of the specified substrings.
*   **Parameters**: `string source`, `IEnumerable<string> values`
*   **Returns**: `bool` – `true` if at least one substring from `values` is found within `source`; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if `source` or `values` is null.

### `StartsWithAny`
Determines whether the source string starts with any of the specified prefixes.
*   **Parameters**: `string source`, `IEnumerable<string> prefixes`
*   **Returns**: `bool` – `true` if `source` begins with at least one string from `prefixes`; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if `source` or `prefixes` is null.

### `ToSlug`
Converts a string into a URL-friendly slug by lowercasing the input, replacing non-alphanumeric characters with hyphens, and removing consecutive hyphens.
*   **Parameters**: `string value`
*   **Returns**: `string` – The formatted slug.
*   **Throws**: `ArgumentNullException` if `value` is null.

### `CapitalizeFirst`
Returns a copy of the string with the first character converted to uppercase and the remaining characters unchanged. If the string is empty, it returns the empty string.
*   **Parameters**: `string value`
*   **Returns**: `string` – The string with the first letter capitalized.
*   **Throws**: `ArgumentNullException` if `value` is null.

### `ToList`
Splits a string into a list of substrings based on a specified separator. Empty entries are typically removed from the resulting list.
*   **Parameters**: `string value`, `char separator` (or `string separator` depending on overload implementation)
*   **Returns**: `List<string>` – A list containing the split substrings.
*   **Throws**: `ArgumentNullException` if `value` is null.

### `IsNumeric`
Validates whether the entire string represents a valid numeric value (integer or decimal).
*   **Parameters**: `string value`
*   **Returns**: `bool` – `true` if the string is a valid number; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if `value` is null.

### `TryParseInt`
Attempts to convert the string representation of a number to its 32-bit signed integer equivalent.
*   **Parameters**: `string value`
*   **Returns**: `int?` – The parsed integer if successful; `null` if the conversion fails.
*   **Throws**: No exceptions thrown; failures are indicated by a `null` return value.

### `TryParseLong`
Attempts to convert the string representation of a number to its 64-bit signed integer equivalent.
*   **Parameters**: `string value`
*   **Returns**: `long?` – The parsed long integer if successful; `null` if the conversion fails.
*   **Throws**: No exceptions thrown; failures are indicated by a `null` return value.

## Usage

The following examples demonstrate typical usage patterns for validating API key formats and sanitizing user input for display or URL routing.

```csharp
using System;
using System.Collections.Generic;

// Example 1: Validating and parsing configuration values
public class ConfigValidator
{
    public void ProcessSettings(string timeoutStr, string allowedPrefixesRaw)
    {
        // Safely parse an integer timeout without try-catch blocks
        int? timeout = StringExtensions.TryParseInt(timeoutStr);
        
        if (timeout.HasValue && timeout > 0)
        {
            Console.WriteLine($"Timeout set to {timeout}ms");
        }

        // Check if an API key starts with any allowed vendor prefixes
        var prefixes = StringExtensions.ToList(allowedPrefixesRaw, ',');
        string incomingKey = "stripe_sk_test_12345";
        
        if (StringExtensions.StartsWithAny(incomingKey, prefixes))
        {
            Console.WriteLine("Key prefix validated.");
        }
    }
}
```

```csharp
// Example 2: Sanitizing and formatting output for logs or URLs
public class DisplayFormatter
{
    public string FormatUserInput(string rawTitle, string description)
    {
        // Generate a URL-safe slug from a title
        string slug = StringExtensions.ToSlug(rawTitle);
        
        // Capitalize the first letter for display headers
        string header = StringExtensions.CapitalizeFirst(description);
        
        // Truncate long descriptions for preview cards, ensuring no layout breakage
        string preview = StringExtensions.TruncateWithEllipsis(description, 100);
        
        return $"{slug}: {header} - {preview}";
    }
}
```

## Notes

*   **Null Handling**: All methods accepting a `string` as the primary input argument throw `ArgumentNullException` if passed `null`, with the exception of `TryParseInt` and `TryParseLong`, which return `null` to indicate failure. Callers must ensure inputs are non-null or handle the specific exception/return logic accordingly.
*   **Thread Safety**: As this class consists entirely of static methods that operate on immutable string instances and do not maintain internal state, all members are inherently thread-safe and can be called concurrently from multiple threads without synchronization.
*   **Edge Cases**:
    *   `TruncateWithEllipsis` requires the `maxLength` parameter to be large enough to accommodate the ellipsis characters; passing a length smaller than the ellipsis string will result in an exception.
    *   `IsNumeric` returns `false` for empty strings, whitespace-only strings, or strings containing culture-specific formatting not recognized by the invariant parser.
    *   `ToSlug` may return an empty string if the input contains no alphanumeric characters.

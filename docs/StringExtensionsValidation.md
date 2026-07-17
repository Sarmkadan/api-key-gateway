# StringExtensionsValidation

`StringExtensionsValidation` is a static helper class that validates the arguments passed to the string extension methods defined in the project.  
Each validation method returns a collection of error messages or a boolean flag, while the *Ensure* methods throw an `ArgumentException` when the supplied arguments are not valid.  The class is intentionally lightweight, stateless, and thread‑safe, making it suitable for use in any context where argument validation is required before invoking the corresponding string extension logic.

## API

| Member | Purpose | Parameters | Return Value | Throws |
|--------|---------|------------|--------------|--------|
| `public static IReadOnlyList<string> ValidateTruncateParameters(string value, int maxLength)` | Validates the arguments for the `Truncate` extension. | `value` – the string to be truncated.<br>`maxLength` – the maximum allowed length. | A list of error messages; empty if the arguments are valid. | None |
| `public static IReadOnlyList<string> ValidateTruncateWithEllipsisParameters(string value, int maxLength)` | Validates the arguments for the `TruncateWithEllipsis` extension. | `value` – the string to be truncated.<br>`maxLength` – the maximum allowed length. | A list of error messages; empty if the arguments are valid. | None |
| `public static IReadOnlyList<string> ValidateContainsAnyParameters(string value, IEnumerable<string> substrings)` | Validates the arguments for the `ContainsAny` extension. | `value` – the string to search.<br>`substrings` – the collection of substrings to look for. | A list of error messages; empty if the arguments are valid. | None |
| `public static IReadOnlyList<string> ValidateStartsWithAnyParameters(string value, IEnumerable<string> prefixes)` | Validates the arguments for the `StartsWithAny` extension. | `value` – the string to test.<br>`prefixes` – the collection of prefixes to check. | A list of error messages; empty if the arguments are valid. | None |
| `public static IReadOnlyList<string> ValidateToListParameters(string value, char separator)` | Validates the arguments for the `ToList` extension. | `value` – the string to split.<br>`separator` – the character used as a delimiter. | A list of error messages; empty if the arguments are valid. | None |
| `public static bool IsValidTruncateParameters(string value, int maxLength)` | Returns `true` if the arguments for `Truncate` are valid. | Same as `ValidateTruncateParameters`. | `true` if no validation errors; otherwise `false`. | None |
| `public static bool IsValidTruncateWithEllipsisParameters(string value, int maxLength)` | Returns `true` if the arguments for `TruncateWithEllipsis` are valid. | Same as `ValidateTruncateWithEllipsisParameters`. | `true` if no validation errors; otherwise `false`. | None |
| `public static bool IsValidContainsAnyParameters(string value, IEnumerable<string> substrings)` | Returns `true` if the arguments for `ContainsAny` are valid. | Same as `ValidateContainsAnyParameters`. | `true` if no validation errors; otherwise `false`. | None |
| `public static bool IsValidStartsWithAnyParameters(string value, IEnumerable<string> prefixes)` | Returns `true` if the arguments for `StartsWithAny` are valid. | Same as `ValidateStartsWithAnyParameters`. | `true` if no validation errors; otherwise `false`. | None |
| `public static bool IsValidToListParameters(string value, char separator)` | Returns `true` if the arguments for `ToList` are valid. | Same as `ValidateToListParameters`. | `true` if no validation errors; otherwise `false`. | None |
| `public static void EnsureValidTruncateParameters(string value, int maxLength)` | Throws an `ArgumentException` if the arguments for `Truncate` are invalid. | Same as `ValidateTruncateParameters`. | None | `ArgumentException` with a message composed of the validation errors. |
| `public static void EnsureValidTruncateWithEllipsisParameters(string value, int maxLength)` | Throws an `ArgumentException` if the arguments for `TruncateWithEllipsis` are invalid. | Same as `ValidateTruncateWithEllipsisParameters`. | None | `ArgumentException` with a message composed of the validation errors. |
| `public static void EnsureValidContainsAnyParameters(string value, IEnumerable<string> substrings)` | Throws an `ArgumentException` if the arguments for `ContainsAny` are invalid. | Same as `ValidateContainsAnyParameters`. | None | `ArgumentException` with a message composed of the validation errors. |
| `public static void EnsureValidStartsWithAnyParameters(string value, IEnumerable<string> prefixes)` | Throws an `ArgumentException` if the arguments for `StartsWithAny` are invalid. | Same as `ValidateStartsWithAnyParameters`. | None | `ArgumentException` with a message composed of the validation errors. |
| `public static void EnsureValidToListParameters(string value, char separator)` | Throws an `ArgumentException` if the arguments for `ToList` are invalid. | Same as `ValidateToListParameters`. | None | `ArgumentException` with a message composed of the validation errors. |

## Usage

```csharp
using ApiKeyGateway.Extensions;

// Example 1: Truncate a string safely
string input = "Hello, world!";
int maxLength = 5;

if (StringExtensionsValidation.IsValidTruncateParameters(input, maxLength))
{
    string truncated = input.Truncate(maxLength); // "Hello"
}
else
{
    // Handle invalid arguments
    var errors = StringExtensionsValidation.ValidateTruncateParameters(input, maxLength);
    throw new InvalidOperationException(string.Join("; ", errors));
}
```

```csharp
using ApiKeyGateway.Extensions;

// Example 2: Split a comma‑separated list into a collection
string csv = "apple,banana,cherry";
char separator = ',';

StringExtensionsValidation.EnsureValidToListParameters(csv, separator);
IEnumerable<string> fruits = csv.ToList(separator); // ["apple", "banana", "cherry"]
```

## Notes

* **Null and empty values** – All validation methods treat a `null` string as invalid and return an error message.  An empty string is considered valid for most operations, except when a non‑empty value is required by the corresponding extension method.
* **Negative or zero lengths** – For truncation methods, a `maxLength` less than or equal to zero is considered invalid.
* **Empty collections** – For `ContainsAny` and `StartsWithAny`, an empty `IEnumerable<string>` is invalid because the operation would always return `false`.
* **Separator validation** – The `ToList` validation treats the separator character as always valid; however, passing a separator that does not appear in the string simply results in a single‑element list containing the original string.
* **Thread safety** – All members are static, stateless, and perform only local computations.  They can be called concurrently from multiple threads without side effects.
* **Performance** – The validation methods perform minimal checks and return immutable lists (`IReadOnlyList<string>`), making them inexpensive to call even in high‑throughput scenarios.

---
# StringExtensionsTestsExtensions

This static class provides a collection of helper methods used in unit tests for string manipulation within the `api-key-gateway` project. The methods are intended to simplify common test scenarios such as checking for substrings, generating test data, and creating slugs or truncated strings for validation.

## API

### ContainsAny
```csharp
public static bool ContainsAny(this string input, IEnumerable<string> candidates)
```
Determines whether `input` contains at least one of the strings in `candidates`.  
- **Parameters**  
  - `input`: The string to be searched.  
  - `candidates`: A collection of substrings to look for.  
- **Return value**: `true` if any candidate is found within `input`; otherwise `false`.  
- **Exceptions**:  
  - `ArgumentNullException` if `input` is `null`.  
  - `ArgumentNullException` if `candidates` is `null`.  

### StartsWithAny
```csharp
public static bool StartsWithAny(this string input, IEnumerable<string> prefixes)
```
Determines whether `input` starts with any of the strings in `prefixes`.  
- **Parameters**  
  - `input`: The string to be examined.  
  - `prefixes`: A collection of possible prefixes.  
- **Return value**: `true` if `input` begins with any prefix; otherwise `false`.  
- **Exceptions**:  
  - `ArgumentNullException` if `input` is `null`.  
  - `ArgumentNullException` if `prefixes` is `null`.  

### ToSlug
```csharp
public static string ToSlug(this string input)
```
Converts `input` into a URL‑friendly slug by lowercasing, trimming whitespace, replacing spaces and underscores with hyphens, and removing non‑alphanumeric characters except hyphens.  
- **Parameters**  
  - `input`: The string to slugify.  
- **Return value**: A slug representation of `input`. Returns an empty string if `input` is `null` or consists only of invalid characters.  
- **Exceptions**: None (handles `null` gracefully).  

### Truncate
```csharp
public static string? Truncate(this string input, int maxLength)
```
Returns `input` truncated to at most `maxLength` characters. If `input` is shorter than `maxLength`, the original string is returned.  
- **Parameters**  
  - `input`: The string to truncate; may be `null`.  
  - `maxLength`: The maximum length of the returned string. Must be non‑negative.  
- **Return value**: The truncated string, or `null` when `input` is `null`.  
- **Exceptions**:  
  - `ArgumentOutOfRangeException` if `maxLength` is less than zero.  

### TruncateWithEllipsis
```csharp
public static string? TruncateWithEllipsis(this string input, int maxLength)
```
Returns `input` truncated to at most `maxLength` characters and appends an ellipsis (`…`) when truncation occurs. If the original string fits within `maxLength`, it is returned unchanged.  
- **Parameters**  
  - `input`: The string to process; may be `null`.  
  - `maxLength`: The maximum length of the returned string **including** the ellipsis. Must be non‑negative.  
- **Return value**: The truncated string with ellipsis when needed, or `null` when `input` is `null`.  
- **Exceptions**:  
  - `ArgumentOutOfRangeException` if `maxLength` is less than zero.  

### CreateTestString
```csharp
public static string CreateTestString(int length, char filler = 'x')
```
Generates a deterministic test string of the specified `length` composed of the `filler` character. Useful for reproducible test data.  
- **Parameters**  
  - `length`: Desired length of the resulting string; must be non‑negative.  
  - `filler`: Character used to fill the string; defaults to `'x'`.  
- **Return value**: A string of length `length` consisting of repeated `filler` characters.  
- **Exceptions**:  
  - `ArgumentOutOfRangeException` if `length` is negative.  

### RepeatPattern
```csharp
public static string RepeatPattern(string pattern, int count)
```
Concatenates `pattern` `count` times. If `count` is zero, returns an empty string.  
- **Parameters**  
  - `pattern`: The substring to repeat; may be `null` (treated as empty).  
  - `count`: Number of repetitions; must be non‑negative.  
- **Return value**: A new string containing `pattern` repeated `count` times.  
- **Exceptions**:  
  - `ArgumentOutOfRangeException` if `count` is negative.  

### CreateEdgeCaseString
```csharp
public static string CreateEdgeCaseString(int length, bool includeUnicode = false, bool includeWhitespace = false)
```
Produces a string designed to exercise edge‑case handling in parsers or validators. The string may contain control characters, surrogate pairs, or whitespace depending on the flags).  
- **Parameters**  
  - `length`: Target length of the string; must be non‑negative.  
  - `includeUnicode`: When `true`, the string may contain Unicode characters outside the BMP.  
  - `includeWhitespace`: When `true`, the string may contain spaces, tabs, and line breaks.  
- **Return value**: A string of approximately `length` characters meeting the requested characteristics.  
- **Exceptions**:  
  - `ArgumentOutOfRangeException` if `length` is negative.  

## Usage

```csharp
using static api_key_gateway.tests.StringExtensionsTestsExtensions;

// Check if a username contains any forbidden substring
string username = "admin_user";
bool isInvalid = username.ContainsAny(new[] {"admin", "root", "super"});
// isInvalid == true

// Generate a slug for a blog title
string title = "  C# 10: New Features!!  ";
string slug = title.ToSlug();
// slug == "c-10-new-features"

// Create a deterministic test payload of 250 'x' characters
string payload = CreateTestString(250);
// payload.Length == 250 and payload consists only of 'x'
```

```csharp
using static api_key_gateway.tests.StringExtensionsTestsExtensions;

// Truncate a long log line with ellipsis for display
string logLine = TimestampProvider.Now + ": " + DetailedMessageBuilder.Build();
string display = logLine.TruncateWithEllipsis(80);
// display is at most 80 characters, ends with "…" if truncated

// Repeat a test pattern to stress‑test a buffer
string pattern = "AB";
string repeated = RepeatPattern(pattern, 5000);
// repeated.Length == 10000
```

## Notes

- All extension methods are **pure**: they do not modify the input string and have no side effects, making them safe to call concurrently from multiple threads.  
- Methods that accept `null` for the string argument either return `null` (`Truncate`, `TruncateWithEllipsis`) or treat it as an empty sequence (`RepeatPattern`). Consumers should still guard against `null` where a non‑nullable return is expected (`ContainsAny`, `StartsWithAny`, `ToSlug`, `CreateTestString`, `CreateEdgeCaseString`).  
- `ToSlug` returns an empty string for inputs that contain no permissible slug characters; callers should verify the result if an empty slug is invalid in their context.  
- `CreateEdgeCaseString` aims for reproducibility; given the same parameters it will produce the same string each invocation, which aids deterministic unit tests.  
- The `maxLength` arguments in `Truncate` and `TruncateWithEllipsis` count **characters**, not UTF‑16 code units; surrogate pairs are counted as a single character for the purpose of length calculation.  
- No method allocates additional storage beyond what is necessary to produce the returned string; however, repeated calls with large `length` or `count` values may cause noticeable GC pressure. Use appropriately sized values in test scenarios.

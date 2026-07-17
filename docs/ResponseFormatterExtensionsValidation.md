# ResponseFormatterExtensionsValidation

Provides validation methods for response formatter configuration types, ensuring that formatter definitions meet the structural and semantic requirements of the API key gateway's response pipeline. These extension methods allow callers to check validity, retrieve detailed validation errors, or enforce validity with exceptions.

## API

### Validate<T>
```csharp
public static IReadOnlyList<string> Validate<T>(this T formatter) where T : class
```
Validates the specified formatter instance and returns a read-only list of validation error messages. If the formatter is valid, the returned list is empty.

**Parameters:**
- `formatter`: The formatter instance to validate. Must not be null.

**Return Value:**
A read-only list of strings, each describing a distinct validation failure. An empty list indicates the formatter is valid.

**Exceptions:**
- `ArgumentNullException`: Thrown when `formatter` is null.

---

### `IsValid<T>`
```csharp
public static bool IsValid<T>(this T response) where T : class
```
Determines whether the specified formatter instance passes all validation rules.

**Parameters:**
- `formatter`: The formatter instance to validate. Must not be null.

**Return Value:**
`true` if the formatter is valid; otherwise `false`.

**Exceptions:**
- `ArgumentNullException`: Thrown when `formatter` is null.

---

### `EnsureValid<T>`
```csharp
public static void EnsureValid<T>(this T response) where T : class
```
Enforces that the formatter instance is valid, throwing an exception if any validation errors are detected.

**Parameters:**
- `formatter`: The formatter instance to validate. Must not be null.

**Exceptions:**
- `ArgumentNullException`: Thrown when `formatter` is null.
- `ValidationException` or a derived type: Thrown when one or more validation errors are present, typically aggregating all error messages.

---

### `Validate<T>` (second overload)
```csharp
public static IReadOnlyList<string> Validate<T>(this T response) where T : class
```
Validates the specified formatter instance and returns a read-only list of validation error messages. This overload may apply additional context-specific rules compared to the first overload, depending on the generic type constraints resolved at runtime.

**Parameters:**
- `formatter`: The formatter instance to validate. Must not be null.

**Return Value:**
A read-only list of validation error strings. An empty list indicates validity.

**Exceptions:**
- `ArgumentNullException`: Thrown when `formatter` is null.

---

### `IsValid<T>` (second overload)
```csharp
public static bool IsValid<T>(this T response) where T : class
```
Determines whether the formatter instance is valid according to the extended validation rules applied by the second `Validate` overload.

**Parameters:**
- `formatter`: The formatter instance to validate. Must not be null.

**Return Value:**
`true` if valid; otherwise `false`.

**Exceptions:**
- `ArgumentNullException`: Thrown when `formatter` is null.

---

### `EnsureValid<T>` (second overload)
```csharp
public static void EnsureValid<T>(this T response) where T : class
```
Enforces validity using the second set of validation rules, throwing an exception if the formatter is invalid.

**Parameters:**
- `formatter`: The formatter instance to validate. Must not be null.

**Exceptions:**
- `ArgumentNullException`: Thrown when `formatter` is null.
- `ValidationError` or similar type: Thrown when validation errors are present.

## Usage

**Example 1: Checking validity before applying a formatter**
```csharp
var formatter = new JsonResponseFormatter
{
    Indent = true,
    DateFormat = "ISO8601"
};

if (formatter.IsValid())
{
    pipeline.ApplyFormatter(formatter);
}
else
{
    var errors = formatter.Validate();
    logger.LogWarning("Formatter validation failed: {Errors}", string.Join("; ", errors));
}
```

**Example 2: Enforcing validity during configuration loading**
```csharp
public void ConfigurePipeline(ResponseFormatterBase formatter)
{
    // Throws immediately if the formatter is misconfigured
    formatter.EnsureValid();

    // Proceed with confidence that the formatter is valid
    responsePipeline.SetFormatter(formatter);
}
```

## Notes

- All methods require a non-null `formatter` argument; passing null will always result in an `ArgumentNullException`.
- The two sets of overloads likely correspond to different validation contexts (e.g., basic structural validation versus full semantic validation), but the exact distinction depends on the runtime type resolution of `T`.
- `Validate` returns an empty list for valid instances, never null. Callers can safely iterate over the result without null checks.
- `EnsureValid` is the strictest method and should be used at configuration boundaries where invalid state must be rejected immediately.
- These methods are extension methods and are invoked on instances of any class that satisfies the generic constraint.
- Thread-safety: These methods are static and do not mutate shared state. They are safe to call concurrently from multiple threads, provided the `formatter` instance itself is not being mutated during validation.

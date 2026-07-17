# ApiKeysControllerValidation

Provides static validation logic for API key controller operations. This class exposes a set of overloaded methods that validate, check, and enforce constraints on API key-related data, returning structured error lists, boolean validity flags, or throwing exceptions when validation fails.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(...)
```

Validates the provided input and returns a read-only list of error messages. An empty list indicates successful validation.

**Parameters**  
The method accepts various input types relevant to API key controller operations (exact parameter lists depend on the specific overload).

**Returns**  
`IReadOnlyList<string>` — A list of validation error messages. An empty list means the input is valid.

**Exceptions**  
None. All validation failures are returned as error strings rather than thrown.

---

### IsValid

```csharp
public static bool IsValid(...)
```

Checks whether the provided input is valid according to the same rules as `Validate`.

**Parameters**  
The method accepts various input types matching the corresponding `Validate` overloads.

**Returns**  
`bool` — `true` if the input passes all validation rules; otherwise `false`.

**Exceptions**  
None.

---

### EnsureValid

```csharp
public static void EnsureValid(...)
```

Validates the input and throws an exception if validation fails. This is a fail-fast variant intended for scenarios where invalid data should immediately halt execution.

**Parameters**  
The method accepts various input types matching the corresponding `Validate` overloads.

**Exceptions**  
Throws an exception (likely `ArgumentException` or a custom validation exception) when validation fails. The exception message typically includes the first error or an aggregated list of errors.

## Usage

### Example 1: Checking validity before processing

```csharp
if (ApiKeysControllerValidation.IsValid(apiKeyRequest))
{
    // Proceed with creating or updating the API key
    apiKeyService.Process(apiKeyRequest);
}
else
{
    var errors = ApiKeysControllerValidation.Validate(apiKeyRequest);
    foreach (var error in errors)
    {
        logger.LogWarning("Validation failed: {Error}", error);
    }
}
```

### Example 2: Fail-fast with EnsureValid

```csharp
try
{
    ApiKeysControllerValidation.EnsureValid(incomingDto);
    // If we reach here, the DTO is valid
    controller.CreateApiKey(incomingDto);
}
catch (Exception ex)
{
    return BadRequest(ex.Message);
}
```

## Notes

- All methods are static and stateless, making them inherently **thread-safe**. No shared mutable state is involved.
- The `Validate` method never throws; it always returns a collection of errors. Use it when you need to accumulate or log all validation issues.
- The `EnsureValid` method throws on the first validation failure (or aggregates all failures into a single exception, depending on the implementation). Do not use it in loops where partial validation results are needed.
- The `IsValid` method is a convenience boolean check that internally relies on the same logic as `Validate`. It is suitable for guard clauses and conditional branching.
- Edge cases: Empty collections, null references, and boundary values (e.g., empty strings, zero-length arrays) are handled according to the validation rules defined in each overload. An empty input that is semantically valid will return an empty error list from `Validate` and `true` from `IsValid`.

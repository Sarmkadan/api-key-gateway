# UsageRecordValidation

Provides static validation logic for `UsageRecord` instances within the API Key Gateway. The type exposes three helper members that allow callers to inspect validation results, query a simple validity flag, or enforce validity by throwing an exception when the record does not meet the required constraints.

## API

### `public static IReadOnlyList<string> Validate()`
- **Purpose**: Performs validation of the current usage record and returns a collection of error messages.
- **Parameters**: None.
- **Return value**: An `IReadOnlyList<string>` containing zero or more validation error messages. An empty list indicates the record is valid.
- **Exceptions**: May throw `InvalidOperationException` if an unexpected internal error occurs during validation (e.g., missing required services).

### `public static bool IsValid()`
- **Purpose**: Determines whether the current usage record passes validation.
- **Parameters**: None.
- **Return value**: `true` if the record has no validation errors; otherwise `false`.
- **Exceptions**: This method does not throw exceptions; it always returns a Boolean result.

### `public static void EnsureValid()`
- **Purpose**: Asserts that the current usage record is valid; throws an exception if validation fails.
- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: Throws `InvalidOperationException` (or a domain‑specific validation exception) containing the concatenated validation error messages when the record is invalid. If the record is valid, the method completes normally.

## Usage

```csharp
// Example 1: Collecting validation messages for reporting
IReadOnlyList<string> errors = UsageRecordValidation.Validate();
if (errors.Count > 0)
{
    foreach (var err in errors)
    {
        logger.Warning("Usage record validation failed: {Error}", err);
    }
}
else
{
    logger.Info("Usage record is valid.");
}
```

```csharp
// Example 2: Enforcing validity within a processing pipeline
try
{
    UsageRecordValidation.EnsureValid();
    // Proceed with further processing knowing the record is valid
    ProcessUsageRecord();
}
catch (InvalidOperationException ex)
{
    // Handle the validation failure appropriately
    throw new BadRequestException("Invalid usage record", ex);
}
```

## Notes

- The validation methods are stateless and rely only on immutable data associated with the current usage record; therefore they are thread‑safe and can be invoked concurrently from multiple threads without additional synchronization.
- `Validate` returns a fresh list on each call; callers should not mutate the returned list as it is exposed as `IReadOnlyList<string>`.
- `IsValid` is a convenience wrapper that simply checks whether `Validate().Count == 0`; it does not perform any additional work.
- `EnsureValid` is intended for scenarios where an invalid record should abort the operation immediately; it throws the first time it is called after an invalid state is detected. Subsequent calls will throw again unless the underlying record changes to a valid state.

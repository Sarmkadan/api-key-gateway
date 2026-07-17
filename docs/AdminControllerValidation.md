# AdminControllerValidation

Provides centralized validation logic for admin controller inputs within the API key gateway. It exposes static methods to check validity, retrieve all validation errors, and assert validity with an exception on failure. The type is designed to be used synchronously and does not maintain any instance state.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(/* parameters specific to admin controller context */)
```

Returns a read-only list of validation error messages. An empty list indicates that the input is fully valid. This overload is one of two context-specific validations exposed by the type; each accepts parameters relevant to a particular admin operation.

- **Parameters:** Input values required for the admin controller operation being validated (exact signature depends on the overload).
- **Returns:** `IReadOnlyList<string>` containing zero or more human-readable error descriptions.
- **Exceptions:** Does not throw by design; all failures are communicated through the returned list.

---

### IsValid

```csharp
public static bool IsValid(/* parameters specific to admin controller context */)
```

Convenience predicate that returns `true` when the associated `Validate` overload would produce an empty error list, and `false` otherwise. Internally delegates to `Validate` and checks for any errors.

- **Parameters:** Same as the corresponding `Validate` overload.
- **Returns:** `true` if validation passes; `false` if one or more errors exist.
- **Exceptions:** Does not throw; any exception from the underlying `Validate` call would propagate, but `Validate` itself is designed not to throw.

---

### EnsureValid

```csharp
public static void EnsureValid(/* parameters specific to admin controller context */)
```

Asserts that the input is valid. Calls the corresponding `Validate` overload and, if any errors are present, throws an exception whose message aggregates the error strings.

- **Parameters:** Same as the corresponding `Validate` overload.
- **Returns:** Nothing (void). Control returns normally only when validation succeeds.
- **Exceptions:** Throws an exception (typically `ArgumentException` or a custom validation exception) when validation fails, with a message built from the error list.

## Usage

### Example 1: Conditional handling with explicit error inspection

```csharp
var errors = AdminControllerValidation.Validate(adminRequest);
if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        logger.LogWarning("Admin validation failure: {Error}", error);
    }
    return Results.BadRequest(new { errors });
}

// Proceed with admin operation
var result = adminService.Execute(adminRequest);
return Results.Ok(result);
```

### Example 2: Fail-fast assertion style

```csharp
// Let the framework catch the exception and convert to a 400 response
AdminControllerValidation.EnsureValid(adminRequest);

// If we reach here, input is guaranteed valid
await adminOrchestrator.ProcessAsync(adminRequest);
return Results.Accepted();
```

## Notes

- **Overloads:** The type exposes two sets of `Validate`/`IsValid`/`EnsureValid` members, each targeting a distinct admin controller scenario. Callers must select the overload that matches their operation’s input shape.
- **Immutability:** The returned `IReadOnlyList<string>` from `Validate` is safe to hold across threads and should not be modified by callers.
- **Thread safety:** All methods are static and operate purely on their supplied arguments without shared mutable state. They are safe to call concurrently from multiple threads.
- **Exception aggregation:** `EnsureValid` throws a single exception whose message combines all validation errors. Callers should avoid parsing the message programmatically; use `Validate` when structured error handling is required.
- **Edge cases:** An empty or null-like input that fails validation will produce error strings in the list rather than a null reference. `IsValid` returns `false` for such inputs, and `EnsureValid` throws.

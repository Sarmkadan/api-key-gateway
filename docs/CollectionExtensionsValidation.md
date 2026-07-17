# CollectionExtensionsValidation

Provides static validation helpers for collection-related operations, returning descriptive error messages instead of throwing exceptions. Designed for fluent validation pipelines where callers accumulate and inspect failures before deciding how to respond.

## API

### `ValidatePaginationParameters(int page, int pageSize, int? maxPageSize = null)`

Validates pagination arguments.

**Parameters**  
- `page` — 1-based page number. Must be ≥ 1.  
- `pageSize` — Number of items per page. Must be ≥ 1.  
- `maxPageSize` — Optional upper bound for `pageSize`. When provided, `pageSize` must not exceed it.

**Returns**  
`IReadOnlyList<string>` — Empty list if valid; otherwise one or more error messages describing each violation.

**Throws**  
Does not throw.

---

### `ValidateBatchParameters(int batchSize, int? maxBatchSize = null)`

Validates batch sizing arguments.

**Parameters**  
- `batchSize` — Number of items per batch. Must be ≥ 1.  
- `maxBatchSize` — Optional upper bound for `batchSize`. When provided, `batchSize` must not exceed it.

**Returns**  
`IReadOnlyList<string>` — Empty list if valid; otherwise error messages for each violation.

**Throws**  
Does not throw.

---

### `ValidateKeySelector<T, TKey>(Func<T, TKey> keySelector, string paramName = "keySelector")`

Validates that a key selector delegate is non-null.

**Parameters**  
- `keySelector` — Delegate used to extract a key from an element.  
- `paramName` — Name used in error messages (default: `"keySelector"`).

**Returns**  
`IReadOnlyList<string>` — Empty list if `keySelector` is not null; otherwise a single message indicating the parameter is null.

**Throws**  
Does not throw.

---

### `ValidateForEachAction<T>(Action<T> action, string paramName = "action")`

Validates that a per-element action delegate is non-null.

**Parameters**  
- `action` — Delegate invoked for each element.  
- `paramName` — Name used in error messages (default: `"action"`).

**Returns**  
`IReadOnlyList<string>` — Empty list if `action` is not null; otherwise a single message indicating the parameter is null.

**Throws**  
Does not throw.

---

### `Validate<T>(IEnumerable<T> source, string paramName = "source")`

Validates that an enumerable source is non-null.

**Parameters**  
- `source` — The collection to validate.  
- `paramName` — Name used in error messages (default: `"source"`).

**Returns**  
`IReadOnlyList<string>` — Empty list if `source` is not null; otherwise a single message indicating the parameter is null.

**Throws**  
Does not throw.

---

### `IsValid<T>(IEnumerable<T> source)`

Convenience check for non-null enumerable.

**Parameters**  
- `source` — The collection to test.

**Returns**  
`bool` — `true` if `source` is not null; otherwise `false`.

**Throws**  
Does not throw.

---

### `EnsureValid<T>(IEnumerable<T> source, string paramName = "source")`

Throws if the enumerable source is null.

**Parameters**  
- `source` — The collection to validate.  
- `paramName` — Name used in the exception (default: `"source"`).

**Returns**  
`void`

**Throws**  
`ArgumentNullException` — If `source` is null.

## Usage

```csharp
var errors = new List<string>();

errors.AddRange(CollectionExtensionsValidation.ValidatePaginationParameters(page, pageSize, 100));
errors.AddRange(CollectionExtensionsValidation.ValidateBatchParameters(batchSize, 500));
errors.AddRange(CollectionExtensionsValidation.ValidateKeySelector(keySelector));
errors.AddRange(CollectionExtensionsValidation.ValidateForEachAction(action));
errors.AddRange(CollectionExtensionsValidation.Validate(items));

if (errors.Count > 0)
{
    return Results.BadRequest(new { Errors = errors });
}
```

```csharp
if (!CollectionExtensionsValidation.IsValid(candidates))
{
    throw new ArgumentNullException(nameof(candidates));
}

CollectionExtensionsValidation.EnsureValid(candidates, nameof(candidates));

foreach (var item in candidates)
{
    processor.Handle(item);
}
```

## Notes

- All `Validate*` methods are pure: they allocate only the returned list and strings, perform no I/O, and are safe to call concurrently.
- `EnsureValid` is the only member that throws; use it at API boundaries where failing fast is preferred over error accumulation.
- `ValidatePaginationParameters` and `ValidateBatchParameters` treat `maxPageSize`/`maxBatchSize` as inclusive upper bounds; pass `null` to disable the upper-bound check.
- The `paramName` arguments on delegate and enumerable validators flow directly into error messages and exception messages—supply the caller's parameter name for actionable diagnostics.
- `IsValid` is a trivial null check; it does not enumerate the source, so it has no side effects and is O(1).

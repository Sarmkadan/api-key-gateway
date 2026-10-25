# ApiKeyRepositoryValidation

Static validation helpers for [`ApiKeyRepository`](ApiKeyRepository.md) (`ApiKeyGateway.Repositories`). The class lives in `src/ApiKeyGateway/Repositories/ApiKeyRepositoryValidation.cs` and is marked `[ExcludeFromCodeCoverage]`.

It has three extension methods, all taking an `ApiKeyRepository` as `this`:

- `Validate(...)` returns an `IReadOnlyList<string>` of problem messages. An empty list means the repository is valid.
- `IsValid(...)` returns a `bool`.
- `EnsureValid(...)` throws an `ArgumentException` when `Validate` reports problems.

## Validation rules

**There are currently no validation rules.** `Validate` never adds a message to its list, so it always returns an empty list for a non-null repository. `IsValid` therefore returns `true` and `EnsureValid` never throws for a non-null repository.

The source documents why. `ApiKeyRepository` is a service class whose dependencies are checked in its own constructor:

- `connection` must not be `null` (`ArgumentNullException`).
- `logger` must not be `null` (`ArgumentNullException`).

Those checks run when the repository is created, so this helper has nothing left to check at runtime. The method is kept as an extension point. New rules go in `Validate`, and each rule adds its message to the `problems` list.

| Method | Rule | Result |
|---|---|---|
| `Validate` | Repository is non-null | Returns an empty list |
| `Validate` | Repository is `null` | Throws `ArgumentNullException` |
| `IsValid` | Repository is non-null | Returns `true` |
| `IsValid` | Repository is `null` | Returns `false`. Does not throw. |
| `EnsureValid` | Repository is non-null | Returns normally |
| `EnsureValid` | Repository is `null` | Throws `ArgumentNullException` |
| `EnsureValid` | `Validate` returns one or more messages | Throws `ArgumentException` (not reachable today) |

## Null handling

`Validate` and `EnsureValid` check the argument with `ArgumentNullException.ThrowIfNull` before any other work. `IsValid` uses a null-conditional (`value?.Validate()`), so a `null` repository gives `false` without calling `Validate`.

```csharp
using ApiKeyGateway.Repositories;

IReadOnlyList<string> problems = repository.Validate();
// [] for any non-null repository

bool ok = ApiKeyRepositoryValidation.IsValid(null);
// false

ApiKeyRepositoryValidation.EnsureValid(null!);
// throws ArgumentNullException
```

## Error format

If a future rule adds messages, `EnsureValid` joins them into one `ArgumentException`. The message starts with `ApiKeyRepository is invalid:` and then lists each problem on its own line, separated by `Environment.NewLine`.

```text
ApiKeyRepository is invalid:
<problem 1>
<problem 2>
```

The exception has no `paramName`, because the failure is about the object's state and not about one argument.

## Related

- [`ApiKeyRepository`](ApiKeyRepository.md): the class being validated.
- [`ApiKeyRepositoryExtensions`](ApiKeyRepositoryExtensions.md): query helpers for the same repository.
- [`CacheKeyGeneratorValidation`](CacheKeyGeneratorValidation.md): the same `Validate` / `IsValid` / `EnsureValid` pattern for cache key parameters.

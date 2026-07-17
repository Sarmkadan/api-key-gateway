# RateLimitRepositoryValidation

Provides static validation methods for `RateLimitRepository` instances. The class centralizes consistency checks on rate‑limit configuration, returning descriptive error messages when the repository’s data violates expected invariants.

## API

### `Validate`
```csharp
public static IReadOnlyList<string> Validate(RateLimitRepository repository)
```
Validates the specified `repository` and returns a list of error messages describing any violations. Returns an empty list if the repository is valid.

- **Parameters**  
  `repository` – The `RateLimitRepository` instance to validate. Must not be `null`.

- **Returns**  
  `IReadOnlyList<string>` – A read‑only collection of human‑readable error messages. An empty collection indicates no validation errors.

- **Throws**  
  `ArgumentNullException` – If `repository` is `null`.

### `IsValid`
```csharp
public static bool IsValid(RateLimitRepository repository)
```
Indicates whether the specified `repository` passes all validation rules.

- **Parameters**  
  `repository` – The `RateLimitRepository` instance to check. Must not be `null`.

- **Returns**  
  `true` if the repository is valid; otherwise `false`.

- **Throws**  
  `ArgumentNullException` – If `repository` is `null`.

### `EnsureValid`
```csharp
public static void EnsureValid(RateLimitRepository repository)
```
Validates the specified `repository` and throws an exception if any validation errors are found.

- **Parameters**  
  `repository` – The `RateLimitRepository` instance to validate. Must not be `null`.

- **Throws**  
  `ArgumentNullException` – If `repository` is `null`.  
  `ValidationException` – If the repository contains one or more validation errors. The exception message includes all error messages, separated by newlines.

## Usage

### Example 1: Checking validity without throwing
```csharp
var repo = new RateLimitRepository
{
    MaxRequests = 100,
    WindowDuration = TimeSpan.FromMinutes(1)
};

if (RateLimitRepositoryValidation.IsValid(repo))
{
    Console.WriteLine("Repository configuration is valid.");
}
else
{
    var errors = RateLimitRepositoryValidation.Validate(repo);
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### Example 2: Using EnsureValid in a guard clause
```csharp
public void ApplyRateLimitConfiguration(RateLimitRepository repo)
{
    RateLimitRepositoryValidation.EnsureValid(repo);
    // Configuration is guaranteed valid from this point onward.
    ApplyConfiguration(repo);
}
```

## Notes

- All methods throw `ArgumentNullException` when `repository` is `null`. Always pass a non‑null instance.
- Validation rules typically check that `MaxRequests` is greater than zero, `WindowDuration` is positive, and that any associated rule collections are well‑formed. The exact rules are defined by the implementation.
- The class is static and its methods are thread‑safe as long as the `repository` instance is not mutated concurrently during validation. If the repository is shared across threads, external synchronization is required to avoid race conditions.
- `Validate` returns a new list each time it is called; callers should not cache the result if the repository may change.
- `EnsureValid` is a convenience wrapper around `Validate` and throws a single exception containing all error messages. For fine‑grained error handling, use `Validate` directly.

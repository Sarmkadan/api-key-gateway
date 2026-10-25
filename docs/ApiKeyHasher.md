# ApiKeyHasher

`ApiKeyHasher` hashes raw API keys for storage and verifies a presented key against a stored hash. Each hash uses a fresh random salt and is stored as a single string with a version prefix, so the salt travels with the hash and no separate column is needed.

The types are defined in `src/ApiKeyGateway/Utilities/ApiKeyHasher.cs`:

- `IApiKeyHasher`: the interface, with the public contract.
- `ApiKeyHasher`: the default implementation.
- `ApiKeyHasherFactory`: a static factory that creates an `IApiKeyHasher`.

`ApiKeyService` uses `IApiKeyHasher` through its optional constructor parameter. When no hasher is passed, it falls back to `ApiKeyHasherFactory.Create()`. The DI registration in `src/ApiKeyGateway/Configuration/ServiceCollectionExtensions.cs` is scoped:

```csharp
services.AddScoped<IApiKeyHasher, ApiKeyHasher>();
```

> **Note:** the hashing is an iterated SHA-256 with a salt. It is not a standard key-derivation function such as PBKDF2, bcrypt, or Argon2, although the source comment describes it as "PBKDF2-like".

## Hash format

A hash produced by `Hash` or `HashWithVersion` has three parts separated by `$`:

```
{version}${base64Salt}${base64Hash}
```

- `version`: the version string, `v1` by default.
- `base64Salt`: 16 random bytes from `RandomNumberGenerator`, base64-encoded.
- `base64Hash`: 32 bytes (SHA-256 output), base64-encoded.

The digest is computed as follows: SHA-256 over the salt bytes followed by the UTF-8 bytes of the key, then 99,999 more SHA-256 passes over the previous digest (100,000 iterations in total).

## Types

### IApiKeyHasher

| Member | Returns | Description |
|---|---|---|
| `Hash(string apiKey)` | `string` | Hashes the key with the default version `v1`. |
| `HashWithVersion(string apiKey, string version = "v1")` | `string` | Hashes the key and prefixes the result with `version`. |
| `Verify(string apiKey, string storedHash)` | `bool` | Checks a raw key against a stored hash in constant time. |
| `GetHashVersion(string storedHash)` | `string` | Returns the text before the first `$`, or an empty string if there is none. |

### ApiKeyHasher

`public class ApiKeyHasher : IApiKeyHasher`

The class has a public parameterless constructor and no state. It is safe to share one instance across threads. The interface members are documented below.

### ApiKeyHasherFactory

`public static class ApiKeyHasherFactory`

| Member | Returns | Description |
|---|---|---|
| `Create()` | `IApiKeyHasher` | Returns a new `ApiKeyHasher` instance. |

## Methods

### Hash

```csharp
string Hash(string apiKey)
```

Equivalent to `HashWithVersion(apiKey, "v1")`. Returns a string in the format described above. Each call produces a different result because the salt is random, so two calls with the same key give different hashes that both verify.

Throws `ArgumentNullException` if `apiKey` is `null`, or `ArgumentException` if it is empty.

### HashWithVersion

```csharp
string HashWithVersion(string apiKey, string version = "v1")
```

Generates a new random salt, runs the iterated SHA-256 described above, and returns `{version}${base64Salt}${base64Hash}`.

- `apiKey`: the raw key. It is encoded as UTF-8 before hashing.
- `version`: written verbatim as the prefix. The method does not validate it. A value that contains `$` produces a string that `Verify` and `GetHashVersion` will not parse correctly.

Throws `ArgumentNullException` or `ArgumentException` if `apiKey` is `null` or empty. The same applies to `version`.

### Verify

```csharp
bool Verify(string apiKey, string storedHash)
```

Checks whether `apiKey` produces `storedHash`. The method:

1. Splits `storedHash` on `$`.
2. If there are exactly three parts, takes the salt from part 2 and the expected digest from part 3. It hashes `apiKey` with that salt using the same iteration count, then compares the two digests with `CryptographicOperations.FixedTimeEquals`.
3. Otherwise, it takes the legacy branch. This branch calls `Hash(apiKey)`, which generates a new random salt, and compares the result to `storedHash`. The comparison is effectively always `false`.

The version segment (part 1) is not checked. A hash stored with `v2` verifies with the same logic as `v1`.

Returns `true` if the digests match, otherwise `false`. Any exception raised while decoding `storedHash` (for example, invalid base64) is caught and returns `false`.

Throws `ArgumentNullException` or `ArgumentException` if `apiKey` or `storedHash` is `null` or empty. These checks run before the `try` block, so they are not converted to `false`.

### GetHashVersion

```csharp
string GetHashVersion(string storedHash)
```

Returns the substring before the first `$`. Examples:

| `storedHash` | Result |
|---|---|
| `"v1$abc$def"` | `"v1"` |
| `"v2$abc$def"` | `"v2"` |
| `"abc"` | `""` (empty string) |
| `"$abc$def"` | `""` (empty string) |

The method does not check that the prefix is a known version. The class-level remark says the method returns `"v1"` when there is no prefix. That is wrong: the code returns an empty string, which matches the interface documentation.

Throws `ArgumentNullException` or `ArgumentException` if `storedHash` is `null` or empty.

## Error handling

Argument checks throw `ArgumentNullException` for `null` and `ArgumentException` for empty strings. `ArgumentNullException` derives from `ArgumentException`, so catching `ArgumentException` covers both. These exceptions come from `ArgumentException.ThrowIfNullOrEmpty`.

`Verify` is the only method that suppresses failures. It returns `false` rather than throwing when the stored value is malformed, so a bad or tampered hash never reveals details to the caller.

The hasher does not log, and it has no external dependencies, so it does not throw `DataAccessException` or `ValidationException`.

## Usage

The example below stores a hash when a key is created and checks a presented key on each request. It is a standalone class, not code from the project:

```csharp
using ApiKeyGateway.Utilities;

public sealed class KeyCheck
{
    private readonly IApiKeyHasher _hasher = ApiKeyHasherFactory.Create();

    // Store only the hash, never the raw key.
    public string CreateStoredHash(string rawApiKey)
    {
        return _hasher.Hash(rawApiKey);
    }

    // Compare a presented key against the stored value.
    public bool IsValid(string presentedKey, string storedHash)
    {
        return _hasher.Verify(presentedKey, storedHash);
    }
}
```

```csharp
var hasher = ApiKeyHasherFactory.Create();

string stored = hasher.Hash("example-key-123");   // e.g. "v1$<salt>$<hash>"

hasher.Verify("example-key-123", stored);         // true
hasher.Verify("wrong-key", stored);               // false
hasher.GetHashVersion(stored);                    // "v1"
```

In application code, prefer injecting `IApiKeyHasher` through the constructor, as `ApiKeyService` does. The factory is shown only to keep the examples self-contained.

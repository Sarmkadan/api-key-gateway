# ApiKeyService

`ApiKeyService` is the default implementation of `IApiKeyService`. It creates, looks up, validates, and manages API keys through an `IApiKeyRepository`. Raw key values are never stored: the service generates a key, hashes it with an `IApiKeyHasher`, and persists only the hash.

The type is defined in `src/ApiKeyGateway/Services/ApiKeyService.cs` together with its interfaces:

- `IApiKeyService`: the public contract that consumers should depend on.
- `ApiKeyService`: `public class ApiKeyService : IApiKeyService`. It is not sealed and has no state beyond its injected dependencies.
- `IApiKeyRepository`: the data-access contract the service calls. It is defined in the same file.

## Constructor

```csharp
public ApiKeyService(IApiKeyRepository repository, ILogger<ApiKeyService> logger, IApiKeyHasher? hasher = null)
```

| Parameter | Type | Required | Description |
|---|---|---|---|
| `repository` | `IApiKeyRepository` | Yes | Data access for API keys. |
| `logger` | `ILogger<ApiKeyService>` | Yes | Receives debug, information, warning, and error messages. |
| `hasher` | `IApiKeyHasher?` | No | Hashes and verifies key values. If omitted, `ApiKeyHasherFactory.Create()` supplies a default instance. |

Throws `ArgumentNullException` if `repository` or `logger` is `null`.

## Constants

| Name | Value | Meaning |
|---|---|---|
| `KeyPrefix` | `"sk"` | Prefix of generated key values and the value stored in `ApiKey.Prefix`. |
| `RandomPartLength` | `32` | Number of random characters after the prefix. |

A generated key has the form `sk_` followed by 32 characters from `A-Z`, `a-z`, and `0-9`. The random part comes from `RandomNumberGenerator.GetString`, so it is cryptographically secure.

## Methods

All methods are asynchronous and return a `Task` or `Task<T>`. Parameters named `keyId` and `consumerId` must not be empty or whitespace, or the method throws `ValidationException`, unless the table says otherwise.

| Method | Returns | Description |
|---|---|---|
| `CreateKeyAsync(string consumerId, string name, int? expirationDays = null)` | `Task<ApiKey>` | Creates and stores a key. The result is the value returned by the repository. |
| `GetByIdAsync(string keyId)` | `Task<ApiKey?>` | Returns the key, or `null` if it does not exist. |
| `ValidateKeyAsync(string keyValue)` | `Task<ApiKey?>` | Hashes the raw key value, looks it up, and returns the key if it is usable. See [Validation](#validation). |
| `DisableKeyAsync(string keyId)` | `Task<bool>` | Disables the key. Returns `false` if the key does not exist. |
| `EnableKeyAsync(string keyId)` | `Task<bool>` | Re-enables a disabled key. Returns `false` if the key does not exist. |
| `RevokeKeyAsync(string keyId)` | `Task<bool>` | Permanently revokes the key. Returns `false` if the key does not exist. |
| `GetConsumerKeysAsync(string consumerId)` | `Task<List<ApiKey>>` | Returns all keys for the consumer. Returns an empty list if `consumerId` is empty or whitespace. |
| `UpdateKeyMetadataAsync(string keyId, Dictionary<string, string> metadata)` | `Task<bool>` | Merges the given pairs into the key's metadata. Existing keys are overwritten and other entries are kept. Returns `false` if the key does not exist. |
| `GetIpWhitelistAsync(string keyId)` | `Task<List<string>>` | Returns the key's allowed IP addresses, trimmed and de-duplicated. An empty list means no restriction. Returns an empty list if the key does not exist. |
| `SetIpWhitelistAsync(string keyId, IEnumerable<string> ips)` | `Task<bool>` | Replaces the whitelist. Blank and duplicate entries are removed. An empty collection clears the restriction. Returns `false` if the key does not exist. |
| `AddIpToWhitelistAsync(string keyId, string ip)` | `Task<bool>` | Adds one IP address. Returns `false` if the key does not exist or the address is already present. |
| `RemoveIpFromWhitelistAsync(string keyId, string ip)` | `Task<bool>` | Removes one IP address. Returns `false` if the key does not exist or the address is not in the list. |
| `GetExpiringKeysAsync(TimeSpan window)` | `Task<List<ApiKey>>` | Returns keys that expire before `DateTime.UtcNow + window`. |
| `GetExpiringKeysAsync(int days)` | `Task<List<ApiKey>>` | Same as the `TimeSpan` overload, using `days` as the window. Throws `ValidationException` if `days` is zero or negative. |
| `FindByPrefixAsync(string prefix)` | `Task<List<ApiKey>>` | Returns keys matching `prefix` through the repository. Throws `ValidationException` if `prefix` is empty. |

### CreateKeyAsync

Parameters:

- `consumerId`: the owner of the key. Must not be empty or whitespace (`ValidationException`).
- `name`: a display name. Must not be empty or whitespace (`ValidationException`).
- `expirationDays`: if set, `ExpiresAt` is `UtcNow` plus this many days. If `null`, the key does not expire.

The service generates the key, hashes it, and passes a new `ApiKey` to `IApiKeyRepository.CreateAsync`. `Id` is a new `Guid` string, `CreatedAt` is `UtcNow`, and `KeyHash` holds the hash. The raw key value is not part of the returned `ApiKey`, so the caller has no way to recover it from the result. **The raw value is not returned by this method in the current implementation.** Any failure during generation, hashing, or persistence is logged and rethrown as `InvalidApiKeyException` with the `KeyGenerationFailed` message.

### ValidateKeyAsync

Validation runs in this order:

1. An empty or whitespace `keyValue` throws `UnauthorizedAccessException` (the project type, `Domain.Exceptions.UnauthorizedAccessException`).
2. The value is hashed and looked up with `IApiKeyRepository.GetByHashAsync`. If nothing matches, the method throws `InvalidApiKeyException` with `ApiKeyNotFound`.
3. If `ApiKey.IsExpired` is `true`, it throws `InvalidApiKeyException` with `ApiKeyExpired` and `isExpired: true`.
4. If `ApiKey.CanBeUsed()` is `false`, it throws `InvalidApiKeyException` with `ApiKeyDisabled`.
5. Otherwise it returns the key.

So a successful call always returns a non-null key. Failures are reported by exceptions, not by a `null` return. The `IApiKeyService` XML comment says `null` is returned for an invalid key. That does not match the implementation.

Repository exceptions from step 2 are not caught here and propagate to the caller.

### Repository-backed updates

`DisableKeyAsync`, `EnableKeyAsync`, `RevokeKeyAsync`, `UpdateKeyMetadataAsync`, and the IP whitelist setters load the key with `GetByIdAsync`, change it in memory, and call `IApiKeyRepository.UpdateAsync`. Any exception in this sequence is logged and rethrown as `DataAccessException`. The exception is not rethrown for "not found": the method returns `false` in that case.

`RevokeKeyAsync` logs at warning level. The other state changes log at information level.

### IP whitelist

The whitelist is stored as one comma-separated string on `ApiKey.IpWhitelist`.

- `GetIpWhitelistAsync` splits that string, trims each entry, removes blanks, and de-duplicates the result.
- `SetIpWhitelistAsync` trims entries, drops nulls and blanks, and de-duplicates them. If nothing is left, `IpWhitelist` is set to `null`. Otherwise the entries are joined with `,`.
- `AddIpToWhitelistAsync` and `RemoveIpFromWhitelistAsync` trim the input IP and then use `GetIpWhitelistAsync` and `SetIpWhitelistAsync`. Both of these validate `keyId` and `ip` themselves (`ValidationException`).

The IP values are plain strings. The service does not check that they are valid IP addresses or CIDR ranges.

### Expiration and prefix queries

`GetExpiringKeysAsync(TimeSpan)` does not check the window: a negative window is passed through, and keys that have already expired may be returned. It delegates to `IApiKeyRepository.GetKeysExpiringBeforeAsync`.

`FindByPrefixAsync` passes the prefix to `IApiKeyRepository.GetByPrefixAsync`. The XML comment says the prefix is the first 8 characters of the key value. However, `CreateKeyAsync` stores `Prefix = "sk"` (the constant `KeyPrefix`), so that is the value this query matches against. Check the repository implementation before relying on a different prefix length.

## Exceptions

| Exception | Raised by |
|---|---|
| `ArgumentNullException` | Constructor, when `repository` or `logger` is `null`. |
| `ValidationException` | Empty or whitespace `consumerId`, `name`, `keyId`, `ip`, or `prefix`; `null` metadata; non-positive `days`. |
| `InvalidApiKeyException` | `CreateKeyAsync` failure (`KeyGenerationFailed`); `ValidateKeyAsync` when the key is not found, expired, or disabled. |
| `UnauthorizedAccessException` | `ValidateKeyAsync` with an empty key value. |
| `DataAccessException` | Failures from the repository in the update, whitelist, and `GetConsumerKeysAsync` / `GetIpWhitelistAsync` paths. |

Exceptions from the repository in `GetByIdAsync`, `ValidateKeyAsync`, `GetExpiringKeysAsync`, and `FindByPrefixAsync` are not wrapped.

## Usage

The example below creates a key, checks it, and restricts it to one address. It uses DI, as the project does. It is a standalone sample, not code from the project:

```csharp
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Services;

public sealed class KeyOnboarding
{
    private readonly IApiKeyService _keys;

    public KeyOnboarding(IApiKeyService keys)
    {
        _keys = keys;
    }

    public async Task<string> IssueKeyAsync(string consumerId)
    {
        // Expires in 90 days. The returned ApiKey holds only the hash, not the raw value.
        var key = await _keys.CreateKeyAsync(consumerId, "ci-pipeline", expirationDays: 90);

        await _keys.AddIpToWhitelistAsync(key.Id, "203.0.113.10");

        return key.Id;
    }

    public async Task<bool> IsAllowedAsync(string presentedKey, string clientIp)
    {
        try
        {
            var key = await _keys.ValidateKeyAsync(presentedKey);
            if (key == null)
            {
                return false;
            }

            var allowed = await _keys.GetIpWhitelistAsync(key.Id);
            return allowed.Count == 0 || allowed.Contains(clientIp);
        }
        catch (InvalidApiKeyException)
        {
            return false;
        }
    }
}
```

Note that `IsAllowedAsync` treats `null` as a defensive check only. `ValidateKeyAsync` throws for invalid keys, so the `InvalidApiKeyException` catch is the path that handles rejection.

## Notes

- Each call logs at debug level on entry. Successful reads and changes log at information level. Missing keys in update methods log at warning level.
- Log messages include key IDs, consumer IDs, and key names, and the hash on a failed lookup in `ValidateKeyAsync`. They never include the raw key value.
- Keys are looked up by their hash, so the stored hash must be produced by the same `IApiKeyHasher` that validates them.
- The service has no caching and no transactions. Each update is a read followed by a separate write, so concurrent changes to the same key can overwrite each other.

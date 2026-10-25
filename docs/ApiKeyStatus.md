# ApiKeyStatus

`src/ApiKeyGateway/Domain/Enums/ApiKeyStatus.cs` is the lifecycle status of an API key. It is stored on `ApiKey.Status` (see [ApiKey](ApiKey.md)) and is persisted as its integer value (`(int)ApiKeyStatus.Active` is passed as a SQL parameter in `ApiKeyRepository`), so the numeric values are part of the storage format and must not change.

```csharp
namespace ApiKeyGateway.Domain.Enums;

public enum ApiKeyStatus
{
    Active = 1,
    Disabled = 2,
    Expired = 3,
    Revoked = 4,
    Suspended = 5
}
```

## Values

| Value | Int | Meaning in the domain |
|-------|-----|-----------------------|
| `Active` | 1 | The key is usable for requests. It is the default for new keys (`ApiKey.Status` initializer). It still has to pass the expiry check, so an `Active` key whose `ExpiresAt` is in the past is not usable. |
| `Disabled` | 2 | The key is switched off but not deleted, and it can be turned back on. `ApiKey.Disable()` sets this value and records `DisabledAt`. `ApiKey.Enable()` returns a disabled key to `Active` and clears `DisabledAt`. |
| `Expired` | 3 | The key has reached its expiration date. Reserved for the lifecycle status; see the note below. |
| `Revoked` | 4 | The key has been permanently revoked by an administrator. `ApiKey.Revoke()` sets this value. There is no transition back to `Active`. |
| `Suspended` | 5 | The key is blocked because of abuse or a policy violation. It is treated as unusable by `ApiKey.CanBeUsed()`. |

## Usability

Only one status lets a key authenticate. A key is usable when all of these hold:

- `Status` is `Active`, which is what `ApiKey.IsActive` checks.
- `ExpiresAt` is unset or in the future, which is what `ApiKey.IsExpired` checks.

`ApiKey.CanBeUsed()` expresses the same rule with an explicit exclusion of `Revoked` and `Suspended`. `AuthenticationService` checks expiry first (`ApiKeyExpired`), then any status other than `Active` (`ApiKeyDisabled`). Because of that ordering, a `Revoked` or `Suspended` key that is also past its expiry date is reported as expired.

## Note on `Expired`

Expiry is currently tracked by `ApiKey.ExpiresAt` and the computed `ApiKey.IsExpired` property, not by writing `Status = Expired`. Nothing in the code sets `ApiKeyStatus.Expired` or `ApiKeyStatus.Suspended`. A key whose time has passed keeps `Status = Active` in storage and is rejected because `IsExpired` is true. The enum values exist in the domain model and in the stored integer format, but the code only assigns `Active`, `Disabled` and `Revoked`.

## Related

- [ApiKey](ApiKey.md): the model that holds `Status`, `ExpiresAt`, `IsActive`, `IsExpired` and `CanBeUsed()`.
- [AuthenticationService](AuthenticationService.md): maps the expiry and status checks to authentication failure reasons.
- [ApiKeyRepository](ApiKeyRepository.md): writes and filters on the integer status value.

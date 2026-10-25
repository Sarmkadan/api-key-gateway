# ApiKeyConsumerExtensions

Static extension methods for `ApiKeyConsumer` (`ApiKeyGateway.Domain.Models`) that add common lifecycle checks, tier handling, email-domain lookup, inactivity detection and type-safe access to custom properties. The class lives in `src/ApiKeyGateway/Domain/Models/ApiKeyConsumerExtensions.cs` and is also declared there together with the `ApiKeyTier` enum.

Every method validates its `consumer` argument with `ArgumentNullException.ThrowIfNull` and does not modify the consumer.

## API

### `IsCurrentlyActive(this ApiKeyConsumer consumer)` → bool

Returns `true` only when `IsActive` is `true` **and** `InactiveSince` is `null`. Use it instead of reading `IsActive` alone, because a consumer can carry a stale `InactiveSince` timestamp.

- **Throws** `ArgumentNullException` if `consumer` is `null`.

```csharp
using ApiKeyGateway.Domain.Models;

var consumer = new ApiKeyConsumer { Id = "consumer-1", Name = "Acme" };

if (consumer.IsCurrentlyActive())
{
    // Allow the request to proceed
}
```

### `GetTier(this ApiKeyConsumer consumer)` → `ApiKeyTier`

Converts the string `Tier` property into the `ApiKeyTier` enum for type-safe comparisons. Matching is case-sensitive and accepts only `"free"`, `"basic"`, `"pro"` and `"enterprise"`.

- **Throws** `ArgumentNullException` if `consumer` is `null`.
- **Throws** `ArgumentException` if `Tier` holds any other value (for example `"Pro"` or `"gold"`).

```csharp
var consumer = new ApiKeyConsumer { Id = "consumer-2", Name = "Globex", Tier = "pro" };

ApiKeyTier tier = consumer.GetTier(); // ApiKeyTier.Pro

if (consumer.GetTier() >= ApiKeyTier.Pro)
{
    // Enable advanced rate limits
}
```

### `CanUpgradeTo(this ApiKeyConsumer consumer, ApiKeyTier targetTier)` → bool

Returns `true` when `targetTier` is strictly higher than the consumer's current tier. It returns `false` for the same tier and for any lower tier. It calls `GetTier()` internally, so an unrecognized `Tier` value throws the same `ArgumentException`.

- **Throws** `ArgumentNullException` if `consumer` is `null`.
- **Throws** `ArgumentException` if the current `Tier` value is not recognized.

```csharp
var consumer = new ApiKeyConsumer { Id = "consumer-3", Name = "Initech", Tier = "basic" };

bool toPro = consumer.CanUpgradeTo(ApiKeyTier.Pro);        // true
bool toBasic = consumer.CanUpgradeTo(ApiKeyTier.Basic);    // false (same tier)
bool toFree = consumer.CanUpgradeTo(ApiKeyTier.Free);      // false (downgrade)
```

### `GetOrganizationDomain(this ApiKeyConsumer consumer)` → `string?`

Returns the part of `Email` after the `@` sign, which can be used for email-based routing or grouping by organization. It returns `null` when:

- `Email` is empty, whitespace, or has no `@`;
- `Email` contains more than one `@` (the split yields more than two parts).

No validation of the domain itself is performed, and the comparison is not normalized to lower case.

- **Throws** `ArgumentNullException` if `consumer` is `null`.

```csharp
var consumer = new ApiKeyConsumer { Id = "consumer-4", Name = "Umbrella", Email = "api@umbrella.example" };

string? domain = consumer.GetOrganizationDomain(); // "umbrella.example"

var noEmail = new ApiKeyConsumer { Id = "consumer-5", Name = "Anonymous" };
string? none = noEmail.GetOrganizationDomain();    // null
```

### `IsInactiveForDays(this ApiKeyConsumer consumer, int daysThreshold)` → bool

Returns `true` when `LastActivityAt` is set and at least `daysThreshold` full days (measured as `TotalDays` against `DateTime.UtcNow`) have passed since then. A consumer that has never recorded activity (`LastActivityAt == null`) returns `false`, so it is not treated as inactive.

- **Throws** `ArgumentNullException` if `consumer` is `null`.
- **Throws** `ArgumentOutOfRangeException` if `daysThreshold` is `0` or negative.

```csharp
var consumer = new ApiKeyConsumer
{
    Id = "consumer-6",
    Name = "Stale Co",
    LastActivityAt = DateTime.UtcNow.AddDays(-45)
};

if (consumer.IsInactiveForDays(30))
{
    // Candidate for deactivation or a reminder email
}

var neverUsed = new ApiKeyConsumer { Id = "consumer-7", Name = "New Co" };
bool stale = neverUsed.IsInactiveForDays(30); // false - no activity recorded
```

### `GetCustomProperty(this ApiKeyConsumer consumer, string propertyKey, string defaultValue = "")` → string

Reads a value from `CustomProperties` by exact (case-sensitive) key. If the key is missing, `defaultValue` is returned. A present key whose value is `""` returns `""`, not `defaultValue`.

- **Throws** `ArgumentNullException` if `consumer` or `propertyKey` is `null`.

```csharp
var consumer = new ApiKeyConsumer { Id = "consumer-8", Name = "Hooli" };
consumer.CustomProperties["region"] = "eu-west";

string region = consumer.GetCustomProperty("region");              // "eu-west"
string segment = consumer.GetCustomProperty("segment", "standard"); // "standard"
```

### `GetCustomPropertyAsInt(this ApiKeyConsumer consumer, string propertyKey, int defaultValue = 0)` → int

Reads a custom property and parses it as an `int` using `NumberStyles.Integer` and the invariant culture. It returns `defaultValue` when the key is missing or the value cannot be parsed (for example `"abc"`, `"1.5"`, or an empty string).

- **Throws** `ArgumentNullException` if `consumer` or `propertyKey` is `null`.

```csharp
var consumer = new ApiKeyConsumer { Id = "consumer-9", Name = "Wayne Enterprises" };
consumer.CustomProperties["maxSeats"] = "250";
consumer.CustomProperties["legacyLimit"] = "unlimited";

int seats = consumer.GetCustomPropertyAsInt("maxSeats");              // 250
int legacy = consumer.GetCustomPropertyAsInt("legacyLimit", 100);     // 100 (not parseable)
int missing = consumer.GetCustomPropertyAsInt("burstLimit", 10);      // 10
```

## Related Types

- [`ApiKeyConsumer`](ApiKeyConsumer.md) - the model these methods extend.
- `ApiKeyTier` - enum declared in the same source file: `Free = 0`, `Basic = 1`, `Pro = 2`, `Enterprise = 3`. Its numeric order drives `CanUpgradeTo` and tier comparisons.

## Usage Notes

- Tier strings are compared case-sensitively. Store tiers in lower case (`"pro"`), or normalize them before calling `GetTier()`.
- `IsCurrentlyActive()` is the recommended check for authorization decisions; reading `IsActive` directly can miss a consumer that was deactivated.
- Inactivity is based on `DateTime.UtcNow`, so the result changes over time even if the consumer's data does not.

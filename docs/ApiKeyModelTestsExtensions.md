# ApiKeyModelTestsExtensions

A static utility class that provides factory methods and fluent assertions for `ApiKey` objects in unit tests. It simplifies the creation of pre-configured `ApiKey` instances with common defaults and offers a set of extension methods to assert state transitions (e.g., enable/disable, usage recording, IP whitelist checks) in a readable, chainable style.

## API

### `public static ApiKey WithDefaultValues`

Creates a new `ApiKey` instance populated with sensible default values suitable for most test scenarios. The returned object is not persisted.

**Returns:** A fully initialized `ApiKey` with default property values.

### `public static ApiKey WithStatus(ApiKeyStatus status)`
Creates a new `ApiKey` with default values and overrides its status to the specified value.

**Parameters:**
- `status` — The `ApiKeyStatus` to assign (e.g., `Active`, `Disabled`, `Revoked`).

**Returns:** An `ApiKey` with the given status and all other properties set to defaults.

### `public static ApiKey WithIpWhitelist(IEnumerable<string> ipAddresses)`
Creates a new `ApiKey` with default values and configures its IP whitelist to the supplied collection of IP addresses.

**Parameters:**
- `ipAddresses` — A collection of IP address strings to populate the whitelist.

**Returns:** An `ApiKey` with the specified IP whitelist and default values for all other properties.

### `public static void ShouldBeUsable(this ApiKey)`
Asserts that the `ApiKey` is in a state where it can be used for requests (e.g., active, not expired, not disabled). Throws an assertion exception if the key is not usable.

**Parameters:**
- `this ApiKey` — The key to evaluate.

**Throws:** `AssertionException` (or equivalent test framework exception) when the key is not usable.

### `public static void ShouldHaveUsage(this ApiKey, int expectedCount)`
Asserts that the key’s usage count matches the expected value.

**Parameters:**
- `expectedCount` — The exact number of recorded usages expected.

**Throws:** `AssertionException` when the actual usage count differs from `expectedCount`.

### `public static void ShouldHaveLastUsedAt(this ApiKey, DateTime expected)`
Asserts that the key’s `LastUsedAt` timestamp equals the specified value.

**Parameters:**
- `expected` — The expected `DateTime` value.

**Throws:** `AssertionException` when the timestamp does not match.

### `public static void ShouldHaveDisabledAt(this ApiKey, DateTime expected)`
Asserts that the key’s `DisabledAt` timestamp equals the specified value.

**Parameters:**
- `expected` — The expected `DateTime` value.

**Throws:** `AssertionException` when the timestamp does not match.

### `public static void ShouldAllowIp(this ApiKey, string ipAddress)`
Asserts that the given IP address is permitted by the key’s IP whitelist configuration. If the whitelist is empty, all IPs are typically considered allowed; if populated, only listed IPs pass.

**Parameters:**
- `ipAddress` — The IP address string to check.

**Throws:** `AssertionException` when the IP is not allowed.

### `public static ApiKey DisableAndAssert(this ApiKey)`
Disables the key and immediately asserts that it is no longer usable. Returns the same instance for chaining.

**Returns:** The disabled `ApiKey` instance.

**Throws:** `AssertionException` if the key remains usable after disabling.

### `public static ApiKey EnableAndAssert(this ApiKey)`
Enables the key and immediately asserts that it becomes usable. Returns the same instance for chaining.

**Returns:** The enabled `ApiKey` instance.

**Throws:** `AssertionException` if the key is not usable after enabling.

### `public static ApiKey RecordUsageAndAssert(this ApiKey)`
Records a single usage event on the key and asserts that the usage count increments by one and the `LastUsedAt` timestamp updates to the current time. Returns the same instance for chaining.

**Returns:** The `ApiKey` instance with updated usage metrics.

**Throws:** `AssertionException` if the usage count or `LastUsedAt` timestamp does not reflect the recorded usage.

## Usage

### Example 1: Basic lifecycle test
```csharp
[Test]
public void ApiKey_Lifecycle_DisableAndEnable()
{
    var key = ApiKeyModelTestsExtensions.WithDefaultValues;

    key.ShouldBeUsable();

    key.DisableAndAssert()
       .ShouldHaveDisabledAt(DateTime.UtcNow);

    key.EnableAndAssert()
        .ShouldBeUsable();
}
```

### Example 2: Usage recording and IP whitelist
```csharp
[Test]
public void ApiKey_UsageAndIpWhitelist_AssertsCorrectly()
{
    var key = ApiKeyModelTestsExtensions
        .WithIpWhitelist(new[] { "192.168.1.10", "10.0.0.1" });

    key.ShouldAllowIp("192.168.1.10");
    key.ShouldNotAllowIp("203.0.113.5");

    key.RecordUsageAndAssert()
        .ShouldHaveUsage(1)
        .ShouldHaveLastUsedAt(DateTime.UtcNow);

    key.RecordUsageAndAssert()
        .ShouldHaveUsage(2);
}
```

## Notes

- **Assertion framework dependency:** The `Should*` methods rely on the test framework’s assertion mechanism (e.g., NUnit, xUnit, FluentAssertions). Ensure the appropriate `using` directives and test runner are configured.
- **Timestamps and precision:** `ShouldHaveLastUsedAt` and `ShouldHaveDisabledAt` compare `DateTime` values. Tests that rely on exact equality should account for potential precision loss (e.g., by truncating to seconds or using a tolerance).
- **IP whitelist semantics:** `ShouldAllowIp` assumes that an empty or null whitelist permits all IPs. Verify this behavior matches the production `ApiKey` implementation.
- **Chaining:** Methods that return `ApiKey` (`DisableAndAssert`, `EnableAndAssert`, `RecordUsageAndAssert`) are designed for fluent chaining. They mutate the instance and return it for immediate subsequent assertions.
- **Thread safety:** These methods are intended for single-threaded unit tests. They are not designed to be thread-safe and should not be used concurrently on the same `ApiKey` instance.
- **State leakage:** Since factory methods return fresh instances, there is no shared state between tests. However, chaining methods mutate the instance in place; avoid reusing the same instance across independent test cases without re-initializing.

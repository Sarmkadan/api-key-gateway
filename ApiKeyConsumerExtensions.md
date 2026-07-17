# ApiKeyConsumerExtensions

Extension methods for `ApiKeyConsumer` providing common operations and validations for API key consumers.

## API

### IsCurrentlyActive

Determines whether the consumer is currently active based on both the `IsActive` flag and the `InactiveSince` timestamp.

- **Parameters:** `consumer` — The consumer instance to check
- **Returns:** `true` if the consumer is active; otherwise `false`
- **Throws:** `ArgumentNullException` if `consumer` is `null`

### GetTier

Gets the consumer's tier level as an enum value for type-safe comparisons.

- **Parameters:** `consumer` — The consumer instance to check
- **Returns:** The `ApiKeyTier` enum value corresponding to the consumer's tier
- **Throws:**
  - `ArgumentNullException` if `consumer` is `null`
  - `ArgumentException` if the tier value is not recognized ("free", "basic", "pro", or "enterprise")

### CanUpgradeTo

Determines if the consumer is eligible for a specific tier upgrade based on their current tier.

- **Parameters:**
  - `consumer` — The consumer instance to check
  - `targetTier` — The target tier to evaluate for upgrade eligibility
- **Returns:** `true` if the consumer can be upgraded to the target tier; otherwise `false`
- **Throws:**
  - `ArgumentNullException` if `consumer` is `null`
  - `ArgumentException` if `targetTier` is not a valid tier value

### GetOrganizationDomain

Extracts the organization domain from the consumer's email address for email-based routing.

- **Parameters:** `consumer` — The consumer instance to check
- **Returns:** The organization domain (e.g., "example.com" from "user@example.com") if available; otherwise `null`
- **Throws:** `ArgumentNullException` if `consumer` is `null`

### IsInactiveForDays

Determines if the consumer has been inactive for at least the specified number of days.

- **Parameters:**
  - `consumer` — The consumer instance to check
  - `daysThreshold` — The minimum number of days of inactivity to check for
- **Returns:** `true` if the consumer has been inactive for at least the specified days; otherwise `false`
- **Throws:**
  - `ArgumentNullException` if `consumer` is `null`
  - `ArgumentOutOfRangeException` if `daysThreshold` is less than 1

### GetCustomProperty

Safely retrieves a custom property value by key, returning a default value if the property does not exist.

- **Parameters:**
  - `consumer` — The consumer instance to check
  - `propertyKey` — The custom property key to retrieve
  - `defaultValue` — The value to return if the property is not found (defaults to empty string)
- **Returns:** The property value if found; otherwise the `defaultValue`
- **Throws:** `ArgumentNullException` if `consumer` is `null` or `propertyKey` is `null`

### GetCustomPropertyAsInt

Safely retrieves a custom property value as an integer, converting from string if necessary.

- **Parameters:**
  - `consumer` — The consumer instance to check
  - `propertyKey` — The custom property key to retrieve
  - `defaultValue` — The value to return if the property is not found or conversion fails (defaults to 0)
- **Returns:** The parsed integer value if found and convertible; otherwise the `defaultValue`
- **Throws:** `ArgumentNullException` if `consumer` is `null` or `propertyKey` is `null`

## Usage

### Checking if a consumer is active

```csharp
var consumer = await _repository.GetConsumerAsync("consumer-id");

if (consumer.IsCurrentlyActive())
{
    // Consumer can make API requests
    Console.WriteLine("Consumer is active and ready to use");
}
else
{
    // Consumer is inactive
    Console.WriteLine("Consumer is not active");
}
```

### Determining upgrade eligibility

```csharp
var consumer = await _repository.GetConsumerAsync("consumer-id");
var currentTier = consumer.GetTier();

if (consumer.CanUpgradeTo(ApiKeyTier.Pro))
{
    Console.WriteLine($"Consumer can upgrade from {currentTier} to Pro tier");
}
else
{
    Console.WriteLine("Consumer cannot upgrade further or is already at a higher tier");
}
```

## Notes

- **Thread Safety:** All extension methods are thread-safe as they only read consumer state and perform immutable operations. No state modification occurs.
- **Null Handling:** All methods validate their parameters and throw `ArgumentNullException` for null inputs, making them safe to use with null consumers.
- **Performance:** Methods that extract values from strings (e.g., `GetOrganizationDomain`) use simple string operations and are efficient for typical email formats.
- **Date Comparisons:** `IsInactiveForDays` uses UTC dates for comparison to avoid timezone issues.
- **Type Safety:** `GetTier` and `CanUpgradeTo` use the `ApiKeyTier` enum to ensure type-safe tier comparisons.
- **Custom Properties:** `GetCustomProperty` and `GetCustomPropertyAsInt` safely handle missing properties without throwing exceptions, making them suitable for optional metadata scenarios.
- **Email Parsing:** `GetOrganizationDomain` returns `null` for malformed emails rather than throwing, allowing graceful degradation in routing logic.
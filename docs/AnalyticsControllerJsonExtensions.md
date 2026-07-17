# AnalyticsControllerJsonExtensions

Provides JSON serialization and deserialization extensions for analytics-related DTOs used in API Key Gateway controllers.

## API

### `ToJson(AnalyticsSummary summary)`

Converts an `AnalyticsSummary` object into its JSON string representation.

- **Parameters**
  - `summary`: The `AnalyticsSummary` instance to serialize.
- **Returns**
  - A JSON string representing the serialized `AnalyticsSummary`.
- **Throws**
  - `ArgumentNullException`: If `summary` is `null`.

---

### `ToJson(List<EndpointStat> stats)`

Converts a list of `EndpointStat` objects into its JSON string representation.

- **Parameters**
  - `stats`: The list of `EndpointStat` instances to serialize.
- **Returns**
  - A JSON string representing the serialized list of `EndpointStat`.
- **Throws**
  - `ArgumentNullException`: If `stats` is `null`.

---

### `ToJson(List<HourlyBucket> buckets)`

Converts a list of `HourlyBucket` objects into its JSON string representation.

- **Parameters**
  - `buckets`: The list of `HourlyBucket` instances to serialize.
- **Returns**
  - A JSON string representing the serialized list of `HourlyBucket`.
- **Throws**
  - `ArgumentNullException`: If `buckets` is `null`.

---
### `ToJson(List<DailyBucket> buckets)`

Converts a list of `DailyBucket` objects into its JSON string representation.

- **Parameters**
  - `buckets`: The list of `DailyBucket` instances to serialize.
- **Returns**
  - A JSON string representing the serialized list of `DailyBucket`.
- **Throws**
  - `ArgumentNullException`: If `buckets` is `null`.

---
### `FromJson(string json)`

Attempts to deserialize a JSON string into an `AnalyticsSummary` object.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**
  - The deserialized `AnalyticsSummary` object, or `null` if deserialization fails.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

---
### `FromJsonToEndpointStats(string json)`

Attempts to deserialize a JSON string into a list of `EndpointStat` objects.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**
  - A list of `EndpointStat` objects, or `null` if deserialization fails.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

---
### `FromJsonToHourlyBuckets(string json)`

Attempts to deserialize a JSON string into a list of `HourlyBucket` objects.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**
  - A list of `HourlyBucket` objects, or `null` if deserialization fails.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

---
### `FromJsonToDailyBuckets(string json)`

Attempts to deserialize a JSON string into a list of `DailyBucket` objects.

- **Parameters**
  - `json`: The JSON string to deserialize.
- **Returns**
  - A list of `DailyBucket` objects, or `null` if deserialization fails.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

---
### `TryFromJson(string json, out AnalyticsSummary? summary)`

Attempts to deserialize a JSON string into an `AnalyticsSummary` object. Returns a boolean indicating success.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `summary`: Output parameter containing the deserialized `AnalyticsSummary` object if successful.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

---
### `TryFromJson(string json, out List<EndpointStat>? stats)`

Attempts to deserialize a JSON string into a list of `EndpointStat` objects. Returns a boolean indicating success.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `stats`: Output parameter containing the deserialized list of `EndpointStat` objects if successful.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

---
### `TryFromJson(string json, out List<HourlyBucket>? buckets)`

Attempts to deserialize a JSON string into a list of `HourlyBucket` objects. Returns a boolean indicating success.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `buckets`: Output parameter containing the deserialized list of `HourlyBucket` objects if successful.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

---
### `TryFromJson(string json, out List<DailyBucket>? buckets)`

Attempts to deserialize a JSON string into a list of `DailyBucket` objects. Returns a boolean indicating success.

- **Parameters**
  - `json`: The JSON string to deserialize.
  - `buckets`: Output parameter containing the deserialized list of `DailyBucket` objects if successful.
- **Returns**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `json` is `null`.

## Usage

```csharp
// Serializing an AnalyticsSummary to JSON
var summary = new AnalyticsSummary
{
    TotalRequests = 1000,
    TotalErrors = 10,
    TopEndpoints = new List<EndpointStat>
    {
        new EndpointStat { Path = "/api/users", RequestCount = 500 },
        new EndpointStat { Path = "/api/products", RequestCount = 300 }
    }
};
string json = AnalyticsControllerJsonExtensions.ToJson(summary);

// Deserializing an AnalyticsSummary from JSON
AnalyticsSummary? deserialized = AnalyticsControllerJsonExtensions.FromJson(json);
if (deserialized != null)
{
    Console.WriteLine($"Total requests: {deserialized.TotalRequests}");
}
```

```csharp
// Serializing and deserializing a list of HourlyBucket
var buckets = new List<HourlyBucket>
{
    new HourlyBucket { Hour = 10, RequestCount = 150 },
    new HourlyBucket { Hour = 11, RequestCount = 200 }
};
string json = AnalyticsControllerJsonExtensions.ToJson(buckets);

List<HourlyBucket>? deserializedBuckets =
    AnalyticsControllerJsonExtensions.FromJsonToHourlyBuckets(json);
if (deserializedBuckets != null)
{
    foreach (var bucket in deserializedBuckets)
    {
        Console.WriteLine($"Hour {bucket.Hour}: {bucket.RequestCount} requests");
    }
}
```

## Notes

- All methods are thread-safe as they do not rely on shared mutable state.
- Deserialization methods (`FromJson`, `FromJsonTo*`, `TryFromJson`) return `null` on failure rather than throwing, allowing callers to handle malformed JSON gracefully.
- Serialization methods (`ToJson`) throw `ArgumentNullException` for `null` inputs, enforcing strict null-checking at the boundary.
- The JSON format assumes default `System.Text.Json` serialization settings; callers must ensure consistent settings if customization is required.

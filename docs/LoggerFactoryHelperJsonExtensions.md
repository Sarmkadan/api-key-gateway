# LoggerFactoryHelperJsonExtensions

Provides JSON serialization and deserialization utilities for `LoggerFactoryConfiguration` objects, enabling configuration to be stored or transmitted as JSON strings.

## API

### `public string? DefaultLogLevel`
Gets or sets the default log level used when no specific level is configured for a logger. The value is a string representation of a valid log level (e.g., "Debug", "Info", "Warning", "Error"). Returns `null` if not set.

### `public bool DebugEnabled`
Gets or sets a value indicating whether debug-level logging is enabled. When `true`, debug messages will be processed by the logger.

### `public bool ConsoleEnabled`
Gets or sets a value indicating whether logging output is directed to the console. When `true`, log messages are written to the standard output stream.

### `public static string ToJson(LoggerFactoryConfiguration configuration)`
Serializes a `LoggerFactoryConfiguration` object into a JSON string.

- **Parameters**
  - `configuration`: The logger factory configuration to serialize. Must not be `null`.
- **Returns**
  A JSON string representation of the configuration.
- **Throws**
  `ArgumentNullException`: Thrown if `configuration` is `null`.

### `public static LoggerFactoryConfiguration? FromJson(string json)`
Deserializes a JSON string into a `LoggerFactoryConfiguration` object.

- **Parameters**
  - `json`: The JSON string to deserialize. Can be `null` or empty.
- **Returns**
  The deserialized `LoggerFactoryConfiguration` object, or `null` if the input is `null` or empty.
- **Throws**
  `JsonException`: Thrown if the JSON is malformed or cannot be mapped to a `LoggerFactoryConfiguration`.

### `public static bool TryFromJson(string json, out LoggerFactoryConfiguration? configuration)`
Attempts to deserialize a JSON string into a `LoggerFactoryConfiguration` object. Unlike `FromJson`, this method does not throw on failure.

- **Parameters**
  - `json`: The JSON string to deserialize. Can be `null` or empty.
  - `configuration`: When this method returns, contains the deserialized configuration if successful; otherwise, `null`.
- **Returns**
  `true` if deserialization succeeded; otherwise, `false`.

## Usage

### Example 1: Serializing a LoggerFactoryConfiguration
```csharp
var config = new LoggerFactoryConfiguration
{
    DefaultLogLevel = "Info",
    DebugEnabled = true,
    ConsoleEnabled = false
};

string json = LoggerFactoryHelperJsonExtensions.ToJson(config);
Console.WriteLine(json);
// Output: {"DefaultLogLevel":"Info","DebugEnabled":true,"ConsoleEnabled":false}
```

### Example 2: Deserializing a LoggerFactoryConfiguration
```csharp
string json = "{\"DefaultLogLevel\":\"Warning\",\"DebugEnabled\":false,\"ConsoleEnabled\":true}";

if (LoggerFactoryHelperJsonExtensions.TryFromJson(json, out var config))
{
    Console.WriteLine($"DefaultLogLevel: {config.DefaultLogLevel}");
    Console.WriteLine($"DebugEnabled: {config.DebugEnabled}");
    Console.WriteLine($"ConsoleEnabled: {config.ConsoleEnabled}");
}
// Output:
// DefaultLogLevel: Warning
// DebugEnabled: False
// ConsoleEnabled: True
```

## Notes

- The `FromJson` method throws `JsonException` on invalid JSON, while `TryFromJson` provides a non-throwing alternative for error handling.
- Thread safety is guaranteed as long as the input `string` is not modified during deserialization. The methods do not maintain shared state.
- Empty or `null` JSON strings passed to `FromJson` or `TryFromJson` result in `null` output, allowing graceful handling of missing configurations.

# CacheKeyGeneratorJsonExtensions

Provides JSON serialization and deserialization support for `CacheKeyGeneratorConfiguration` objects, enabling them to be converted to and from JSON strings. This class also exposes the `Prefix` and `Separator` properties used when constructing cache keys, and offers a factory method to create a configuration from an existing `CacheKeyGenerator` instance.

## API

### `public string Prefix`

Gets the prefix string that is prepended to generated cache keys. This value is used to namespace keys and prevent collisions across different key generators.

### `public char Separator`

Gets the character used to separate the prefix from the key body in generated cache keys. Typically a colon (`:`) or similar delimiter.

### `public static CacheKeyGeneratorConfiguration FromCacheKeyGenerator`

Creates a `CacheKeyGeneratorConfiguration` instance populated with the settings from the specified `CacheKeyGenerator`.

- **Parameters:** An implicit or explicit `CacheKeyGenerator` instance (the exact parameter signature is inferred from the member name).
- **Returns:** A new `CacheKeyGeneratorConfiguration` with `Prefix` and `Separator` values copied from the source generator.
- **Throws:** `ArgumentNullException` if the provided generator is `null`.

### `public static string ToJson`

Serializes a `CacheKeyGeneratorConfiguration` to its JSON string representation.

- **Parameters:** A `CacheKeyGeneratorConfiguration` instance to serialize.
- **Returns:** A JSON string representing the configuration.
- **Throws:** `ArgumentNullException` if the configuration is `null`. May throw `JsonException` if the object graph contains non-serializable data (unlikely for this simple type).

### `public static CacheKeyGeneratorConfiguration? FromJson`

Deserializes a JSON string into a `CacheKeyGeneratorConfiguration` object.

- **Parameters:** A JSON string previously produced by `ToJson` or conforming to the expected schema.
- **Returns:** A `CacheKeyGeneratorConfiguration` instance, or `null` if the input string is `null`.
- **Throws:** `JsonException` if the JSON is malformed or does not match the expected structure. `ArgumentNullException` is not thrown; `null` input returns `null`.

### `public static bool TryFromJson`

Attempts to deserialize a JSON string into a `CacheKeyGeneratorConfiguration` without throwing on failure.

- **Parameters:** A JSON string to parse, and an `out` parameter of type `CacheKeyGeneratorConfiguration?` that receives the result.
- **Returns:** `true` if deserialization succeeded and the output parameter holds a valid configuration; `false` if the JSON was invalid, malformed, or the input string was `null`.
- **Throws:** Does not throw exceptions under normal circumstances. All errors are captured in the return value.

## Usage

### Example 1: Round-tripping a configuration to JSON and back

```csharp
// Create a configuration from an existing generator
var generator = new CacheKeyGenerator("myapp", ':');
var config = CacheKeyGeneratorJsonExtensions.FromCacheKeyGenerator(generator);

// Serialize to JSON for storage or transmission
string json = CacheKeyGeneratorJsonExtensions.ToJson(config);
Console.WriteLine(json); // {"Prefix":"myapp","Separator":":"}

// Deserialize back to a configuration object
CacheKeyGeneratorConfiguration? restored = CacheKeyGeneratorJsonExtensions.FromJson(json);
if (restored != null)
{
    Console.WriteLine($"Prefix: {restored.Prefix}, Separator: {restored.Separator}");
}
```

### Example 2: Safe deserialization with TryFromJson

```csharp
string userInput = GetUserProvidedJson(); // May be malformed

if (CacheKeyGeneratorJsonExtensions.TryFromJson(userInput, out var config))
{
    // Use the successfully parsed configuration
    string prefix = config.Prefix;
    char separator = config.Separator;
    Console.WriteLine($"Using prefix '{prefix}' with separator '{separator}'");
}
else
{
    // Fall back to defaults
    Console.WriteLine("Invalid configuration JSON, using defaults.");
    string prefix = CacheKeyGeneratorJsonExtensions.Prefix;
    char separator = CacheKeyGeneratorJsonExtensions.Separator;
}
```

## Notes

- **Thread safety:** All public static methods are stateless and operate on their input parameters without shared mutable state. They are safe to call concurrently from multiple threads. The `Prefix` and `Separator` properties are read-only and safe for concurrent access.
- **Null handling:** `FromJson` returns `null` when given a `null` string, while `TryFromJson` returns `false` and sets the output parameter to `null` in the same case. `ToJson` does not accept `null` and will throw `ArgumentNullException`.
- **Edge cases:** The `Separator` property is a `char`, so JSON representations must supply a single-character string. Multi-character strings or empty strings in the JSON will cause `FromJson` to throw a `JsonException` and `TryFromJson` to return `false`. The `Prefix` may be an empty string, which is valid and serializes as `""`.
- **Schema stability:** The JSON schema produced by `ToJson` is determined by the serialization settings in use. Consumers relying on `FromJson` or `TryFromJson` should ensure the JSON was produced by a compatible version of `ToJson` to avoid deserialization failures.

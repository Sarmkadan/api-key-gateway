# JsonSerializationHelperJsonExtensions
The `JsonSerializationHelperJsonExtensions` type provides a set of helper methods and properties for JSON serialization and deserialization in the context of the `api-key-gateway` project. It allows for customization of JSON naming policies, ignore conditions, and formatting, as well as conversion between JSON strings and `JsonSerializationSettings` objects.

## API
* `public JsonNamingPolicy PropertyNamingPolicy`: Gets the JSON naming policy used for property names.
* `public JsonIgnoreCondition DefaultIgnoreCondition`: Gets the default ignore condition for JSON serialization.
* `public bool WriteIndented`: Gets a value indicating whether to write JSON with indentation.
* `public static string ToJson`: Converts an object to a JSON string.
* `public static JsonSerializationSettings? FromJson`: Deserializes a JSON string into a `JsonSerializationSettings` object.
* `public static bool TryFromJson`: Attempts to deserialize a JSON string into a `JsonSerializationSettings` object, returning a boolean indicating success.

## Usage
The following examples demonstrate how to use the `JsonSerializationHelperJsonExtensions` type:
```csharp
// Example 1: Serializing an object to JSON
var settings = new JsonSerializationSettings();
var json = JsonSerializationHelperJsonExtensions.ToJson(settings);
Console.WriteLine(json);

// Example 2: Deserializing JSON to a JsonSerializationSettings object
var json = "{\"PropertyNamingPolicy\":\"CamelCase\"}";
if (JsonSerializationHelperJsonExtensions.TryFromJson(json, out var settings))
{
    Console.WriteLine(settings.PropertyNamingPolicy);
}
else
{
    Console.WriteLine("Deserialization failed");
}
```

## Notes
When using the `JsonSerializationHelperJsonExtensions` type, note that the `FromJson` and `TryFromJson` methods may throw exceptions if the input JSON string is invalid or cannot be deserialized into a `JsonSerializationSettings` object. Additionally, the `WriteIndented` property only affects the formatting of the output JSON string and does not impact the deserialization process. The `JsonSerializationHelperJsonExtensions` type is designed to be thread-safe, allowing for concurrent access to its properties and methods. However, the `JsonSerializationSettings` objects created or modified by this type should be treated as immutable to avoid unexpected behavior.

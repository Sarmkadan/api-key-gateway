# JsonSerializationHelper
The `JsonSerializationHelper` class provides a set of static methods for serializing and deserializing JSON data in a .NET environment, specifically designed to work with the `api-key-gateway` project. It offers methods for compact and formatted serialization, as well as safe deserialization, making it a versatile tool for handling JSON data.

## API
### SerializeCompact<T>
Serializes an object of type `T` into a compact JSON string. This method is useful when the size of the JSON output is a concern, such as in network transmissions or storage constraints.
- Parameters: The object of type `T` to be serialized.
- Return Value: A JSON string representing the object in a compact format.
- Throws: Exceptions related to serialization failures.

### SerializeFormatted<T>
Serializes an object of type `T` into a formatted JSON string. This method is useful for debugging or logging purposes where readability of the JSON output is important.
- Parameters: The object of type `T` to be serialized.
- Return Value: A JSON string representing the object in a formatted and readable format.
- Throws: Exceptions related to serialization failures.

### Deserialize<T>
Deserializes a JSON string into an object of type `T`. This method attempts to parse the JSON string and create an instance of `T` based on the JSON data.
- Parameters: The JSON string to be deserialized.
- Return Value: An instance of `T` if deserialization is successful, otherwise `null`.
- Throws: Exceptions related to deserialization failures.

### SafeDeserialize<T>
Safely deserializes a JSON string into an object of type `T`, handling potential exceptions that may occur during the deserialization process.
- Parameters: The JSON string to be deserialized.
- Return Value: An instance of `T` if deserialization is successful, otherwise `null`.
- Throws: No exceptions are thrown; instead, `null` is returned if deserialization fails.

### IsValidJson
Checks if a given string is valid JSON.
- Parameters: The string to be checked.
- Return Value: `true` if the string is valid JSON, `false` otherwise.
- Throws: No exceptions are thrown.

## Usage
```csharp
// Example 1: Serializing an object
var exampleObject = new { id = 1, name = "Example" };
var compactJson = JsonSerializationHelper.SerializeCompact(exampleObject);
Console.WriteLine(compactJson); // Output: {"id":1,"name":"Example"}

// Example 2: Deserializing JSON
var json = "{\"id\":2,\"name\":\"Another Example\"}";
var deserializedObject = JsonSerializationHelper.Deserialize<dynamic>(json);
Console.WriteLine(deserializedObject.id); // Output: 2
Console.WriteLine(deserializedObject.name); // Output: Another Example
```

## Notes
- **Thread Safety**: All methods in `JsonSerializationHelper` are static and do not maintain any state, making them thread-safe for concurrent access.
- **Edge Cases**: When dealing with very large JSON strings or complex objects, consider the performance implications of serialization and deserialization. Additionally, be aware that `Deserialize` and `SafeDeserialize` may return `null` if the JSON string is invalid or cannot be deserialized into the specified type.
- **JSON Validation**: The `IsValidJson` method can be used to pre-validate JSON strings before attempting deserialization, potentially reducing the number of exceptions thrown during the deserialization process.

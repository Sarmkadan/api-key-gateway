# ApiResponseBuilderJsonExtensions
The `ApiResponseBuilderJsonExtensions` type provides a set of extension methods for working with JSON data in the context of API responses. It enables serialization and deserialization of API response data, allowing for easy conversion between JSON strings and strongly-typed `ApiResponseBuilder` objects.

## API
* `public static string ToJson<T>(this ApiResponseBuilder<T> builder)`: Serializes an `ApiResponseBuilder` object into a JSON string. The `T` type parameter represents the type of data contained in the response.
* `public static ApiResponseBuilder<T>? FromJson<T>(string json)`: Deserializes a JSON string into an `ApiResponseBuilder` object. The `T` type parameter represents the type of data contained in the response. Returns `null` if deserialization fails.
* `public static bool TryFromJson<T>(string json, out ApiResponseBuilder<T>? builder)`: Attempts to deserialize a JSON string into an `ApiResponseBuilder` object. The `T` type parameter represents the type of data contained in the response. Returns `true` if deserialization is successful, and sets the `builder` out parameter to the deserialized object.
* `public bool Success`: Gets a value indicating whether the API response was successful.
* `public int? StatusCode`: Gets the HTTP status code of the API response.
* `public string? Message`: Gets a message associated with the API response.
* `public string? ErrorCode`: Gets an error code associated with the API response.
* `public object? Data`: Gets the data contained in the API response.
* `public List<string>? Errors`: Gets a list of error messages associated with the API response.
* `public Dictionary<string, object>? Metadata`: Gets a dictionary of metadata associated with the API response.

## Usage
```csharp
// Example 1: Serializing an ApiResponseBuilder to JSON
var builder = new ApiResponseBuilder<string>();
builder.Success = true;
builder.StatusCode = 200;
builder.Data = "Hello, World!";
var json = builder.ToJson();
Console.WriteLine(json); // Output: {"Success":true,"StatusCode":200,"Data":"Hello, World!"}

// Example 2: Deserializing JSON to an ApiResponseBuilder
var json = "{\"Success\":true,\"StatusCode\":200,\"Data\":\"Hello, World!\"}";
var builder = ApiResponseBuilderJsonExtensions.FromJson<string>(json);
Console.WriteLine(builder.Success); // Output: True
Console.WriteLine(builder.StatusCode); // Output: 200
Console.WriteLine(builder.Data); // Output: Hello, World!
```

## Notes
When using the `FromJson` and `TryFromJson` methods, be aware that they may throw exceptions if the JSON string is malformed or cannot be deserialized into an `ApiResponseBuilder` object. Additionally, the `Data` property may contain any type of object, so caution should be exercised when accessing its value. The `ApiResponseBuilderJsonExtensions` type is designed to be thread-safe, but it is still important to ensure that any shared instances of `ApiResponseBuilder` objects are properly synchronized to avoid concurrency issues.

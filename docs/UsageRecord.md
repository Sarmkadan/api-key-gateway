# UsageRecord
The `UsageRecord` type represents a single usage event of an API key, capturing details such as the API key used, the endpoint accessed, the response status code, and other relevant metrics. This type is used to track and analyze API usage, providing valuable insights into how APIs are being utilized.

## API
The `UsageRecord` type has the following public members:
* `Id`: A unique identifier for the usage record.
* `ApiKeyId`: The ID of the API key used for the request.
* `ConsumerId`: The ID of the consumer that made the request.
* `RecordedAt`: The date and time when the usage record was created.
* `Endpoint`: The endpoint that was accessed.
* `Method`: The HTTP method used for the request.
* `ResponseStatusCode`: The HTTP status code of the response.
* `RequestBytes`: The number of bytes in the request body.
* `ResponseBytes`: The number of bytes in the response body.
* `ResponseTimeMs`: The response time in milliseconds.
* `ErrorCode`: An optional error code if the request failed.
* `SourceIp`: The IP address of the client that made the request.
* `UserAgent`: The user agent string of the client that made the request.
* `Tags`: A dictionary of tags associated with the usage record.
* `CalculateTotalBytes`: A static method that calculates the total bytes of a collection of usage records.
* `CalculateAverageResponseTime`: A static method that calculates the average response time of a collection of usage records.
* `CountSuccessfulRequests`: A static method that counts the number of successful requests in a collection of usage records.
* `CountErrorRequests`: A static method that counts the number of error requests in a collection of usage records.

## Usage
Here are two examples of using the `UsageRecord` type:
```csharp
// Create a new usage record
var usageRecord = new UsageRecord
{
    Id = Guid.NewGuid().ToString(),
    ApiKeyId = "my-api-key",
    ConsumerId = "my-consumer",
    RecordedAt = DateTime.UtcNow,
    Endpoint = "/api/endpoint",
    Method = "GET",
    ResponseStatusCode = 200,
    RequestBytes = 0,
    ResponseBytes = 1024,
    ResponseTimeMs = 50,
    Tags = new Dictionary<string, string> { { "tag1", "value1" } }
};

// Calculate the total bytes of a collection of usage records
var usageRecords = new List<UsageRecord> { usageRecord, usageRecord };
var totalBytes = UsageRecord.CalculateTotalBytes(usageRecords);
Console.WriteLine($"Total bytes: {totalBytes}");
```

## Notes
When using the `UsageRecord` type, note that the `Tags` dictionary is not thread-safe, and concurrent modifications may result in unexpected behavior. Additionally, the `CalculateTotalBytes`, `CalculateAverageResponseTime`, `CountSuccessfulRequests`, and `CountErrorRequests` static methods do not throw exceptions, but may return default values if the input collection is empty. The `ErrorCode`, `SourceIp`, and `UserAgent` properties may be null if the corresponding information is not available. The `ResponseTimeMs` property may be zero if the response time is not measured. The `RequestBytes` and `ResponseBytes` properties may be zero if the request or response body is empty.

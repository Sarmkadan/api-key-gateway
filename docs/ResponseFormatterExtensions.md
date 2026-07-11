# ResponseFormatterExtensions

The `ResponseFormatterExtensions` class provides a set of static helper methods and extension properties designed to standardize the structure of API responses within the `api-key-gateway` project. It facilitates the consistent creation of success and error payloads, as well as paginated result sets, ensuring that all endpoints return data adhering to a unified schema containing status codes, timestamps, error codes, and payload data.

## API

### Static Methods

#### `Success<T>`
```csharp
public static ApiResponse<T> Success<T>(T data, string? message = null)
```
Constructs a standardized successful API response.
*   **Purpose**: Generates an `ApiResponse<T>` instance indicating a successful operation.
*   **Parameters**:
    *   `data`: The payload object to be returned to the client.
    *   `message`: An optional human-readable status message.
*   **Return Value**: An `ApiResponse<T>` with `Success` set to `true`, `StatusCode` typically set to 200, and the provided `data` populated in the `Data` property.
*   **Throws**: No exceptions are thrown under normal usage; relies on the default constructor behavior of `ApiResponse<T>`.

#### `Error<T>`
```csharp
public static ApiResponse<T> Error<T>(string errorCode, string message, object? details = null, int statusCode = 400)
```
Constructs a standardized error API response.
*   **Purpose**: Generates an `ApiResponse<T>` instance indicating a failed operation.
*   **Parameters**:
    *   `errorCode`: A machine-readable string code identifying the specific error type.
    *   `message`: A human-readable description of the error.
    *   `details`: An optional object containing additional context or validation errors.
    *   `statusCode`: The HTTP status code to associate with the error (defaults to 400).
*   **Return Value**: An `ApiResponse<T>` with `Success` set to `false`, the specified `StatusCode`, `ErrorCode`, `Message`, and `Details` populated. The `Data` property is null.
*   **Throws**: No exceptions are thrown; invalid arguments may result in a response with null fields depending on implementation, but the method itself is safe.

#### `Paginated<T>`
```csharp
public static PaginatedResponse<T> Paginated<T>(List<T> items, int pageNumber, int pageSize, int totalCount)
```
Constructs a standardized paginated response.
*   **Purpose**: Generates a `PaginatedResponse<T>` containing a subset of data along with metadata required for client-side pagination.
*   **Parameters**:
    *   `items`: The list of items for the current page.
    *   `pageNumber`: The current page index (1-based).
    *   `pageSize`: The number of items per page.
    *   `totalCount`: The total number of items available across all pages.
*   **Return Value**: A `PaginatedResponse<T>` with `Items`, `PageNumber`, `PageSize`, `TotalCount`, `TotalPages`, and `HasMore` calculated and populated automatically. Includes a `Timestamp`.
*   **Throws**: May throw `ArgumentOutOfRangeException` if `pageNumber` or `pageSize` is less than 1, or if `totalCount` is negative.

### Response Properties

The following properties are exposed on the response objects (`ApiResponse<T>` and `PaginatedResponse<T>`) generated or utilized by this extension class:

*   **`Success`** (`bool`): Indicates whether the operation completed successfully.
*   **`StatusCode`** (`int`): The HTTP status code associated with the response.
*   **`Data`** (`T?`): The primary payload of the response; null in case of errors or empty results.
*   **`Message`** (`string?`): A human-readable summary of the result or error.
*   **`ErrorCode`** (`string?`): A machine-readable identifier for specific error conditions.
*   **`Details`** (`object?`): Additional contextual data, often used for validation error lists or stack traces in development.
*   **`Timestamp`** (`DateTime`): The UTC time at which the response was generated.
*   **`Items`** (`List<T>`): The collection of entities returned in a paginated response.
*   **`PageNumber`** (`int`): The current page number being returned.
*   **`PageSize`** (`int`): The maximum number of items requested per page.
*   **`TotalCount`** (`int`): The total number of records available in the dataset.
*   **`TotalPages`** (`int`): The calculated total number of pages based on `TotalCount` and `PageSize`.
*   **`HasMore`** (`bool`): A boolean flag indicating if additional pages exist after the current one.

## Usage

### Example 1: Handling a Standard Success and Error Scenario
This example demonstrates how to use the static helpers to return consistent JSON structures from a controller action when fetching an API key configuration.

```csharp
public async Task<IActionResult> GetApiKeyConfiguration(string keyId)
{
    try
    {
        var config = await _repository.GetByIdAsync(keyId);
        if (config == null)
        {
            // Return a 404 structured error
            var errorResponse = ResponseFormatterExtensions.Error<ApiKeyConfig>(
                errorCode: "KEY_NOT_FOUND",
                message: "The specified API key does not exist.",
                statusCode: 404
            );
            return NotFound(errorResponse);
        }

        // Return a 200 structured success
        var successResponse = ResponseFormatterExtensions.Success(config, "Configuration retrieved successfully.");
        return Ok(successResponse);
    }
    catch (Exception ex)
    {
        // Return a 500 structured error with details
        var errorResponse = ResponseFormatterExtensions.Error<ApiKeyConfig>(
            errorCode: "INTERNAL_SERVER_ERROR",
            message: "An unexpected error occurred while fetching configuration.",
            details: new { ExceptionType = ex.GetType().Name },
            statusCode: 500
        );
        return StatusCode(500, errorResponse);
    }
}
```

### Example 2: Returning Paginated Usage Logs
This example illustrates the creation of a paginated response for listing usage logs, automatically calculating pagination metadata.

```csharp
public async Task<IActionResult> GetUsageLogs(int page, int pageSize)
{
    // Ensure valid pagination parameters
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 10;

    var logs = await _usageRepository.GetPageAsync(page, pageSize);
    var totalCount = await _usageRepository.GetTotalCountAsync();

    // Generate the standardized paginated response
    var paginatedResponse = ResponseFormatterExtensions.Paginated<UsageLog>(
        items: logs,
        pageNumber: page,
        pageSize: pageSize,
        totalCount: totalCount
    );

    // The response includes Items, TotalPages, HasMore, and Timestamp automatically
    return Ok(paginatedResponse);
}
```

## Notes

*   **Thread Safety**: As `ResponseFormatterExtensions` consists entirely of static methods that do not maintain internal mutable state, it is inherently thread-safe. Multiple concurrent requests can safely invoke `Success`, `Error`, or `Paginated` simultaneously.
*   **Generic Type Constraints**: The generic type `T` used in `Success<T>`, `Error<T>`, and `Paginated<T>` has no specific constraints. Care should be taken to ensure `T` is serializable by the configured JSON serializer to avoid runtime serialization errors.
*   **Pagination Edge Cases**: When using `Paginated<T>`, if `totalCount` is 0, the `TotalPages` property will resolve to 0 (or 1 depending on specific implementation logic regarding empty sets), and `HasMore` will be `false`. Passing a `pageSize` of 0 or negative values to the helper method may result in division by zero exceptions during the calculation of `TotalPages`; callers should validate inputs before invocation.
*   **Timestamp Consistency**: The `Timestamp` property is populated at the moment of object creation. In high-throughput scenarios, ensure that system clock synchronization is maintained across server instances if response times are used for auditing or cache invalidation logic.
*   **Null Handling**: The `Data` property in success responses and `Details` in error responses are nullable. Clients consuming these APIs must handle cases where these fields are explicitly null versus absent, depending on the serializer configuration.

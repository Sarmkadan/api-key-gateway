# IAuditLogRepository
The `IAuditLogRepository` interface is designed to provide a standardized way of interacting with audit log data, allowing for the creation, retrieval, and deletion of audit log entries. This interface is crucial for maintaining a record of system activities, enabling auditing, and ensuring compliance with regulatory requirements.

## API
### `AuditLogRepository`
The `AuditLogRepository` property provides an instance of the `AuditLogRepository` class, which implements the `IAuditLogRepository` interface.

### `CreateAsync`
Creates a new audit log entry asynchronously.
* Parameters: Not specified in the provided information.
* Return Value: The method returns a `Task`, indicating an asynchronous operation.
* Exceptions: Not specified in the provided information, but it may throw exceptions related to data access or validation errors.

### `GetByResourceIdAsync`
Retrieves a list of audit log entries associated with a specific resource ID asynchronously.
* Parameters: `resourceId` - The ID of the resource for which to retrieve audit log entries.
* Return Value: A `Task` that returns a `List<AuditLog>`, containing the audit log entries for the specified resource ID.
* Exceptions: May throw exceptions related to data access or validation errors.

### `GetByDateRangeAsync`
Retrieves a list of audit log entries within a specified date range asynchronously.
* Parameters: Not specified in the provided information, but it is expected to include start and end dates.
* Return Value: A `Task` that returns a `List<AuditLog>`, containing the audit log entries within the specified date range.
* Exceptions: May throw exceptions related to data access or validation errors.

### `DeleteOlderThanAsync`
Deletes audit log entries older than a specified date or time asynchronously.
* Parameters: Not specified in the provided information, but it is expected to include a date or time threshold.
* Return Value: A `Task` that returns an `int`, indicating the number of deleted audit log entries.
* Exceptions: May throw exceptions related to data access or validation errors.

## Usage
The following examples demonstrate how to use the `IAuditLogRepository` interface:
```csharp
// Example 1: Creating a new audit log entry
var auditLogRepository = new AuditLogRepository();
await auditLogRepository.CreateAsync(...); // Assuming parameters are provided

// Example 2: Retrieving audit log entries by resource ID
var resourceId = "example-resource-id";
var auditLogs = await auditLogRepository.GetByResourceIdAsync(resourceId);
foreach (var auditLog in auditLogs)
{
    Console.WriteLine($"Resource ID: {auditLog.ResourceId}, Log Date: {auditLog.LogDate}");
}
```

## Notes
When using the `IAuditLogRepository` interface, consider the following:
* The `CreateAsync` method may throw exceptions if the provided data is invalid or if there are issues with data access.
* The `GetByResourceIdAsync` and `GetByDateRangeAsync` methods may return an empty list if no audit log entries match the specified criteria.
* The `DeleteOlderThanAsync` method may throw exceptions if there are issues with data access or if the specified date or time threshold is invalid.
* The `IAuditLogRepository` interface is designed to be thread-safe, allowing for concurrent access and manipulation of audit log data. However, it is essential to ensure that the underlying data storage and retrieval mechanisms are also thread-safe to avoid data corruption or inconsistencies.

# IBatchOperationHandler

Interface for handling batch operations on API keys, providing execution tracking, result aggregation, and error management.

## API

### `Id`
Unique identifier for the batch operation instance. Read-only.

### `OperationType`
Gets the type of operation being performed (e.g., "Rotate", "Disable", "Enable"). Required during initialization.

### `ApiKeyIds`
List of API key identifiers targeted by the batch operation. Required during initialization.

### `Parameters`
Optional dictionary of additional parameters for the operation. May be `null`.

### `CreatedAt`
Timestamp indicating when the batch operation was created. Read-only.

### `OperationId`
Identifier of the parent operation this batch belongs to. Read-only.

### `TotalCount`
Total number of API keys in the batch. Read-only.

### `SuccessCount`
Number of successfully processed API keys. Read-only.

### `FailureCount`
Number of failed API key operations. Read-only.

### `Items`
List of individual operation results for each API key in the batch. Read-only.

### `CompletedAt`
Timestamp indicating when the batch operation completed. `null` if not completed. Read-only.

### `ApiKeyId`
Identifier of the API key associated with a single-item operation result. Read-only.

### `Success`
Indicates whether the single-item operation succeeded. Read-only.

### `ErrorMessage`
Error message if the single-item operation failed. `null` if successful. Read-only.

### `Result`
Operation result payload if successful. `null` if failed or not applicable. Read-only.

### `ExecuteAsync`
Executes the batch operation asynchronously.

**Returns**
`Task<BatchOperationResult>` containing overall batch success status and summary.

**Throws**
`InvalidOperationException` if the batch is already completed or contains no API keys.
`OperationCanceledException` if cancellation is requested during execution.

## Usage

### Example 1: Executing a batch operation

# AuditLog

The `AuditLog` type records administrative or system actions performed against resources in the API Key Gateway, such as key creation, updates, or deletions. It captures contextual details including the actor, timestamp, affected resource, and the nature of the change, enabling auditability and compliance tracking.

## API

### `public string Id`
A unique identifier for the audit log entry. This value is generated when the log is created and is immutable.

### `public string ResourceId`
The identifier of the resource (e.g., an API key or gateway) to which the logged action pertains. This field is non-null and must correspond to a valid resource in the system.

### `public string ResourceType`
The type of resource affected by the action (e.g., `"ApiKey"`, `"GatewayConfiguration"`). This field is non-null and restricts the scope of the audit entry to a known resource category.

### `public Enums.AuditAction Action`
The type of action performed (e.g., `Create`, `Update`, `Delete`, `Rotate`). This field is non-null and determines the semantic meaning of the log entry.

### `public string PerformedBy`
The identifier (e.g., user ID, service principal) of the entity that initiated the action. This field is non-null and reflects the authenticated principal responsible for the change.

### `public DateTime PerformedAt`
The timestamp at which the action was performed. This value is set automatically when the log is created and is immutable.

### `public int? HttpStatusCode`
The HTTP status code returned by the API in response to the action, if applicable (e.g., `200`, `404`). This field is optional and may be `null` for non-HTTP actions or internal system events.

### `public string? SourceIp`
The IP address from which the action was initiated, if available. This field is optional and may be `null` for actions originating from internal services or when IP tracking is disabled.

### `public string? Reason`
A human-readable justification or rationale for the action, particularly for privileged or destructive operations. This field is optional and may be `null` if no reason was provided.

### `public Dictionary<string, object> Changes`
A collection of key-value pairs representing the before-and-after state of the resource, or the specific fields modified during the action. This field is non-null and may be empty if no state changes were recorded.

### `public string? ErrorMessage`
A description of any error that occurred during the action, if the operation failed. This field is optional and may be `null` for successful operations.

### `public bool IsSuccess`
Indicates whether the action completed successfully (`true`) or encountered an error (`false`). This field is set automatically based on the presence or absence of an `ErrorMessage`.

### `public string GetActionDescription()`
Returns a human-readable description of the action based on its `Action` value and any additional context (e.g., `"Created API key 'abc123'"` or `"Failed to delete key: NotFound"`). This method has no parameters and always returns a non-null string.

### `public void RecordChange(string fieldName, object oldValue, object newValue)`
Records a change to a specific field of the resource. The `fieldName` parameter identifies the field, while `oldValue` and `newValue` capture its previous and current states. This method has no return value and may be called multiple times to accumulate changes. It throws an `ArgumentNullException` if `fieldName` is `null`.

## Usage

### Example 1: Recording a successful key creation

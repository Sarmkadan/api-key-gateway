# IDataExportService

The `IDataExportService` provides asynchronous methods to export sensitive data such as API keys, audit logs, and usage metrics in a structured format. It is designed for administrative and compliance purposes where bulk data retrieval is required without exposing raw data through primary endpoints.

## API

### `ExportApiKeysAsync`

Exports all active API keys in the system as a structured string.

- **Parameters**: None
- **Return value**: `Task<string>` containing the exported data in a predefined format (e.g., JSON or CSV).
- **Exceptions**:
  - Throws `UnauthorizedAccessException` if the caller lacks sufficient permissions.
  - Throws `DataExportException` if the export operation fails due to system constraints.

### `ExportAuditLogsAsync`

Exports all audit logs recorded in the system as a structured string.

- **Parameters**: None
- **Return value**: `Task<string>` containing the exported audit logs in a predefined format.
- **Exceptions**:
  - Throws `UnauthorizedAccessException` if the caller lacks sufficient permissions.
  - Throws `DataExportException` if the export operation fails due to system constraints.

### `ExportUsageAsync`

Exports aggregated usage metrics for all tracked resources as a structured string.

- **Parameters**: None
- **Return value**: `Task<string>` containing the exported usage data in a predefined format.
- **Exceptions**:
  - Throws `UnauthorizedAccessException` if the caller lacks sufficient permissions.
  - Throws `DataExportException` if the export operation fails due to system constraints.

## Usage

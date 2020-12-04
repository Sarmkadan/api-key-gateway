# ApiKeyGatewayException

A custom exception type used within the `api-key-gateway` project to represent errors specific to API key gateway operations. This exception provides structured error information, including an optional error code and a timestamp, to facilitate consistent error handling and logging across the system.

## API

### `ErrorCode`
A nullable string property that represents a machine-readable error code associated with the exception. This can be used to programmatically identify the type of error without parsing the message text. This property is set during construction and is read-only.

### `OccurredAt`
A `DateTime` property indicating the exact moment when the exception was instantiated. This timestamp is set at construction time and is read-only. It provides precise tracking of when an error occurred, which is useful for debugging and monitoring.

### `ApiKeyGatewayException(string message)`
Constructs a new instance of `ApiKeyGatewayException` with the specified error message. The `ErrorCode` property will be `null`, and `OccurredAt` will be set to the current UTC time.

- **Parameters**:
  - `message` (string): A human-readable description of the error.
- **Throws**: Does not throw; always constructs a valid exception.

### `ApiKeyGatewayException(string message, string errorCode)`
Constructs a new instance of `ApiKeyGatewayException` with the specified error message and error code. The `OccurredAt` property will be set to the current UTC time.

- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `errorCode` (string): A machine-readable identifier for the error type.
- **Throws**: Does not throw; always constructs a valid exception.

### `ApiKeyGatewayException(string message, Exception innerException)`
Constructs a new instance of `ApiKeyGatewayException` with the specified error message and an inner exception. The `ErrorCode` property will be `null`, and `OccurredAt` will be set to the current UTC time.

- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `innerException` (Exception): The inner exception that caused this exception to be thrown.
- **Throws**: Does not throw; always constructs a valid exception.

### `ApiKeyGatewayException(string message, string errorCode, Exception innerException)`
Constructs a new instance of `ApiKeyGatewayException` with the specified error message, error code, and inner exception. The `OccurredAt` property will be set to the current UTC time.

- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `errorCode` (string): A machine-readable identifier for the error type.
  - `innerException` (Exception): The inner exception that caused this exception to be thrown.
- **Throws**: Does not throw; always constructs a valid exception.

## Usage

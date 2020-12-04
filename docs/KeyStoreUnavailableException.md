# KeyStoreUnavailableException

Exception thrown when the key store required for an API key operation is unavailable or inaccessible. This typically occurs during key retrieval, storage, or management operations when the underlying key store service (e.g., HSM, database, or cloud KMS) is unreachable, misconfigured, or under maintenance.

## API

### Properties

- **`Operation`** (string?)
  - Gets the name of the key store operation that failed (e.g., "Retrieve", "Store", "Delete").
  - May be `null` if the operation was not specified during construction.

### Constructors

- **`KeyStoreUnavailableException(string message)`**
  - Initializes a new instance with a specified error message.
  - Parameters:
    - `message` (string): The message describing the exception.

- **`KeyStoreUnavailableException(string message, string operation)`**
  - Initializes a new instance with a specified error message and the key store operation that failed.
  - Parameters:
    - `message` (string): The message describing the exception.
    - `operation` (string): The name of the key store operation that failed.

- **`KeyStoreUnavailableException(string message, Exception? innerException)`**
  - Initializes a new instance with a specified error message and a reference to the inner exception that is the cause of this exception.
  - Parameters:
    - `message` (string): The message describing the exception.
    - `innerException` (Exception?): The inner exception that caused this exception.

- **`KeyStoreUnavailableException(string message, string operation, Exception? innerException)`**
  - Initializes a new instance with a specified error message, the key store operation that failed, and a reference to the inner exception that is the cause of this exception.
  - Parameters:
    - `message` (string): The message describing the exception.
    - `operation` (string): The name of the key store operation that failed.
    - `innerException` (Exception?): The inner exception that caused this exception.

## Usage

### Example 1: Handling a key store failure during key retrieval

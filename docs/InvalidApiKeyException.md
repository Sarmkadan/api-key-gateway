# InvalidApiKeyException

An exception thrown by the `api-key-gateway` project when an invalid or expired API key is encountered during request processing. This exception provides contextual details about the failure, including the hashed key value, occurrence timestamp, and expiration status.

## API

### Properties

- **`ApiKeyHash`** (type: `string?`)
  The SHA-256 hash of the invalid API key, if available. This value is redacted in production logs and used internally for auditing and debugging. May be `null` if the key was not provided or could not be hashed.

- **`OccurredAt`** (type: `DateTime?`)
  The UTC timestamp when the exception was created. Defaults to `DateTime.UtcNow` if not explicitly set. Used for tracking when invalid key usage occurred and correlating with other system events.

- **`IsExpired`** (type: `bool`)
  Indicates whether the API key was rejected due to expiration. When `true`, the key was valid but past its allowed usage window. When `false`, the key was structurally invalid or revoked.

### Constructors

- **`InvalidApiKeyException(string message)`**
  Initializes a new instance with a custom error message. Sets `OccurredAt` to `DateTime.UtcNow` and `IsExpired` to `false`. The `ApiKeyHash` remains `null`.

- **`InvalidApiKeyException(string message, string apiKeyHash)`**
  Initializes a new instance with a custom error message and the SHA-256 hash of the invalid API key. Sets `OccurredAt` to `DateTime.UtcNow` and `IsExpired` to `false`.

- **`InvalidApiKeyException(string message, bool isExpired)`**
  Initializes a new instance with a custom error message and an expiration flag. Sets `OccurredAt` to `DateTime.UtcNow`. The `ApiKeyHash` remains `null`.

## Usage

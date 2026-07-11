# UsageRepository

Central repository for persisting and querying API usage records. Provides CRUD operations for tracking per-request consumption metrics, enabling rate limiting, quota enforcement, and analytics.

## API

### `public UsageRepository`

Constructor that initializes the repository with a database connection. The connection is expected to be open and managed externally.

### `public async Task CreateAsync(UsageRecord record)`

Creates a new usage record in the underlying storage.

- **Parameters**
  - `record`: The usage record to persist. Must not be null.
- **Return value**
  - A `Task` representing the asynchronous operation.
- **Exceptions**
  - Throws `ArgumentNullException` if `record` is null.

### `public async Task<List<UsageRecord>> GetByApiKeyAndDateRangeAsync(string apiKey, DateTimeOffset start, DateTimeOffset end)`

Retrieves all usage records for a given API key within a specified date range.

- **Parameters**
  - `apiKey`: The API key to filter by. Must not be null or empty.
  - `start`: The inclusive start of the date range.
  - `end`: The inclusive end of the date range.
- **Return value**
  - A `Task` resolving to a list of matching `UsageRecord` entries. Returns an empty list if no records exist.
- **Exceptions**
  - Throws `ArgumentException` if `apiKey` is null or empty.
  - Throws `ArgumentOutOfRangeException` if `start` is after `end`.

### `public async Task<List<UsageRecord>> GetUsageAsync(DateTimeOffset start, DateTimeOffset end)`

Retrieves all usage records within a specified date range, regardless of API key.

- **Parameters**
  - `start`: The inclusive start of the date range.
  - `end`: The inclusive end of the date range.
- **Return value**
  - A `Task` resolving to a list of all matching `UsageRecord` entries. Returns an empty list if no records exist.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `start` is after `end`.

### `public async Task<List<UsageRecord>> GetByConsumerAndDateRangeAsync(string consumerId, DateTimeOffset start, DateTimeOffset end)`

Retrieves all usage records for a given consumer within a specified date range.

- **Parameters**
  - `consumerId`: The consumer identifier to filter by. Must not be null or empty.
  - `start`: The inclusive start of the date range.
  - `end`: The inclusive end of the date range.
- **Return value**
  - A `Task` resolving to a list of matching `UsageRecord` entries. Returns an empty list if no records exist.
- **Exceptions**
  - Throws `ArgumentException` if `consumerId` is null or empty.
  - Throws `ArgumentOutOfRangeException` if `start` is after `end`.

### `public async Task DeleteOldRecordsAsync(DateTimeOffset threshold)`

Deletes all usage records older than the specified threshold.

- **Parameters**
  - `threshold`: The cutoff date; records with timestamps strictly older than this value will be removed.
- **Return value**
  - A `Task` representing the asynchronous deletion operation.
- **Exceptions**
  - None.

## Usage

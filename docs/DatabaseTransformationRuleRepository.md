# DatabaseTransformationRuleRepository

`DatabaseTransformationRuleRepository` is the database-backed implementation of `ITransformationRuleRepository`. It stores `TransformationRule` entities in the `TransformationRules` table through the gateway's `IDbConnection` abstraction (raw ADO.NET, hand-written SQL, no EF) and is used by the transformation pipeline to load the rules that apply to a request.

- **Namespace:** `ApiKeyGateway.Repositories`
- **Assembly:** `ApiKeyGateway`
- **Declaration:** `public sealed class DatabaseTransformationRuleRepository : ITransformationRuleRepository`
- **Source:** `src/ApiKeyGateway/Repositories/DatabaseTransformationRuleRepository.cs`

## Behavior

- **Connection handling.** Each public method calls `IDbConnection.OpenAsync()` before running its command and `CloseAsync()` in a `finally` block. The connection is therefore opened and closed per call. `IDbConnection` wraps a single connection, so register the repository with a scoped lifetime and do not share one instance across concurrent operations.
- **Read filtering.** All `Get*` methods return only rows where `IsEnabled = 1`. Results are ordered by `Priority` ascending, so lower values run first.
- **Soft delete.** `DeleteAsync` does not remove the row. It sets `IsEnabled = 0` and updates `UpdatedAt`.
- **Enabled on create.** `CreateAsync` always stores `IsEnabled = true`, regardless of the value on the input rule.
- **Id assignment.** `CreateAsync` assigns a new GUID only when `rule.Id` is `null`. `TransformationRule.Id` defaults to `string.Empty`, so a rule created with the default Id is inserted with an empty-string Id. Set `Id` explicitly, or leave it `null`, to get a generated GUID.
- **Timestamps.** `CreateAsync` sets `CreatedAt` to `DateTime.UtcNow`. `UpdateAsync` sets `UpdatedAt` to `DateTime.UtcNow`.
- **Error mapping.** Any exception raised by the database call is logged and rethrown as `DataAccessException`, with the original exception as the inner exception. The one exception is `UpdateAsync`'s Id check, which throws `ArgumentException` before any database work.
- **Storage format.** `Scope`, `Type`, and `Action` are stored as integers. `Parameters` is stored as a JSON object of string keys and values. A `NULL` `Parameters` column is read back as an empty dictionary.

## Constructor

### `DatabaseTransformationRuleRepository(IDbConnection dbConnection, ILogger<DatabaseTransformationRuleRepository> logger)`

Creates a repository that uses the given connection and logger.

- **Parameters**
  - `dbConnection` (`IDbConnection`): The connection used for all queries.
  - `logger` (`ILogger<DatabaseTransformationRuleRepository>`): Logger for failed operations.
- **Exceptions**
  - `ArgumentNullException`: `dbConnection` or `logger` is `null`.

## Methods

### `Task<IReadOnlyList<TransformationRule>> GetByApiKeyAsync(string apiKeyId, CancellationToken cancellationToken = default)`

Returns the enabled rules whose `ApiKeyId` equals `apiKeyId`, ordered by `Priority` ascending.

- **Parameters**
  - `apiKeyId` (`string`): The API key identifier to match.
  - `cancellationToken` (`CancellationToken`): Cancels the reader operations. Optional.
- **Returns:** A read-only list. The list is empty when no enabled rules match.
- **Exceptions**
  - `DataAccessException`: The query failed.

### `Task<IReadOnlyList<TransformationRule>> GetByConsumerAsync(string consumerId, CancellationToken cancellationToken = default)`

Returns the enabled rules whose `ConsumerId` equals `consumerId`, ordered by `Priority` ascending.

- **Parameters**
  - `consumerId` (`string`): The consumer identifier to match.
  - `cancellationToken` (`CancellationToken`): Cancels the reader operations. Optional.
- **Returns:** A read-only list. The list is empty when no enabled rules match.
- **Exceptions**
  - `DataAccessException`: The query failed.

### `Task<IReadOnlyList<TransformationRule>> GetGlobalRulesAsync(CancellationToken cancellationToken = default)`

Returns the enabled rules with `Scope = Global`, ordered by `Priority` ascending.

- **Parameters**
  - `cancellationToken` (`CancellationToken`): Cancels the reader operations. Optional.
- **Returns:** A read-only list. The list is empty when no enabled global rules exist.
- **Exceptions**
  - `DataAccessException`: The query failed.

### `Task<string> CreateAsync(TransformationRule rule, CancellationToken cancellationToken = default)`

Inserts a new rule. The method sets `CreatedAt` to the current UTC time and `IsEnabled` to `true` on the passed-in object. It assigns a GUID to `Id` only if `Id` is `null`.

- **Parameters**
  - `rule` (`TransformationRule`): The rule to insert. Its `Id`, `CreatedAt`, and `IsEnabled` properties are modified.
  - `cancellationToken` (`CancellationToken`): Cancels the command. Optional.
- **Returns:** The Id of the inserted rule.
- **Exceptions**
  - `DataAccessException`: The insert failed, for example because of a duplicate Id.

### `Task<bool> UpdateAsync(TransformationRule rule, CancellationToken cancellationToken = default)`

Overwrites all editable columns of the row whose Id matches `rule.Id`, including `IsEnabled`. It sets `UpdatedAt` to the current UTC time on the passed-in object. Set `IsEnabled = false` on the rule to disable it through this method.

- **Parameters**
  - `rule` (`TransformationRule`): The rule with new values. `Id` identifies the row. `UpdatedAt` is modified.
  - `cancellationToken` (`CancellationToken`): Cancels the command. Optional.
- **Returns:** `true` if a row was updated. `false` if no row has that Id.
- **Exceptions**
  - `ArgumentException`: `rule.Id` is `null` or empty.
  - `DataAccessException`: The update failed.

Note that `CreatedAt` is not written by this method, but `CreatedBy` is.

### `Task<bool> DeleteAsync(string ruleId, CancellationToken cancellationToken = default)`

Soft-deletes a rule by setting `IsEnabled = 0` and `UpdatedAt` to the current UTC time. The row is kept in the table.

- **Parameters**
  - `ruleId` (`string`): The Id of the rule to disable.
  - `cancellationToken` (`CancellationToken`): Cancels the command. Optional.
- **Returns:** `true` if a row was affected. `false` if no row has that Id.
- **Exceptions**
  - `DataAccessException`: The update failed.

## Validation

`DatabaseTransformationRuleRepositoryValidation` (same namespace) provides extension methods for checking the repository instance:

- `IReadOnlyList<string> Validate(this DatabaseTransformationRuleRepository value)`: Returns a list of problems. The list is empty when the instance is valid. It throws `ArgumentNullException` if `value` is `null`.
- `bool IsValid(this DatabaseTransformationRuleRepository value)`: Returns `true` when `Validate` finds no problems.
- `void EnsureValid(this DatabaseTransformationRuleRepository value)`: Throws `ArgumentException` with the list of problems when `Validate` finds any. It throws `ArgumentNullException` if `value` is `null`.

The checks use reflection to confirm that the `_dbConnection` and `_logger` fields are not `null`.

## Usage

The example below creates a rule, loads the enabled rules for an API key, and disables the rule. It uses `SqlServerConnection` as the `IDbConnection` implementation. Obtain the connection string from configuration and never hard-code it.

```csharp
using ApiKeyGateway.Data;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Repositories;
using Microsoft.Extensions.Logging;

// Build the dependencies. In the application, these are provided by the DI container.
ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
string connectionString = configuration.GetConnectionString("Gateway")!; // Load from configuration, not source code.

using IDbConnection dbConnection = new SqlServerConnection(
    connectionString,
    loggerFactory.CreateLogger<SqlServerConnection>());

var repository = new DatabaseTransformationRuleRepository(
    dbConnection,
    loggerFactory.CreateLogger<DatabaseTransformationRuleRepository>());

// Create a rule scoped to one API key. Id is left null so a GUID is generated.
var rule = new TransformationRule
{
    Name = "Add tenant header",
    Scope = TransformationScope.ApiKey,
    ApiKeyId = "key-123",
    Type = TransformationRuleType.BuiltIn,
    Action = BuiltInAction.AddHeader,
    Parameters = new Dictionary<string, string>
    {
        ["HeaderName"] = "X-Tenant",
        ["HeaderValue"] = "acme"
    },
    Priority = 10
};

string ruleId = await repository.CreateAsync(rule);

// Load enabled rules for the key, ordered by Priority ascending.
IReadOnlyList<TransformationRule> rules = await repository.GetByApiKeyAsync("key-123");

// Disable the rule. The row is kept; it stops appearing in Get* results.
bool disabled = await repository.DeleteAsync(ruleId);
```

Calls made from the request pipeline should use a scoped `IDbConnection`. Pass the request's `CancellationToken` into each method so that a cancelled request stops its database work.

## Related

- `ITransformationRuleRepository`: the interface this class implements.
- `TransformationRule`: the entity stored by this repository.
- `TransformationScope`, `TransformationRuleType`, `BuiltInAction`: enums used by `TransformationRule`.
- `IDbConnection` and `SqlServerConnection` (`ApiKeyGateway.Data`): the connection abstraction and its SQL Server implementation.
- `DataAccessException`: the exception type thrown on database failures.

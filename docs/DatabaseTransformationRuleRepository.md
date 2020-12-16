# DatabaseTransformationRuleRepository

A repository implementation that provides CRUD operations for transformation rules stored in a database. It is used to manage API-specific and global transformation rules that modify request or response payloads during API key gateway processing.

## API

### `DatabaseTransformationRuleRepository`

Public constructor for the repository. Initializes a new instance of the repository with the required database connection and optional transaction scope.

### `async Task<IReadOnlyList<TransformationRule>> GetByApiKeyAsync(string apiKey)`

Retrieves all transformation rules associated with the specified API key.

- **Parameters**
  - `apiKey` (string): The API key for which to retrieve transformation rules.
- **Return value**
  - A read-only list of `TransformationRule` objects, or an empty list if no rules exist for the key.
- **Exceptions**
  - Throws `ArgumentNullException` if `apiKey` is null.
  - Throws `ArgumentException` if `apiKey` is empty or whitespace.

### `async Task<IReadOnlyList<TransformationRule>> GetByConsumerAsync(string consumerId)`

Retrieves all transformation rules associated with the specified consumer.

- **Parameters**
  - `consumerId` (string): The consumer identifier for which to retrieve transformation rules.
- **Return value**
  - A read-only list of `TransformationRule` objects, or an empty list if no rules exist for the consumer.
- **Exceptions**
  - Throws `ArgumentNullException` if `consumerId` is null.
  - Throws `ArgumentException` if `consumerId` is empty or whitespace.

### `async Task<IReadOnlyList<TransformationRule>> GetGlobalRulesAsync()`

Retrieves all global transformation rules that apply to all API keys and consumers.

- **Return value**
  - A read-only list of `TransformationRule` objects, or an empty list if no global rules exist.
- **Exceptions**
  - Does not throw under normal operation.

### `async Task<string> CreateAsync(TransformationRule rule)`

Creates a new transformation rule in the database and returns the generated identifier.

- **Parameters**
  - `rule` (TransformationRule): The transformation rule to create.
- **Return value**
  - The unique identifier (string) of the newly created rule.
- **Exceptions**
  - Throws `ArgumentNullException` if `rule` is null.
  - Throws `ArgumentException` if required properties of `rule` are invalid.

### `async Task<bool> UpdateAsync(string ruleId, TransformationRule rule)`

Updates an existing transformation rule in the database.

- **Parameters**
  - `ruleId` (string): The unique identifier of the rule to update.
  - `rule` (TransformationRule): The updated transformation rule data.
- **Return value**
  - `true` if the rule was found and updated; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `ruleId` or `rule` is null.
  - Throws `ArgumentException` if `ruleId` or `rule` contains invalid values.

### `async Task<bool> DeleteAsync(string ruleId)`

Deletes a transformation rule from the database.

- **Parameters**
  - `ruleId` (string): The unique identifier of the rule to delete.
- **Return value**
  - `true` if the rule was found and deleted; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `ruleId` is null.
  - Throws `ArgumentException` if `ruleId` is empty or whitespace.

## Usage

# TransformationRule

Represents a rule that defines how an API key or consumer request is transformed within the API‑Key‑Gateway pipeline. A rule encapsulates metadata such as identifiers, scope, priority, and the actual transformation logic (built‑in action, Lua script, or parameter map).

## API

### Id
**Type:** `string`  
**Purpose:** Unique identifier for the rule, typically a GUID or database key.  
**Remarks:** Read‑write; must not be null or empty when persisted.

### Name
**Type:** `string`  
**Purpose:** Human‑readable name of the rule.  
**Remarks:** Read‑write; required for UI display and logging.

### Description
**Type:** `string?`  
**Purpose:** Optional free‑form text describing the rule’s intent or usage.  
**Remarks:** May be null.

### Scope
**Type:** `TransformationScope`  
**Purpose:** Defines the context in which the rule applies (e.g., global, per‑API‑Key, per‑Consumer).  
**Remarks:** Read‑write; determines which identifier fields are relevant.

### ApiKeyId
**Type:** `string?`  
**Purpose:** When `Scope` is `ApiKey`, holds the identifier of the API key to which the rule is bound.  
**Remarks:** Null for other scopes.

### ConsumerId
**Type:** `string?`  
**Purpose:** When `Scope` is `Consumer`, holds the identifier of the consumer to which the rule is bound.  
**Remarks:** Null for other scopes.

### Type
**Type:** `TransformationRuleType`  
**Purpose:** Indicates the kind of transformation performed (e.g., HeaderRewrite, QueryAdd, Lua).  
**Remarks:** Read‑write; drives interpretation of `Action`, `LuaScript`, and `Parameters`.

### Action
**Type:** `BuiltInAction?`  
**Purpose:** Reference to a built‑in transformation action when `Type` corresponds to a predefined operation.  
**Remarks:** Null if the rule uses a Lua script or custom parameter mapping.

### LuaScript
**Type:** `string?`  
**Purpose:** Lua script source executed when `Type` is `Lua`.  
**Remarks:** Null for non‑Lua rules; scripts are validated at runtime.

### Parameters
**Type:** `Dictionary<string, string>`  
**Purpose:** Key‑value pairs used by certain transformation types (e.g., header values, query string overrides).  
**Remarks:** Never null; initialized empty. Keys and values must not be null.

### Priority
**Type:** `int`  
**Purpose:** Relative order of execution; lower numbers run first.  
**Remarks:** Read‑write; typical range is 0‑1000 but not enforced.

### IsEnabled
**Type:** `bool`  
**Purpose:** Flag indicating whether the rule is active and should be evaluated.  
**Remarks:** Read‑write; disabled rules are ignored regardless of priority.

### CreatedAt
**Type:** `DateTime`  
**Purpose:** Timestamp when the rule was first persisted.  
**Remarks:** Set by the data access layer; treated as immutable after creation.

### UpdatedAt
**Type:** `DateTime`  
**Purpose:** Timestamp of the last modification to the rule.  
**Remarks:** Updated automatically on each save operation.

### CreatedBy
**Type:** `string?`  
**Purpose:** Identifier of the user or service that created the rule.  
**Remarks:** May be null if creation was system‑generated.

### TransformationRuleDto
**Type:** `public sealed record TransformationRuleDto`  
**Purpose:** Data‑transfer object used to serialize rule information across service boundaries or to clients.  
**Remarks:** Immutable after creation; contains only the essential fields required for rule evaluation.

### From
**Type:** `public static TransformationRuleDto From(TransformationRule source)`  
**Purpose:** Creates a `TransformationRuleDto` instance from a `TransformationRule` entity.  
**Parameters:**  
- `source`: The rule to convert; must not be null.  
**Return Value:** A new `TransformationRuleDto` populated with `source`’s `Name`, `Description`, and `Scope`.  
**Exceptions:**  
- `ArgumentNullException` if `source` is null.

### Name (Dto)
**Type:** `public required string Name`  
**Purpose:** The rule’s name as transferred in the DTO.  
**Remarks:** Required property; must be supplied when initializing the DTO.

### Description (Dto)
**Type:** `public string? Description`  
**Purpose:** Optional description transferred in the DTO.  
**Remarks:** May be null.

### Scope (Dto)
**Type:** `public required TransformationScope Scope`  
**Purpose:** The rule’s scope as transferred in the DTO.  
**Remarks:** Required property; determines how the DTO is interpreted by consumers.

## Usage

### Creating a rule and exporting a DTO
```csharp
using ApiKeyGateway.Models;

var rule = new TransformationRule
{
    Id = Guid.NewGuid().ToString(),
    Name = "Add API version header",
    Description = "Injects X-API-Version header with value 2.0",
    Scope = TransformationScope.Global,
    Type = TransformationRuleType.HeaderRewrite,
    Action = BuiltInAction.SetHeader,
    Parameters = new Dictionary<string, string>
    {
        ["X-API-Version"] = "2.0"
    },
    Priority = 10,
    IsEnabled = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    CreatedBy = "admin"
};

var dto = TransformationRule.From(rule);
// dto can now be sent over HTTP or stored in a cache.
```

### Building a rule from a DTO (client‑side)
```csharp
using ApiKeyGateway.Models;

// Assume dto was received from an API endpoint.
TransformationRuleDto dto = GetDtoFromService();

var rule = new TransformationRule
{
    Id = Guid.NewGuid().ToString(),
    Name = dto.Name,
    Description = dto.Description,
    Scope = dto.Scope,
    // Additional fields must be supplied based on the rule type.
    Type = TransformationRuleType.Lua,
    LuaScript = "return kong.service.request.set_header('X-Custom', 'value')",
    Parameters = new Dictionary<string, string>(),
    Priority = 5,
    IsEnabled = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    CreatedBy = "integration"
};
```

## Notes

- The `TransformationRule` class is mutable; concurrent modifications to the same instance from multiple threads are not thread‑safe. External synchronization is required if the instance is shared.
- The `TransformationRuleDto` record is immutable after initialization; therefore it can be safely published to multiple threads without additional locking.
- When `Scope` is set to `ApiKey` or `Consumer`, the corresponding `ApiKeyId` or `ConsumerId` property must be populated; otherwise validation layers may reject the rule.
- The `Parameters` dictionary should never be replaced with null; doing so will break internal logic that expects an empty dictionary when no parameters are needed.
- `CreatedAt` and `UpdatedAt` are managed by the persistence layer; manually setting them may lead to inconsistent audit trails.
- The `From` method only copies the three required DTO fields; additional rule properties are not transferred. Consumers needing a full clone must map the remaining properties manually.

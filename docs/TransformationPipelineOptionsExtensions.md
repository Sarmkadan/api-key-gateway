# TransformationPipelineOptionsExtensions

Extension methods for `TransformationPipelineOptions` that validate the options and manage the static rule list (`StaticRules`) held in memory. They live in the `ApiKeyGateway.Configuration` namespace, next to the options type they extend.

## API

### `Validate`

Checks the numeric limits of a `TransformationPipelineOptions` instance and throws if any of them is out of range. Validation stops at the first failing check.

- **Signature**: `void Validate(this TransformationPipelineOptions options)`
- **Parameters**:
  - `options`: The options instance to validate.
- **Checks** (in order):
  - `MaxRulesPerRequest` must be at least `1`.
  - `MaxBodySizeBytes` must be non-negative.
  - `RuleCacheTtl` must be non-negative. `TimeSpan.Zero` is allowed and disables the cache.
  - `Lua` must not be `null`.
  - `Lua.MaxExecutionMs` must be at least `1`.
  - `Lua.MaxScriptSizeBytes` must be non-negative.
- **Not checked**: `IsEnabled`, `StopOnError`, `EnableBodyCapture`, `StaticRules`, `Lua.IsEnabled`, `Lua.MaxMemoryBytes`, and the quarantine threshold. Use `TransformationPipelineOptionsValidation` if you need those covered.
- **Throws**:
  - `ArgumentNullException`: when `options` is `null`.
  - `InvalidOperationException`: when a check fails. The message names the property and its value.

> **Note**: `TransformationPipelineOptionsValidation` (same namespace) also defines a `Validate` extension on this type, with a different return type (`IReadOnlyList<string>`). Because both are in scope, a call written as `options.Validate()` in a file that imports both is ambiguous and will not compile. Call one of them through its class name (for example, `TransformationPipelineOptionsExtensions.Validate(options)`) or pick one.

### `AddStaticRule`

Appends a `TransformationRule` to `options.StaticRules`. The method does not check for duplicate `Id` values, so calling it twice with the same ID adds two entries.

- **Signature**: `void AddStaticRule(this TransformationPipelineOptions options, TransformationRule rule)`
- **Parameters**:
  - `options`: The options instance to modify.
  - `rule`: The rule to append.
- **Returns**: Nothing.
- **Throws**: `ArgumentNullException` when `options` or `rule` is `null`.
- **Side effects**: Mutates `options.StaticRules` in place.

### `RemoveStaticRule`

Removes every rule in `options.StaticRules` whose `Id` equals `ruleId`. The comparison is an ordinal, case-sensitive string match.

- **Signature**: `bool RemoveStaticRule(this TransformationPipelineOptions options, string ruleId)`
- **Parameters**:
  - `options`: The options instance to modify.
  - `ruleId`: The `TransformationRule.Id` of the rule(s) to remove.
- **Returns**: `true` if at least one rule was removed; otherwise `false`.
- **Throws**:
  - `ArgumentNullException`: when `options` is `null`, or when `ruleId` is `null`.
  - `ArgumentException`: when `ruleId` is empty or whitespace.
- **Side effects**: Mutates `options.StaticRules` in place. If duplicates share the ID, all of them are removed.

## Example

```csharp
var options = new TransformationPipelineOptions();

options.AddStaticRule(new TransformationRule { Id = "strip-debug-header" });

if (!options.RemoveStaticRule("strip-debug-header"))
{
    // No rule with that ID was present.
}

TransformationPipelineOptionsExtensions.Validate(options); // throws InvalidOperationException on bad limits
```

The `TransformationRule` shown above is a sketch: real rules also need their type, scope, and payload set. See [TransformationRule](TransformationRule.md).

## See also

- [TransformationPipelineOptions](TransformationPipelineOptions.md)
- [TransformationOptions](TransformationOptions.md)
- [TransformationRule](TransformationRule.md)

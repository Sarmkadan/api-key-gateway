# ITransformationPipeline

`ITransformationPipeline` is the contract for the request transformation stage of the gateway. Given a `TransformationContext` wrapping an in-flight HTTP request, the pipeline loads the matching `TransformationRule` entries, runs them in ascending priority order, and reports what happened in a `TransformationResult`. It also exposes two helpers: a cache warm-up call and a static Lua script validator.

Source: `src/ApiKeyGateway/Transformation/ITransformationPipeline.cs`. The only implementation is `TransformationPipelineService` (`src/ApiKeyGateway/Transformation/TransformationPipelineService.cs`).

## Interface

```csharp
public interface ITransformationPipeline
{
    Task<TransformationResult> ApplyAsync(TransformationContext context, CancellationToken cancellationToken = default);
    Task WarmAsync(string apiKeyId, CancellationToken cancellationToken = default);
    ScriptValidationResult ValidateLuaScript(string luaScript);
}
```

### `Task<TransformationResult> ApplyAsync(TransformationContext context, CancellationToken cancellationToken = default)`

Runs every enabled rule that applies to the request and mutates `context` in place.

- **Rule selection:** rules come from three scopes: global rules, rules for the context's `ConsumerId` (when not null), and rules for the context's `ApiKeyId` (when not null). The sets are merged, duplicates by `Id` are removed, disabled rules are dropped, and the result is sorted by `Priority` ascending (lower runs first).
- **Rule cap:** at most `TransformationPipelineOptions.MaxRulesPerRequest` rules (default `50`) are kept, taken after sorting.
- **Sequencing:** each rule sees the mutations made by earlier rules. Rules are not run in parallel.
- **Blocking:** when any rule sets `context.IsBlocked`, no further rules run. The result reports `IsBlocked = true` and `BlockReason = context.BlockReason`.
- **Errors:** an exception thrown by a rule is recorded in `Errors`, keyed by rule `Id`. The pipeline continues with the next rule unless `TransformationPipelineOptions.StopOnError` is `true`. An `OperationCanceledException` from a rule is recorded as `"Execution timed out"`, and the same `StopOnError` rule applies.
- **Cancellation:** when `cancellationToken` is cancelled, the loop stops before the next rule. Rules already run are kept.
- **Disabled pipeline:** when `TransformationPipelineOptions.IsEnabled` is `false`, no rules are loaded and the method returns a skipped result (`Success = true`, all counts zero, `Elapsed = TimeSpan.Zero`).

The method never mutates the live `HttpRequest`. The caller is responsible for writing `context` back onto the request. `RequestTransformationMiddleware` does this (see [Callers](#callers)).

### `Task WarmAsync(string apiKeyId, CancellationToken cancellationToken = default)`

Loads the per-key rules for `apiKeyId` and stores them in the implementation's rule cache, so the first request for that key does not wait on the repository.

- Cache entries live for `TransformationPipelineOptions.RuleCacheTtl` (default 60 seconds). A zero TTL disables caching, and `WarmAsync` then has no lasting effect.
- Only the per-key rules are cached. Global and consumer rules are loaded on every `ApplyAsync` call.
- The XML comment on the interface says to call this from the authentication middleware after key validation. Nothing in `src/` makes that call at the time of writing. See [Notes](#notes).

### `ScriptValidationResult ValidateLuaScript(string luaScript)`

Checks a Lua script for syntax and security problems without running it. It delegates to `ILuaScriptExecutor.Validate`. The intended use is an admin endpoint that checks a script before the rule is saved. Like `WarmAsync`, no caller in `src/` uses it yet.

- Returns `ScriptValidationResult.IsValid` and a list of `Errors` (empty when valid). An empty or whitespace script is reported as invalid with the error `"Script is empty."`.
- Checks include the maximum script size, a scan for disallowed globals, and any extra disallowed globals from configuration.

## Related types

These types are defined in the same file as the interface or in `Domain/Models/TransformationRule.cs`.

### `TransformationContext`

The mutable object passed through the pipeline. Built from an `HttpRequest` plus the identity resolved by authentication:

```csharp
new TransformationContext(request, apiKeyId, consumerId);
```

| Member | Writable | Meaning |
|---|---|---|
| `ApiKeyId`, `ConsumerId` | No | Identity from authentication; `null` for anonymous requests or unlinked keys. |
| `Method`, `Path` | Yes | Rewritten by scripts or built-in actions. `Path` is initialised from `Request.Path`. |
| `QueryParameters` | Yes (dictionary) | Query-string values. |
| `Headers` | Yes (dictionary) | Request headers. `Authorization`, `X-API-Key` and `Cookie` are excluded so rules cannot read or overwrite them. |
| `Body` | Yes | Raw UTF-8 body. `null` unless body capture is enabled. |
| `SourceIp` | No | `X-Forwarded-For` (first entry) if present, otherwise the connection's remote address, otherwise `"unknown"`. |
| `Properties` | Yes (dictionary) | Free-form values passed between rules or into the audit log. |
| `IsBlocked`, `BlockReason` | Yes | Set by a rule to stop the pipeline. |
| `RuleId`, `RuleName` | Yes | Set by the pipeline to identify the currently executing rule. |

### `TransformationResult`

Immutable summary of one `ApplyAsync` call.

| Member | Meaning |
|---|---|
| `Success` | `true` when no rule raised an error. |
| `IsBlocked`, `BlockReason` | Copied from the context after the run. |
| `RulesEvaluated` | Rules that were started. |
| `RulesApplied` | Rules whose handler reported that it changed something. |
| `Elapsed` | Wall-clock time for the whole call. |
| `Errors` | Rule `Id` to error message. Empty when `Success` is `true`. |

### `ScriptValidationResult`

| Member | Meaning |
|---|---|
| `IsValid` | `true` when the script passed checks. |
| `Errors` | Validation messages. Empty when valid. |

### `TransformationRule`

| Property | Meaning |
|---|---|
| `Id`, `Name`, `Description` | Identity and documentation. `Id` is the key for de-duplication and error reporting. |
| `Scope` | `TransformationScope.Global`, `ApiKey` or `Consumer`. Determines which lookup returns the rule. |
| `ApiKeyId`, `ConsumerId` | Target for `ApiKey` and `Consumer` scopes. |
| `Type` | `TransformationRuleType.BuiltIn` or `LuaScript`. |
| `Action` | `BuiltInAction` for built-in rules (see below). |
| `Parameters` | Arguments for the built-in action. |
| `LuaScript` | Source for Lua rules. Rules with a blank script are not executed. |
| `Priority` | Lower runs first. Default `100`. |
| `IsEnabled` | Disabled rules are dropped before execution. |

`BuiltInAction` values and their parameters:

| Action | Parameters | Effect |
|---|---|---|
| `AddHeader` | `HeaderName`, `HeaderValue` | Adds or overwrites a header. |
| `RemoveHeader` | `HeaderName` | Removes a header from `context.Headers`. |
| `SetQueryParam` | `ParamName`, `ParamValue` | Adds or overwrites a query parameter. |
| `RemoveQueryParam` | `ParamName` | Removes a query parameter. |
| `RewritePath` | `PathTemplate` | Replaces `{path}` in the template with the current path. |
| `InjectConsumerId` | none | Sets `X-Consumer-Id` when a consumer is known. |
| `InjectApiKeyId` | none | Sets `X-Api-Key-Id` when a key is known. |

A built-in rule whose parameters are missing, or whose action is unknown, is not applied and is counted as skipped.

Lua rules are run by `ILuaScriptExecutor.ExecuteAsync`. A script that returns the string `"BLOCK"` (case-insensitive) sets `IsBlocked` and is not counted as applied. A script that fails at runtime rethrows, and the pipeline records the error for that rule.

## Implementation: `TransformationPipelineService`

`TransformationPipelineService` is `sealed` and registered as a singleton. It takes:

- `ITransformationRuleRepository`: rule source. The default is `ConfigurationTransformationRuleRepository`, which reads `Transformation:StaticRules` from configuration. `DatabaseTransformationRuleRepository` is the database-backed alternative.
- `ILuaScriptExecutor`: runs and validates Lua rules.
- `TransformationPipelineOptions`: `IsEnabled`, `MaxRulesPerRequest`, `StopOnError`, `EnableBodyCapture`, `MaxBodySizeBytes`, `RuleCacheTtl`, `StaticRules`, and nested `Lua` options.
- `ILogger<TransformationPipelineService>`.

It keeps a per-key rule cache (`ConcurrentDictionary`) that holds the per-key rule list until the TTL expires. Expired entries are overwritten on the next lookup; there is no background eviction.

Registration is in `AddRequestTransformationPipeline` (`src/ApiKeyGateway/Configuration/TransformationOptions.cs`):

```csharp
services.AddSingleton<ITransformationRuleRepository, ConfigurationTransformationRuleRepository>();
services.AddSingleton<ILuaScriptExecutor, LuaScriptExecutor>();
services.AddSingleton<ITransformationPipeline, TransformationPipelineService>();
```

To use database-backed rules, register a different `ITransformationRuleRepository` in its place. `DatabaseTransformationRuleRepository` depends on a scoped `IDbConnection`, so the registration needs a scope-aware approach (see the remarks in `TransformationOptions.cs`).

## Callers

`RequestTransformationMiddleware` (`src/ApiKeyGateway/Middleware/RequestTransformationMiddleware.cs`) is the only caller of `ApplyAsync`. It is added with `app.UseRequestTransformation()` in `Program.cs`, after authentication.

For each request it:

1. Reads `ApiKey` and `ConsumerId` from `HttpContext.Items`, which authentication sets.
2. Builds a `TransformationContext`. The body is read first when `EnableBodyCapture` is on.
3. Calls `ApplyAsync`.
4. If `IsBlocked`, writes an RFC 7807 problem response with `BlockReason` and stops. Downstream middleware does not run.
5. Otherwise writes the context back to the request: headers, query string (rebuilt from `QueryParameters`), path (if changed), and body (if `Body` is set).

## Usage

```csharp
public async Task<bool> TransformAsync(HttpContext httpContext, ITransformationPipeline pipeline)
{
    var apiKey = httpContext.Items["ApiKey"] as ApiKey;
    var context = new TransformationContext(httpContext.Request, apiKey?.Id, consumerId: null);

    var result = await pipeline.ApplyAsync(context, httpContext.RequestAborted);

    if (!result.Success)
    {
        // Some rules failed. The rest still ran unless StopOnError is set.
    }

    return !result.IsBlocked;
}
```

Validating a script before saving a rule:

```csharp
var validation = pipeline.ValidateLuaScript(script);
if (!validation.IsValid)
{
    return Results.BadRequest(validation.Errors);
}
```

## Notes

- **Unused helpers.** As of this writing, `WarmAsync` and `ValidateLuaScript` have no callers in `src/`. The interface comments describe intended use (warm-up from the authentication middleware; validation in an admin endpoint). Wiring them in would be a separate change.
- **Header removal does not reach the request.** `RemoveHeader` changes `context.Headers`, but the middleware only writes entries back and never deletes headers from the live request. A removed header therefore stays on the request. Query-parameter removal does take effect, because the query string is rebuilt from the dictionary.
- **Sensitive headers are invisible.** `Authorization`, `X-API-Key` and `Cookie` are not in `context.Headers`, so no rule can read or change them.
- **Ordering across scopes.** Global, consumer and key rules are merged and sorted together by `Priority`. Equal priorities have no guaranteed order.
- **Quarantined Lua scripts.** A script quarantined after repeated failures is skipped without running, but `ExecuteAsync` returns `true`. The pipeline counts it as applied and the request proceeds.
- **Cap before execution.** The `MaxRulesPerRequest` cap is applied after sorting, so the lowest-priority-value rules are kept and higher-numbered ones are dropped when the cap is hit.

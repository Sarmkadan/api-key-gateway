# TransformationPipelineService

`TransformationPipelineService` is the default implementation of `ITransformationPipeline`. For each incoming request it loads the matching `TransformationRule` entries (global, consumer-scoped and API-key-scoped), merges and orders them by priority, and applies them in sequence to a mutable `TransformationContext`. Rules are either built-in actions (header, query, and path mutations) or Lua scripts run by `ILuaScriptExecutor`.

The class is `sealed` and lives in the `ApiKeyGateway.Transformation` namespace. Its file is `src/ApiKeyGateway/Transformation/TransformationPipelineService.cs`.

## API

### Constructor

```csharp
public TransformationPipelineService(
    ITransformationRuleRepository repository,
    ILuaScriptExecutor luaExecutor,
    TransformationPipelineOptions options,
    ILogger<TransformationPipelineService> logger)
```

- **repository** (`ITransformationRuleRepository`): source of rules. Supplies global, consumer, and API-key rule sets.
- **luaExecutor** (`ILuaScriptExecutor`): executes and validates Lua rules.
- **options** (`TransformationPipelineOptions`): pipeline configuration (see [Options](#options)).
- **logger** (`ILogger<TransformationPipelineService>`): receives debug, information, warning, and error logs.

Throws `ArgumentNullException` if any argument is `null`.

### Methods

#### `ApplyAsync`

```csharp
public Task<TransformationResult> ApplyAsync(
    TransformationContext context,
    CancellationToken cancellationToken = default)
```

Applies the pipeline to `context` and returns a `TransformationResult`.

Behaviour, in order:

1. If `TransformationPipelineOptions.IsEnabled` is `false`, returns a skipped result immediately (`Success = true`, `RulesEvaluated = 0`, `RulesApplied = 0`) and does not touch the repository.
2. Resolves rules via `ResolveRulesAsync` (see [Rule resolution](#rule-resolution)).
3. Iterates the rules in ascending `Priority` order. Each rule sees the mutations made by earlier rules. Iteration stops early when `context.IsBlocked` becomes `true` or `cancellationToken` is cancelled.
4. Each rule runs through `ApplyRuleAsync`:
   - `BuiltIn` rules run the matching `BuiltInAction`. A rule that is missing a required parameter, or whose action needs context that is absent (for example `InjectConsumerId` with no consumer), is reported as not applied.
   - `LuaScript` rules run only when `LuaScript` is non-blank. Otherwise they are reported as not applied.
   - Any other type is reported as not applied.
5. A failing rule is recorded in `Errors` under its rule ID. If `StopOnError` is `true`, processing stops at that rule; otherwise the next rule runs.
6. Logs a summary at `Information` level and returns the result.

`ApplyAsync` does not write anything back to `HttpContext`. The caller reads the mutations from `context` after it returns. See [Usage](#usage).

Exceptions:
- An `OperationCanceledException` raised by a single rule is logged as a timeout and recorded as `"Execution timed out"`. It does not propagate.
- Other exceptions from a rule are logged and recorded as the exception message. They do not propagate.
- Exceptions thrown by the repository while loading rules are **not** caught and propagate to the caller.

#### `WarmAsync`

```csharp
public Task WarmAsync(string apiKeyId, CancellationToken cancellationToken = default)
```

Loads the API-key-scoped rules for `apiKeyId` into the in-memory cache, so the first request for that key does not wait on the repository. It does not return the rules. While `RuleCacheTtl` is greater than zero, calling it again before the entry expires does not reload from the repository.

#### `ValidateLuaScript`

```csharp
public ScriptValidationResult ValidateLuaScript(string luaScript)
```

Delegates to `ILuaScriptExecutor.Validate`. It runs static syntax and security checks without executing the script. It is safe to call from an admin endpoint before a rule is saved.

## Rule resolution

Rules come from three sources, loaded concurrently:

| Source | Repository call | When loaded |
|---|---|---|
| Global | `GetGlobalRulesAsync` | Always |
| Consumer | `GetByConsumerAsync(consumerId)` | When `TransformationContext.ConsumerId` is not `null` |
| API key | `LoadKeyRulesAsync(apiKeyId)`, which uses the cache | When `TransformationContext.ApiKeyId` is not `null` |

The combined list is then:

1. Filtered to `IsEnabled == true`.
2. De-duplicated by `Id` (`DistinctBy`), so a rule returned by more than one source runs once.
3. Sorted by `Priority` ascending. Lower values run first.
4. Truncated to `MaxRulesPerRequest`. Truncation happens after sorting, so the highest-priority-value rules are dropped first.

### Rule cache

API-key rules are cached in a `ConcurrentDictionary` keyed by `apiKeyId`. Each entry holds the rule list and an expiry time of `UtcNow + RuleCacheTtl`.

- When `RuleCacheTtl` is greater than zero, a cache hit before expiry returns the cached list. A miss or expired entry calls the repository and stores the result.
- When `RuleCacheTtl` is zero or negative, the cache is bypassed and every call goes to the repository.
- Expired entries are not removed. They are overwritten the next time that key is loaded. The cache therefore holds at most one entry per distinct API key seen.
- Global and consumer rules are not cached by this class.

## Built-in actions

| `BuiltInAction` | Required parameters | Effect |
|---|---|---|
| `AddHeader` | `HeaderName`, `HeaderValue` | Sets `Headers[HeaderName]`, overwriting any existing value |
| `RemoveHeader` | `HeaderName` | Removes the header if present |
| `SetQueryParam` | `ParamName`, `ParamValue` | Sets `QueryParameters[ParamName]` |
| `RemoveQueryParam` | `ParamName` | Removes the query parameter if present |
| `RewritePath` | `PathTemplate` | Replaces every `{path}` in the template with the current `Path` (case-insensitive match) and assigns the result to `Path` |
| `InjectConsumerId` | none | Sets `X-Consumer-Id` to `ConsumerId`. Not applied when `ConsumerId` is `null` |
| `InjectApiKeyId` | none | Sets `X-Api-Key-Id` to `ApiKeyId`. Not applied when `ApiKeyId` is `null` |

A rule whose `Action` is `null` or unrecognised, or whose parameters are missing, is reported as not applied.

Header and query-parameter names are not normalised. `TransformationContext.Headers` and `QueryParameters` are plain dictionaries built with the default string comparer, so keys are matched case-sensitively. A rule that adds `x-trace-id` does not replace an existing `X-Trace-Id`. Use the exact casing the downstream expects.

## Options

The pipeline reads these `TransformationPipelineOptions` properties. The defaults shown are the values in the current code.

| Property | Default | Used by this service |
|---|---|---|
| `IsEnabled` | `true` | Short-circuits `ApplyAsync` when `false` |
| `MaxRulesPerRequest` | `50` | Caps the resolved rule list |
| `StopOnError` | `false` | Stops execution after the first failing rule |
| `RuleCacheTtl` | `60 seconds` | Lifetime of API-key rule cache entries. Zero disables caching |

The remaining properties (`EnableBodyCapture`, `MaxBodySizeBytes`, `StaticRules`, `Lua`) are used by other components and are not read by this class.

## Usage

### Registration and injection

`AddRequestTransformationPipeline` registers this service as a singleton `ITransformationPipeline`, together with `ILuaScriptExecutor`, the default `ITransformationRuleRepository`, and the options. It binds `TransformationPipelineOptions` from the `Transformation` configuration section.

```csharp
// Program.cs
builder.Services.AddRequestTransformationPipeline(builder.Configuration);
```

`RequestTransformationMiddleware` resolves `ITransformationPipeline` and calls it on every request. To use the pipeline directly, inject the interface:

```csharp
using ApiKeyGateway.Transformation;

public sealed class RequestRewriter
{
    private readonly ITransformationPipeline _pipeline;

    public RequestRewriter(ITransformationPipeline pipeline)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
    }

    public async Task<TransformationResult> RewriteAsync(
        HttpRequest request,
        string? apiKeyId,
        string? consumerId,
        CancellationToken cancellationToken)
    {
        var context = new TransformationContext(request, apiKeyId, consumerId);

        var result = await _pipeline.ApplyAsync(context, cancellationToken);

        if (result.IsBlocked)
        {
            // context.BlockReason explains the block; the caller decides the response.
            return result;
        }

        // Write the mutations back to the request before the next stage.
        foreach (var (name, value) in context.Headers)
        {
            request.Headers[name] = value;
        }

        request.Path = context.Path;
        request.QueryString = QueryString.Create(context.QueryParameters);

        return result;
    }
}
```

### Warming the cache

Call `WarmAsync` from authentication code once the key is validated:

```csharp
await _pipeline.WarmAsync(validatedKey.Id, cancellationToken);
```

### Validating a script before saving

```csharp
var validation = _pipeline.ValidateLuaScript(dto.LuaScript);

if (!validation.IsValid)
{
    return BadRequest(validation.Errors);
}
```

### Constructing the service directly

When not using the DI extension, supply the dependencies yourself. The repository and Lua executor are shown as placeholders for your own implementations:

```csharp
var options = new TransformationPipelineOptions
{
    MaxRulesPerRequest = 20,
    StopOnError = true,
    RuleCacheTtl = TimeSpan.FromMinutes(5)
};

var pipeline = new TransformationPipelineService(
    repository,      // your ITransformationRuleRepository
    luaExecutor,     // your ILuaScriptExecutor
    options,
    logger);         // ILogger<TransformationPipelineService>
```

## Related types

- `ITransformationPipeline`: the interface this class implements.
- `ITransformationRuleRepository`: rule source (`DatabaseTransformationRuleRepository` for SQL Server, or the configuration-backed default).
- `ILuaScriptExecutor` / `LuaScriptExecutor`: executes and validates Lua rules.
- `TransformationContext`: mutable request snapshot that rules modify.
- `TransformationResult`: summary returned by `ApplyAsync`.
- `TransformationRule` / `BuiltInAction` / `TransformationRuleType` / `TransformationScope`: rule definition types in `Domain.Models`.
- `TransformationPipelineOptions`: configuration for the pipeline.
- `RequestTransformationMiddleware`: the HTTP middleware that calls `ApplyAsync` and writes results back to the request.

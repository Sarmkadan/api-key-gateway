# LuaScriptExecutor

`LuaScriptExecutor` runs tenant-supplied Lua scripts inside the request transformation pipeline. It is the MoonSharp-backed implementation of `ILuaScriptExecutor` and lives in `src/ApiKeyGateway/Transformation/LuaScriptExecutor.cs`.

Each invocation gets a fresh, sandboxed MoonSharp `Script` with a `request` global built from the current `TransformationContext`. Changes the script makes to headers, query parameters, `path`, `method`, and `block_reason` are copied back to the context after the script returns. Scripts can also block the request by returning the string `"BLOCK"`.

The executor is registered as a singleton (`ILuaScriptExecutor` → `LuaScriptExecutor`) by the transformation service registration, and it is consumed by `TransformationPipelineService`.

## Sandbox

- Only the `Basic`, `String`, `Table`, `Math`, and `Bit32` core module sets are loaded. OS, IO, debug, and package facilities are not available.
- The following globals are always removed: `dofile`, `loadfile`, `load`, `require`, `collectgarbage`, `rawset`, `rawget`, `setmetatable`.
- Any names in `LuaExecutionOptions.DisallowedGlobals` are removed as well.
- The script loader is replaced with one that rejects every file load.
- A new `Script` is created per call, so no globals or metatables survive between requests or API keys.

## The `request` table

| Field | Type | Writable by script | Written back to context |
|---|---|---|---|
| `request.headers` | table (string → string) | yes | yes (string entries only) |
| `request.query` | table (string → string) | yes | yes (string entries only) |
| `request.path` | string | yes | yes, if still a string |
| `request.method` | string | yes | yes, if still a string |
| `request.block_reason` | string | yes | yes, copied to `BlockReason` |
| `request.body` | string or nil | yes | no |
| `request.ip` | string | yes | no |
| `request.consumer_id` | string or nil | yes | no |
| `request.api_key_id` | string or nil | yes | no |

Notes:

- Header and query keys are passed through exactly as stored in the context.
- Sensitive headers (`Authorization`, `X-API-Key`, `Cookie`) are never placed in `request.headers`, because `TransformationContext` filters them out.
- Only `headers`, `query`, `path`, `method`, and `block_reason` are synced back. Changes to `body` and the other scalar fields are not written back.

## Public API

### `ILuaScriptExecutor`

The interface implemented by `LuaScriptExecutor`. Code that consumes the executor should depend on this interface.

### `LuaScriptExecutor` constructor

```csharp
public LuaScriptExecutor(
    LuaExecutionOptions options,
    ILogger<LuaScriptExecutor> logger,
    IEventPublisher eventPublisher)
```

- **Parameters**:
  - `options` (`LuaExecutionOptions`): Execution limits, quarantine settings, and disallowed globals.
  - `logger` (`ILogger<LuaScriptExecutor>`): Structured logger for quarantine, block, and validation messages.
  - `eventPublisher` (`IEventPublisher`): Receives `LuaScriptFailedEvent` and `LuaScriptQuarantinedEvent`.
- **Exceptions**: `ArgumentNullException` if any argument is `null`.

### `ExecuteAsync`

```csharp
public async Task<bool> ExecuteAsync(
    string script,
    TransformationContext context,
    CancellationToken cancellationToken = default)
```

Runs the script against `context` on a worker thread, with a wall-clock limit of `LuaExecutionOptions.MaxExecutionMs`.

- **Parameters**:
  - `script` (`string`): Lua source code. Must not be null, empty, or whitespace.
  - `context` (`TransformationContext`): The request state exposed to the script. Mutations are written back to it.
  - `cancellationToken` (`CancellationToken`): Caller cancellation. Combined with the timeout through a linked token.
- **Return value**: `Task<bool>`
  - `true`: the request should continue. This includes the case where the script is quarantined and was skipped.
  - `false`: the script returned `"BLOCK"` (case-insensitive). `context.IsBlocked` is set to `true`.
- **Exceptions**:
  - `ArgumentNullException` when `script` or `context` is `null`.
  - `ArgumentException` when `script` is empty or whitespace.
  - `TransformationScriptException` when the script is larger than `MaxScriptSizeBytes` (UTF-8 bytes), has a syntax error, raises a runtime error, or exceeds `MaxMemoryBytes`.
  - `OperationCanceledException` when the script exceeds `MaxExecutionMs`. This is not a `TransformationScriptException`, so callers that catch script errors must handle it separately.

Execution order:

1. If the `ApiKeyId`/`ConsumerId` pair is quarantined, publish `LuaScriptQuarantinedEvent` and return `true` without running the script.
2. Reject scripts over `MaxScriptSizeBytes`.
3. Run the script on a worker thread with the timeout and the instruction-level guard attached.
4. On success, check the return value for `"BLOCK"`.

### `Validate`

```csharp
public ScriptValidationResult Validate(string script)
```

Checks a script without executing it. Intended for admin endpoints that validate a rule before it is saved.

- **Parameters**:
  - `script` (`string`): Lua source code to check.
- **Return value**: `ScriptValidationResult` with `IsValid` and `Errors`. `Errors` contains one entry for each problem found:
  - `Script is empty.` for null, empty, or whitespace input (returns early).
  - `Script exceeds maximum size of N bytes.`
  - `Use of disallowed global 'NAME' detected.` for each built-in or configured blocked name found in the source.
  - `Syntax error: ...` when the script does not parse.
  - `Validation error: ...` for any other unexpected failure while parsing.
- **Exceptions**: None. Errors are returned in the result.

Note that the global check is a plain substring match, so a script that contains `download` is reported because it contains `load`. Treat it as a conservative pre-check, not a parser.

## Limits and options

All limits come from `LuaExecutionOptions` (bound from configuration).

| Option | Default | Effect |
|---|---|---|
| `IsEnabled` | `true` | Read by `TransformationPipelineService`. When `false`, Lua rules are skipped before the executor is called. |
| `MaxExecutionMs` | `50` | Wall-clock timeout per invocation. Enforced between Lua instructions, so tight loops are stopped. |
| `MaxScriptSizeBytes` | `65536` | Maximum UTF-8 size of the script source. `0` disables the check. |
| `MaxMemoryBytes` | `67108864` | Managed bytes allocated on the worker thread during the run. `0` disables the check. |
| `QuarantineThreshold` | `5` | Failures before a key/consumer pair is quarantined. `0` disables quarantine. |
| `QuarantineDurationSeconds` | `300` | How long a quarantine lasts. |
| `DisallowedGlobals` | empty | Extra global names removed from the sandbox and rejected by `Validate`. |

### Failure handling and quarantine

Failures are counted per `"{ApiKeyId}:{ConsumerId}"` key. Each of the following counts as one failure:

- timeout
- runtime error
- syntax error
- memory limit exceeded
- any other unexpected exception

Successful runs do not reset the count. When the count reaches `QuarantineThreshold`, the key is quarantined until `QuarantineDurationSeconds` has passed. While quarantined, the script is skipped and the request continues. When the quarantine expires, the entry is removed and the count starts again from zero.

Scripts that return `"BLOCK"` are not failures.

## Events

Failures and quarantine skips are published through `IEventPublisher`. Publishing errors are logged and do not affect the request.

| Event | When | Key fields |
|---|---|---|
| `LuaScriptFailedEvent` | A script fails | `FailureType` and `ErrorType` (same value): `timeout`, `runtime_error`, `syntax_error`, `out_of_memory`, or `unexpected_error`; `ErrorMessage`; `ExecutionTimeMs`; `MemoryUsedBytes` |
| `LuaScriptQuarantinedEvent` | A quarantined script is skipped | `ConsecutiveFailures`, `QuarantineThreshold`, `ErrorType` (`quarantined`), `ErrorMessage` |

Both events carry `ScriptId` and `ScriptName` from `context.RuleId` and `context.RuleName` (or `"unknown"` when they are not set), plus `ApiKeyId` and `ConsumerId`.

## Usage

### Example 1: Validate a rule, then execute it in the pipeline

The admin API validates a rule before saving it. The pipeline calls `ExecuteAsync` for each request.

```csharp
using ApiKeyGateway.Transformation;

public sealed class LuaRuleService
{
    private readonly ILuaScriptExecutor _executor;

    public LuaRuleService(ILuaScriptExecutor executor)
    {
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    public IReadOnlyList<string> ValidateRule(string script)
    {
        var result = _executor.Validate(script);
        return result.Errors;
    }
}
```

Example script for a rule that blocks requests without a tenant header and rewrites the path:

```lua
if request.headers["x-tenant"] == nil then
    request.block_reason = "missing x-tenant header"
    return "BLOCK"
end

request.headers["x-routed-by"] = "gateway"
request.path = "/v2" .. request.path
return "OK"
```

Running it for a request:

```csharp
var context = new TransformationContext(httpRequest, apiKeyId, consumerId)
{
    RuleId = "rule-tenant-guard",
    RuleName = "Tenant guard"
};

try
{
    var proceed = await _executor.ExecuteAsync(script, context, httpContext.RequestAborted);

    if (!proceed)
    {
        // context.IsBlocked is true; context.BlockReason holds the script's reason.
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }

    // Mutated headers and path are now on the context.
}
catch (TransformationScriptException ex)
{
    // Syntax, runtime, size, or memory failure. The failure was already published as an event.
    _logger.LogWarning(ex, "Lua rule {RuleId} failed", context.RuleId);
}
catch (OperationCanceledException) when (!httpContext.RequestAborted.IsCancellationRequested)
{
    // The script ran past MaxExecutionMs.
}
```

### Example 2: Registering options

```csharp
services.Configure<LuaExecutionOptions>(options =>
{
    options.MaxExecutionMs = 25;
    options.MaxScriptSizeBytes = 16_384;
    options.QuarantineThreshold = 3;
    options.QuarantineDurationSeconds = 600;
    options.DisallowedGlobals = ["os", "io", "print"];
});
```

## Notes

- Caller cancellation is treated as a failure. If the caller cancels `cancellationToken` while the script runs, the executor publishes a `LuaScriptFailedEvent` with `unexpected_error`, counts it toward quarantine, and rethrows the `OperationCanceledException`.
- `LuaExecutionOptions` XML docs describe the quarantine threshold as "consecutive" failures. In the current implementation the count is not reset by successful runs. It is cleared only when a quarantine expires.
- Because a fresh `Script` is created on every call, parsing cost is paid on every invocation. Keep scripts small; `MaxScriptSizeBytes` bounds this.

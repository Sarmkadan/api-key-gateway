# TransformationOptions

Documentation for `src/ApiKeyGateway/Configuration/TransformationOptions.cs`.

## Overview

This file defines the configuration classes for the request transformation pipeline, a default in-memory rule repository, and DI registration extensions. Bind settings from `appsettings.json` under the `Transformation` key.

## TransformationPipelineOptions

Top-level options for the transformation pipeline.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Whether the transformation pipeline is active. |
| `MaxRulesPerRequest` | `int` | `50` | Maximum rules evaluated per request across all scopes. Acts as a circuit-breaker against runaway rule sets. |
| `StopOnError` | `bool` | `false` | When `true`, a non-fatal error in any rule aborts the remainder of the pipeline. When `false`, the error is logged and processing continues. |
| `EnableBodyCapture` | `bool` | `false` | Whether the middleware buffers the request body so transformation rules can read and modify `TransformationContext.Body`. Has memory cost proportional to request size. |
| `MaxBodySizeBytes` | `int` | `65536` (64 KiB) | Maximum body size in bytes that will be buffered when `EnableBodyCapture` is enabled. Requests exceeding this limit pass through without body capture. |
| `RuleCacheTtl` | `TimeSpan` | `60 s` | Time-to-live for the per-API-key rule cache in `TransformationPipelineService`. Set to `TimeSpan.Zero` to disable caching. |
| `StaticRules` | `List<TransformationRule>` | `[]` | Rules loaded directly from configuration (e.g. `appsettings.json`). For dynamic rule management, use a database-backed `ITransformationRuleRepository`. |
| `Lua` | `LuaExecutionOptions` | _(defaults below)_ | Lua script execution options. |

## LuaExecutionOptions

Options governing Lua script execution inside the transformation pipeline.

| Property | Type | Default | Description |
|---|---|---|---|
| `IsEnabled` | `bool` | `true` | Whether Lua script execution is permitted. When `false`, `LuaScript` rules are skipped. |
| `MaxExecutionMs` | `int` | `50` | Wall-clock timeout in milliseconds per script invocation. Keep low to prevent slow scripts from degrading throughput. |
| `MaxScriptSizeBytes` | `int` | `65536` (64 KiB) | Maximum script source size in UTF-8 bytes. Larger scripts are rejected before execution. |
| `MaxMemoryBytes` | `int` | `1048576` (1 MiB) | Maximum memory a Lua script may consume. Scripts exceeding this are terminated. Set to `0` to disable enforcement. Memory tracking is approximate. |
| `QuarantineThreshold` | `int` | `5` | Consecutive failures/timeouts before a script is automatically quarantined. Set to `0` to disable. |
| `QuarantineDurationSeconds` | `int` | `300` (5 min) | Minimum time a quarantined script stays disabled before automatic re-enablement. |

## ConfigurationTransformationRuleRepository (internal)

Default `ITransformationRuleRepository` backed by `TransformationPipelineOptions.StaticRules`. Rules are held in memory and read-only at runtime; mutations via `CreateAsync`, `UpdateAsync`, and `DeleteAsync` are not persisted across restarts. Replace this registration with a database-backed implementation for dynamic rule management.

## TransformationServiceExtensions

Provides `AddRequestTransformationPipeline(IServiceCollection, IConfiguration)` to register:

- `TransformationPipelineOptions` and `LuaExecutionOptions` as singletons
- `ConfigurationTransformationRuleRepository` as the default `ITransformationRuleRepository`
- `LuaScriptExecutor` as `ILuaScriptExecutor`
- `TransformationPipelineService` as `ITransformationPipeline`

To use a database-backed rule repository, call `services.AddScoped<ITransformationRuleRepository, YourRepository>()` after `AddRequestTransformationPipeline` - the later registration wins.

## Configuration Example

```json
{
  "Transformation": {
    "IsEnabled": true,
    "MaxRulesPerRequest": 50,
    "StopOnError": false,
    "EnableBodyCapture": true,
    "MaxBodySizeBytes": 131072,
    "RuleCacheTtl": "00:02:00",
    "Lua": {
      "IsEnabled": true,
      "MaxExecutionMs": 50,
      "MaxScriptSizeBytes": 65536,
      "MaxMemoryBytes": 1048576,
      "QuarantineThreshold": 5,
      "QuarantineDurationSeconds": 300
    },
    "StaticRules": []
  }
}
```

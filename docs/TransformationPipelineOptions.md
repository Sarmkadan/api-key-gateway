# TransformationPipelineOptions

Configuration options for the request transformation pipeline that applies transformation rules to incoming API requests before they reach protected endpoints. The pipeline can be enabled/disabled, supports static and dynamic rule sources, and provides controls for performance, caching, and error handling.

## API

### Properties

- **IsEnabled** (`bool`)
  Controls whether the transformation pipeline is active. When `false`, all requests bypass transformation processing. Defaults to `true`.

- **MaxRulesPerRequest** (`int`)
  Maximum number of transformation rules that can be applied to a single request. Values less than `1` are treated as `1`. Defaults to `10`.

- **StopOnError** (`bool`)
  Determines whether processing stops after the first encountered error during rule execution. When `true`, subsequent rules are skipped on failure. Defaults to `false`.

- **EnableBodyCapture** (`bool`)
  Enables capture of request bodies for transformation rules that require body inspection. When `false`, body-based rules cannot execute. Defaults to `true`.

- **MaxBodySizeBytes** (`int`)
  Maximum allowed size (in bytes) of a request body that can be captured and processed. Bodies exceeding this size are rejected with an error. Must be a positive value. Defaults to `1_048_576` (1 MB).

- **RuleCacheTtl** (`TimeSpan`)
  Duration for which dynamically loaded transformation rules are cached. Shorter values reduce memory usage but increase load frequency. Must be non-negative. Defaults to `TimeSpan.FromMinutes(5)`.

- **StaticRules** (`List<TransformationRule>`)
  Predefined transformation rules applied to every request when dynamic rule sources are unavailable or disabled. Can be empty. Not thread-safe; modifications should occur at startup.

### Nested Options

- **Lua** (`LuaExecutionOptions`)
  Configuration for Lua script execution within transformation rules. Defines limits on script size, execution time, and sandbox behavior. See `LuaExecutionOptions` for details.

### Rule Repository

- **ConfigurationTransformationRuleRepository** (`ConfigurationTransformationRuleRepository`)
  Service abstraction for retrieving transformation rules from configuration-based sources. Used when dynamic rule loading is not configured.

### Methods

- **GetByApiKeyAsync** (`Task<IReadOnlyList<TransformationRule>>`)
  Retrieves all transformation rules associated with the specified API key. Returns an empty list if no rules exist. May throw `ArgumentNullException` if `apiKey` is null.

- **GetByConsumerAsync** (`Task<IReadOnlyList<TransformationRule>>`)
  Retrieves all transformation rules associated with the specified consumer identifier. Returns an empty list if no rules exist. May throw `ArgumentNullException` if `consumerId` is null.

- **GetGlobalRulesAsync** (`Task<IReadOnlyList<TransformationRule>>`)
  Retrieves all globally applicable transformation rules. Returns an empty list if none are defined. Never throws.

- **CreateAsync** (`Task<string>`)
  Creates a new transformation rule and returns its unique identifier. The returned ID can be used for subsequent updates or deletions. May throw `ArgumentNullException` or `InvalidOperationException` on validation failure.

- **UpdateAsync** (`Task<bool>`)
  Updates an existing transformation rule identified by its rule ID. Returns `true` if the rule existed and was updated; otherwise `false`. May throw `ArgumentNullException` or `InvalidOperationException` on validation failure.

- **DeleteAsync** (`Task<bool>`)
  Deletes a transformation rule identified by its rule ID. Returns `true` if the rule existed and was removed; otherwise `false`. May throw `ArgumentNullException`.

### Registration

- **AddRequestTransformationPipeline** (`static IServiceCollection AddRequestTransformationPipeline`)
  Extension method for `IServiceCollection` that registers required services for the transformation pipeline, including rule repositories, caching, and Lua execution. Returns the collection for chaining. Should be called once during application startup.

## Usage

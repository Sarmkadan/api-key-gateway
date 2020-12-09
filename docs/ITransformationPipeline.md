# ITransformationPipeline

Represents an immutable snapshot of the request transformation process in the API key gateway, capturing all inputs, intermediate state, and outcomes of rule evaluation for auditing and decision making.

## API

### `TransformationContext`
Gets the original context in which the transformation pipeline was created. This context is immutable and contains the raw request data before any transformations were applied.

### `string? ApiKeyId`
Gets the identifier of the API key used in the request, if one was provided. This value is extracted from the request and may be null if no key was supplied.

### `string? ConsumerId`
Gets the identifier of the consumer associated with the API key used in the request, if available. This value is resolved after the API key is validated and may be null if the key is not linked to a consumer.

### `public string Method`
Gets the HTTP method (e.g., "GET", "POST") of the incoming request. This value is extracted directly from the request line and is never null.

### `public string Path`
Gets the normalized request path after any gateway-level routing or prefix stripping. This value represents the internal path used for rule matching and is never null.

### `public Dictionary<string, string> QueryParameters`
Gets the collection of query parameters extracted from the request URL. The dictionary is case-sensitive and preserves the original parameter names and values. Modifications to this dictionary have no effect on the pipeline.

### `public Dictionary<string, string> Headers`
Gets the collection of HTTP headers from the incoming request. Header names are case-insensitive and values are stored as provided. This dictionary is read-only for the purposes of transformation and auditing.

### `public string? Body`
Gets the raw request body as a string, if present. This value is captured after any content decoding and is null if the request had no body.

### `public string SourceIp`
Gets the source IP address of the client making the request. This value is captured from the connection and is never null or empty.

### `public Dictionary<string, object?> Properties`
Gets a mutable dictionary of custom properties attached during transformation. This dictionary is initialized empty and can be used by rules to store intermediate or final state for auditing or downstream processing.

### `public bool IsBlocked`
Gets a value indicating whether the request was blocked by a transformation rule. This value is set during rule evaluation and is true if any rule explicitly blocked the request.

### `public string? BlockReason`
Gets the reason provided when the request was blocked, if any. This value is populated by the rule that enforced the block and is null if the request was not blocked.

### `public required bool Success`
Gets a value indicating whether the transformation pipeline completed successfully without unhandled exceptions. This value is true if all rules executed without throwing, regardless of blocking decisions.

### `public required int RulesEvaluated`
Gets the total number of transformation rules evaluated during this pipeline execution. This count includes rules that were skipped due to conditional logic.

### `public required int RulesApplied`
Gets the number of transformation rules that were actually applied (i.e., executed and modified state). This count excludes rules that were evaluated but did not run due to conditions or early termination.

### `public required TimeSpan Elapsed`
Gets the total time taken to execute the transformation pipeline. This duration includes rule evaluation, state updates, and any internal overhead, measured with high precision.

### `public IReadOnlyDictionary<string, string> Errors`
Gets a read-only dictionary of error messages keyed by rule identifier. Errors are captured when rules throw exceptions and are reported without altering pipeline execution flow.

## Usage

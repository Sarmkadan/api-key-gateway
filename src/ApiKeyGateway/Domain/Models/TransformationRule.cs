// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace ApiKeyGateway.Domain.Models;

/// <summary>
/// Defines which set of requests a <see cref="TransformationRule"/> matches.
/// </summary>
public enum TransformationScope
{
    /// <summary>Rule applies to every request that passes through the transformation pipeline.</summary>
    Global = 0,

    /// <summary>Rule applies only to requests authenticated with a specific API key.</summary>
    ApiKey = 1,

    /// <summary>Rule applies to all API keys that belong to a specific consumer.</summary>
    Consumer = 2
}

/// <summary>
/// Classifies the execution strategy used by a <see cref="TransformationRule"/>.
/// </summary>
public enum TransformationRuleType
{
    /// <summary>
    /// Runs one of the pre-defined built-in operations configured via
    /// <see cref="TransformationRule.Action"/> and <see cref="TransformationRule.Parameters"/>.
    /// </summary>
    BuiltIn = 0,

    /// <summary>
    /// Executes the Lua source code supplied in <see cref="TransformationRule.LuaScript"/>,
    /// giving the script full read/write access to the mutable request context.
    /// </summary>
    LuaScript = 1
}

/// <summary>
/// Enumerates the supported built-in transformation actions that do not require a Lua script.
/// </summary>
public enum BuiltInAction
{
    /// <summary>Adds or overwrites a named request header. Parameters: <c>HeaderName</c>, <c>HeaderValue</c>.</summary>
    AddHeader = 0,

    /// <summary>Removes a named request header when present. Parameters: <c>HeaderName</c>.</summary>
    RemoveHeader = 1,

    /// <summary>Adds or overwrites a query-string parameter. Parameters: <c>ParamName</c>, <c>ParamValue</c>.</summary>
    SetQueryParam = 2,

    /// <summary>Removes a query-string parameter when present. Parameters: <c>ParamName</c>.</summary>
    RemoveQueryParam = 3,

    /// <summary>
    /// Rewrites the request path using a template string.
    /// <c>{path}</c> is substituted with the original path value.
    /// Parameters: <c>PathTemplate</c>.
    /// </summary>
    RewritePath = 4,

    /// <summary>
    /// Injects the authenticated consumer identifier as the <c>X-Consumer-Id</c> request header.
    /// No parameters required.
    /// </summary>
    InjectConsumerId = 5,

    /// <summary>
    /// Injects the authenticated API key identifier as the <c>X-Api-Key-Id</c> request header.
    /// No parameters required.
    /// </summary>
    InjectApiKeyId = 6
}

/// <summary>
/// Persistent entity that describes a single step in the request transformation pipeline.
/// Rules are evaluated in ascending <see cref="Priority"/> order; each rule may mutate
/// headers, query parameters, the request path, or body, and Lua-backed rules may also
/// block the request entirely.
/// </summary>
public sealed class TransformationRule
{
    /// <summary>Gets or sets the unique rule identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets a human-readable name displayed in audit logs and dashboards.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional description of the rule's business purpose.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the scope that determines which requests this rule matches.</summary>
    public TransformationScope Scope { get; set; } = TransformationScope.Global;

    /// <summary>
    /// Gets or sets the API key identifier this rule targets.
    /// Required when <see cref="Scope"/> is <see cref="TransformationScope.ApiKey"/>.
    /// </summary>
    public string? ApiKeyId { get; set; }

    /// <summary>
    /// Gets or sets the consumer identifier this rule targets.
    /// Required when <see cref="Scope"/> is <see cref="TransformationScope.Consumer"/>.
    /// </summary>
    public string? ConsumerId { get; set; }

    /// <summary>Gets or sets the implementation type for this rule.</summary>
    public TransformationRuleType Type { get; set; } = TransformationRuleType.BuiltIn;

    /// <summary>
    /// Gets or sets the built-in action to execute.
    /// Populated only when <see cref="Type"/> is <see cref="TransformationRuleType.BuiltIn"/>.
    /// </summary>
    public BuiltInAction? Action { get; set; }

    /// <summary>
    /// Gets or sets the Lua script source code.
    /// Populated only when <see cref="Type"/> is <see cref="TransformationRuleType.LuaScript"/>.
    /// </summary>
    public string? LuaScript { get; set; }

    /// <summary>
    /// Gets the parameter dictionary used to configure built-in actions.
    /// Common keys: <c>HeaderName</c>, <c>HeaderValue</c>, <c>ParamName</c>,
    /// <c>ParamValue</c>, <c>PathTemplate</c>.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = [];

    /// <summary>
    /// Gets or sets the execution priority. Lower values run first.
    /// Use multiples of 10 (10, 20, 30 …) to leave room for future rules between existing ones.
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>Gets or sets whether the rule participates in pipeline execution.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Gets or sets the UTC timestamp when the rule was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC timestamp when the rule was last modified.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the identifier of the administrator who created the rule.</summary>
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Immutable read-only DTO returned by the admin API for transformation rule queries.
/// </summary>
/// <param name="Id">Unique rule identifier.</param>
/// <param name="Name">Human-readable rule name.</param>
/// <param name="Description">Optional business-purpose description.</param>
/// <param name="Scope">Scope that determines which requests the rule matches.</param>
/// <param name="ApiKeyId">API key identifier for <see cref="TransformationScope.ApiKey"/> rules.</param>
/// <param name="ConsumerId">Consumer identifier for <see cref="TransformationScope.Consumer"/> rules.</param>
/// <param name="Type">Rule implementation type.</param>
/// <param name="Action">Built-in action for <see cref="TransformationRuleType.BuiltIn"/> rules.</param>
/// <param name="Parameters">Action parameters dictionary.</param>
/// <param name="Priority">Execution priority (ascending).</param>
/// <param name="IsEnabled">Whether the rule participates in pipeline execution.</param>
/// <param name="CreatedAt">UTC creation timestamp.</param>
/// <param name="UpdatedAt">UTC last-modified timestamp.</param>
public sealed record TransformationRuleDto(
    string Id,
    string Name,
    string? Description,
    TransformationScope Scope,
    string? ApiKeyId,
    string? ConsumerId,
    TransformationRuleType Type,
    BuiltInAction? Action,
    IReadOnlyDictionary<string, string> Parameters,
    int Priority,
    bool IsEnabled,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    /// <summary>Maps a <see cref="TransformationRule"/> entity to its DTO representation.</summary>
    public static TransformationRuleDto From(TransformationRule rule) => new(
        rule.Id,
        rule.Name,
        rule.Description,
        rule.Scope,
        rule.ApiKeyId,
        rule.ConsumerId,
        rule.Type,
        rule.Action,
        rule.Parameters.AsReadOnly(),
        rule.Priority,
        rule.IsEnabled,
        rule.CreatedAt,
        rule.UpdatedAt);
}

/// <summary>
/// Request body for creating a new transformation rule via the admin API.
/// </summary>
public sealed record CreateTransformationRuleRequest
{
    /// <summary>Gets the rule name. Should be unique within the same scope.</summary>
    public required string Name { get; init; }

    /// <summary>Gets an optional description of the rule's business purpose.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the scope that determines which requests this rule matches.</summary>
    public required TransformationScope Scope { get; init; }

    /// <summary>
    /// Gets the API key identifier this rule targets.
    /// Required when <see cref="Scope"/> is <see cref="TransformationScope.ApiKey"/>.
    /// </summary>
    public string? ApiKeyId { get; init; }

    /// <summary>
    /// Gets the consumer identifier this rule targets.
    /// Required when <see cref="Scope"/> is <see cref="TransformationScope.Consumer"/>.
    /// </summary>
    public string? ConsumerId { get; init; }

    /// <summary>Gets the rule implementation type.</summary>
    public required TransformationRuleType Type { get; init; }

    /// <summary>
    /// Gets the built-in action to execute.
    /// Required when <see cref="Type"/> is <see cref="TransformationRuleType.BuiltIn"/>.
    /// </summary>
    public BuiltInAction? Action { get; init; }

    /// <summary>
    /// Gets the Lua script source code.
    /// Required when <see cref="Type"/> is <see cref="TransformationRuleType.LuaScript"/>.
    /// </summary>
    public string? LuaScript { get; init; }

    /// <summary>Gets the parameter dictionary used to configure built-in actions.</summary>
    public Dictionary<string, string> Parameters { get; init; } = [];

    /// <summary>Gets the execution priority. Lower values run first. Defaults to 100.</summary>
    public int Priority { get; init; } = 100;
}

/// <summary>
/// Request body for updating an existing transformation rule via the admin API.
/// Only populated fields are applied; <see langword="null"/> values are ignored.
/// </summary>
public sealed record UpdateTransformationRuleRequest
{
    /// <summary>Gets the updated rule name, or <see langword="null"/> to leave unchanged.</summary>
    public string? Name { get; init; }

    /// <summary>Gets the updated description, or <see langword="null"/> to leave unchanged.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the updated Lua script, or <see langword="null"/> to leave unchanged.</summary>
    public string? LuaScript { get; init; }

    /// <summary>Gets the updated parameter dictionary, or <see langword="null"/> to leave unchanged.</summary>
    public Dictionary<string, string>? Parameters { get; init; }

    /// <summary>Gets the updated execution priority, or <see langword="null"/> to leave unchanged.</summary>
    public int? Priority { get; init; }

    /// <summary>Gets the updated enabled state, or <see langword="null"/> to leave unchanged.</summary>
    public bool? IsEnabled { get; init; }
}

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Transformation;

/// <summary>
/// Defines the contract for the request transformation pipeline.
/// Implementations evaluate all matching <see cref="TransformationRule"/> entries in
/// ascending priority order, applying header, query, path, body, and Lua-script
/// transformations to an in-flight HTTP request before it reaches its destination handler.
/// </summary>
public interface ITransformationPipeline
{
    /// <summary>
    /// Applies every enabled rule that matches the current request context.
    /// Rules execute in ascending <see cref="TransformationRule.Priority"/> order;
    /// each rule observes mutations made by earlier rules. Processing halts early when
    /// any rule marks <see cref="TransformationContext.IsBlocked"/> as <see langword="true"/>.
    /// </summary>
    /// <param name="context">
    /// The mutable transformation context wrapping the HTTP request. Callers should
    /// read mutations back from this object after the method returns.
    /// </param>
    /// <param name="cancellationToken">Token used to abort long-running script execution.</param>
    /// <returns>
    /// A <see cref="TransformationResult"/> summarising which rules ran, whether the
    /// request was blocked, and any non-fatal errors that occurred during rule execution.
    /// </returns>
    Task<TransformationResult> ApplyAsync(
        TransformationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Pre-fetches and caches transformation rules for the given API key so that the
    /// first authenticated request incurs no rule-loading latency. Call this from the
    /// authentication middleware immediately after key validation.
    /// </summary>
    /// <param name="apiKeyId">Identifier of the authenticated API key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task WarmAsync(string apiKeyId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides persistence and query operations for <see cref="TransformationRule"/> entities.
/// Implementations may source rules from a relational database, a configuration file,
/// an in-memory store, or any other durable backing store.
/// </summary>
public interface ITransformationRuleRepository
{
    /// <summary>
    /// Returns all enabled rules scoped to the specified API key, sorted by
    /// <see cref="TransformationRule.Priority"/> ascending.
    /// </summary>
    Task<IReadOnlyList<TransformationRule>> GetByApiKeyAsync(
        string apiKeyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all enabled rules scoped to the specified consumer, sorted by
    /// <see cref="TransformationRule.Priority"/> ascending.
    /// </summary>
    Task<IReadOnlyList<TransformationRule>> GetByConsumerAsync(
        string consumerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all globally-scoped enabled rules (no API key or consumer constraint),
    /// sorted by <see cref="TransformationRule.Priority"/> ascending.
    /// </summary>
    Task<IReadOnlyList<TransformationRule>> GetGlobalRulesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Persists a new rule and returns its assigned identifier.</summary>
    Task<string> CreateAsync(
        TransformationRule rule,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces all mutable fields on an existing rule.
    /// Returns <see langword="false"/> when no rule with <see cref="TransformationRule.Id"/> exists.
    /// </summary>
    Task<bool> UpdateAsync(
        TransformationRule rule,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes (disables) a rule by its identifier.
    /// Returns <see langword="false"/> when no matching rule is found.
    /// </summary>
    Task<bool> DeleteAsync(
        string ruleId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Mutable context object threaded through every stage of the transformation pipeline.
/// Captures a snapshot of the relevant HTTP request data and exposes it to both built-in
/// transformers and Lua scripts. The pipeline middleware writes final mutations back onto
/// the live <see cref="Microsoft.AspNetCore.Http.HttpContext"/> after all rules execute.
/// </summary>
public sealed class TransformationContext
{
    // Headers that must never be exposed to or overwritten by transformation scripts.
    private static readonly HashSet<string> _sensitiveHeaders =
        new(StringComparer.OrdinalIgnoreCase) { "Authorization", "X-API-Key", "Cookie" };

    /// <summary>
    /// Initialises the context from a live <see cref="HttpRequest"/> and the identity
    /// resolved by the authentication middleware.
    /// </summary>
    /// <param name="request">The current HTTP request.</param>
    /// <param name="apiKeyId">Authenticated API key identifier, or <see langword="null"/> for anonymous requests.</param>
    /// <param name="consumerId">Consumer identifier associated with the authenticated key.</param>
    public TransformationContext(HttpRequest request, string? apiKeyId, string? consumerId)
    {
        ApiKeyId    = apiKeyId;
        ConsumerId  = consumerId;
        Method      = request.Method;
        // Hotfix: Use ToString() to properly handle encoded path segments
        Path        = request.Path.ToString() ?? "/";
        SourceIp    = ExtractIp(request.HttpContext);

        QueryParameters = request.Query
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

        Headers = request.Headers
            .Where(h => !_sensitiveHeaders.Contains(h.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
    }

    /// <summary>Gets the authenticated API key identifier, or <see langword="null"/> for anonymous requests.</summary>
    public string? ApiKeyId { get; }

    /// <summary>Gets the consumer identifier associated with the authenticated key.</summary>
    public string? ConsumerId { get; }

    /// <summary>
    /// Gets or sets the HTTP method.
    /// Scripts may rewrite this to change routing semantics (e.g. <c>GET → POST</c>).
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// Gets or sets the request path.
    /// Scripts may rewrite this to proxy to a different endpoint.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets the mutable dictionary of query-string parameters.
    /// Scripts and built-in actions may add, overwrite, or remove entries.
    /// </summary>
    public Dictionary<string, string> QueryParameters { get; }

    /// <summary>
    /// Gets the mutable dictionary of request headers.
    /// Sensitive system headers (<c>Authorization</c>, <c>X-API-Key</c>, <c>Cookie</c>)
    /// are excluded from this dictionary.
    /// </summary>
    public Dictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets or sets the raw request body as a UTF-8 string, or <see langword="null"/> when the
    /// body has not been buffered. Enable body capture via <see cref="Configuration.TransformationPipelineOptions.EnableBodyCapture"/>.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>Gets the originating client IP address.</summary>
    public string SourceIp { get; }

    /// <summary>
    /// A property bag for arbitrary key/value data. Rules may write values here for
    /// downstream rules to read, or for the audit log to capture.
    /// </summary>
    public Dictionary<string, object?> Properties { get; } = [];

    /// <summary>
    /// When <see langword="true"/>, the pipeline stops processing further rules and the
    /// middleware returns HTTP 403. Lua scripts signal a block by returning the string
    /// literal <c>"BLOCK"</c>.
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Optional human-readable reason written to the audit log when
    /// <see cref="IsBlocked"/> is <see langword="true"/>.
    /// </summary>
    public string? BlockReason { get; set; }

    private static string ExtractIp(HttpContext ctx)
    {
        if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var fwd))
            return fwd.ToString().Split(',')[0].Trim();
        return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

/// <summary>
/// Immutable summary of a complete transformation pipeline execution.
/// </summary>
public sealed record TransformationResult
{
    /// <summary>Gets whether all rules executed without throwing an unhandled exception.</summary>
    public required bool Success { get; init; }

    /// <summary>Gets whether any rule blocked the request.</summary>
    public required bool IsBlocked { get; init; }

    /// <summary>Gets the block reason written to the audit log, when <see cref="IsBlocked"/> is <see langword="true"/>.</summary>
    public string? BlockReason { get; init; }

    /// <summary>Gets the number of rules loaded and evaluated during this execution.</summary>
    public required int RulesEvaluated { get; init; }

    /// <summary>Gets the number of rules that matched and were fully applied.</summary>
    public required int RulesApplied { get; init; }

    /// <summary>Gets the total wall-clock time spent across all rule evaluations.</summary>
    public required TimeSpan Elapsed { get; init; }

    /// <summary>
    /// Gets any non-fatal errors raised during rule execution, keyed by rule identifier.
    /// Non-empty when <see cref="Success"/> is <see langword="false"/>.
    /// </summary>
    public IReadOnlyDictionary<string, string> Errors { get; init; } =
        new Dictionary<string, string>();

    /// <summary>Returns a no-op result used when the pipeline is globally disabled.</summary>
    internal static TransformationResult Skipped() => new()
    {
        Success        = true,
        IsBlocked      = false,
        RulesEvaluated = 0,
        RulesApplied   = 0,
        Elapsed        = TimeSpan.Zero
    };
}

/// <summary>
/// Result returned by <see cref="ILuaScriptExecutor.Validate"/> after static analysis of a Lua script.
/// </summary>
public sealed record ScriptValidationResult
{
    /// <summary>Gets whether the script passed all syntax and security checks.</summary>
    public required bool IsValid { get; init; }

    /// <summary>Gets the list of validation errors. Empty when <see cref="IsValid"/> is <see langword="true"/>.</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];
}

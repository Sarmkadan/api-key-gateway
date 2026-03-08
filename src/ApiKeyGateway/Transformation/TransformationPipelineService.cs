// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics;
using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Transformation;

/// <summary>
/// Core engine that orchestrates the request transformation pipeline.
/// Resolves all <see cref="TransformationRule"/> entries that match the incoming request,
/// executes them in ascending priority order, and applies mutations (header injection,
/// query rewriting, path rewriting, Lua scripts) to the mutable
/// <see cref="TransformationContext"/> before the request proceeds to its handler.
/// </summary>
/// <remarks>
/// <para>
/// Rule sets are loaded concurrently from global, per-consumer, and per-key scopes,
/// then merged and deduplicated before execution begins.
/// </para>
/// <para>
/// A lightweight rule-cache keyed on API key identifier reduces repository round-trips
/// on hot paths. The cache entry expires after <see cref="TransformationPipelineOptions.RuleCacheTtl"/>.
/// </para>
/// </remarks>
public sealed class TransformationPipelineService : ITransformationPipeline
{
    private readonly ITransformationRuleRepository _repository;
    private readonly ILuaScriptExecutor _luaExecutor;
    private readonly TransformationPipelineOptions _options;
    private readonly ILogger<TransformationPipelineService> _logger;

    // Per-API-key rule cache: value is (rules snapshot, expiry timestamp).
    private readonly ConcurrentDictionary<string, (IReadOnlyList<TransformationRule> Rules, DateTime Expires)>
        _ruleCache = new(StringComparer.Ordinal);

    /// <summary>
    /// Initialises a new instance of <see cref="TransformationPipelineService"/>.
    /// </summary>
    public TransformationPipelineService(
        ITransformationRuleRepository repository,
        ILuaScriptExecutor luaExecutor,
        TransformationPipelineOptions options,
        ILogger<TransformationPipelineService> logger)
    {
        _repository  = repository  ?? throw new ArgumentNullException(nameof(repository));
        _luaExecutor = luaExecutor ?? throw new ArgumentNullException(nameof(luaExecutor));
        _options     = options     ?? throw new ArgumentNullException(nameof(options));
        _logger      = logger      ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TransformationResult> ApplyAsync(
        TransformationContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsEnabled)
            return TransformationResult.Skipped();

        var sw = Stopwatch.StartNew();
        var errors = new Dictionary<string, string>(StringComparer.Ordinal);
        var evaluated = 0;
        var applied = 0;

        var rules = await ResolveRulesAsync(context, cancellationToken);

        foreach (var rule in rules)
        {
            if (context.IsBlocked || cancellationToken.IsCancellationRequested)
                break;

            evaluated++;

            try
            {
                var didApply = await ApplyRuleAsync(rule, context, cancellationToken);
                if (didApply) applied++;

                _logger.LogDebug(
                    "Rule {RuleId} ({RuleName}) — {Status}",
                    rule.Id, rule.Name, didApply ? "applied" : "skipped");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Rule {RuleId} ({RuleName}) timed out", rule.Id, rule.Name);
                errors[rule.Id] = "Execution timed out";
                if (_options.StopOnError) break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rule {RuleId} ({RuleName}) failed", rule.Id, rule.Name);
                errors[rule.Id] = ex.Message;
                if (_options.StopOnError) break;
            }
        }

        sw.Stop();

        _logger.LogInformation(
            "Transformation pipeline: {Evaluated} evaluated, {Applied} applied, blocked={IsBlocked} in {Elapsed}ms",
            evaluated, applied, context.IsBlocked, sw.ElapsedMilliseconds);

        return new TransformationResult
        {
            Success        = errors.Count == 0,
            IsBlocked      = context.IsBlocked,
            BlockReason    = context.BlockReason,
            RulesEvaluated = evaluated,
            RulesApplied   = applied,
            Elapsed        = sw.Elapsed,
            Errors         = errors
        };
    }

    /// <inheritdoc />
    public async Task WarmAsync(string apiKeyId, CancellationToken cancellationToken = default)
    {
        _ = await LoadKeyRulesAsync(apiKeyId, cancellationToken);
        _logger.LogDebug("Transformation rule cache warmed for API key {ApiKeyId}", apiKeyId);
    }

    // -------------------------------------------------------------------------
    // Rule resolution
    // -------------------------------------------------------------------------

    private async Task<IReadOnlyList<TransformationRule>> ResolveRulesAsync(
        TransformationContext context,
        CancellationToken ct)
    {
        var tasks = new List<Task<IReadOnlyList<TransformationRule>>>
        {
            _repository.GetGlobalRulesAsync(ct)
        };

        if (context.ConsumerId is not null)
            tasks.Add(_repository.GetByConsumerAsync(context.ConsumerId, ct));

        if (context.ApiKeyId is not null)
            tasks.Add(LoadKeyRulesAsync(context.ApiKeyId, ct));

        var buckets = await Task.WhenAll(tasks);

        return buckets
            .SelectMany(b => b)
            .Where(r => r.IsEnabled)
            .DistinctBy(r => r.Id)
            .OrderBy(r => r.Priority)
            .Take(_options.MaxRulesPerRequest)
            .ToList();
    }

    private async Task<IReadOnlyList<TransformationRule>> LoadKeyRulesAsync(
        string apiKeyId,
        CancellationToken ct)
    {
        var ttl = _options.RuleCacheTtl;

        if (ttl > TimeSpan.Zero
            && _ruleCache.TryGetValue(apiKeyId, out var cached)
            && cached.Expires > DateTime.UtcNow)
        {
            return cached.Rules;
        }

        var rules = await _repository.GetByApiKeyAsync(apiKeyId, ct);

        if (ttl > TimeSpan.Zero)
            _ruleCache[apiKeyId] = (rules, DateTime.UtcNow.Add(ttl));

        return rules;
    }

    // -------------------------------------------------------------------------
    // Rule execution dispatch
    // -------------------------------------------------------------------------

    private async Task<bool> ApplyRuleAsync(
        TransformationRule rule,
        TransformationContext context,
        CancellationToken ct)
    {
        return rule.Type switch
        {
            TransformationRuleType.BuiltIn    => ApplyBuiltIn(rule, context),
            TransformationRuleType.LuaScript
                when !string.IsNullOrWhiteSpace(rule.LuaScript)
                => await _luaExecutor.ExecuteAsync(rule.LuaScript, context, ct),
            _ => false
        };
    }

    // -------------------------------------------------------------------------
    // Built-in action handlers
    // -------------------------------------------------------------------------

    private static bool ApplyBuiltIn(TransformationRule rule, TransformationContext ctx) =>
        rule.Action switch
        {
            BuiltInAction.AddHeader
                when rule.Parameters.TryGetValue("HeaderName", out var name)
                  && rule.Parameters.TryGetValue("HeaderValue", out var value)
                => Assign(ctx.Headers, name, value),

            BuiltInAction.RemoveHeader
                when rule.Parameters.TryGetValue("HeaderName", out var name)
                => ctx.Headers.Remove(name),

            BuiltInAction.SetQueryParam
                when rule.Parameters.TryGetValue("ParamName", out var name)
                  && rule.Parameters.TryGetValue("ParamValue", out var value)
                => Assign(ctx.QueryParameters, name, value),

            BuiltInAction.RemoveQueryParam
                when rule.Parameters.TryGetValue("ParamName", out var name)
                => ctx.QueryParameters.Remove(name),

            BuiltInAction.RewritePath
                when rule.Parameters.TryGetValue("PathTemplate", out var template)
                => RewritePath(ctx, template),

            BuiltInAction.InjectConsumerId
                when ctx.ConsumerId is not null
                => Assign(ctx.Headers, "X-Consumer-Id", ctx.ConsumerId),

            BuiltInAction.InjectApiKeyId
                when ctx.ApiKeyId is not null
                => Assign(ctx.Headers, "X-Api-Key-Id", ctx.ApiKeyId),

            _ => false
        };

    private static bool Assign(Dictionary<string, string> dict, string key, string value)
    {
        dict[key] = value;
        return true;
    }

    private static bool RewritePath(TransformationContext ctx, string template)
    {
        ctx.Path = template.Replace("{path}", ctx.Path, StringComparison.OrdinalIgnoreCase);
        return true;
    }
}

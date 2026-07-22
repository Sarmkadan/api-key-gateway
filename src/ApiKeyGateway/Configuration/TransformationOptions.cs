// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Repositories;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Transformation;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Top-level configuration options for the request transformation pipeline.
/// Bind these from <c>appsettings.json</c> under the <c>Transformation</c> key,
/// or configure them programmatically via <see cref="TransformationServiceExtensions"/>.
/// </summary>
public sealed class TransformationPipelineOptions
{
    /// <summary>Gets or sets whether the transformation pipeline is active. Defaults to <see langword="true"/>.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of rules evaluated per request, across all scopes.
    /// Acts as a circuit-breaker against runaway rule sets. Defaults to <c>50</c>.
    /// </summary>
    public int MaxRulesPerRequest { get; set; } = 50;

    /// <summary>
    /// Gets or sets whether a non-fatal error in any single rule aborts the remainder of the
    /// pipeline (<see langword="true"/>) or logs the error and continues (<see langword="false"/>).
    /// Defaults to <see langword="false"/> so a bad rule does not break all transformations.
    /// </summary>
    public bool StopOnError { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the middleware buffers the request body so transformation rules
    /// and scripts can read and modify <see cref="TransformationContext.Body"/>.
    /// Enabling body capture has a memory cost proportional to request size;
    /// consider pairing with a body size limit. Defaults to <see langword="false"/>.
    /// </summary>
    public bool EnableBodyCapture { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum body size in bytes that will be buffered when
    /// <see cref="EnableBodyCapture"/> is <see langword="true"/>. Requests exceeding this
    /// limit are passed through without body capture. Defaults to <c>65 536</c> bytes (64 KiB).
    /// </summary>
    public int MaxBodySizeBytes { get; set; } = 65_536;

    /// <summary>
    /// Gets or sets the time-to-live for the per-API-key rule cache maintained inside
    /// <see cref="Transformation.TransformationPipelineService"/>.
    /// Set to <see cref="TimeSpan.Zero"/> to disable caching entirely. Defaults to <c>60 s</c>.
    /// </summary>
    public TimeSpan RuleCacheTtl { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets or sets the list of <see cref="TransformationRule"/> entries loaded directly from
    /// configuration (e.g. <c>appsettings.json</c>). This allows deploying simple rules without
    /// a database; for dynamic rule management use a database-backed
    /// <see cref="ITransformationRuleRepository"/> implementation.
    /// </summary>
    public List<TransformationRule> StaticRules { get; set; } = [];

    /// <summary>Gets or sets the Lua script execution options.</summary>
    public LuaExecutionOptions Lua { get; set; } = new();
}

/// <summary>
/// Configuration options governing Lua script execution inside the transformation pipeline.
/// </summary>
public sealed class LuaExecutionOptions
{
    /// <summary>
    /// Gets or sets whether Lua script execution is permitted.
    /// When <see langword="false"/>, any <see cref="TransformationRuleType.LuaScript"/> rule
    /// is skipped rather than executed. Defaults to <see langword="true"/>.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the wall-clock timeout in milliseconds applied per script invocation.
    /// Scripts that do not complete within this window are abandoned.
    /// Keep this low (≤ 50 ms) to prevent slow scripts from degrading gateway throughput.
    /// Defaults to <c>50</c> ms.
    /// </summary>
    public int MaxExecutionMs { get; set; } = 50;

    /// <summary>
    /// Gets or sets the maximum script source size in UTF-8 bytes.
    /// Scripts larger than this limit are rejected before execution begins.
    /// Defaults to <c>65 536</c> bytes (64 KiB).
    /// </summary>
    public int MaxScriptSizeBytes { get; set; } = 65_536;

    /// <summary>
    /// Gets or sets the maximum memory in bytes that a Lua script is allowed to consume.
    /// Scripts exceeding this limit are terminated and marked as failed.
    /// Defaults to <c>1 048 576</c> bytes (1 MiB).
    /// </summary>
    /// <remarks>
    /// Memory tracking is approximate and measured at intervals during execution.
    /// Set to <c>0</c> to disable memory enforcement.
    /// </remarks>
    public int MaxMemoryBytes { get; set; } = 1_048_576;

    /// <summary>
    /// Gets or sets the number of consecutive failures/timeouts after which a script
    /// is automatically quarantined and skipped until an operator re-enables it.
    /// Defaults to <c>5</c> failures.
    /// </summary>
    /// <remarks>
    /// Set to <c>0</c> to disable automatic quarantine.
    /// </remarks>
    public int QuarantineThreshold { get; set; } = 5;

    /// <summary>
    /// Gets or sets the minimum time interval in seconds that a quarantined script
    /// must remain disabled before being automatically re-enabled.
    /// Defaults to <c>300</c> seconds (5 minutes).
    /// </summary>
    public int QuarantineDurationSeconds { get; set; } = 300;
}

// ---------------------------------------------------------------------------
// Default in-configuration rule repository
// ---------------------------------------------------------------------------

/// <summary>
/// Default <see cref="ITransformationRuleRepository"/> backed by the static rule list in
/// <see cref="TransformationPipelineOptions.StaticRules"/>. Rules are held in memory and
/// are read-only at runtime; mutations via <see cref="CreateAsync"/>, <see cref="UpdateAsync"/>,
/// and <see cref="DeleteAsync"/> are not persisted across restarts.
/// Replace this registration with a database-backed implementation for dynamic rule management.
/// </summary>
internal sealed class ConfigurationTransformationRuleRepository : ITransformationRuleRepository
{
    private readonly List<TransformationRule> _rules;

    public ConfigurationTransformationRuleRepository(TransformationPipelineOptions options)
    {
        // Deep-copy to avoid concurrent modification of the options object.
        _rules = options.StaticRules
            .Select(r => new TransformationRule
            {
                Id          = string.IsNullOrEmpty(r.Id) ? Guid.NewGuid().ToString() : r.Id,
                Name        = r.Name,
                Description = r.Description,
                Scope       = r.Scope,
                ApiKeyId    = r.ApiKeyId,
                ConsumerId  = r.ConsumerId,
                Type        = r.Type,
                Action      = r.Action,
                LuaScript   = r.LuaScript,
                Parameters  = new Dictionary<string, string>(r.Parameters),
                Priority    = r.Priority,
                IsEnabled   = r.IsEnabled,
                CreatedAt   = r.CreatedAt,
                UpdatedAt   = r.UpdatedAt,
                CreatedBy   = r.CreatedBy
            })
            .ToList();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TransformationRule>> GetByApiKeyAsync(
        string apiKeyId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TransformationRule> result = _rules
            .Where(r => r.IsEnabled && r.Scope == TransformationScope.ApiKey
                     && r.ApiKeyId == apiKeyId)
            .OrderBy(r => r.Priority)
            .ToList();

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TransformationRule>> GetByConsumerAsync(
        string consumerId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TransformationRule> result = _rules
            .Where(r => r.IsEnabled && r.Scope == TransformationScope.Consumer
                     && r.ConsumerId == consumerId)
            .OrderBy(r => r.Priority)
            .ToList();

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TransformationRule>> GetGlobalRulesAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TransformationRule> result = _rules
            .Where(r => r.IsEnabled && r.Scope == TransformationScope.Global)
            .OrderBy(r => r.Priority)
            .ToList();

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<string> CreateAsync(TransformationRule rule, CancellationToken cancellationToken = default)
    {
        rule.Id = Guid.NewGuid().ToString();
        _rules.Add(rule);
        return Task.FromResult(rule.Id);
    }

    /// <inheritdoc />
    public Task<bool> UpdateAsync(TransformationRule rule, CancellationToken cancellationToken = default)
    {
        var idx = _rules.FindIndex(r => r.Id == rule.Id);
        if (idx < 0) return Task.FromResult(false);
        _rules[idx] = rule;
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        var rule = _rules.FirstOrDefault(r => r.Id == ruleId);
        if (rule is null) return Task.FromResult(false);
        rule.IsEnabled = false;
        return Task.FromResult(true);
    }
}

// ---------------------------------------------------------------------------
// DI extension methods
// ---------------------------------------------------------------------------

/// <summary>
/// Extension methods for registering the request transformation pipeline in the
/// dependency-injection container.
/// </summary>
public static class TransformationServiceExtensions
{
    /// <summary>
    /// Registers the request transformation pipeline, Lua script executor, and the default
    /// configuration-backed rule repository in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">
    /// Application configuration; the <c>Transformation</c> section is bound to
    /// <see cref="TransformationPipelineOptions"/>.
    /// </param>
    /// <returns>The original <paramref name="services"/> for fluent chaining.</returns>
    /// <remarks>
    /// To use a database-backed rule repository instead of the built-in configuration
    /// repository, call <c>services.AddScoped&lt;ITransformationRuleRepository, YourRepository&gt;()</c>
    /// after this call — the later registration wins.
    /// </remarks>
    public static IServiceCollection AddRequestTransformationPipeline(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection("Transformation")
            .Get<TransformationPipelineOptions>()
            ?? new TransformationPipelineOptions();

        services.AddSingleton(options);
        services.AddSingleton(options.Lua);

        // Register the built-in configuration-backed rule repository as the default.
        // It has no scoped dependencies, so it is safe to consume from the singleton
        // pipeline. The database-backed DatabaseTransformationRuleRepository depends
        // on the scoped IDbConnection and must be registered per-scope by the caller
        // (see remarks) when dynamic rule management is required.
        services.AddSingleton<ITransformationRuleRepository, ConfigurationTransformationRuleRepository>();

        services.AddSingleton<ILuaScriptExecutor, LuaScriptExecutor>();
        services.AddSingleton<ITransformationPipeline, TransformationPipelineService>();

        return services;
    }
}

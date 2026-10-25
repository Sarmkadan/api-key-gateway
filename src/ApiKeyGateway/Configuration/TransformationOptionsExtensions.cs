// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using ApiKeyGateway.Domain.Models;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Extension methods for merging and summarizing <see cref="TransformationPipelineOptions"/>.
/// Validation lives in <see cref="TransformationPipelineOptionsValidation"/> and
/// <see cref="TransformationPipelineOptionsExtensions"/>.
/// </summary>
public static class TransformationOptionsExtensions
{
    /// <summary>
    /// Layers <paramref name="overrides"/> on top of this instance and returns the result as a new
    /// <see cref="TransformationPipelineOptions"/>. Neither input is modified.
    /// </summary>
    /// <remarks>
    /// Scalar settings are taken from <paramref name="overrides"/> only when they differ from the
    /// built-in default, because a plain value cannot tell "unset" from "set to the default".
    /// As a consequence, an override cannot restore a default over a non-default base value.
    /// <see cref="TransformationPipelineOptions.StaticRules"/> are concatenated; an override rule
    /// with the same non-empty <see cref="TransformationRule.Id"/> replaces the base rule.
    /// <see cref="LuaExecutionOptions.DisallowedGlobals"/> are combined without duplicates.
    /// </remarks>
    /// <param name="options">The base options.</param>
    /// <param name="overrides">The options whose non-default settings take precedence.</param>
    /// <returns>A new merged <see cref="TransformationPipelineOptions"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="overrides"/> is <see langword="null"/>.</exception>
    public static TransformationPipelineOptions Merge(
        this TransformationPipelineOptions options,
        TransformationPipelineOptions overrides)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(overrides);

        var defaults = new TransformationPipelineOptions();

        return new TransformationPipelineOptions
        {
            IsEnabled = overrides.IsEnabled != defaults.IsEnabled ? overrides.IsEnabled : options.IsEnabled,
            MaxRulesPerRequest = overrides.MaxRulesPerRequest != defaults.MaxRulesPerRequest ? overrides.MaxRulesPerRequest : options.MaxRulesPerRequest,
            StopOnError = overrides.StopOnError != defaults.StopOnError ? overrides.StopOnError : options.StopOnError,
            EnableBodyCapture = overrides.EnableBodyCapture != defaults.EnableBodyCapture ? overrides.EnableBodyCapture : options.EnableBodyCapture,
            MaxBodySizeBytes = overrides.MaxBodySizeBytes != defaults.MaxBodySizeBytes ? overrides.MaxBodySizeBytes : options.MaxBodySizeBytes,
            RuleCacheTtl = overrides.RuleCacheTtl != defaults.RuleCacheTtl ? overrides.RuleCacheTtl : options.RuleCacheTtl,
            StaticRules = MergeRules(options.StaticRules, overrides.StaticRules),
            Lua = MergeLua(options.Lua ?? new LuaExecutionOptions(), overrides.Lua ?? new LuaExecutionOptions())
        };
    }

    /// <summary>
    /// Returns a single-line, human-readable summary of the options for logs and diagnostics.
    /// </summary>
    /// <param name="options">The options to summarize.</param>
    /// <returns>A summary string such as <c>enabled=True maxRules=50 ...</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    public static string ToSummaryString(this TransformationPipelineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var lua = options.Lua ?? new LuaExecutionOptions();
        var ttlSeconds = options.RuleCacheTtl.TotalSeconds.ToString(CultureInfo.InvariantCulture);

        var sb = new StringBuilder();
        sb.Append("pipeline: enabled=").Append(options.IsEnabled)
          .Append(", maxRules=").Append(options.MaxRulesPerRequest)
          .Append(", stopOnError=").Append(options.StopOnError)
          .Append(", bodyCapture=").Append(options.EnableBodyCapture)
          .Append(" (maxBody=").Append(options.MaxBodySizeBytes).Append("B)")
          .Append(", ruleCacheTtl=").Append(ttlSeconds).Append("s")
          .Append(", staticRules=").Append(options.StaticRules?.Count ?? 0);
        sb.Append("; lua: enabled=").Append(lua.IsEnabled)
          .Append(", timeout=").Append(lua.MaxExecutionMs).Append("ms")
          .Append(", maxScript=").Append(lua.MaxScriptSizeBytes).Append("B")
          .Append(", maxMemory=").Append(lua.MaxMemoryBytes).Append("B")
          .Append(", quarantine=").Append(lua.QuarantineThreshold)
          .Append('/').Append(lua.QuarantineDurationSeconds).Append("s")
          .Append(", disallowedGlobals=").Append(lua.DisallowedGlobals?.Count ?? 0);

        return sb.ToString();
    }

    private static List<TransformationRule> MergeRules(
        List<TransformationRule>? baseRules,
        List<TransformationRule>? overrideRules)
    {
        var merged = new List<TransformationRule>();
        var overrideList = overrideRules ?? [];

        foreach (var rule in baseRules ?? [])
        {
            // A base rule is dropped when an override rule carries the same non-empty Id.
            if (!string.IsNullOrEmpty(rule.Id) && overrideList.Any(o => o.Id == rule.Id))
            {
                continue;
            }

            merged.Add(rule);
        }

        merged.AddRange(overrideList);
        return merged;
    }

    private static LuaExecutionOptions MergeLua(LuaExecutionOptions baseLua, LuaExecutionOptions overrideLua)
    {
        var defaults = new LuaExecutionOptions();

        return new LuaExecutionOptions
        {
            IsEnabled = overrideLua.IsEnabled != defaults.IsEnabled ? overrideLua.IsEnabled : baseLua.IsEnabled,
            MaxExecutionMs = overrideLua.MaxExecutionMs != defaults.MaxExecutionMs ? overrideLua.MaxExecutionMs : baseLua.MaxExecutionMs,
            MaxScriptSizeBytes = overrideLua.MaxScriptSizeBytes != defaults.MaxScriptSizeBytes ? overrideLua.MaxScriptSizeBytes : baseLua.MaxScriptSizeBytes,
            MaxMemoryBytes = overrideLua.MaxMemoryBytes != defaults.MaxMemoryBytes ? overrideLua.MaxMemoryBytes : baseLua.MaxMemoryBytes,
            QuarantineThreshold = overrideLua.QuarantineThreshold != defaults.QuarantineThreshold ? overrideLua.QuarantineThreshold : baseLua.QuarantineThreshold,
            QuarantineDurationSeconds = overrideLua.QuarantineDurationSeconds != defaults.QuarantineDurationSeconds ? overrideLua.QuarantineDurationSeconds : baseLua.QuarantineDurationSeconds,
            DisallowedGlobals = (baseLua.DisallowedGlobals ?? Array.Empty<string>())
                .Concat(overrideLua.DisallowedGlobals ?? Array.Empty<string>())
                .Distinct()
                .ToList()
        };
    }
}

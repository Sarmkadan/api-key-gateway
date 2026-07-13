// =============================================================================
// Author: [Your Name]
// =============================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeyGateway.Configuration;

/// <summary>
/// Extension methods for <see cref="TransformationPipelineOptions"/>.
/// </summary>
public static class TransformationPipelineOptionsExtensions
{
    /// <summary>
    /// Validates the <see cref="TransformationPipelineOptions"/> instance.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    public static void Validate(this TransformationPipelineOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.MaxRulesPerRequest < 1)
        {
            throw new InvalidOperationException($"MaxRulesPerRequest must be at least 1, but was {options.MaxRulesPerRequest}.");
        }

        if (options.MaxBodySizeBytes < 0)
        {
            throw new InvalidOperationException($"MaxBodySizeBytes must be non-negative, but was {options.MaxBodySizeBytes}.");
        }

        if (options.RuleCacheTtl < TimeSpan.Zero)
        {
            throw new InvalidOperationException($"RuleCacheTtl must be non-negative, but was {options.RuleCacheTtl}.");
        }

        if (options.Lua is null)
        {
            throw new InvalidOperationException("Lua execution options are not configured.");
        }

        if (options.Lua.MaxExecutionMs < 1)
        {
            throw new InvalidOperationException($"Lua MaxExecutionMs must be at least 1, but was {options.Lua.MaxExecutionMs}.");
        }

        if (options.Lua.MaxScriptSizeBytes < 0)
        {
            throw new InvalidOperationException($"Lua MaxScriptSizeBytes must be non-negative, but was {options.Lua.MaxScriptSizeBytes}.");
        }
    }

    /// <summary>
    /// Adds a transformation rule to the static rule list.
    /// </summary>
    /// <param name="options">The options instance.</param>
    /// <param name="rule">The rule to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="rule"/> is <see langword="null"/>.</exception>
    public static void AddStaticRule(this TransformationPipelineOptions options, TransformationRule rule)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(rule);

        options.StaticRules.Add(rule);
    }

    /// <summary>
    /// Removes a transformation rule from the static rule list by its <see cref="TransformationRule.Id"/>.
    /// </summary>
    /// <param name="options">The options instance.</param>
    /// <param name="ruleId">The ID of the rule to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <returns><see langword="true"/> if the rule was found and removed; otherwise, <see langword="false"/>.</returns>
    public static bool RemoveStaticRule(this TransformationPipelineOptions options, string ruleId)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.StaticRules.RemoveAll(r => r.Id == ruleId) > 0;
    }
}

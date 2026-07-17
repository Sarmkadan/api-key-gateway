// src/ApiKeyGateway/Domain/Models/TransformationRuleExtensions.cs
// 
// Author: [Your Name]
// Date: [Today's Date]
// Description: Adds extension methods to the TransformationRule class.

using System;
using System.Collections.Generic;
using System.Linq;
using ApiKeyGateway.Domain.Models;

public static class TransformationRuleExtensions
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the <paramref name="rule"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="rule">The <see cref="TransformationRule"/> to check.</param>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <see langword="null"/>.</exception>
    public static void ThrowIfNull(this TransformationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the <paramref name="rule"/>'s <see cref="TransformationRule.Name"/> is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="rule">The <see cref="TransformationRule"/> to check.</param>
    /// <exception cref="ArgumentException"><see cref="TransformationRule.Name"/> is <see langword="null"/> or empty.</exception>
    public static void ThrowIfNameNullOrEmpty(this TransformationRule rule)
    {
        ArgumentException.ThrowIfNullOrEmpty(rule.Name);
    }

    /// <summary>
    /// Returns a new <see cref="TransformationRule"/> with the specified <paramref name="name"/> and <paramref name="description"/>.
    /// </summary>
    /// <param name="rule">The <see cref="TransformationRule"/> to clone.</param>
    /// <param name="name">The new <see cref="TransformationRule.Name"/> for the <see cref="TransformationRule"/>.</param>
    /// <param name="description">The new <see cref="TransformationRule.Description"/> for the <see cref="TransformationRule"/>.</param>
    /// <returns>A new <see cref="TransformationRule"/> with the specified <see cref="TransformationRule.Name"/> and <see cref="TransformationRule.Description"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    public static TransformationRule WithNameAndDescription(this TransformationRule rule, string name, string? description)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(name);

        return new TransformationRule
        {
            Name = name,
            Description = description,
            Scope = rule.Scope,
            ApiKeyId = rule.ApiKeyId,
            ConsumerId = rule.ConsumerId,
            Type = rule.Type,
            Action = rule.Action,
            LuaScript = rule.LuaScript,
            Parameters = new Dictionary<string, string>(rule.Parameters),
            Priority = rule.Priority,
            IsEnabled = rule.IsEnabled,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt,
            CreatedBy = rule.CreatedBy
        };
    }

    /// <summary>
    /// Returns a new <see cref="TransformationRule"/> with the specified <paramref name="parameters"/>.
    /// </summary>
    /// <param name="rule">The <see cref="TransformationRule"/> to clone.</param>
    /// <param name="parameters">The new <see cref="TransformationRule.Parameters"/> for the <see cref="TransformationRule"/>.</param>
    /// <returns>A new <see cref="TransformationRule"/> with the specified <see cref="TransformationRule.Parameters"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="parameters"/> is <see langword="null"/>.</exception>
    public static TransformationRule WithParameters(this TransformationRule rule, Dictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(parameters);

        return new TransformationRule
        {
            Name = rule.Name,
            Description = rule.Description,
            Scope = rule.Scope,
            ApiKeyId = rule.ApiKeyId,
            ConsumerId = rule.ConsumerId,
            Type = rule.Type,
            Action = rule.Action,
            LuaScript = rule.LuaScript,
            Parameters = new Dictionary<string, string>(parameters),
            Priority = rule.Priority,
            IsEnabled = rule.IsEnabled,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt,
            CreatedBy = rule.CreatedBy
        };
    }

    /// <summary>
    /// Returns a string representation of the <see cref="TransformationRule.Parameters"/>.
    /// </summary>
    /// <param name="rule">The <see cref="TransformationRule"/> to get the <see cref="TransformationRule.Parameters"/> from.</param>
    /// <returns>A string representation of the <see cref="TransformationRule.Parameters"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/>.Parameters is <see langword="null"/>.</exception>
    public static string GetParametersString(this TransformationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(rule.Parameters);

        return string.Join(", ", rule.Parameters.Select(x => $"{x.Key}={x.Value}"));
    }
}

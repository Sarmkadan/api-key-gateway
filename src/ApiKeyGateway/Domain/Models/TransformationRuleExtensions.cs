// src/ApiKeyGateway/Domain/Models/TransformationRuleExtensions.cs
// 
// Author: [Your Name]
// Date: [Today's Date]
// Description: Adds extension methods to the TransformationRule class.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ApiKeyGateway.Domain.Models;

public static class TransformationRuleExtensions
{
    /// <summary>
    /// Throws an exception if the TransformationRule is null.
    /// </summary>
    /// <param name="rule">The TransformationRule to check.</param>
    /// <exception cref="ArgumentNullException">Thrown if the TransformationRule is null.</exception>
    public static void ThrowIfNull(this TransformationRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
    }

    /// <summary>
    /// Throws an exception if the TransformationRule's Name is null or empty.
    /// </summary>
    /// <param name="rule">The TransformationRule to check.</param>
    /// <exception cref="ArgumentException">Thrown if the TransformationRule's Name is null or empty.</exception>
    public static void ThrowIfNameNullOrEmpty(this TransformationRule rule)
    {
        ArgumentException.ThrowIfNullOrEmpty(rule.Name);
    }

    /// <summary>
    /// Returns a new TransformationRule with the specified Name and Description.
    /// </summary>
    /// <param name="rule">The TransformationRule to clone.</param>
    /// <param name="name">The new Name for the TransformationRule.</param>
    /// <param name="description">The new Description for the TransformationRule.</param>
    /// <returns>A new TransformationRule with the specified Name and Description.</returns>
    public static TransformationRule WithNameAndDescription(this TransformationRule rule, string name, string? description)
    {
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
            Parameters = rule.Parameters,
            Priority = rule.Priority,
            IsEnabled = rule.IsEnabled,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt,
            CreatedBy = rule.CreatedBy
        };
    }

    /// <summary>
    /// Returns a new TransformationRule with the specified Parameters.
    /// </summary>
    /// <param name="rule">The TransformationRule to clone.</param>
    /// <param name="parameters">The new Parameters for the TransformationRule.</param>
    /// <returns>A new TransformationRule with the specified Parameters.</returns>
    public static TransformationRule WithParameters(this TransformationRule rule, Dictionary<string, string> parameters)
    {
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
            Parameters = parameters,
            Priority = rule.Priority,
            IsEnabled = rule.IsEnabled,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt,
            CreatedBy = rule.CreatedBy
        };
    }

    /// <summary>
    /// Returns a string representation of the TransformationRule's Parameters.
    /// </summary>
    /// <param name="rule">The TransformationRule to get the Parameters from.</param>
    /// <returns>A string representation of the TransformationRule's Parameters.</returns>
    public static string GetParametersString(this TransformationRule rule)
    {
        return string.Join(", ", rule.Parameters.Select(x => $"{x.Key}={x.Value}"));
    }
}

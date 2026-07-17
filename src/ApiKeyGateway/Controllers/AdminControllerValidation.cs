// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Validation helpers for administrative operations.
/// Provides validation for usage data export parameters.
/// </summary>
public static class AdminControllerValidation
{
	/// <summary>
	/// Validates export usage data parameters.
	/// </summary>
	/// <param name="format">The export format (csv, xml, json).</param>
	/// <param name="startDate">The start date for the export period.</param>
	/// <param name="endDate">The end date for the export period.</param>
	/// <returns>A list of validation problems; empty if valid.</returns>
	public static IReadOnlyList<string> Validate(
		this string? format,
		DateTime? startDate,
		DateTime? endDate)
	{
		var problems = new List<string>();

		// Validate format
		if (string.IsNullOrWhiteSpace(format))
		{
			problems.Add("Export format cannot be null or whitespace.");
		}
		else
		{
			var normalizedFormat = format.Trim().ToLowerInvariant();
			if (normalizedFormat != "csv" && normalizedFormat != "xml" && normalizedFormat != "json")
			{
				problems.Add("Export format must be 'csv', 'xml', or 'json'.");
			}
		}

		// Validate date range
		if (startDate.HasValue && endDate.HasValue)
		{
			if (endDate.Value < startDate.Value)
			{
				problems.Add("End date must be after start date.");
			}

			// Validate dates are not default (MinValue)
			if (startDate.Value == default)
			{
				problems.Add("Start date cannot be the default DateTime value.");
			}

			if (endDate.Value == default)
			{
				problems.Add("End date cannot be the default DateTime value.");
			}

			// Validate dates are reasonable (not in the distant past or future)
			var now = DateTime.UtcNow;
			if (startDate.Value < now.AddYears(-1))
			{
				problems.Add("Start date cannot be more than one year in the past.");
			}

			if (startDate.Value > now.AddDays(1))
			{
				problems.Add("Start date cannot be in the future.");
			}

			if (endDate.Value < now.AddYears(-1))
			{
				problems.Add("End date cannot be more than one year in the past.");
			}

			if (endDate.Value > now.AddDays(1))
			{
				problems.Add("End date cannot be in the future.");
			}
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Determines whether the specified export parameters are valid.
	/// </summary>
	/// <param name="format">The export format.</param>
	/// <param name="startDate">The start date.</param>
	/// <param name="endDate">The end date.</param>
	/// <returns>True if the parameters are valid; otherwise, false.</returns>
	public static bool IsValid(
		this string? format,
		DateTime? startDate,
		DateTime? endDate)
	{
		return format.Validate(startDate, endDate).Count == 0;
	}

	/// <summary>
	/// Ensures that the specified export parameters are valid.
	/// </summary>
	/// <param name="format">The export format.</param>
	/// <param name="startDate">The start date.</param>
	/// <param name="endDate">The end date.</param>
	/// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
	public static void EnsureValid(
		this string? format,
		DateTime? startDate,
		DateTime? endDate)
	{
		var problems = format.Validate(startDate, endDate);
		if (problems.Count > 0)
		{
			throw new ArgumentException(
				"Export parameters validation failed. Problems: " + string.Join(" ", problems),
				nameof(format));
		}
	}
}

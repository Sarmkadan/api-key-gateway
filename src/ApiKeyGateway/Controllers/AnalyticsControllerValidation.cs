using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiKeyGateway.Controllers;

/// <summary>
/// Provides validation helpers for <see cref="AnalyticsController"/>.
/// </summary>
public static class AnalyticsControllerValidation
{
	/// <summary>
	/// Validates the specified <paramref name="value"/> and returns a list of human-readable problems.
	/// </summary>
	/// <param name="value">The <see cref="AnalyticsController"/> instance to validate.</param>
	/// <returns>An empty list if the instance is valid; otherwise, a list of problems.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static IReadOnlyList<string> Validate(this AnalyticsController? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var problems = new List<string>();

		// No specific validation rules for AnalyticsController,
		// as its public members are all async methods and do not have settable properties.

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Determines whether the specified <paramref name="value"/> is valid.
	/// </summary>
	/// <param name="value">The <see cref="AnalyticsController"/> instance to validate.</param>
	/// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static bool IsValid(this AnalyticsController? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		return !Validate(value).Any();
	}

	/// <summary>
	/// Ensures that the specified <paramref name="value"/> is valid.
	/// Throws an <see cref="ArgumentException"/> if the instance is not valid.
	/// </summary>
	/// <param name="value">The <see cref="AnalyticsController"/> instance to validate.</param>
	/// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
	public static void EnsureValid(this AnalyticsController? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var problems = Validate(value).ToList();

		if (problems.Any())
		{
			throw new ArgumentException($"Invalid AnalyticsController instance: {string.Join(", ", problems)}", nameof(value));
		}
	}
}
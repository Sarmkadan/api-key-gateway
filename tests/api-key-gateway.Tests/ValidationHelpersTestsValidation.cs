// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using ApiKeyGateway.Validation;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Validation helpers for <see cref="ValidationResult"/> instances
/// </summary>
public static class ValidationHelpersTestsValidation
{
	/// <summary>
	/// Validates all aspects of a <see cref="ValidationResult"/> instance
	/// </summary>
	/// <param name="value">The validation result to validate</param>
	/// <returns>List of human-readable problems, empty if valid</returns>
	/// <exception cref="ArgumentNullException">Thrown if value is null</exception>
	public static IReadOnlyList<string> Validate(this ValidationResult? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var problems = new List<string>();

		if (value.IsValid)
		{
			if (value.Message is not null)
			{
				problems.Add("ValidationResult.IsValid is true but Message should be null");
			}

			if (value.Errors.Count != 0)
			{
				problems.Add("ValidationResult.IsValid is true but Errors collection should be empty");
			}
		}
		else
		{
			if (string.IsNullOrWhiteSpace(value.Message) && value.Errors.Count == 0)
			{
				problems.Add("ValidationResult.IsValid is false but both Message and Errors are empty/invalid");
			}
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Checks if a <see cref="ValidationResult"/> instance is valid
	/// </summary>
	/// <param name="value">The validation result to check</param>
	/// <returns>True if valid, false otherwise</returns>
	/// <exception cref="ArgumentNullException">Thrown if value is null</exception>
	public static bool IsValid(this ValidationResult? value)
	{
		return value is not null && Validate(value).Count == 0;
	}

	/// <summary>
	/// Ensures a <see cref="ValidationResult"/> instance is valid, throwing if not
	/// </summary>
	/// <param name="value">The validation result to validate</param>
	/// <exception cref="ArgumentNullException">Thrown if value is null</exception>
	/// <exception cref="ArgumentException">Thrown if validation fails, with problem details</exception>
	public static void EnsureValid(this ValidationResult? value)
	{
		ArgumentNullException.ThrowIfNull(value);

		var problems = Validate(value);
		if (problems.Count > 0)
		{
			throw new ArgumentException(
				$"ValidationResult is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
		}
	}
}

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Validation helpers for DateTime operations
/// </summary>
public static class DateTimeExtensionsValidation
{
	/// <summary>
	/// Validates a DateTime value against common issues
	/// </summary>
	/// <param name="date">The DateTime value to validate</param>
	/// <returns>A list of human-readable validation problems; empty if valid</returns>
	public static IReadOnlyList<string> Validate(this DateTime date)
	{
		var problems = new List<string>();

		// Check for default/minimum DateTime value
		if (date == default)
		{
			problems.Add("DateTime cannot be the default value (DateTime.MinValue)");
		}

		// Check for dates in the past when future is expected
		if (date.IsInPast())
		{
			problems.Add("DateTime cannot be in the past");
		}

		// Check for dates that are too far in the future (arbitrary reasonable limit)
		if (date > DateTime.UtcNow.AddYears(10))
		{
			problems.Add("DateTime cannot be more than 10 years in the future");
		}

		return problems.AsReadOnly();
	}

	/// <summary>
	/// Checks if a DateTime value is valid
	/// </summary>
	/// <param name="date">The DateTime value to check</param>
	/// <returns>True if the DateTime is valid; otherwise false</returns>
	public static bool IsValid(this DateTime date)
	{
		return date.Validate().Count == 0;
	}

	/// <summary>
	/// Ensures that a DateTime value is valid, throwing an exception if not
	/// </summary>
	/// <param name="date">The DateTime value to validate</param>
	/// <exception cref="ArgumentException">Thrown if the DateTime is not valid, containing a list of problems</exception>
	public static void EnsureValid(this DateTime date)
	{
		var problems = date.Validate();
		if (problems.Count > 0)
		{
			throw new ArgumentException(
				$"DateTime validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems.Select(p => $" - {p}"))}");
		}
	}
}

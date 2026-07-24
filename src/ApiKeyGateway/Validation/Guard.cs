// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Provides guard clause methods to validate method arguments and state.
// Follows the "Fail Fast" principle by throwing exceptions immediately when
// preconditions aren't met. All methods are designed to be used at the
// beginning of public methods to ensure valid inputs before processing.
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ApiKeyGateway.Validation;

/// <summary>
/// Provides guard clause methods to validate method arguments and state.
/// Follows the "Fail Fast" principle by throwing exceptions immediately when preconditions aren't met.
/// All methods are designed to be used at the beginning of public methods to ensure valid inputs
/// before processing.
/// </summary>
/// <remarks>
/// This class serves as a centralized validation contract that unifies the previous separate
/// validation classes (StringExtensionsValidation, ApiKeyValidatorValidation, and the exception
/// validation extensions). It provides consistent exception types, parameter naming, and message
/// formatting across the entire codebase.
/// </remarks>
public static class Guard
{
    /// <summary>
    /// Ensures the specified argument is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="argument">The argument to validate.</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is <see langword="null"/>.</exception>
    [Obsolete("Use ArgumentNullException.ThrowIfNull() instead. This method will be removed in a future version.")]
    public static void NotNull<T>([NotNull] T? argument, string? paramName = null) where T : class
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName ?? nameof(argument));
        }
    }

    /// <summary>
    /// Ensures the specified nullable value type argument is not <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="argument">The argument to validate.</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is <see langword="null"/>.</exception>
    [Obsolete("Use ArgumentNullException.ThrowIfNull() instead. This method will be removed in a future version.")]
    public static void NotNull<T>(T? argument, string? paramName = null) where T : struct
    {
        if (argument is null)
        {
            throw new ArgumentNullException(paramName ?? nameof(argument));
        }
    }

    /// <summary>
    /// Ensures the specified string argument is not <see langword="null"/>, empty, or consists only of whitespace.
    /// </summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="argument"/> is empty or consists only of whitespace.</exception>
    public static void NotNullOrWhiteSpace([NotNull] string? argument, string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(argument);

        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException("String cannot be empty or whitespace.", paramName ?? nameof(argument));
        }
    }

    /// <summary>
    /// Ensures the specified string argument is not <see langword="null"/> or empty.
    /// </summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="argument"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="argument"/> is empty.</exception>
    public static void NotNullOrEmpty([NotNull] string? argument, string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(argument);

        if (argument.Length == 0)
        {
            throw new ArgumentException("String cannot be empty.", paramName ?? nameof(argument));
        }
    }

    /// <summary>
    /// Ensures the specified integer is positive (greater than zero).
    /// </summary>
    /// <param name="value">The integer value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than or equal to zero.</exception>
    public static void Positive(int value, string? paramName = null)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName ?? nameof(value), value, "Value must be positive (greater than zero).");
        }
    }

    /// <summary>
    /// Ensures the specified long integer is positive (greater than zero).
    /// </summary>
    /// <param name="value">The long integer value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than or equal to zero.</exception>
    public static void Positive(long value, string? paramName = null)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName ?? nameof(value), value, "Value must be positive (greater than zero).");
        }
    }

    /// <summary>
    /// Ensures the specified integer is non-negative (greater than or equal to zero).
    /// </summary>
    /// <param name="value">The integer value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    public static void NonNegative(int value, string? paramName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName ?? nameof(value), value, "Value cannot be negative.");
        }
    }

    /// <summary>
    /// Ensures the specified integer is within the specified range [min, max].
    /// </summary>
    /// <param name="value">The integer value to validate.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is outside the specified range.</exception>
    public static void InRange(int value, int min, int max, string? paramName = null)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(
                paramName ?? nameof(value),
                value,
                $"Value must be between {min} and {max} (inclusive).");
        }
    }

    /// <summary>
    /// Ensures the specified condition is <see langword="true"/>.
    /// </summary>
    /// <param name="condition">The condition to validate.</param>
    /// <param name="message">The error message to include in the exception when the condition is false.</param>
    /// <param name="paramName">The name of the parameter being validated (optional).</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="condition"/> is <see langword="false"/>.</exception>
    public static void Against(bool condition, string message, string? paramName = null)
    {
        if (!condition)
        {
            throw new ArgumentException(message, paramName);
        }
    }

    /// <summary>
    /// Ensures the specified collection is not empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="collection"/> is empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is <see langword="null"/>.</exception>
    public static void NotEmpty<T>(IReadOnlyCollection<T>? collection, string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection.Count == 0)
        {
            throw new ArgumentException("Collection cannot be empty.", paramName ?? nameof(collection));
        }
    }

    /// <summary>
    /// Ensures the specified collection contains at least the specified minimum number of elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="minCount">The minimum number of elements required.</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentException">Thrown when the collection has fewer than <paramref name="minCount"/> elements.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is <see langword="null"/>.</exception>
    public static void MinCount<T>(IReadOnlyCollection<T>? collection, int minCount, string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection.Count < minCount)
        {
            throw new ArgumentException(
                $"Collection must contain at least {minCount} element{(minCount == 1 ? "" : "s")}.",
                paramName ?? nameof(collection));
        }
    }

    /// <summary>
    /// Ensures the specified value is not the default value for its type.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated. If not provided, the compiler will use the parameter name.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is the default value for its type.</exception>
    public static void NotDefault<T>(T value, string? paramName = null)
    {
        if (EqualityComparer<T>.Default.Equals(value, default!))
        {
            throw new ArgumentException("Value cannot be the default.", paramName ?? nameof(value));
        }
    }
}
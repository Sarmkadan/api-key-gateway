// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace ApiKeyGateway.Extensions;

/// <summary>
/// Validation extension methods for collections and operations from CollectionExtensions.
/// Provides validation helpers to ensure collections and their operations are valid.
/// </summary>
public static class CollectionExtensionsValidation
{
    /// <summary>
    /// Validates pagination parameters.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidatePaginationParameters(int pageNumber, int pageSize)
    {
        var problems = new List<string>();

        if (pageNumber < 1)
        {
            problems.Add($"Page number must be 1 or greater, but was {pageNumber}.");
        }

        if (pageSize < 1)
        {
            problems.Add($"Page size must be 1 or greater, but was {pageSize}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates batch parameters.
    /// </summary>
    /// <param name="batchSize">The maximum number of items per batch.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateBatchParameters(int batchSize)
    {
        var problems = new List<string>();

        if (batchSize < 1)
        {
            problems.Add($"Batch size must be 1 or greater, but was {batchSize}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the key selector function for operations that use it.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="keySelector">The function to extract the key from each element.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateKeySelector<T, TKey>(Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(keySelector);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates the action for ForEachSafe operations.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="action">The action to execute for each item.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> ValidateForEachAction<T>(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates a collection for common issues.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate<T>(this IEnumerable<T>? source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if a collection is valid.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid<T>(this IEnumerable<T>? source)
    {
        return source is not null;
    }

    /// <summary>
    /// Ensures a collection is valid, throwing if not.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when the collection is null.</exception>
    public static void EnsureValid<T>(this IEnumerable<T>? source)
    {
        ArgumentNullException.ThrowIfNull(source);
    }
}
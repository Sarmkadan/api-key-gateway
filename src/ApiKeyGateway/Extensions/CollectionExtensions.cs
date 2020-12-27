// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace ApiKeyGateway.Extensions;

/// <summary>
/// Extension methods for common collection operations.
/// Reduces boilerplate code for pagination, grouping, and filtering.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Applies pagination to a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <param name="source">The source collection to paginate.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated subset of the source collection.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageNumber"/> or <paramref name="pageSize"/> is less than 1.</exception>
    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var skipCount = (pageNumber - 1) * pageSize;
        return source.Skip(skipCount).Take(pageSize);
    }

    /// <summary>
    /// Checks if collection is empty in a readable way.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection to check.</param>
    /// <returns><see langword="true"/> if the collection is null or empty; otherwise, <see langword="false"/>.</returns>
    public static bool IsEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? source) =>
        !source?.Any() ?? true;

    /// <summary>
    /// Checks if collection has items.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The source collection to check.</param>
    /// <returns><see langword="true"/> if the collection is not null and contains items; otherwise, <see langword="false"/>.</returns>
    public static bool HasItems<T>([NotNullWhen(true)] this IEnumerable<T>? source) =>
        source?.Any() ?? false;

    /// <summary>
    /// Groups items and counts occurrences.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="TKey">The type of the key used for grouping.</typeparam>
    /// <param name="source">The source collection to process.</param>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <returns>A dictionary mapping keys to their occurrence counts.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static Dictionary<TKey, int> CountBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        return source.GroupBy(keySelector).ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Batches items into groups of specified size.
    /// Useful for bulk operations.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <param name="source">The source collection to batch.</param>
    /// <param name="batchSize">The maximum number of items per batch.</param>
    /// <returns>An enumerable of batches, where each batch is an enumerable of items.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="batchSize"/> is less than 1.</exception>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return BatchEnumerator(enumerator, batchSize - 1);
        }
    }

    private static IEnumerable<T> BatchEnumerator<T>(IEnumerator<T> enumerator, int batchSize)
    {
        yield return enumerator.Current;
        for (int i = 0; i < batchSize && enumerator.MoveNext(); i++)
        {
            yield return enumerator.Current;
        }
    }

    /// <summary>
    /// Returns distinct items based on key selector.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <typeparam name="TKey">The type of the key used for distinct comparison.</typeparam>
    /// <param name="source">The source collection to process.</param>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <returns>An enumerable containing only distinct elements based on the key selector.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is <see langword="null"/>.</exception>
    public static IEnumerable<T> DistinctBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        return source.GroupBy(keySelector).Select(g => g.First());
    }

    /// <summary>
    /// Safely executes action for each item without throwing on individual failures.
    /// </summary>
    /// <typeparam name="T">The type of elements in the source collection.</typeparam>
    /// <param name="source">The source collection to iterate over.</param>
    /// <param name="action">The action to execute for each item.</param>
    /// <param name="logger">Optional logger for capturing errors.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is <see langword="null"/>.</exception>
    public static void ForEachSafe<T>(
        this IEnumerable<T> source,
        Action<T> action,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            try
            {
                action(item);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Error in ForEachSafe");
                // Continue to next item
            }
        }
    }
}
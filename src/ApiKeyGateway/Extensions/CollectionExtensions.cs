// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var skipCount = (pageNumber - 1) * pageSize;
        return source.Skip(skipCount).Take(pageSize);
    }

    /// <summary>
    /// Checks if collection is empty in a readable way.
    /// </summary>
    public static bool IsEmpty<T>(this IEnumerable<T> source) =>
        !source?.Any() ?? true;

    /// <summary>
    /// Checks if collection has items.
    /// </summary>
    public static bool HasItems<T>(this IEnumerable<T> source) =>
        source?.Any() ?? false;

    /// <summary>
    /// Groups items and counts occurrences.
    /// </summary>
    public static Dictionary<TKey, int> CountBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector) where TKey : notnull =>
        source.GroupBy(keySelector).ToDictionary(g => g.Key, g => g.Count());

    /// <summary>
    /// Batches items into groups of specified size.
    /// Useful for bulk operations.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
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
    public static IEnumerable<T> DistinctBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector) =>
        source.GroupBy(keySelector).Select(g => g.First());

    /// <summary>
    /// Safely executes action for each item without throwing on individual failures.
    /// </summary>
    public static void ForEachSafe<T>(
        this IEnumerable<T> source,
        Action<T> action,
        ILogger? logger = null)
    {
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

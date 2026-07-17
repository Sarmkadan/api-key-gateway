using System;
using System.Collections.Generic;

namespace api_key_gateway.Tests
{
    /// <summary>
    /// Extension methods for <see cref="CollectionExtensionsTests"/> to simplify common test assertions and operations.
    /// </summary>
    public static class CollectionExtensionsTestsExtensions
    {
        /// <summary>
        /// Asserts that the collection is empty.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection to check.</param>
        /// <param name="paramName">The name of the parameter for error reporting.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the collection is not empty.</exception>
        public static void ShouldBeEmpty<T>(this IEnumerable<T> collection, string paramName = null)
        {
            ArgumentNullException.ThrowIfNull(collection, paramName ?? nameof(collection));

            if (collection.GetEnumerator().MoveNext())
            {
                throw new ArgumentException("Collection is not empty.", paramName ?? nameof(collection));
            }
        }

        /// <summary>
        /// Asserts that the collection is not empty.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection to check.</param>
        /// <param name="paramName">The name of the parameter for error reporting.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the collection is empty.</exception>
        public static void ShouldNotBeEmpty<T>(this IEnumerable<T> collection, string paramName = null)
        {
            ArgumentNullException.ThrowIfNull(collection, paramName ?? nameof(collection));

            if (!collection.GetEnumerator().MoveNext())
            {
                throw new ArgumentException("Collection is empty.", paramName ?? nameof(collection));
            }
        }

        /// <summary>
        /// Asserts that the collection has the expected count of items.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection to check.</param>
        /// <param name="expectedCount">The expected number of items.</param>
        /// <param name="paramName">The name of the parameter for error reporting.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="expectedCount"/> is negative.</exception>
        /// <exception cref="ArgumentException">Thrown if the collection count doesn't match the expected count.</exception>
        public static void ShouldHaveCount<T>(this IEnumerable<T> collection, int expectedCount, string paramName = null)
        {
            ArgumentNullException.ThrowIfNull(collection, paramName ?? nameof(collection));
            ArgumentOutOfRangeException.ThrowIfNegative(expectedCount, nameof(expectedCount));

            var actualCount = 0;
            foreach (var _ in collection)
            {
                actualCount++;
            }

            if (actualCount != expectedCount)
            {
                throw new ArgumentException(
                    $"Collection count mismatch. Expected: {expectedCount}, Actual: {actualCount}.",
                    paramName ?? nameof(collection));
            }
        }

        /// <summary>
        /// Creates a dictionary from the collection using key selector and element selector functions.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <typeparam name="TElement">The type of the element returned by elementSelector.</typeparam>
        /// <param name="source">The collection to create the dictionary from.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
        /// <returns>A dictionary that contains values of type <typeparamref name="TElement"/> selected from the input sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/>, <paramref name="keySelector"/>, or <paramref name="elementSelector"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if duplicate keys are encountered.</exception>
        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
            where TKey : notnull
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(keySelector);
            ArgumentNullException.ThrowIfNull(elementSelector);

            var dictionary = new Dictionary<TKey, TElement>();

            foreach (var item in source)
            {
                var key = keySelector(item);
                var element = elementSelector(item);

                if (!dictionary.TryAdd(key, element))
                {
                    throw new ArgumentException($"Duplicate key detected: {key}", nameof(keySelector));
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Gets the first element of a sequence, or a default value if the sequence is empty.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">The collection to get the first element from.</param>
        /// <param name="defaultValue">The default value to return if the sequence is empty.</param>
        /// <returns><paramref name="defaultValue"/> if source is empty; otherwise, the first element in source.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        public static T FirstOrDefault<T>(this IEnumerable<T> source, T defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (var item in source)
            {
                return item;
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets the last element of a sequence, or a default value if the sequence is empty.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">The collection to get the last element from.</param>
        /// <param name="defaultValue">The default value to return if the sequence is empty.</param>
        /// <returns><paramref name="defaultValue"/> if source is empty; otherwise, the last element in source.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        public static T LastOrDefault<T>(this IEnumerable<T> source, T defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(source);

            var result = defaultValue;
            foreach (var item in source)
            {
                result = item;
            }

            return result;
        }

        /// <summary>
        /// Determines whether all elements of a sequence satisfy a condition.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">The collection to check.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>true if every element of the source sequence passes the test in the specified predicate, or if the sequence is empty; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static bool All<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            foreach (var item in source)
            {
                if (!predicate(item))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">The collection to check.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>true if any elements in the source sequence pass the test in the specified predicate; otherwise, false. If the sequence is empty, returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
        public static bool Any<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);

            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a new sequence with the specified item added to the end.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="item">The item to append.</param>
        /// <returns>A new sequence with the item appended.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (var element in source)
            {
                yield return element;
            }

            yield return item;
        }

        /// <summary>
        /// Returns a new sequence with the specified item added to the beginning.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="item">The item to prepend.</param>
        /// <returns>A new sequence with the item prepended.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item)
        {
            ArgumentNullException.ThrowIfNull(source);

            yield return item;

            foreach (var element in source)
            {
                yield return element;
            }
        }
    }
}
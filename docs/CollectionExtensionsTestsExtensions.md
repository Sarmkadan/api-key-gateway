# CollectionExtensionsTestsExtensions

Provides a set of extension methods intended for use in unit‑test scenarios to assert collection state or to perform common LINQ‑like operations with a fluent, test‑friendly API.

## API

### `public static void ShouldBeEmpty<T>(this IEnumerable<T> source)`
- **Purpose**: Asserts that the collection contains no elements.
- **Parameters**: `source` – the collection to test.
- **Return value**: None.
- **Throws**: 
  - `ArgumentNullException` if `source` is `null`.
  - `AssertionException` (or the test framework’s equivalent) if `source` contains one or more elements.

### `public static void ShouldNotBeEmpty<T>(this IEnumerable<T> source)`
- **Purpose**: Asserts that the collection contains at least one element.
- **Parameters**: `source` – the collection to test.
- **Return value**: None.
- **Throws**: 
  - `ArgumentNullException` if `source` is `null`.
  - `AssertionException` if `source` is empty.

### `public static void ShouldHaveCount<T>(this IEnumerable<T> source, int expectedCount)`
- **Purpose**: Asserts that the collection contains exactly `expectedCount` elements.
- **Parameters**: 
  - `source` – the collection to test.
  - `expectedCount` – the expected number of elements.
- **Return value**: None.
- **Throws**: 
  - `ArgumentNullException` if `source` is `null`.
  - `AssertionException` if the actual count differs from `expectedCount`.

### `public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)`
- **Purpose**: Projects the elements of a sequence into a dictionary using the specified key and element selector functions.
- **Parameters**: 
  - `source` – the sequence to project.
  - `keySelector` – a function to extract a key from each element.
  - `elementSelector` – a function to transform each element into a dictionary value.
- **Return value**: A `Dictionary<TKey, TElement>` containing the projected elements.
- **Throws**: 
  - `ArgumentNullException` if `source`, `keySelector`, or `elementSelector` is `null`.
  - `ArgumentException` if `keySelector` produces duplicate keys.

### `public static T FirstOrDefault<T>(this IEnumerable<T> source)`
- **Purpose**: Returns the first element of a sequence, or the default value for `T` if the sequence is empty.
- **Parameters**: `source` – the sequence to enumerate.
- **Return value**: The first element, or `default(T)` if no elements exist.
- **Throws**: `ArgumentNullException` if `source` is `null`.

### `public static T LastOrDefault<T>(this IEnumerable<T> source)`
- **Purpose**: Returns the last element of a sequence, or the default value for `T` if the sequence is empty.
- **Parameters**: `source` – the sequence to enumerate.
- **Return value**: The last element, or `default(T)` if no elements exist.
- **Throws**: `ArgumentNullException` if `source` is `null`.

### `public static bool All<T>(this IEnumerable<T> source, Func<T, bool> predicate)`
- **Purpose**: Determines whether all elements of a sequence satisfy a condition.
- **Parameters**: 
  - `source` – the sequence to test.
  - `predicate` – a function to test each element for a condition.
- **Return value**: `true` if every element satisfies the condition or if the sequence is empty; otherwise `false`.
- **Throws**: 
  - `ArgumentNullException` if `source` or `predicate` is `null`.

### `public static bool Any<T>(this IEnumerable<T> source, Func<T, bool> predicate)`
- **Purpose**: Determines whether any element of a sequence satisfies a condition.
- **Parameters**: 
  - `source` – the sequence to test.
  - `predicate` – a function to test each element for a condition.
- **Return value**: `true` if at least one element satisfies the condition; otherwise `false`.
- **Throws**: 
  - `ArgumentNullException` if `source` or `predicate` is `null`.

### `public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T element)`
- **Purpose**: Returns a new sequence that appends the specified element to the end of the source sequence.
- **Parameters**: 
  - `source` – the sequence to append to.
  - `element` – the value to append.
- **Return value**: An `IEnumerable<T>` that yields the elements of `source` followed by `element`.
- **Throws**: `ArgumentNullException` if `source` is `null`.

### `public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T element)`
- **Purpose**: Returns a new sequence that prepends the specified element to the beginning of the source sequence.
- **Parameters**: 
  - `source` – the sequence to prepend to.
  - `element` – the value to prepend.
- **Return value**: An `IEnumerable<T>` that yields `element` followed by the elements of `source`.
- **Throws**: `ArgumentNullException` if `source` is `null`.

## Usage

```csharp
using NUnit.Framework;
using static CollectionExtensionsTestsExtensions;

[Test]
public void CollectionAssertions()
{
    var numbers = new List<int> { 1, 2, 3 };

    // Assert that the collection is not empty and has exactly three items
    numbers.ShouldNotBeEmpty();
    numbers.ShouldHaveCount(3);

    // Assert that a filtered collection is empty
    var evens = numbers.Where(n => n % 2 == 0);
    evens.ShouldBeEmpty(); // fails, because 2 is present
}
```

```csharp
using System.Linq;
using static CollectionExtensionsTestsExtensions;

[Test]
public void FluentLinqOperations()
{
    var words = new[] { "apple", "banana", "cherry" };

    // Prepend a word, then append another, and finally convert to a dictionary keyed by length
    var dict = words
        .Prepend("apricot")
        .Append("date")
        .ToDictionary(w => w.Length, w => w.ToUpperInvariant());

    // Verify the dictionary contents
    Assert.AreEqual(5, dict.Count);
    Assert.IsTrue(dict.ContainsKey(5)); // "apple", "apricot"
    Assert.AreEqual("APPLE", dict[5]); // note: duplicate key throws; this example assumes unique lengths
}
```

## Notes

- All methods that accept an `IEnumerable<T>` source will throw `ArgumentNullException` if the source is `null`.  
- Predicate‑based methods (`All`, `Any`) also throw `ArgumentNullException` when the predicate is `null`.  
- `ToDictionary` follows LINQ semantics: duplicate keys cause an `ArgumentException`, and `null` selectors cause `ArgumentNullException`.  
- The assertion methods (`ShouldBeEmpty`, `ShouldNotBeEmpty`, `ShouldHaveCount`) are intended to be used with a unit‑testing framework; they throw the framework’s assertion exception type when the condition is not met.  
- These extension methods do not modify the original source; they operate lazily (except where materialization is required, e.g., `ToDictionary` and the count‑based assertions).  
- Because they rely only on the input parameters and have no internal static state, the methods are thread‑safe as long as the caller ensures that the supplied delegates and collections are not mutated concurrently in an unsafe manner.  
- The generic type parameters are inferred from usage; explicit type arguments are rarely needed.

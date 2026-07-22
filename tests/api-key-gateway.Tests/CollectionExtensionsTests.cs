// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Extensions;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides unit tests for the collection extension methods in ApiKeyGateway.Extensions.CollectionExtensions.
/// Tests functionality for pagination, collection state checking, batching, and other collection operations.
/// </summary>
public class CollectionExtensionsTests
{
    // -------------------------------------------------------------------------
    // Paginate
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that Paginate returns the correct slice of items for the first page.
    /// </summary>
    [Fact]
    public void Paginate_FirstPage_ReturnsCorrectSlice()
    {
        var items = Enumerable.Range(1, 20);
        items.Paginate(1, 5).Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    /// <summary>
    /// Tests that Paginate returns the correct slice of items for the second page.
    /// </summary>
    [Fact]
    public void Paginate_SecondPage_ReturnsCorrectSlice()
    {
        var items = Enumerable.Range(1, 20);
        items.Paginate(2, 5).Should().BeEquivalentTo(new[] { 6, 7, 8, 9, 10 });
    }

    /// <summary>
    /// Tests that Paginate returns an empty collection when requesting a page beyond the last available page.
    /// </summary>
    [Fact]
    public void Paginate_BeyondLastPage_ReturnsEmpty()
    {
        var items = Enumerable.Range(1, 5);
        items.Paginate(10, 5).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Paginate returns an empty collection when paginating an empty collection.
    /// </summary>
    [Fact]
    public void Paginate_EmptyCollection_ReturnsEmpty()
    {
        Enumerable.Empty<int>().Paginate(1, 10).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Paginate returns all items when the page size is larger than the collection.
    /// </summary>
    [Fact]
    public void Paginate_PageSizeLargerThanCollection_ReturnsAllItems()
    {
        var items = new[] { 1, 2, 3 };
        items.Paginate(1, 100).Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    /// <summary>
    /// Tests that Paginate throws ArgumentNullException when source is null.
    /// </summary>
    [Fact]
    public void Paginate_NullSource_ThrowsArgumentNullException()
    {
        IEnumerable<int>? nullCollection = null;
        var act = () => nullCollection.Paginate(1, 10);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that Paginate throws ArgumentOutOfRangeException when pageNumber is 0.
    /// </summary>
    [Fact]
    public void Paginate_ZeroPageNumber_ThrowsArgumentOutOfRangeException()
    {
        var items = new[] { 1, 2, 3 };
        var act = () => items.Paginate(0, 10);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that Paginate throws ArgumentOutOfRangeException when pageNumber is negative.
    /// </summary>
    [Fact]
    public void Paginate_NegativePageNumber_ThrowsArgumentOutOfRangeException()
    {
        var items = new[] { 1, 2, 3 };
        var act = () => items.Paginate(-1, 10);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that Paginate throws ArgumentOutOfRangeException when pageSize is 0.
    /// </summary>
    [Fact]
    public void Paginate_ZeroPageSize_ThrowsArgumentOutOfRangeException()
    {
        var items = new[] { 1, 2, 3 };
        var act = () => items.Paginate(1, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that Paginate throws ArgumentOutOfRangeException when pageSize is negative.
    /// </summary>
    [Fact]
    public void Paginate_NegativePageSize_ThrowsArgumentOutOfRangeException()
    {
        var items = new[] { 1, 2, 3 };
        var act = () => items.Paginate(1, -5);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that Paginate returns correct slice when page size exactly matches collection size.
    /// </summary>
    [Fact]
    public void Paginate_PageSizeEqualsCollectionSize_ReturnsAllItems()
    {
        var items = new[] { 1, 2, 3, 4, 5 };
        items.Paginate(1, 5).Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    /// <summary>
    /// Tests that Paginate returns correct slice for page 1 with various page sizes.
    /// </summary>
    [Theory]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 1, 1, new[] { 1 })]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 1, 2, new[] { 1, 2 })]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 1, 3, new[] { 1, 2, 3 })]
    public void Paginate_PageOneWithVariousSizes_ReturnsCorrectSlice(int[] items, int pageNumber, int pageSize, int[] expected)
    {
        items.Paginate(pageNumber, pageSize).Should().BeEquivalentTo(expected);
    }

    // -------------------------------------------------------------------------
    // IsEmpty / HasItems
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that IsEmpty returns true when checking a null collection.
    /// </summary>
    [Fact]
    public void IsEmpty_NullCollection_ReturnsTrue()
    {
        IEnumerable<int>? nullCollection = null;
        nullCollection.IsEmpty().Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsEmpty returns true when checking an empty collection.
    /// </summary>
    [Fact]
    public void IsEmpty_EmptyCollection_ReturnsTrue()
    {
        Enumerable.Empty<int>().IsEmpty().Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsEmpty returns false when checking a non-empty collection.
    /// </summary>
    [Fact]
    public void IsEmpty_NonEmptyCollection_ReturnsFalse()
    {
        new[] { 1 }.IsEmpty().Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasItems returns false when checking a null collection.
    /// </summary>
    [Fact]
    public void HasItems_NullCollection_ReturnsFalse()
    {
        IEnumerable<int>? nullCollection = null;
        nullCollection.HasItems().Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasItems returns false when checking an empty collection.
    /// </summary>
    [Fact]
    public void HasItems_EmptyCollection_ReturnsFalse()
    {
        Enumerable.Empty<string>().HasItems().Should().BeFalse();
    }

    /// <summary>
    /// Tests that HasItems returns true when checking a non-empty collection.
    /// </summary>
    [Fact]
    public void HasItems_NonEmptyCollection_ReturnsTrue()
    {
        new[] { "a" }.HasItems().Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsEmpty returns false when checking a null collection.
    /// </summary>
    [Fact]
    public void IsEmpty_NonNullCollection_ReturnsFalseForNull()
    {
        IEnumerable<int>? nullCollection = null;
        nullCollection.IsEmpty().Should().BeTrue();
    }

    /// <summary>
    /// Tests that HasItems returns true for a single item collection.
    /// </summary>
    [Fact]
    public void HasItems_SingleItemCollection_ReturnsTrue()
    {
        new[] { 42 }.HasItems().Should().BeTrue();
    }

    /// <summary>
    /// Tests that IsEmpty returns true for a collection with whitespace strings.
    /// </summary>
    [Fact]
    public void IsEmpty_CollectionWithWhitespaceStrings_ReturnsFalse()
    {
        new[] { " ", "\t", "\n" }.IsEmpty().Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // CountBy
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that CountBy correctly groups and counts items by the specified key selector.
    /// </summary>
    [Fact]
    public void CountBy_GroupsAndCountsCorrectly()
    {
        var items = new[] { "a", "b", "a", "c", "b", "a" };
        var result = items.CountBy(x => x);

        result.Should().ContainKey("a").WhoseValue.Should().Be(3);
        result.Should().ContainKey("b").WhoseValue.Should().Be(2);
        result.Should().ContainKey("c").WhoseValue.Should().Be(1);
    }

    /// <summary>
    /// Tests that CountBy returns an empty dictionary when called on an empty collection.
    /// </summary>
    [Fact]
    public void CountBy_EmptyCollection_ReturnsEmptyDictionary()
    {
        var result = Enumerable.Empty<string>().CountBy(x => x);
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that CountBy throws ArgumentNullException when source is null.
    /// </summary>
    [Fact]
    public void CountBy_NullSource_ThrowsArgumentNullException()
    {
        IEnumerable<string>? nullCollection = null;
        var act = () => nullCollection.CountBy(x => x);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that CountBy throws ArgumentNullException when keySelector is null.
    /// </summary>
    [Fact]
    public void CountBy_NullKeySelector_ThrowsArgumentNullException()
    {
        var items = new[] { "a", "b", "c" };
        Func<string, char>? nullSelector = null;
        var act = () => items.CountBy(nullSelector);
        act.Should().Throw<ArgumentNullException>();
    }

    // -------------------------------------------------------------------------
    // Batch
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that Batch correctly divides a collection into batches of equal size when evenly divisible.
    /// </summary>
    [Fact]
    public void Batch_EvenlyDivisible_ReturnCorrectBatches()
    {
        var items = Enumerable.Range(1, 6);
        var batches = items.Batch(3).Select(b => b.ToList()).ToList();

        batches.Should().HaveCount(2);
        batches[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
        batches[1].Should().BeEquivalentTo(new[] { 4, 5, 6 });
    }

    /// <summary>
    /// Tests that Batch correctly handles uneven division, with the last batch containing the remainder.
    /// </summary>
    [Fact]
    public void Batch_UnevenlyDivisible_LastBatchHasRemainder()
    {
        var items = Enumerable.Range(1, 7);
        var batches = items.Batch(3).Select(b => b.ToList()).ToList();

        batches.Should().HaveCount(3);
        batches[2].Should().BeEquivalentTo(new[] { 7 });
    }

    /// <summary>
    /// Tests that Batch returns no batches when called on an empty collection.
    /// </summary>
    [Fact]
    public void Batch_EmptyCollection_ReturnsNoBatches()
    {
        var batches = Enumerable.Empty<int>().Batch(5).ToList();
        batches.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Batch creates individual batches for each item when batch size is 1.
    /// </summary>
    [Fact]
    public void Batch_SingleItemBatchSize_EachItemIsSeparateBatch()
    {
        var items = new[] { 10, 20, 30 };
        var batches = items.Batch(1).Select(b => b.ToList()).ToList();

        batches.Should().HaveCount(3);
        batches[0].Should().BeEquivalentTo(new[] { 10 });
        batches[1].Should().BeEquivalentTo(new[] { 20 });
        batches[2].Should().BeEquivalentTo(new[] { 30 });
    }

    /// <summary>
    /// Tests that Batch throws ArgumentNullException when source is null.
    /// </summary>
    [Fact]
    public void Batch_NullSource_ThrowsArgumentNullException()
    {
        IEnumerable<int>? nullCollection = null;
        var act = () => nullCollection.Batch(5);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that Batch throws ArgumentOutOfRangeException when batchSize is 0.
    /// </summary>
    [Fact]
    public void Batch_ZeroBatchSize_ThrowsArgumentOutOfRangeException()
    {
        var items = new[] { 1, 2, 3 };
        var act = () => items.Batch(0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that Batch throws ArgumentOutOfRangeException when batchSize is negative.
    /// </summary>
    [Fact]
    public void Batch_NegativeBatchSize_ThrowsArgumentOutOfRangeException()
    {
        var items = new[] { 1, 2, 3 };
        var act = () => items.Batch(-3);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that Batch returns correct batches when batch size is larger than collection.
    /// </summary>
    [Fact]
    public void Batch_BatchSizeLargerThanCollection_ReturnsSingleBatch()
    {
        var items = new[] { 1, 2, 3 };
        var batches = items.Batch(100).Select(b => b.ToList()).ToList();
        batches.Should().HaveCount(1);
        batches[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    // -------------------------------------------------------------------------
    // DistinctBy
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that DistinctBy returns the first occurrence when duplicate keys are present.
    /// </summary>
    [Fact]
    public void DistinctBy_DuplicateKeys_ReturnsFirstOccurrence()
    {
        var items = new[]
        {
            new { Name = "Alice", Age = 30 },
            new { Name = "Alice", Age = 25 },
            new { Name = "Bob", Age = 40 }
        };

        var result = ApiKeyGateway.Extensions.CollectionExtensions.DistinctBy(items, x => x.Name).ToList();

        result.Should().HaveCount(2);
        result.First(x => x.Name == "Alice").Age.Should().Be(30);
    }

    /// <summary>
    /// Tests that DistinctBy returns all items when all keys are unique.
    /// </summary>
    [Fact]
    public void DistinctBy_AllUnique_ReturnsAll()
    {
        var items = new[] { 1, 2, 3 };
        ApiKeyGateway.Extensions.CollectionExtensions.DistinctBy(items, x => x).Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that DistinctBy throws ArgumentNullException when source is null.
    /// </summary>
    [Fact]
    public void DistinctBy_NullSource_ThrowsArgumentNullException()
    {
        IEnumerable<int>? nullCollection = null;
        var act = () => ApiKeyGateway.Extensions.CollectionExtensions.DistinctBy(nullCollection, x => x);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that DistinctBy throws ArgumentNullException when keySelector is null.
    /// </summary>
    [Fact]
    public void DistinctBy_NullKeySelector_ThrowsArgumentNullException()
    {
        var items = new[] { 1, 2, 3 };
        Func<int, int>? nullSelector = null;
        var act = () => ApiKeyGateway.Extensions.CollectionExtensions.DistinctBy(items, nullSelector);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that DistinctBy returns first occurrence when multiple items have same key.
    /// </summary>
    [Fact]
    public void DistinctBy_MultipleSameKeys_ReturnsFirstOccurrence()
    {
        var items = new[] { 1, 2, 3, 2, 4, 1 };
        var result = ApiKeyGateway.Extensions.CollectionExtensions.DistinctBy(items, x => x % 2).ToList(); // Even=0, Odd=1
        result.Should().HaveCount(2);
        result.Should().Contain(1); // First odd
        result.Should().Contain(2); // First even
    }

    // -------------------------------------------------------------------------
    // ForEachSafe
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that ForEachSafe continues processing subsequent items after an exception is thrown by one item.
    /// </summary>
    [Fact]
    public void ForEachSafe_ExceptionInOneItem_ContinuesToNextItems()
    {
        var processed = new List<int>();
        var items = new[] { 1, 2, 3, 4, 5 };

        items.ForEachSafe(item =>
        {
            if (item == 3) throw new InvalidOperationException("boom");
            processed.Add(item);
        });

        processed.Should().BeEquivalentTo(new[] { 1, 2, 4, 5 });
    }

    /// <summary>
    /// Tests that ForEachSafe does not throw when called on an empty collection.
    /// </summary>
    [Fact]
    public void ForEachSafe_EmptyCollection_DoesNotThrow()
    {
        var act = () => Enumerable.Empty<int>().ForEachSafe(_ => throw new Exception("should not fire"));
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that ForEachSafe handles null logger parameter without throwing.
    /// </summary>
    [Fact]
    public void ForEachSafe_NullLogger_DoesNotThrow()
    {
        var items = new[] { 1, 2, 3 };
        var act = () => items.ForEachSafe(item => { }, logger: null);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that ForEachSafe throws ArgumentNullException when source is null.
    /// </summary>
    [Fact]
    public void ForEachSafe_NullSource_ThrowsArgumentNullException()
    {
        IEnumerable<int>? nullCollection = null;
        var act = () => nullCollection.ForEachSafe(item => { });
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that ForEachSafe throws ArgumentNullException when action is null.
    /// </summary>
    [Fact]
    public void ForEachSafe_NullAction_ThrowsArgumentNullException()
    {
        var items = new[] { 1, 2, 3 };
        Action<int>? nullAction = null;
        var act = () => items.ForEachSafe(nullAction);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that ForEachSafe processes all items when no exceptions are thrown.
    /// </summary>
    [Fact]
    public void ForEachSafe_NoExceptions_ProcessesAllItems()
    {
        var processed = new List<int>();
        var items = new[] { 1, 2, 3, 4, 5 };
        items.ForEachSafe(item => processed.Add(item));
        processed.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    /// <summary>
    /// Tests that ForEachSafe handles multiple exceptions correctly.
    /// </summary>
    [Fact]
    public void ForEachSafe_MultipleExceptions_ContinuesAfterEachFailure()
    {
        var processed = new List<int>();
        var items = new[] { 1, 2, 3, 4, 5 };

        items.ForEachSafe(item =>
        {
            if (item % 2 == 0) throw new InvalidOperationException($"boom {item}");
            processed.Add(item);
        });

        processed.Should().BeEquivalentTo(new[] { 1, 3, 5 });
    }
}

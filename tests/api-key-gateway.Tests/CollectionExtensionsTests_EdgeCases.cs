// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// Additional edge case tests for CollectionExtensions to cover empty collections
// and boundary conditions as requested in the task.
// =============================================================================

using Xunit;
using ApiKeyGateway.Extensions;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Provides additional edge case unit tests for collection extension methods.
/// Focuses on empty collections, boundary conditions, and edge cases.
/// </summary>
public class CollectionExtensionsTests_EdgeCases
{
    // -------------------------------------------------------------------------
    // Paginate edge cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that Paginate throws ArgumentOutOfRangeException when pageSize is 0.
    /// </summary>
    [Fact]
    public void Paginate_ZeroPageSize_ThrowsArgumentOutOfRangeException()
    {
        var items = Enumerable.Range(1, 10);
        var act = () => items.Paginate(1, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that Paginate throws ArgumentOutOfRangeException when pageNumber is 0 and pageSize is 0.
    /// </summary>
    [Fact]
    public void Paginate_ZeroPageNumberAndSize_ThrowsArgumentOutOfRangeException()
    {
        var items = Enumerable.Range(1, 10);
        var act = () => items.Paginate(0, 0);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that Paginate returns empty when requesting page beyond collection.
    /// </summary>
    [Fact]
    public void Paginate_PageBeyondCollection_ReturnsEmpty()
    {
        var items = Enumerable.Range(1, 5);
        items.Paginate(100, 5).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that Paginate returns correct slice when pageNumber equals pageSize.
    /// </summary>
    [Fact]
    public void Paginate_PageNumberEqualsPageSize_ReturnsCorrectSlice()
    {
        var items = Enumerable.Range(1, 25);
        items.Paginate(5, 5).Should().BeEquivalentTo(new[] { 21, 22, 23, 24, 25 });
    }

    /// <summary>
    /// Tests that Paginate returns empty when pageNumber is larger than collection size.
    /// </summary>
    [Fact]
    public void Paginate_PageNumberLargerThanCollectionSize_ReturnsEmpty()
    {
        var items = new[] { 1, 2, 3 };
        items.Paginate(10, 5).Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // IsEmpty edge cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that IsEmpty returns false for a collection with only whitespace strings.
    /// </summary>
    [Fact]
    public void IsEmpty_CollectionWithOnlyWhitespaceStrings_ReturnsFalse()
    {
        new[] { " ", "\t", "\n" }.IsEmpty().Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsEmpty returns false for a collection with empty strings.
    /// </summary>
    [Fact]
    public void IsEmpty_CollectionWithEmptyStrings_ReturnsFalse()
    {
        new[] { "", "", "" }.IsEmpty().Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsEmpty returns false for a collection with null elements.
    /// </summary>
    [Fact]
    public void IsEmpty_CollectionWithNullElements_ReturnsFalse()
    {
        new string?[] { null, null }.IsEmpty().Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsEmpty returns true for a collection with a single whitespace string.
    /// </summary>
    [Fact]
    public void IsEmpty_SingleWhitespaceString_ReturnsFalse()
    {
        new[] { " " }.IsEmpty().Should().BeFalse();
    }

    /// <summary>
    /// Tests that IsEmpty returns true for a collection with a single empty string.
    /// </summary>
    [Fact]
    public void IsEmpty_SingleEmptyString_ReturnsFalse()
    {
        new[] { "" }.IsEmpty().Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // HasItems edge cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that HasItems returns true for a collection with only whitespace strings.
    /// </summary>
    [Fact]
    public void HasItems_CollectionWithOnlyWhitespaceStrings_ReturnsTrue()
    {
        new[] { " ", "\t", "\n" }.HasItems().Should().BeTrue();
    }

    /// <summary>
    /// Tests that HasItems returns true for a collection with empty strings.
    /// </summary>
    [Fact]
    public void HasItems_CollectionWithEmptyStrings_ReturnsTrue()
    {
        new[] { "", "", "" }.HasItems().Should().BeTrue();
    }

    /// <summary>
    /// Tests that HasItems returns true for a collection with null elements.
    /// </summary>
    [Fact]
    public void HasItems_CollectionWithNullElements_ReturnsTrue()
    {
        new string?[] { null, null }.HasItems().Should().BeTrue();
    }

    /// <summary>
    /// Tests that HasItems returns true for a single whitespace string.
    /// </summary>
    [Fact]
    public void HasItems_SingleWhitespaceString_ReturnsTrue()
    {
        new[] { " " }.HasItems().Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // CountBy edge cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that CountBy returns correct counts when all items have the same key.
    /// </summary>
    [Fact]
    public void CountBy_AllItemsSameKey_ReturnsSingleEntryWithCorrectCount()
    {
        var items = new[] { 1, 1, 1, 1, 1 };
        var result = items.CountBy(x => x);

        result.Should().HaveCount(1);
        result.Should().ContainKey(1).WhoseValue.Should().Be(5);
    }

    /// <summary>
    /// Tests that CountBy returns empty dictionary when called on empty collection.
    /// </summary>
    [Fact]
    public void CountBy_EmptyCollection_ReturnsEmptyDictionary()
    {
        var result = Enumerable.Empty<int>().CountBy(x => x);
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that CountBy works correctly with complex objects and key selectors.
    /// </summary>
    [Fact]
    public void CountBy_ComplexObjects_ReturnsCorrectCounts()
    {
        var items = new[]
        {
            new TestItem { Category = "A", Value = 10 },
            new TestItem { Category = "B", Value = 20 },
            new TestItem { Category = "A", Value = 30 },
            new TestItem { Category = "C", Value = 40 },
            new TestItem { Category = "B", Value = 50 }
        };

        var result = items.CountBy(x => x.Category);

        result.Should().HaveCount(3);
        result.Should().ContainKey("A").WhoseValue.Should().Be(2);
        result.Should().ContainKey("B").WhoseValue.Should().Be(2);
        result.Should().ContainKey("C").WhoseValue.Should().Be(1);
    }

    /// <summary>
    /// Tests that CountBy with null key selector throws ArgumentNullException.
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
    // Batch edge cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that Batch returns correct batches for a single item collection.
    /// </summary>
    [Fact]
    public void Batch_SingleItemCollection_ReturnsSingleBatch()
    {
        var items = new[] { 42 };
        var batches = items.Batch(1).Select(b => b.ToList()).ToList();

        batches.Should().HaveCount(1);
        batches[0].Should().BeEquivalentTo(new[] { 42 });
    }

    /// <summary>
    /// Tests that Batch returns correct batches when batch size equals collection size.
    /// </summary>
    [Fact]
    public void Batch_BatchSizeEqualsCollectionSize_ReturnsSingleBatch()
    {
        var items = new[] { 1, 2, 3 };
        var batches = items.Batch(3).Select(b => b.ToList()).ToList();

        batches.Should().HaveCount(1);
        batches[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    /// <summary>
    /// Tests that Batch returns no batches when batch size is larger than collection.
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
    // DistinctBy edge cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that DistinctBy works correctly with complex objects.
    /// </summary>
    [Fact]
    public void DistinctBy_ComplexObjects_ReturnsDistinctByKey()
    {
        var items = new[]
        {
            new TestItem { Category = "A", Value = 10 },
            new TestItem { Category = "B", Value = 20 },
            new TestItem { Category = "A", Value = 30 },
            new TestItem { Category = "C", Value = 40 },
            new TestItem { Category = "B", Value = 50 }
        };

        var result = ApiKeyGateway.Extensions.CollectionExtensions.DistinctBy(items, x => x.Category).ToList();

        result.Should().HaveCount(3);
        result.Should().Contain(x => x.Category == "A" && x.Value == 10);
        result.Should().Contain(x => x.Category == "B" && x.Value == 20);
        result.Should().Contain(x => x.Category == "C" && x.Value == 40);
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

    // -------------------------------------------------------------------------
    // ForEachSafe edge cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tests that ForEachSafe handles empty collection gracefully.
    /// </summary>
    [Fact]
    public void ForEachSafe_EmptyCollection_HandlesGracefully()
    {
        var processed = new List<int>();
        var act = () => processed.ForEachSafe(item => throw new Exception("should not fire"));
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that ForEachSafe handles null logger parameter without throwing.
    /// </summary>
    [Fact]
    public void ForEachSafe_NullLogger_HandlesGracefully()
    {
        var items = new[] { 1, 2, 3 };
        var act = () => items.ForEachSafe(item => { }, logger: null);
        act.Should().NotThrow();
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
    /// Test item class for testing complex scenarios.
    /// </summary>
    private class TestItem
    {
        public string Category { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
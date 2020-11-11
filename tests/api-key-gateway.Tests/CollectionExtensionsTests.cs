// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;
using ApiKeyGateway.Extensions;
using FluentAssertions;

namespace ApiKeyGateway.Tests;

public class CollectionExtensionsTests
{
    // -------------------------------------------------------------------------
    // Paginate
    // -------------------------------------------------------------------------

    [Fact]
    public void Paginate_FirstPage_ReturnsCorrectSlice()
    {
        var items = Enumerable.Range(1, 20);
        items.Paginate(1, 5).Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void Paginate_SecondPage_ReturnsCorrectSlice()
    {
        var items = Enumerable.Range(1, 20);
        items.Paginate(2, 5).Should().BeEquivalentTo(new[] { 6, 7, 8, 9, 10 });
    }

    [Fact]
    public void Paginate_BeyondLastPage_ReturnsEmpty()
    {
        var items = Enumerable.Range(1, 5);
        items.Paginate(10, 5).Should().BeEmpty();
    }

    [Fact]
    public void Paginate_EmptyCollection_ReturnsEmpty()
    {
        Enumerable.Empty<int>().Paginate(1, 10).Should().BeEmpty();
    }

    [Fact]
    public void Paginate_PageSizeLargerThanCollection_ReturnsAllItems()
    {
        var items = new[] { 1, 2, 3 };
        items.Paginate(1, 100).Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    // -------------------------------------------------------------------------
    // IsEmpty / HasItems
    // -------------------------------------------------------------------------

    [Fact]
    public void IsEmpty_NullCollection_ReturnsTrue()
    {
        IEnumerable<int>? nullCollection = null;
        nullCollection.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_EmptyCollection_ReturnsTrue()
    {
        Enumerable.Empty<int>().IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_NonEmptyCollection_ReturnsFalse()
    {
        new[] { 1 }.IsEmpty().Should().BeFalse();
    }

    [Fact]
    public void HasItems_NullCollection_ReturnsFalse()
    {
        IEnumerable<int>? nullCollection = null;
        nullCollection.HasItems().Should().BeFalse();
    }

    [Fact]
    public void HasItems_EmptyCollection_ReturnsFalse()
    {
        Enumerable.Empty<string>().HasItems().Should().BeFalse();
    }

    [Fact]
    public void HasItems_NonEmptyCollection_ReturnsTrue()
    {
        new[] { "a" }.HasItems().Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // CountBy
    // -------------------------------------------------------------------------

    [Fact]
    public void CountBy_GroupsAndCountsCorrectly()
    {
        var items = new[] { "a", "b", "a", "c", "b", "a" };
        var result = items.CountBy(x => x);

        result.Should().ContainKey("a").WhoseValue.Should().Be(3);
        result.Should().ContainKey("b").WhoseValue.Should().Be(2);
        result.Should().ContainKey("c").WhoseValue.Should().Be(1);
    }

    [Fact]
    public void CountBy_EmptyCollection_ReturnsEmptyDictionary()
    {
        var result = Enumerable.Empty<string>().CountBy(x => x);
        result.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // Batch
    // -------------------------------------------------------------------------

    [Fact]
    public void Batch_EvenlyDivisible_ReturnCorrectBatches()
    {
        var items = Enumerable.Range(1, 6);
        var batches = items.Batch(3).Select(b => b.ToList()).ToList();

        batches.Should().HaveCount(2);
        batches[0].Should().BeEquivalentTo(new[] { 1, 2, 3 });
        batches[1].Should().BeEquivalentTo(new[] { 4, 5, 6 });
    }

    [Fact]
    public void Batch_UnevenlyDivisible_LastBatchHasRemainder()
    {
        var items = Enumerable.Range(1, 7);
        var batches = items.Batch(3).Select(b => b.ToList()).ToList();

        batches.Should().HaveCount(3);
        batches[2].Should().BeEquivalentTo(new[] { 7 });
    }

    [Fact]
    public void Batch_EmptyCollection_ReturnsNoBatches()
    {
        var batches = Enumerable.Empty<int>().Batch(5).ToList();
        batches.Should().BeEmpty();
    }

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

    // -------------------------------------------------------------------------
    // DistinctBy
    // -------------------------------------------------------------------------

    [Fact]
    public void DistinctBy_DuplicateKeys_ReturnsFirstOccurrence()
    {
        var items = new[]
        {
            new { Name = "Alice", Age = 30 },
            new { Name = "Alice", Age = 25 },
            new { Name = "Bob", Age = 40 }
        };

        var result = items.DistinctBy(x => x.Name).ToList();

        result.Should().HaveCount(2);
        result.First(x => x.Name == "Alice").Age.Should().Be(30);
    }

    [Fact]
    public void DistinctBy_AllUnique_ReturnsAll()
    {
        var items = new[] { 1, 2, 3 };
        items.DistinctBy(x => x).Should().HaveCount(3);
    }

    // -------------------------------------------------------------------------
    // ForEachSafe
    // -------------------------------------------------------------------------

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

    [Fact]
    public void ForEachSafe_EmptyCollection_DoesNotThrow()
    {
        var act = () => Enumerable.Empty<int>().ForEachSafe(_ => throw new Exception("should not fire"));
        act.Should().NotThrow();
    }
}

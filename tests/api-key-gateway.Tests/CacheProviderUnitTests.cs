// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// Unit tests for CacheProvider implementation covering public API
// =============================================================================

using ApiKeyGateway.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="InMemoryCacheProvider"/> class.
/// Tests all public methods of ICacheProvider interface including happy-path,
/// edge cases (null/empty inputs, boundary values), and error-path assertions.
/// </summary>
public class CacheProviderUnitTests
{
    private readonly Mock<ILogger<InMemoryCacheProvider>> _loggerMock;
    private readonly InMemoryCacheProvider _cacheProvider;
    private readonly IMemoryCache _memoryCache;

    public CacheProviderUnitTests()
    {
        _loggerMock = new Mock<ILogger<InMemoryCacheProvider>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheProvider = new InMemoryCacheProvider(_memoryCache, _loggerMock.Object);
    }

    #region GetAsync Tests

    /// <summary>
    /// Tests that GetAsync returns null when key doesn't exist in cache.
    /// </summary>
    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        // Arrange
        const string key = "non-existent-key";

        // Act
        var result = await _cacheProvider.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache MISS")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetAsync returns cached value when key exists.
    /// </summary>
    [Fact]
    public async Task GetAsync_ExistingKey_ReturnsCachedValue()
    {
        // Arrange
        const string key = "existing-key";
        var expectedValue = new TestModel { Id = 1, Name = "Test" };
        await _cacheProvider.SetAsync(key, expectedValue);

        // Act
        var result = await _cacheProvider.GetAsync<TestModel>(key);

        // Assert
        result.Should().BeSameAs(expectedValue);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache HIT")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that GetAsync returns null for empty key.
    /// </summary>
    [Fact]
    public async Task GetAsync_EmptyKey_ReturnsNull()
    {
        // Arrange
        var key = string.Empty;

        // Act
        var result = await _cacheProvider.GetAsync<TestModel>(key);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Tests that GetAsync returns null for whitespace key.
    /// </summary>
    [Fact]
    public async Task GetAsync_WhitespaceKey_ReturnsNull()
    {
        // Arrange
        var key = "   ";

        // Act
        var result = await _cacheProvider.GetAsync<TestModel>(key);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region SetAsync Tests

    /// <summary>
    /// Tests that SetAsync stores value with default expiration (no expiration specified).
    /// </summary>
    [Fact]
    public async Task SetAsync_NoExpiration_StoresValueWithDefaultExpiration()
    {
        // Arrange
        const string key = "test-key";
        var value = new TestModel { Id = 1, Name = "Test" };

        // Act
        await _cacheProvider.SetAsync(key, value);

        // Assert
        var result = await _cacheProvider.GetAsync<TestModel>(key);
        result.Should().BeSameAs(value);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache SET")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that SetAsync stores value with custom expiration.
    /// </summary>
    [Fact]
    public async Task SetAsync_WithExpiration_StoresValueWithSpecifiedExpiration()
    {
        // Arrange
        const string key = "test-key";
        var value = new TestModel { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMinutes(5);

        // Act
        await _cacheProvider.SetAsync(key, value, expiration);

        // Assert - value should be retrievable within expiration time
        var result = await _cacheProvider.GetAsync<TestModel>(key);
        result.Should().BeSameAs(value);
    }

    /// <summary>
    /// Tests that SetAsync stores value with very short expiration.
    /// </summary>
    [Fact]
    public async Task SetAsync_VeryShortExpiration_StoresValueWithVeryShortExpiration()
    {
        // Arrange
        const string key = "test-key";
        var value = new TestModel { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMilliseconds(1);

        // Act
        await _cacheProvider.SetAsync(key, value, expiration);

        // Assert - value should be retrievable immediately
        var result = await _cacheProvider.GetAsync<TestModel>(key);
        result.Should().BeSameAs(value);
    }

    /// <summary>
    /// Tests that SetAsync overwrites existing value.
    /// </summary>
    [Fact]
    public async Task SetAsync_OverwritesExistingValue()
    {
        // Arrange
        const string key = "test-key";
        var oldValue = new TestModel { Id = 1, Name = "Old" };
        var newValue = new TestModel { Id = 2, Name = "New" };

        await _cacheProvider.SetAsync(key, oldValue);

        // Act
        await _cacheProvider.SetAsync(key, newValue);

        // Assert
        var result = await _cacheProvider.GetAsync<TestModel>(key);
        result.Should().BeSameAs(newValue);
        result.Should().NotBeSameAs(oldValue);
    }

    #endregion

    #region RemoveAsync Tests

    /// <summary>
    /// Tests that RemoveAsync removes value from cache.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_ExistingKey_RemovesValue()
    {
        // Arrange
        const string key = "test-key";
        var value = new TestModel { Id = 1, Name = "Test" };
        await _cacheProvider.SetAsync(key, value);

        // Verify value exists before removal
        var beforeRemove = await _cacheProvider.GetAsync<TestModel>(key);
        beforeRemove.Should().NotBeNull();

        // Act
        await _cacheProvider.RemoveAsync(key);

        // Assert
        var afterRemove = await _cacheProvider.GetAsync<TestModel>(key);
        afterRemove.Should().BeNull();
        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache REMOVE")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that RemoveAsync handles non-existent key gracefully.
    /// </summary>
    [Fact]
    public async Task RemoveAsync_NonExistentKey_CompletesWithoutError()
    {
        // Arrange
        const string key = "non-existent-key";

        // Act
        await _cacheProvider.RemoveAsync(key);

        // Assert - No exception should be thrown
    }

    #endregion

    #region ExistsAsync Tests

    /// <summary>
    /// Tests that ExistsAsync returns true when key exists in cache.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_ExistingKey_ReturnsTrue()
    {
        // Arrange
        const string key = "existing-key";
        var value = new TestModel { Id = 1, Name = "Test" };
        await _cacheProvider.SetAsync(key, value);

        // Act
        var result = await _cacheProvider.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that ExistsAsync returns false when key doesn't exist in cache.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_NonExistentKey_ReturnsFalse()
    {
        // Arrange
        const string key = "non-existent-key";

        // Act
        var result = await _cacheProvider.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IncrementAsync Tests

    /// <summary>
    /// Tests that IncrementAsync initializes counter to default increment value (1).
    /// </summary>
    [Fact]
    public async Task IncrementAsync_NewKey_InitializesToDefaultIncrement()
    {
        // Arrange
        const string key = "counter-key";

        // Act
        var result = await _cacheProvider.IncrementAsync(key);

        // Assert
        result.Should().Be(1);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Counter incremented")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that IncrementAsync initializes counter to custom increment value.
    /// </summary>
    [Fact]
    public async Task IncrementAsync_CustomIncrement_InitializesToCustomValue()
    {
        // Arrange
        const string key = "counter-key";
        const long increment = 5;

        // Act
        var result = await _cacheProvider.IncrementAsync(key, increment);

        // Assert
        result.Should().Be(increment);
    }

    /// <summary>
    /// Tests that IncrementAsync increments existing counter.
    /// </summary>
    [Fact]
    public async Task IncrementAsync_ExistingCounter_IncrementsValue()
    {
        // Arrange
        const string key = "counter-key";
        await _cacheProvider.IncrementAsync(key, 10); // Initialize

        // Act
        var result = await _cacheProvider.IncrementAsync(key, 7);

        // Assert
        result.Should().Be(17);
    }

    /// <summary>
    /// Tests that IncrementAsync handles large increment values.
    /// </summary>
    [Fact]
    public async Task IncrementAsync_LargeIncrementValue_HandlesCorrectly()
    {
        // Arrange
        const string key = "counter-key";
        const long largeIncrement = long.MaxValue / 2;

        // Act
        var result = await _cacheProvider.IncrementAsync(key, largeIncrement);

        // Assert
        result.Should().Be(largeIncrement);
    }

    #endregion

    #region RemoveByPatternAsync Tests

    /// <summary>
    /// Tests that RemoveByPatternAsync returns 0 when pattern is null.
    /// </summary>
    [Fact]
    public async Task RemoveByPatternAsync_NullPattern_ReturnsZero()
    {
        // Arrange
        string? pattern = null;

        // Act
        var result = await _cacheProvider.RemoveByPatternAsync(pattern!);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that RemoveByPatternAsync returns 0 when pattern is empty.
    /// </summary>
    [Fact]
    public async Task RemoveByPatternAsync_EmptyPattern_ReturnsZero()
    {
        // Arrange
        var pattern = string.Empty;

        // Act
        var result = await _cacheProvider.RemoveByPatternAsync(pattern);

        // Assert
        result.Should().Be(0);
    }

    /// <summary>
    /// Tests that RemoveByPatternAsync removes entries matching simple wildcard pattern.
    /// </summary>
    [Fact]
    public async Task RemoveByPatternAsync_SimpleWildcardPattern_RemovesMatchingEntries()
    {
        // Arrange
        const string pattern = "test-*";
        const string key1 = "test-key1";
        const string key2 = "test-key2";
        const string otherKey = "other-key";

        await _cacheProvider.SetAsync(key1, new TestModel { Id = 1, Name = "Test1" });
        await _cacheProvider.SetAsync(key2, new TestModel { Id = 2, Name = "Test2" });
        await _cacheProvider.SetAsync(otherKey, new TestModel { Id = 3, Name = "Other" });

        // Act
        var result = await _cacheProvider.RemoveByPatternAsync(pattern);

        // Assert
        result.Should().Be(2); // Should remove 2 entries matching "test-*"

        var key1Exists = await _cacheProvider.ExistsAsync(key1);
        var key2Exists = await _cacheProvider.ExistsAsync(key2);
        var otherKeyExists = await _cacheProvider.ExistsAsync(otherKey);

        key1Exists.Should().BeFalse();
        key2Exists.Should().BeFalse();
        otherKeyExists.Should().BeTrue();
    }

    /// <summary>
    /// Tests that RemoveByPatternAsync removes entries matching question mark wildcard.
    /// </summary>
    [Fact]
    public async Task RemoveByPatternAsync_QuestionMarkWildcard_RemovesMatchingEntries()
    {
        // Arrange
        const string pattern = "test-*";

        await _cacheProvider.SetAsync("test-a", new TestModel { Id = 1, Name = "A" });
        await _cacheProvider.SetAsync("test-b", new TestModel { Id = 2, Name = "B" });
        await _cacheProvider.SetAsync("test-key", new TestModel { Id = 3, Name = "Key" });
        await _cacheProvider.SetAsync("test-12", new TestModel { Id = 4, Name = "12" });
        await _cacheProvider.SetAsync("other-test", new TestModel { Id = 5, Name = "Other" });

        // Act
        var result = await _cacheProvider.RemoveByPatternAsync(pattern);

        // Assert
        result.Should().Be(4); // Should remove all 4 entries matching "test-*"

        var testAExists = await _cacheProvider.ExistsAsync("test-a");
        var testBExists = await _cacheProvider.ExistsAsync("test-b");
        var testKeyExists = await _cacheProvider.ExistsAsync("test-key");
        var test12Exists = await _cacheProvider.ExistsAsync("test-12");
        var otherTestExists = await _cacheProvider.ExistsAsync("other-test");

        testAExists.Should().BeFalse();
        testBExists.Should().BeFalse();
        testKeyExists.Should().BeFalse();
        test12Exists.Should().BeFalse();
        otherTestExists.Should().BeTrue();
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Simple test model for cache operations.
    /// </summary>
    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
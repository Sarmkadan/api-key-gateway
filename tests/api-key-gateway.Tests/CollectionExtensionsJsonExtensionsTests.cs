using System;
using System.Collections.Generic;
using System.Text.Json;
using ApiKeyGateway.Extensions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Tests for the <see cref="CollectionExtensionsJsonExtensions"/> class.
/// Contains test cases for JSON serialization and deserialization extension methods.
/// </summary>
public class CollectionExtensionsJsonExtensionsTests
{
    /// <summary>
    /// Returns a string representation of the test class using a sample <see cref="TestItem"/>.
    /// </summary>
    /// <returns>A formatted string showing the class name and sample item properties.</returns>
    public override string ToString()
    {
        var sample = new TestItem();
        return $"CollectionExtensionsJsonExtensionsTests {{ Id = {sample.Id}, Name = {sample.Name} }}";
    }

    private class TestItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    /// <summary>
    /// Tests for the <see cref="CollectionExtensionsJsonExtensions.ToJson{T}(IEnumerable{T}, bool)"/> method.
    /// </summary>
    public class ToJson
    {
        /// <summary>
        /// Verifies that converting a non-empty collection to JSON produces a valid JSON string containing all items.
        /// </summary>
        [Fact]
        public void ToJson_WithNonEmptyCollection_ReturnsValidJsonString()
        {
            // Arrange
            var collection = new List<string> { "item1", "item2", "item3" };

            // Act
            var result = collection.ToJson();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("item1");
            result.Should().Contain("item2");
            result.Should().Contain("item3");
        }

        /// <summary>
        /// Verifies that converting an empty collection to JSON produces an empty JSON array.
        /// </summary>
        [Fact]
        public void ToJson_WithEmptyCollection_ReturnsEmptyArrayJson()
        {
            // Arrange
            var collection = new List<string>();

            // Act
            var result = collection.ToJson();

            // Assert
            result.Should().Be("[]");
        }

        /// <summary>
        /// Verifies that converting a collection to JSON with indentation produces formatted JSON containing newlines.
        /// </summary>
        [Fact]
        public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
        {
            // Arrange
            var collection = new List<int> { 1, 2, 3 };

            // Act
            var result = collection.ToJson(indented: true);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("[");
            result.Should().Contain("]");
            result.Should().Contain("1");
            result.Should().Contain("2");
            result.Should().Contain("3");
            // Should have newlines and indentation
            result.Should().Contain("\n");
        }

        /// <summary>
        /// Verifies that converting a null collection throws an <see cref="ArgumentNullException"/>.
        /// </summary>
        [Fact]
        public void ToJson_WithNullCollection_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<string>? collection = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection!.ToJson());
        }

        /// <summary>
        /// Verifies that converting a collection of complex objects serializes correctly with camelCase property names.
        /// </summary>
        [Fact]
        public void ToJson_WithComplexObjectCollection_SerializesCorrectly()
        {
            // Arrange
            var collection = new List<TestItem>
            {
                new TestItem { Id = 1, Name = "First" },
                new TestItem { Id = 2, Name = "Second" }
            };

            // Act
            var result = collection.ToJson();

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("1");
            result.Should().Contain("First");
            result.Should().Contain("2");
            result.Should().Contain("Second");
            // Should use camelCase
            result.Should().Contain("id");
            result.Should().Contain("name");
        }
    }

    /// <summary>
    /// Tests for the <see cref="CollectionExtensionsJsonExtensions.FromJson{T}(string)"/> method.
    /// </summary>
    public class FromJson
    {
        /// <summary>
        /// Verifies that deserializing a valid JSON array produces a collection equivalent to the expected values.
        /// </summary>
        [Fact]
        public void FromJson_WithValidJson_ReturnsDeserializedCollection()
        {
            // Arrange
            var json = "[\"item1\",\"item2\",\"item3\"]";

            // Act
            var result = CollectionExtensionsJsonExtensions.FromJson<string>(json);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new List<string> { "item1", "item2", "item3" });
        }

        /// <summary>
        /// Verifies that deserializing an empty JSON array produces an empty collection.
        /// </summary>
        [Fact]
        public void FromJson_WithEmptyArray_ReturnsEmptyCollection()
        {
            // Arrange
            var json = "[]";

            // Act
            var result = CollectionExtensionsJsonExtensions.FromJson<string>(json);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Verifies that deserializing null, empty, or whitespace-only JSON returns null.
        /// </summary>
        [Fact]
        public void FromJson_WithNullOrWhitespaceJson_ReturnsNull()
        {
            // Arrange & Act
            var result1 = CollectionExtensionsJsonExtensions.FromJson<string>((string?)null);
            var result2 = CollectionExtensionsJsonExtensions.FromJson<string>("");
            var result3 = CollectionExtensionsJsonExtensions.FromJson<string>("   ");

            // Assert
            result1.Should().BeNull();
            result2.Should().BeNull();
            result3.Should().BeNull();
        }

        /// <summary>
        /// Verifies that deserializing invalid JSON throws a <see cref="JsonException"/>.
        /// </summary>
        [Fact]
        public void FromJson_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = "invalid json";

            // Act & Assert
            Assert.Throws<JsonException>(() => CollectionExtensionsJsonExtensions.FromJson<string>(json));
        }

        /// <summary>
        /// Verifies that deserializing a JSON array of complex objects produces the correct collection with expected property values.
        /// </summary>
        [Fact]
        public void FromJson_WithComplexObjectCollection_DeserializesCorrectly()
        {
            // Arrange
            var json = "[{\"id\":1,\"name\":\"First\"},{\"id\":2,\"name\":\"Second\"}]";

            // Act
            var result = CollectionExtensionsJsonExtensions.FromJson<TestItem>(json);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainSingle(x => x.Id == 1 && x.Name == "First");
            result.Should().ContainSingle(x => x.Id == 2 && x.Name == "Second");
        }

        /// <summary>
        /// Verifies that deserializing an empty string returns null.
        /// </summary>
        [Fact]
        public void FromJson_WithEmptyString_ReturnsNull()
        {
            // Arrange
            var json = "";

            // Act
            var result = CollectionExtensionsJsonExtensions.FromJson<int>(json);

            // Assert
            result.Should().BeNull();
        }
    }

    /// <summary>
    /// Tests for the <see cref="CollectionExtensionsJsonExtensions.TryFromJson{T}(string, out IEnumerable{T}?)"/> method.
    /// </summary>
    public class TryFromJson
    {
        /// <summary>
        /// Verifies that trying to deserialize a valid JSON array returns true and produces the expected collection.
        /// </summary>
        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializesCollection()
        {
            // Arrange
            var json = "[1,2,3]";
            IEnumerable<int>? result = null;

            // Act
            var success = CollectionExtensionsJsonExtensions.TryFromJson(json, out result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new List<int> { 1, 2, 3 });
        }

        /// <summary>
        /// Verifies that trying to deserialize an empty JSON array returns true and produces an empty collection.
        /// </summary>
        [Fact]
        public void TryFromJson_WithEmptyArray_ReturnsTrueAndEmptyCollection()
        {
            // Arrange
            var json = "[]";
            IEnumerable<string>? result = null;

            // Act
            var success = CollectionExtensionsJsonExtensions.TryFromJson(json, out result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Verifies that trying to deserialize null JSON throws an <see cref="ArgumentNullException"/>.
        /// </summary>
        [Fact]
        public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;
            IEnumerable<string>? result = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CollectionExtensionsJsonExtensions.TryFromJson(json!, out result));
        }

        /// <summary>
        /// Verifies that trying to deserialize whitespace-only JSON returns true and produces null.
        /// </summary>
        [Fact]
        public void TryFromJson_WithWhitespaceJson_ReturnsTrueAndNull()
        {
            // Arrange
            var json = "   ";
            IEnumerable<string>? result = null;

            // Act
            var success = CollectionExtensionsJsonExtensions.TryFromJson(json, out result);

            // Assert
            success.Should().BeTrue();
            result.Should().BeNull();
        }

        /// <summary>
        /// Verifies that trying to deserialize an empty string returns true and produces null.
        /// </summary>
        [Fact]
        public void TryFromJson_WithEmptyString_ReturnsTrueAndNull()
        {
            // Arrange
            var json = "";
            IEnumerable<int>? result = null;

            // Act
            var success = CollectionExtensionsJsonExtensions.TryFromJson(json, out result);

            // Assert
            success.Should().BeTrue();
            result.Should().BeNull();
        }

        /// <summary>
        /// Verifies that trying to deserialize invalid JSON returns false and produces null.
        /// </summary>
        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "invalid json";
            IEnumerable<string>? result = null;

            // Act
            var success = CollectionExtensionsJsonExtensions.TryFromJson(json, out result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        /// <summary>
        /// Verifies that trying to deserialize a JSON array of complex objects returns true and produces the correct collection.
        /// </summary>
        [Fact]
        public void TryFromJson_WithComplexObjectCollection_DeserializesCorrectly()
        {
            // Arrange
            var json = "[{\"id\":1,\"name\":\"First\"},{\"id\":2,\"name\":\"Second\"}]";
            IEnumerable<TestItem>? result = null;

            // Act
            var success = CollectionExtensionsJsonExtensions.TryFromJson(json, out result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainSingle(x => x.Id == 1 && x.Name == "First");
            result.Should().ContainSingle(x => x.Id == 2 && x.Name == "Second");
        }

        /// <summary>
        /// Verifies that trying to deserialize a JSON array of strings works correctly with mixed valid types.
        /// </summary>
        [Fact]
        public void TryFromJson_WithMixedValidTypes_WorksCorrectly()
        {
            // Arrange
            var json = "[\"a\",\"b\",\"c\"]";
            IEnumerable<string>? result = null;

            // Act
            var success = CollectionExtensionsJsonExtensions.TryFromJson(json, out result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new List<string> { "a", "b", "c" });
        }
    }
}
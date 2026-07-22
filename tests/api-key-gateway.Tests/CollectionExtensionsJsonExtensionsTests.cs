using System;
using System.Collections.Generic;
using System.Text.Json;
using ApiKeyGateway.Extensions;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

public class CollectionExtensionsJsonExtensionsTests
{
    private class TestItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class ToJson
    {
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

        [Fact]
        public void ToJson_WithNullCollection_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<string>? collection = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => collection!.ToJson());
        }

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

    public class FromJson
    {
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

        [Fact]
        public void FromJson_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = "invalid json";

            // Act & Assert
            Assert.Throws<JsonException>(() => CollectionExtensionsJsonExtensions.FromJson<string>(json));
        }

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

    public class TryFromJson
    {
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

        [Fact]
        public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;
            IEnumerable<string>? result = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CollectionExtensionsJsonExtensions.TryFromJson(json!, out result));
        }

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

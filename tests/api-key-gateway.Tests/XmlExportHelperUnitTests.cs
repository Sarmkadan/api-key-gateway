// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Xml;
using ApiKeyGateway.Utilities;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="XmlExportHelper"/> class.
/// Tests XML export functionality including proper escaping of special characters,
/// handling of empty collections, and well-formed XML output.
/// </summary>
public class XmlExportHelperUnitTests
{
    /// <summary>
    /// Simple test model for XML export testing.
    /// </summary>
    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>
    /// Tests that ToXml returns well-formed XML for a single object.
    /// </summary>
    [Fact]
    public void ToXml_SingleObject_ReturnsWellFormedXml()
    {
        // Arrange
        var item = new TestModel
        {
            Id = 1,
            Name = "TestItem",
            Description = "A test description",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 15, 10, 30, 0),
            UpdatedAt = new DateTimeOffset(2026, 1, 16, 14, 45, 30, TimeSpan.Zero),
            Price = 99.99m
        };

        // Act
        var result = XmlExportHelper.ToXml(item);

        // Assert
        result.Should().NotBeEmpty();

        // Verify it's well-formed XML
        var doc = new XmlDocument();
        Action act = () => doc.LoadXml(result);
        act.Should().NotThrow("XML should be well-formed");

        // Verify root element name matches type name
        doc.DocumentElement.Should().NotBeNull();
        doc.DocumentElement.Name.Should().Be("TestModel");

        // Verify content
        result.Should().Contain("<Id>1</Id>");
        result.Should().Contain("<Name>TestItem</Name>");
        result.Should().Contain("<Description>A test description</Description>");
        result.Should().Contain("<IsActive>true</IsActive>");
        result.Should().Contain("<CreatedAt>2026-01-15T10:30:00.0000000</CreatedAt>");
        result.Should().Contain("<UpdatedAt>2026-01-16T14:45:30.0000000</UpdatedAt>");
        result.Should().Contain("<Price>99.99</Price>");
    }

    /// <summary>
    /// Tests that ToXml with custom root element name uses the provided name.
    /// </summary>
    [Fact]
    public void ToXml_SingleObjectWithCustomRootName_UsesCustomName()
    {
        // Arrange
        var item = new TestModel { Id = 1, Name = "Test" };

        // Act
        var result = XmlExportHelper.ToXml(item, "CustomRoot");

        // Assert
        result.Should().NotBeEmpty();

        var doc = new XmlDocument();
        Action act = () => doc.LoadXml(result);
        act.Should().NotThrow();

        doc.DocumentElement.Should().NotBeNull();
        doc.DocumentElement.Name.Should().Be("CustomRoot");
    }

    /// <summary>
    /// Tests that ToXml handles null input by returning empty string.
    /// </summary>
    [Fact]
    public void ToXml_NullInput_ReturnsEmptyString()
    {
        // Act
        var result = XmlExportHelper.ToXml<object>(null!);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that ToXml handles empty collection by returning well-formed XML with empty root.
    /// </summary>
    [Fact]
    public void ToXml_EmptyCollection_ReturnsWellFormedXmlWithEmptyRoot()
    {
        // Arrange
        var items = new List<TestModel>();

        // Act
        var result = XmlExportHelper.ToXml(items);

        // Assert
        result.Should().NotBeEmpty();

        var doc = new XmlDocument();
        Action act = () => doc.LoadXml(result);
        act.Should().NotThrow("XML should be well-formed even with empty collection");

        doc.DocumentElement.Should().NotBeNull();
        doc.DocumentElement.Name.Should().Be("root");
        doc.DocumentElement.ChildNodes.Count.Should().Be(0, "Empty collection should have no child nodes");
    }

    /// <summary>
    /// Tests that ToXml handles collection with items correctly.
    /// </summary>
    [Fact]
    public void ToXml_CollectionWithItems_ReturnsWellFormedXmlWithItems()
    {
        // Arrange
        var items = new List<TestModel>
        {
            new TestModel { Id = 1, Name = "Item1", Description = "First", IsActive = true, CreatedAt = new DateTime(2026, 1, 1) },
            new TestModel { Id = 2, Name = "Item2", Description = "Second", IsActive = false, CreatedAt = new DateTime(2026, 1, 2) }
        };

        // Act
        var result = XmlExportHelper.ToXml(items);

        // Assert
        result.Should().NotBeEmpty();

        var doc = new XmlDocument();
        Action act = () => doc.LoadXml(result);
        act.Should().NotThrow();

        var root = doc.DocumentElement;
        root.Should().NotBeNull();
        root.Name.Should().Be("root");
        root.ChildNodes.Count.Should().Be(2, "Should have 2 child nodes (TestModel elements)");

        // Verify first item
        var firstItem = root.ChildNodes[0] as XmlElement;
        firstItem.Should().NotBeNull();
        firstItem.Name.Should().Be("TestModel");
        firstItem["Id"].Should().NotBeNull();
        firstItem["Id"].InnerText.Should().Be("1");

        // Verify second item
        var secondItem = root.ChildNodes[1] as XmlElement;
        secondItem.Should().NotBeNull();
        secondItem.Name.Should().Be("TestModel");
        secondItem["Id"].Should().NotBeNull();
        secondItem["Id"].InnerText.Should().Be("2");
    }

    /// <summary>
    /// Tests that ToXml handles custom root and item names for collections.
    /// </summary>
    [Fact]
    public void ToXml_CollectionWithCustomNames_UsesCustomNames()
    {
        // Arrange
        var items = new List<TestModel> { new TestModel { Id = 1, Name = "Test" } };

        // Act
        var result = XmlExportHelper.ToXml(items, "ItemsCollection", "CustomItem");

        // Assert
        result.Should().NotBeEmpty();

        var doc = new XmlDocument();
        Action act = () => doc.LoadXml(result);
        act.Should().NotThrow();

        doc.DocumentElement.Should().NotBeNull();
        doc.DocumentElement.Name.Should().Be("ItemsCollection");

        var item = doc.DocumentElement.FirstChild as XmlElement;
        item.Should().NotBeNull();
        item.Name.Should().Be("CustomItem");
    }

    /// <summary>
    /// Tests that ToXml properly escapes special XML characters in property values.
    /// </summary>
    [Fact]
    public void ToXml_SpecialCharacters_AreProperlyEscaped()
    {
        // Arrange
        var item = new TestModel
        {
            Id = 1,
            Name = "Test & Item <with> special \"chars\"",
            Description = "Line1\nLine2\r\nLine3",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 15)
        };

        // Act
        var result = XmlExportHelper.ToXml(item);

        // Assert
        result.Should().NotBeEmpty();

        // Verify XML is well-formed (this would throw if not properly escaped)
        var doc = new XmlDocument();
        Action act = () => doc.LoadXml(result);
        act.Should().NotThrow("XML should be well-formed even with special characters");

        // Verify the content is preserved correctly
        var nameElement = doc.SelectSingleNode("//Name") as XmlElement;
        nameElement.Should().NotBeNull();
        nameElement.InnerText.Should().Be("Test & Item <with> special \"chars\"");

        var descElement = doc.SelectSingleNode("//Description") as XmlElement;
        descElement.Should().NotBeNull();
        descElement.InnerText.Should().Be("Line1\nLine2\r\nLine3");
    }

    /// <summary>
    /// Tests that ToXml handles property names with special characters by converting them to valid XML names.
    /// </summary>
    [Fact]
    public void ToXml_PropertyNamesWithSpecialChars_AreConvertedToValidXmlNames()
    {
        // Arrange - Create a model with property names that contain special characters
        var item = new TestSpecialPropertyNames
        {
            NormalProperty = "value1",
            Property_With_Underscores = "value2",
            Property_With_Dots = "value3",
            Property_With_Dashes = "value4",
            Property_With_Symbols = "value5"
        };

        // Act
        var result = XmlExportHelper.ToXml(item);

        // Assert
        result.Should().NotBeEmpty();

        var doc = new XmlDocument();
        Action act = () => doc.LoadXml(result);
        act.Should().NotThrow();

        // Verify all property names are converted to valid XML element names
        var root = doc.DocumentElement;
        root.Should().NotBeNull();

        // Check that we have elements for all properties
        root["NormalProperty"].Should().NotBeNull();
        root["Property_With_Underscores"].Should().NotBeNull();
        root["Property_With_Dots"].Should().NotBeNull();
        root["Property_With_Dashes"].Should().NotBeNull();
        root["Property_With_Symbols"].Should().NotBeNull();
    }

    /// <summary>
    /// Test class for property name escaping tests.
    /// </summary>
    private class TestSpecialPropertyNames
    {
        public string NormalProperty { get; set; } = string.Empty;
        public string Property_With_Underscores { get; set; } = string.Empty;
        public string Property_With_Dots { get; set; } = string.Empty;
        public string Property_With_Dashes { get; set; } = string.Empty;
        public string Property_With_Symbols { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tests that ToXml handles null property values correctly.
    /// </summary>
    [Fact]
    public void ToXml_NullPropertyValues_AreHandledCorrectly()
    {
        // Arrange
        var item = new TestModel
        {
            Id = 1,
            Name = "Test",
            Description = null,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 15)
        };

        // Act
        var result = XmlExportHelper.ToXml(item);

        // Assert
        result.Should().NotBeEmpty();

        var doc = new XmlDocument();
        Action act = () => doc.LoadXml(result);
        act.Should().NotThrow();

        // Null description should not create an element
        var descElement = doc.SelectSingleNode("//Description");
        descElement.Should().BeNull("Null properties should not create XML elements");

        // But other properties should be present
        result.Should().Contain("<Id>1</Id>");
        result.Should().Contain("<Name>Test</Name>");
        result.Should().Contain("<IsActive>true</IsActive>");
    }

    /// <summary>
    /// Tests that ToXml handles various data types with invariant culture formatting.
    /// </summary>
    [Fact]
    public void ToXml_VariousDataTypes_UsesInvariantCulture()
    {
        // Arrange
        var item = new TestModel
        {
            Id = 42,
            Name = "Test",
            Description = null,
            IsActive = true,
            CreatedAt = new DateTime(2026, 7, 21, 14, 30, 45),
            UpdatedAt = new DateTimeOffset(2026, 7, 21, 14, 30, 45, TimeSpan.FromHours(2)),
            Price = 1234.56m
        };

        // Act
        var result = XmlExportHelper.ToXml(item);

        // Assert
        result.Should().NotBeEmpty();

        // Should use invariant culture format for DateTime (round-trip format)
        result.Should().Contain("<CreatedAt>2026-07-21T14:30:45.0000000</CreatedAt>");

        // Should use invariant culture format for DateTimeOffset
        result.Should().Contain("<UpdatedAt>2026-07-21T14:30:45.0000000+02:00</UpdatedAt>");

        // Should format decimal correctly
        result.Should().Contain("<Price>1234.56</Price>");

        // Should not contain culture-specific formats
        result.Should().NotContain("21.07.2026"); // German format
        result.Should().NotContain("07/21/2026"); // US format
        result.Should().NotContain("21-07-2026"); // Other formats
    }

    /// <summary>
    /// Tests that ToXml handles nested objects by stringifying them.
    /// </summary>
    [Fact]
    public void ToXml_NestedObjects_AreStringified()
    {
        // Arrange
        var item = new TestModel
        {
            Id = 1,
            Name = "Test",
            Description = "Parent",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 15)
        };

        // Add a nested object property (will be stringified by ToString())
        var wrapper = new { Inner = item, InnerText = "wrapped" };

        // Act
        var result = XmlExportHelper.ToXml(wrapper);

        // Assert
        result.Should().NotBeEmpty();

        var doc = new XmlDocument();
        Action act = () => doc.LoadXml(result);
        act.Should().NotThrow();

        // Should have elements for both properties
        doc["wrapper"].Should().NotBeNull();
        doc["wrapper"]?["Inner"].Should().NotBeNull();
        doc["wrapper"]?["InnerText"].Should().NotBeNull();
    }

    /// <summary>
    /// Tests that ToXml handles collections of strings.
    /// </summary>
    [Fact]
    public void ToXml_CollectionOfStrings_ReturnsWellFormedXml()
    {
        // Arrange
        var strings = new List<string> { "first", "second", "third" };

        // Act
        var result = XmlExportHelper.ToXml(strings, "Strings", "String");

        // Assert
        result.Should().NotBeEmpty();

        var doc = new XmlDocument();
        Action act = () => doc.LoadXml(result);
        act.Should().NotThrow();

        var root = doc.DocumentElement;
        root.Should().NotBeNull();
        root.Name.Should().Be("Strings");
        root.ChildNodes.Count.Should().Be(3);

        var firstElement = root.ChildNodes[0] as XmlElement;
        firstElement.Should().NotBeNull();
        firstElement.Name.Should().Be("String");
        firstElement.InnerText.Should().Be("first");

        var secondElement = root.ChildNodes[1] as XmlElement;
        secondElement.Should().NotBeNull();
        secondElement.Name.Should().Be("String");
        secondElement.InnerText.Should().Be("second");

        var thirdElement = root.ChildNodes[2] as XmlElement;
        thirdElement.Should().NotBeNull();
        thirdElement.Name.Should().Be("String");
        thirdElement.InnerText.Should().Be("third");
    }

    /// <summary>
    /// Tests that ToXml produces UTF-8 encoded output.
    /// </summary>
    [Fact]
    public void ToXml_Output_IsUtf8Encoded()
    {
        // Arrange
        var item = new TestModel
        {
            Id = 1,
            Name = "Test",
            Description = "Description with unicode: 你好, مرحبا, Привет, 🎉",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 15)
        };

        // Act
        var result = XmlExportHelper.ToXml(item);

        // Assert
        result.Should().NotBeEmpty();

        // Verify UTF-8 encoding declaration or well-formed XML
        var lines = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        lines[0].Trim().Should().StartWith("<?xml version=")
            .And.Contain("encoding=")
            .And.Match("*utf-8*");
        result.Should().Contain("<Description>");

        // Verify unicode characters are preserved
        result.Should().Contain("你好")
            .And.Contain("مرحبا")
            .And.Contain("Привет")
            .And.Contain("🎉");
    }

    /// <summary>
    /// Tests that ToXml produces indented (pretty-printed) XML.
    /// </summary>
    [Fact]
    public void ToXml_Output_IsIndented()
    {
        // Arrange
        var item = new TestModel { Id = 1, Name = "Test", Description = "Test description" };

        // Act
        var result = XmlExportHelper.ToXml(item);

        // Assert
        result.Should().NotBeEmpty();

        // Verify indentation - should have newlines and indentation
        var lines = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().BeGreaterThan(1, "Should have multiple lines when indented");

        // First line should be XML declaration
        lines[0].Trim().Should().StartWith("<?xml version=");

        // Last line should be closing tag
        lines[^1].Trim().Should().Be("</TestModel>");
    }
}

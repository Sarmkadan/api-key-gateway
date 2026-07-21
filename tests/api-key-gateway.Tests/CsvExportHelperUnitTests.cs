// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ApiKeyGateway.Utilities;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Contains unit tests for the <see cref="CsvExportHelper"/> class.
/// Tests CSV export functionality including proper escaping, headers, and async streaming.
/// </summary>
public class CsvExportHelperUnitTests
{
    /// <summary>
    /// Simple test model for CSV export testing.
    /// </summary>
    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Tests that ToCsv returns empty string for null input.
    /// </summary>
    [Fact]
    public void ToCsv_NullInput_ReturnsEmptyString()
    {
        // Act
        var result = CsvExportHelper.ToCsv<TestModel>(null!);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that ToCsv returns empty string for empty collection.
    /// </>
    [Fact]
    public void ToCsv_EmptyCollection_ReturnsEmptyString()
    {
        // Arrange
        var items = new List<TestModel>();

        // Act
        var result = CsvExportHelper.ToCsv(items);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that ToCsv generates correct CSV with headers for simple data.
    /// </summary>
    [Fact]
    public void ToCsv_SimpleData_IncludesHeadersAndValues()
    {
        // Arrange
        var items = new List<TestModel>
        {
            new TestModel { Id = 1, Name = "Test", Description = "A test item", IsActive = true, CreatedAt = new DateTime(2026, 1, 15, 10, 30, 0) },
            new TestModel { Id = 2, Name = "Another", Description = null, IsActive = false, CreatedAt = new DateTime(2026, 1, 16, 14, 45, 30) }
        };

        // Act
        var result = CsvExportHelper.ToCsv(items);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("Id,Name,Description,IsActive,CreatedAt");
        result.Should().Contain("1,Test,A test item,True,2026-01-15T10:30:00.0000000");
        result.Should().Contain("2,Another,,False,2026-01-16T14:45:30.0000000");
    }

    /// <summary>
    /// Tests that ToCsv excludes headers when includeHeaders is false.
    /// </summary>
    [Fact]
    public void ToCsv_IncludeHeadersFalse_ExcludesHeaders()
    {
        // Arrange
        var items = new List<TestModel>
        {
            new TestModel { Id = 1, Name = "Test", Description = "A test", IsActive = true, CreatedAt = new DateTime(2026, 1, 15) }
        };

        // Act
        var result = CsvExportHelper.ToCsv(items, includeHeaders: false);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().NotContain("Id,Name,Description,IsActive,CreatedAt");
        result.Should().Contain("1,Test,A test,True,2026-01-15T00:00:00.0000000");
    }

    /// <summary>
    /// Tests that ToCsv properly escapes values containing commas.
    /// </summary>
    [Fact]
    public void ToCsv_ValuesWithCommas_AreProperlyQuoted()
    {
        // Arrange
        var items = new List<TestModel>
        {
            new TestModel { Id = 1, Name = "Test, with comma", Description = "Normal", IsActive = true, CreatedAt = new DateTime(2026, 1, 15) }
        };

        // Act
        var result = CsvExportHelper.ToCsv(items);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("\"Test, with comma\"");
        result.Should().Contain("1,\"Test, with comma\",Normal,True");
    }

    /// <summary>
    /// Tests that ToCsv properly escapes values containing quotes.
    /// </summary>
    [Fact]
    public void ToCsv_ValuesWithQuotes_AreProperlyEscaped()
    {
        // Arrange
        var items = new List<TestModel>
        {
            new TestModel { Id = 1, Name = "Test \"quoted\" value", Description = "Normal", IsActive = false, CreatedAt = new DateTime(2026, 1, 15) }
        };

        // Act
        var result = CsvExportHelper.ToCsv(items);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("\"Test \"\"quoted\"\" value\"");
    }

    /// <summary>
    /// Tests that ToCsv properly escapes values containing newlines.
    /// </summary>
    [Fact]
    public void ToCsv_ValuesWithNewlines_AreProperlyQuoted()
    {
        // Arrange
        var items = new List<TestModel>
        {
            new TestModel { Id = 1, Name = "Line1\nLine2", Description = "Normal", IsActive = true, CreatedAt = new DateTime(2026, 1, 15) }
        };

        // Act
        var result = CsvExportHelper.ToCsv(items);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("\"Line1\nLine2\"");
    }

    /// <summary>
    /// Tests that ToCsv handles various data types correctly using invariant culture.
    /// </summary>
    [Fact]
    public void ToCsv_VariousDataTypes_UsesInvariantCulture()
    {
        // Arrange
        var items = new List<TestModel>
        {
            new TestModel { Id = 42, Name = "Test", Description = null, IsActive = true, CreatedAt = new DateTime(2026, 7, 21, 14, 30, 45) }
        };

        // Act
        var result = CsvExportHelper.ToCsv(items);

        // Assert
        result.Should().NotBeEmpty();
        // Should use invariant culture format for DateTime (round-trip format)
        result.Should().Contain("42,Test,,True,2026-07-21T14:30:45.0000000");
        // Should not contain culture-specific formats
        result.Should().NotContain("21.07.2026"); // German format
        result.Should().NotContain("07/21/2026"); // US format
    }

    /// <summary>
    /// Tests that ExportToCsvAsync correctly writes CSV data to stream.
    /// </summary>
    [Fact]
    public async Task ExportToCsvAsync_SimpleData_WritesCorrectCsvToStream()
    {
        // Arrange
        var items = new List<TestModel>
        {
            new TestModel { Id = 1, Name = "Test", Description = "A test", IsActive = true, CreatedAt = new DateTime(2026, 1, 15) },
            new TestModel { Id = 2, Name = "Another", Description = null, IsActive = false, CreatedAt = new DateTime(2026, 1, 16) }
        };

        await using var outputStream = new MemoryStream();

        // Act
        await CsvExportHelper.ExportToCsvAsync(items.ToAsyncEnumerable(), outputStream);
        outputStream.Position = 0; // Reset position for reading

        var result = await new StreamReader(outputStream, Encoding.UTF8).ReadToEndAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("Id,Name,Description,IsActive,CreatedAt");
        result.Should().Contain("1,Test,A test,True,2026-01-15T00:00:00.0000000");
        result.Should().Contain("2,Another,,False,2026-01-16T00:00:00.0000000");
    }

    /// <summary>
    /// Tests that ExportToCsvAsync works with empty collection.
    /// </summary>
    [Fact]
    public async Task ExportToCsvAsync_EmptyCollection_WritesOnlyHeaders()
    {
        // Arrange
        var items = new List<TestModel>();

        await using var outputStream = new MemoryStream();

        // Act
        await CsvExportHelper.ExportToCsvAsync(items.ToAsyncEnumerable(), outputStream, includeHeaders: true);
        outputStream.Position = 0;

        var result = await new StreamReader(outputStream, Encoding.UTF8).ReadToEndAsync();

        // Assert
        result.Should().StartWith("Id,Name,Description,IsActive,CreatedAt");
            result.Should().Match(s => s.EndsWith("\r\n") || s.EndsWith("\n"));
    }

    /// <summary>
    /// Tests that ExportToCsvAsync can exclude headers.
    /// </summary>
    [Fact]
    public async Task ExportToCsvAsync_IncludeHeadersFalse_WritesNoHeaders()
    {
        // Arrange
        var items = new List<TestModel>
        {
            new TestModel { Id = 1, Name = "Test", Description = "A test", IsActive = true, CreatedAt = new DateTime(2026, 1, 15) }
        };

        await using var outputStream = new MemoryStream();

        // Act
        await CsvExportHelper.ExportToCsvAsync(items.ToAsyncEnumerable(), outputStream, includeHeaders: false);
        outputStream.Position = 0;

        var result = await new StreamReader(outputStream, Encoding.UTF8).ReadToEndAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().NotContain("Id,Name,Description,IsActive,CreatedAt");
        result.Should().Contain("1,Test,A test,True,2026-01-15T00:00:00.0000000");
        result.Should().NotContain("\r\n\r\n"); // Should not have extra blank lines
    }
}
// =============================================================================
// Author: Automated Generation
// =============================================================================

using System;
using Xunit;
using ApiKeyGateway.Controllers;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Unit tests for <see cref="AdminControllerValidation"/>.
/// </summary>
public class AdminControllerValidationTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    [Fact]
    public void Validate_ReturnsEmpty_ForValidParameters()
    {
        // Arrange
        string format = "csv";
        DateTime start = Now.AddDays(-1);
        DateTime end = Now;

        // Act
        var problems = format.Validate(start, end);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_ReturnsTrue_ForValidParameters()
    {
        // Arrange
        string format = "json";
        DateTime start = Now.AddDays(-2);
        DateTime end = Now.AddDays(-1);

        // Act
        bool result = format.IsValid(start, end);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Validate_ReturnsProblems_ForNullOrEmptyFormat()
    {
        // Arrange
        string? nullFormat = null;
        string emptyFormat = "   ";
        DateTime start = Now.AddDays(-1);
        DateTime end = Now;

        // Act
        var nullProblems = nullFormat.Validate(start, end);
        var emptyProblems = emptyFormat.Validate(start, end);

        // Assert
        Assert.Contains("Export format cannot be null or whitespace.", nullProblems);
        Assert.Contains("Export format cannot be null or whitespace.", emptyProblems);
    }

    [Fact]
    public void Validate_ReturnsProblems_ForInvalidFormat()
    {
        // Arrange
        string format = "txt";
        DateTime start = Now.AddDays(-1);
        DateTime end = Now;

        // Act
        var problems = format.Validate(start, end);

        // Assert
        Assert.Contains("Export format must be 'csv', 'xml', or 'json'.", problems);
    }

    [Fact]
    public void Validate_ReturnsProblems_ForDateRangeIssues()
    {
        // Arrange
        string format = "xml";
        DateTime start = Now.AddDays(2); // future start
        DateTime end = Now.AddDays(1);   // end before start

        // Act
        var problems = format.Validate(start, end);

        // Assert
        Assert.Contains("End date must be after start date.", problems);
        Assert.Contains("Start date cannot be in the future.", problems);
        Assert.Contains("End date cannot be in the future.", problems);
    }

    [Fact]
    public void Validate_ReturnsProblems_ForDefaultAndOutOfRangeDates()
    {
        // Arrange
        string format = "json";
        DateTime start = default; // DateTime.MinValue
        DateTime end = Now.AddYears(-2); // more than a year in the past

        // Act
        var problems = format.Validate(start, end);

        // Assert
        Assert.Contains("Start date cannot be the default DateTime value.", problems);
        Assert.Contains("End date cannot be more than one year in the past.", problems);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_ForValidParameters()
    {
        // Arrange
        string format = "csv";
        DateTime start = Now.AddDays(-1);
        DateTime end = Now;

        // Act & Assert
        var exception = Record.Exception(() => format.EnsureValid(start, end));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_ForInvalidParameters()
    {
        // Arrange
        string format = "invalid";
        DateTime start = Now.AddDays(-1);
        DateTime end = Now;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => format.EnsureValid(start, end));
        Assert.Contains("Export parameters validation failed.", ex.Message);
    }
}

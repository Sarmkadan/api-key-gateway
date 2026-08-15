// SPDX-License-Identifier: MIT
// Tests for ApiKeyGateway.Utilities.ResponseFormatterExtensions
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests;

public class ResponseFormatterExtensionsTests
{
    [Fact]
    public void Success_WithData_ReturnsSuccessfulResponse()
    {
        // Arrange
        var data = "hello world";

        // Act
        var response = ResponseFormatterExtensions.Success(data);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(data, response.Data);
        Assert.Equal("Operation successful", response.Message);
        Assert.Null(response.ErrorCode);
        Assert.Null(response.Details);
        Assert.True((DateTime.UtcNow - response.Timestamp).TotalSeconds < 2);
    }

    [Fact]
    public void Success_WithCustomMessage_ReturnsResponseWithMessage()
    {
        // Arrange
        var data = new[] { 1, 2, 3 };
        var customMessage = "All good";

        // Act
        var response = ResponseFormatterExtensions.Success(data, customMessage);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(customMessage, response.Message);
        Assert.Equal(data, response.Data);
    }

    [Fact]
    public void Success_NullData_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullData = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ResponseFormatterExtensions.Success(nullData!));
    }

    [Fact]
    public void Error_WithValidParameters_ReturnsErrorResponse()
    {
        // Arrange
        int statusCode = 404;
        string message = "Not found";
        string errorCode = "ERR404";
        var details = new { Reason = "Missing" };

        // Act
        var response = ResponseFormatterExtensions.Error<object>(statusCode, message, errorCode, details);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal(message, response.Message);
        Assert.Equal(errorCode, response.ErrorCode);
        Assert.Equal(details, response.Details);
        Assert.True((DateTime.UtcNow - response.Timestamp).TotalSeconds < 2);
    }

    [Fact]
    public void Error_NullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        int statusCode = 400;
        string? nullMessage = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ResponseFormatterExtensions.Error<object>(statusCode, nullMessage!));
    }

    [Theory]
    [InlineData(200)]
    [InlineData(399)]
    [InlineData(600)]
    public void Error_InvalidStatusCode_ThrowsArgumentOutOfRangeException(int invalidStatus)
    {
        // Arrange
        string message = "Bad request";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ResponseFormatterExtensions.Error<object>(invalidStatus, message));
    }

    [Fact]
    public void Paginated_WithValidParameters_ReturnsPaginatedResponse()
    {
        // Arrange
        var items = Enumerable.Range(1, 3).ToList(); // page items
        int pageNumber = 2;
        int pageSize = 3;
        int totalCount = 8; // total items across all pages

        // Act
        var response = ResponseFormatterExtensions.Paginated(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Equal(items, response.Items);
        Assert.Equal(pageNumber, response.PageNumber);
        Assert.Equal(pageSize, response.PageSize);
        Assert.Equal(totalCount, response.TotalCount);
        int expectedTotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        Assert.Equal(expectedTotalPages, response.TotalPages);
        Assert.True(response.HasMore); // page 2 of 3
        Assert.True((DateTime.UtcNow - response.Timestamp).TotalSeconds < 2);
    }

    [Fact]
    public void Paginated_NullItems_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int>? nullItems = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ResponseFormatterExtensions.Paginated(nullItems!, 1, 10, 0));
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 10, -5)]
    public void Paginated_InvalidArguments_ThrowArgumentOutOfRangeException(int pageNumber, int pageSize, int totalCount)
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ResponseFormatterExtensions.Paginated(items, pageNumber, pageSize, totalCount));
    }
}

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Unit tests for ApiResponseBuilder and ApiResponseBuilderFactory classes.
// Tests all public methods including happy paths, edge cases, and error scenarios.
// =============================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace ApiKeyGateway.Utilities;

/// <summary>
/// Contains unit tests for the <see cref="ApiResponseBuilder{T}"/> class.
/// Tests fluent builder pattern, success/error responses, metadata, and error collections.
/// </summary>
public class ApiResponseBuilderUnitTests
{
    /// <summary>
    /// Test model for response data.
    /// </summary>
    private class TestDataModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    /// <summary>
    /// Tests that WithData sets the data correctly.
    /// </summary>
    [Fact]
    public void WithData_SetsDataCorrectly()
    {
        // Arrange
        var data = new TestDataModel { Id = 42, Name = "Test" };
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder.WithData(data).Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((int)response.data.Id).Should().Be(42);
        ((string)response.data.Name).Should().Be("Test");
    }

    /// <summary>
    /// Tests that WithData handles null data correctly.
    /// </summary>
    [Fact]
    public void WithData_NullData_SetsNullData()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder.WithData(null).Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        response.data.Should().BeNull();
    }

    /// <summary>
    /// Tests that WithData handles empty object correctly.
    /// </summary>
    [Fact]
    public void WithData_EmptyObject_SetsEmptyObject()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder.WithData(new TestDataModel()).Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((int)response.data.Id).Should().Be(0);
        ((string)response.data.Name).Should().BeNull();
    }

    /// <summary>
    /// Tests that Success marks response as successful with default message.
    /// </summary>
    [Fact]
    public void Success_MarksResponseAsSuccessful_WithDefaultMessage()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder.Success().Build();

        // Assert
        dynamic response = result;
        ((bool)response.success).Should().BeTrue();
        ((int)response.statusCode).Should().Be(200);
        ((string)response.message).Should().Be("Success");
        ((string)response.errorCode).Should().BeNull();
    }

    /// <summary>
    /// Tests that Success with custom message sets correct message.
    /// </summary>
    [Fact]
    public void Success_WithCustomMessage_SetsCustomMessage()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder.Success("Custom success message").Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((string)response.message).Should().Be("Custom success message");
    }

    /// <summary>
    /// Tests that Success with null message uses default message.
    /// </summary>
    [Fact]
    public void Success_WithNullMessage_UsesDefaultMessage()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder.Success(null).Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((string)response.message).Should().Be("Success");
    }

    /// <summary>
    /// Tests that Error marks response as failed with correct status code and message.
    /// </summary>
    [Fact]
    public void Error_MarksResponseAsFailed_WithCorrectStatusCodeAndMessage()
    {
        // Arrange & Act
        var result = new ApiResponseBuilder<TestDataModel>()
            .Error(400, "Bad request error", "BAD_REQUEST")
            .Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((bool)response.success).Should().BeFalse();
        ((int)response.statusCode).Should().Be(400);
        ((string)response.message).Should().Be("Bad request error");
        ((string)response.errorCode).Should().Be("BAD_REQUEST");
    }

    /// <summary>
    /// Tests that Error with null error code handles it correctly.
    /// </summary>
    [Fact]
    public void Error_WithNullErrorCode_HandlesNullErrorCode()
    {
        // Arrange & Act
        var result = new ApiResponseBuilder<TestDataModel>()
            .Error(500, "Internal server error", null)
            .Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((string)response.errorCode).Should().BeNull();
    }

    /// <summary>
    /// Tests that Error with empty message sets empty message.
    /// </summary>
    [Fact]
    public void Error_WithEmptyMessage_SetsEmptyMessage()
    {
        // Arrange & Act
        var result = new ApiResponseBuilder<TestDataModel>()
            .Error(404, "", "NOT_FOUND")
            .Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((string)response.message).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that WithMetadata adds metadata to response.
    /// </summary>
    [Fact]
    public void WithMetadata_AddsMetadataToResponse()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder
            .WithMetadata("page", 1)
            .WithMetadata("pageSize", 10)
            .WithMetadata("total", 100)
            .Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        response.metadata.Should().NotBeNull();
        ((int)response.metadata["page"]).Should().Be(1);
        ((int)response.metadata["pageSize"]).Should().Be(10);
        ((int)response.metadata["total"]).Should().Be(100);
    }

    /// <summary>
    /// Tests that WithMetadata called multiple times accumulates metadata.
    /// </summary>
    [Fact]
    public void WithMetadata_MultipleCalls_AccumulatesMetadata()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder
            .WithMetadata("key1", "value1")
            .WithMetadata("key2", "value2")
            .WithMetadata("key3", "value3")
            .Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        response.metadata.Should().NotBeNull();
        ((string)response.metadata["key1"]).Should().Be("value1");
        ((string)response.metadata["key2"]).Should().Be("value2");
        ((string)response.metadata["key3"]).Should().Be("value3");
    }

    /// <summary>
    /// Tests that AddError adds error to errors collection.
    /// </summary>
    [Fact]
    public void AddError_AddsErrorToErrorsCollection()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder
            .AddError("First error")
            .AddError("Second error")
            .AddError("Third error")
            .Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        response.errors.Should().NotBeNull();
        response.errors.Should().BeOfType<List<string>>();
        ((List<string>)response.errors).Should().HaveCount(3);
        ((List<string>)response.errors)[0].Should().Be("First error");
        ((List<string>)response.errors)[1].Should().Be("Second error");
        ((List<string>)response.errors)[2].Should().Be("Third error");
    }

    /// <summary>
    /// Tests that AddError called on builder without errors initializes collection.
    /// </summary>
    [Fact]
    public void AddError_InitializesErrorsCollection()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder.AddError("Single error").Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        response.errors.Should().NotBeNull();
        response.errors.Should().BeOfType<List<string>>();
        ((List<string>)response.errors).Should().HaveCount(1);
        ((List<string>)response.errors)[0].Should().Be("Single error");
    }

    /// <summary>
    /// Tests that fluent interface allows chaining multiple operations.
    /// </summary>
    [Fact]
    public void FluentInterface_AllowsChainingMultipleOperations()
    {
        // Arrange
        var data = new TestDataModel { Id = 1, Name = "Test" };

        // Act
        var result = new ApiResponseBuilder<TestDataModel>()
            .WithData(data)
            .Success("Operation completed")
            .WithMetadata("version", "1.0")
            .WithMetadata("timestamp", DateTime.UtcNow)
            .AddError("Minor warning")
            .Build();

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((bool)response.success).Should().BeTrue();
        ((int)response.statusCode).Should().Be(200);
        ((string)response.message).Should().Be("Operation completed");
        ((dynamic)response.data).Id.Should().Be(1);
        ((dynamic)response.data).Name.Should().Be("Test");
        response.metadata.Should().NotBeNull();
        response.errors.Should().NotBeNull();
        ((List<string>)response.errors).Should().HaveCount(1);
    }

    /// <summary>
    /// Tests that timestamp is set and is recent.
    /// </summary>
    [Fact]
    public void Build_SetsTimestamp_ToRecentDate()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder.Success().Build();
        var timestamp = (DateTime)((dynamic)result).timestamp;

        // Assert
        timestamp.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
        timestamp.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));
    }

    /// <summary>
    /// Tests that Build returns anonymous object with all expected properties.
    /// </summary>
    [Fact]
    public void Build_ReturnsAnonymousObject_WithAllExpectedProperties()
    {
        // Arrange
        var builder = new ApiResponseBuilder<TestDataModel>();

        // Act
        var result = builder.Success("Test message").Build();

        // Assert
        result.Should().BeOfType<object>();
        dynamic response = result;

        // Check all expected properties exist
        var _ = response.success;
        var _2 = response.statusCode;
        var _3 = response.message;
        var _4 = response.errorCode;
        var _5 = response.data;
        var _6 = response.errors;
        var _7 = response.metadata;
        var _8 = response.timestamp;
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.Success creates successful response with data.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_Success_CreatesSuccessfulResponseWithData()
    {
        // Arrange
        var data = new TestDataModel { Id = 100, Name = "Factory Test" };

        // Act
        var result = ApiResponseBuilderFactory.Success(data, "Factory success message");

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((bool)response.success).Should().BeTrue();
        ((int)response.statusCode).Should().Be(200);
        ((string)response.message).Should().Be("Factory success message");
        ((dynamic)response.data).Id.Should().Be(100);
        ((dynamic)response.data).Name.Should().Be("Factory Test");
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.Success with null data.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_Success_WithNullData_HandlesNullData()
    {
        // Act
        var result = ApiResponseBuilderFactory.Success<TestDataModel>(null, "Success with null");

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((bool)response.success).Should().BeTrue();
        ((dynamic)response.data).Should().BeNull();
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.Error creates error response with correct status.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_Error_CreatesErrorResponseWithCorrectStatus()
    {
        // Act
        var result = ApiResponseBuilderFactory.Error<TestDataModel>(404, "Not found", "NOT_FOUND");

        // Assert
        result.Should().BeOfType<dynamic>();
        var response = (dynamic)result;
        ((bool)response.success).Should().BeFalse();
        ((int)response.statusCode).Should().Be(404);
        ((string)response.message).Should().Be("Not found");
        ((string)response.errorCode).Should().Be("NOT_FOUND");
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.NotFound creates 404 response.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_NotFound_Creates404Response()
    {
        // Act
        var result = ApiResponseBuilderFactory.NotFound("User");

        // Assert
        result.Should().BeOfType<object>();
        dynamic response = result;
        ((bool)response.success).Should().BeFalse();
        ((int)response.statusCode).Should().Be(404);
        ((string)response.message).Should().Be("User not found");
        ((string)response.errorCode).Should().Be("NOT_FOUND");
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.NotFound with default resource.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_NotFound_WithDefaultResource_Creates404Response()
    {
        // Act
        var result = ApiResponseBuilderFactory.NotFound();

        // Assert
        result.Should().BeOfType<object>();
        dynamic response = result;
        ((string)response.message).Should().Be("Resource not found");
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.BadRequest creates 400 response with errors.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_BadRequest_Creates400ResponseWithErrors()
    {
        // Act
        var result = ApiResponseBuilderFactory.BadRequest("Invalid input", "Field1 is required", "Field2 is invalid");

        // Assert
        result.Should().BeOfType<object>();
        dynamic response = result;
        ((bool)response.success).Should().BeFalse();
        ((int)response.statusCode).Should().Be(400);
        ((string)response.message).Should().Be("Invalid input");
        ((string)response.errorCode).Should().Be("BAD_REQUEST");
        response.errors.Should().NotBeNull();
        response.errors.Should().BeOfType<List<string>>();
        ((List<string>)response.errors).Should().HaveCount(2);
        ((List<string>)response.errors)[0].Should().Be("Field1 is required");
        ((List<string>)response.errors)[1].Should().Be("Field2 is invalid");
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.Unauthorized creates 401 response.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_Unauthorized_Creates401Response()
    {
        // Act
        var result = ApiResponseBuilderFactory.Unauthorized("Invalid credentials");

        // Assert
        result.Should().BeOfType<object>();
        dynamic response = result;
        ((bool)response.success).Should().BeFalse();
        ((int)response.statusCode).Should().Be(401);
        ((string)response.message).Should().Be("Invalid credentials");
        ((string)response.errorCode).Should().Be("UNAUTHORIZED");
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.Unauthorized with default message.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_Unauthorized_WithDefaultMessage_Creates401Response()
    {
        // Act
        var result = ApiResponseBuilderFactory.Unauthorized();

        // Assert
        result.Should().BeOfType<object>();
        dynamic response = result;
        ((string)response.message).Should().Be("Unauthorized");
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.Forbidden creates 403 response.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_Forbidden_Creates403Response()
    {
        // Act
        var result = ApiResponseBuilderFactory.Forbidden("Access denied to resource");

        // Assert
        result.Should().BeOfType<object>();
        dynamic response = result;
        ((bool)response.success).Should().BeFalse();
        ((int)response.statusCode).Should().Be(403);
        ((string)response.message).Should().Be("Access denied to resource");
        ((string)response.errorCode).Should().Be("FORBIDDEN");
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.TooManyRequests creates 429 response.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_TooManyRequests_Creates429Response()
    {
        // Act
        var result = ApiResponseBuilderFactory.TooManyRequests("Rate limit exceeded");

        // Assert
        result.Should().BeOfType<object>();
        dynamic response = result;
        ((bool)response.success).Should().BeFalse();
        ((int)response.statusCode).Should().Be(429);
        ((string)response.message).Should().Be("Rate limit exceeded");
        ((string)response.errorCode).Should().Be("RATE_LIMIT_EXCEEDED");
    }

    /// <summary>
    /// Tests ApiResponseBuilderFactory.InternalServerError creates 500 response.
    /// </summary>
    [Fact]
    public void ApiResponseBuilderFactory_InternalServerError_Creates500Response()
    {
        // Act
        var result = ApiResponseBuilderFactory.InternalServerError("Server error occurred");

        // Assert
        result.Should().BeOfType<object>();
        dynamic response = result;
        ((bool)response.success).Should().BeFalse();
        ((int)response.statusCode).Should().Be(500);
        ((string)response.message).Should().Be("Server error occurred");
        ((string)response.errorCode).Should().Be("INTERNAL_SERVER_ERROR");
    }
}

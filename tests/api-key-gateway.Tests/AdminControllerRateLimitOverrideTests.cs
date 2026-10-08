// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Configuration;
using ApiKeyGateway.Controllers;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Tests for <see cref="AdminController.SetRateLimitOverride"/>: policy references and override
/// values are validated, and invalid requests return 400 ProblemDetails.
/// </summary>
public class AdminControllerRateLimitOverrideTests
{
    private const string ApiKeyId = "key-1";

    private readonly Mock<ILogger<AdminController>> _loggerMock = new();
    private readonly Mock<IMetricsCollectionService> _metricsMock = new();
    private readonly Mock<IDataExportService> _exportMock = new();
    private readonly Mock<IAuditLogRepository> _auditMock = new();
    private readonly Mock<IRateLimitingService> _rateLimitingMock = new();

    private static RateLimitingOptions BuildOptions() => new()
    {
        Policies = new Dictionary<string, RateLimitPolicyOptions>(StringComparer.OrdinalIgnoreCase)
        {
            ["free"] = new RateLimitPolicyOptions { RequestsPerUnit = 100, WindowSeconds = 3600 },
            ["pro"] = new RateLimitPolicyOptions { RequestsPerUnit = 10000, WindowSeconds = 60 }
        }
    };

    private AdminController CreateController(RateLimitingOptions? options = null)
    {
        var controller = new AdminController(
            _loggerMock.Object,
            _metricsMock.Object,
            _exportMock.Object,
            _auditMock.Object,
            _rateLimitingMock.Object,
            Options.Create(options ?? BuildOptions()));

        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    [Fact]
    public async Task SetRateLimitOverride_ReturnsBadRequestProblem_WhenPolicyUnknown()
    {
        // Arrange
        var controller = CreateController();
        var request = new RateLimitKeyOverrideOptions { Policy = "enterprise-gold" };

        // Act
        var result = await controller.SetRateLimitOverride(ApiKeyId, request);

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var problem = badRequest.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Detail.Should().Contain("enterprise-gold");
        problem.Extensions.Should().ContainKey("errors");
        _rateLimitingMock.Verify(s => s.UpdateLimitAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<RateLimitUnit>()), Times.Never);
    }

    [Fact]
    public async Task SetRateLimitOverride_ReturnsBadRequestProblem_WhenPolicyMissing()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.SetRateLimitOverride(ApiKeyId, new RateLimitKeyOverrideOptions());

        // Assert
        var problem = result.Should().BeOfType<BadRequestObjectResult>().Subject.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Detail.Should().Contain("policy name");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task SetRateLimitOverride_ReturnsBadRequestProblem_WhenRequestsPerUnitNotPositive(int requests)
    {
        // Arrange
        var controller = CreateController();
        var request = new RateLimitKeyOverrideOptions { Policy = "free", RequestsPerUnit = requests };

        // Act
        var result = await controller.SetRateLimitOverride(ApiKeyId, request);

        // Assert
        var problem = result.Should().BeOfType<BadRequestObjectResult>().Subject.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Detail.Should().Contain("RequestsPerUnit");
        _rateLimitingMock.Verify(s => s.UpdateLimitAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<RateLimitUnit>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(86401)]
    public async Task SetRateLimitOverride_ReturnsBadRequestProblem_WhenWindowOutOfRange(int window)
    {
        // Arrange
        var controller = CreateController();
        var request = new RateLimitKeyOverrideOptions { Policy = "free", WindowSeconds = window };

        // Act
        var result = await controller.SetRateLimitOverride(ApiKeyId, request);

        // Assert
        var problem = result.Should().BeOfType<BadRequestObjectResult>().Subject.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Detail.Should().Contain("WindowSeconds");
    }

    [Fact]
    public async Task SetRateLimitOverride_ReturnsBadRequestProblem_WhenWindowNotSupportedUnit()
    {
        // Arrange
        var controller = CreateController();
        var request = new RateLimitKeyOverrideOptions { Policy = "free", WindowSeconds = 90 };

        // Act
        var result = await controller.SetRateLimitOverride(ApiKeyId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _rateLimitingMock.Verify(s => s.UpdateLimitAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<RateLimitUnit>()), Times.Never);
    }

    [Fact]
    public async Task SetRateLimitOverride_AppliesPolicyValues_WhenOnlyPolicySet()
    {
        // Arrange
        _rateLimitingMock
            .Setup(s => s.UpdateLimitAsync(ApiKeyId, 100, RateLimitUnit.Hour))
            .ReturnsAsync(true);
        var controller = CreateController();

        // Act
        var result = await controller.SetRateLimitOverride(ApiKeyId, new RateLimitKeyOverrideOptions { Policy = "free" });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _rateLimitingMock.Verify(s => s.UpdateLimitAsync(ApiKeyId, 100, RateLimitUnit.Hour), Times.Once);
    }

    [Fact]
    public async Task SetRateLimitOverride_AppliesOverrideValues_WhenProvided()
    {
        // Arrange
        _rateLimitingMock
            .Setup(s => s.UpdateLimitAsync(ApiKeyId, 500, RateLimitUnit.Minute))
            .ReturnsAsync(true);
        var controller = CreateController();
        var request = new RateLimitKeyOverrideOptions { Policy = "free", RequestsPerUnit = 500, WindowSeconds = 60 };

        // Act
        var result = await controller.SetRateLimitOverride(ApiKeyId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _rateLimitingMock.Verify(s => s.UpdateLimitAsync(ApiKeyId, 500, RateLimitUnit.Minute), Times.Once);
    }

    [Fact]
    public async Task SetRateLimitOverride_ReturnsNotFound_WhenKeyHasNoRateLimit()
    {
        // Arrange
        _rateLimitingMock
            .Setup(s => s.UpdateLimitAsync(ApiKeyId, It.IsAny<int>(), It.IsAny<RateLimitUnit>()))
            .ReturnsAsync(false);
        var controller = CreateController();

        // Act
        var result = await controller.SetRateLimitOverride(ApiKeyId, new RateLimitKeyOverrideOptions { Policy = "pro" });

        // Assert
        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.Value.Should().BeOfType<ProblemDetails>();
    }
}

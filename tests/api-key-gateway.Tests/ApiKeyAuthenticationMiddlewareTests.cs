// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Threading.Tasks;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Middleware;
using ApiKeyGateway.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ApiKeyGateway.Tests;

public class ApiKeyAuthenticationMiddlewareTests
{
    private readonly Mock<IAuthenticationService> _authMock = new();
    private readonly Mock<IUsageTrackingService> _usageMock = new();
    private readonly Mock<IRateLimitingService> _rateLimitMock = new();
    private readonly Mock<IUsageQuotaService> _quotaMock = new();
    private readonly Mock<IQuotaAlertEvaluator> _alertMock = new();
    private bool _nextCalled;

    private RequestDelegate TrackingNext() => _ => { _nextCalled = true; return Task.CompletedTask; };

    private Task RunAsync(HttpContext context, RequestDelegate? next = null) =>
        new ApiKeyAuthenticationMiddleware(next ?? TrackingNext(), NullLogger<ApiKeyAuthenticationMiddleware>.Instance)
            .InvokeAsync(context, _authMock.Object, _usageMock.Object, _rateLimitMock.Object, _quotaMock.Object, _alertMock.Object);

    private static HttpContext CreateContext(string? apiKeyHeader = null, string? authorization = null)
    {
        var context = new DefaultHttpContext();
        if (apiKeyHeader != null)
        {
            context.Request.Headers["X-API-Key"] = apiKeyHeader;
        }

        if (authorization != null)
        {
            context.Request.Headers["Authorization"] = authorization;
        }

        return context;
    }

    private void SetupValidKey(string keyId)
    {
        var apiKey = new ApiKey { Id = keyId, ConsumerId = "consumer-1" };
        _authMock.Setup(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(AuthenticationResult.SuccessResult(apiKey));
        _quotaMock.Setup(q => q.CheckAndRecordAsync(keyId))
            .ReturnsAsync(new UsageQuotaResult(false, long.MaxValue, long.MaxValue, DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenNextIsNull() =>
        new Action(() => new ApiKeyAuthenticationMiddleware(null!, NullLogger<ApiKeyAuthenticationMiddleware>.Instance))
            .Should().Throw<ArgumentNullException>().WithParameterName("next");

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task InvokeAsync_CallsNextWithoutAuthentication_WhenNoUsableKeyIsSupplied(string? headerValue)
    {
        // Arrange
        var context = CreateContext(apiKeyHeader: headerValue);

        // Act
        await RunAsync(context);

        // Assert
        _nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        _authMock.Verify(a => a.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatesBearerToken_AndAddsRateLimitHeaders()
    {
        // Arrange
        SetupValidKey("key-1");
        _rateLimitMock.Setup(r => r.GetLimitAsync("key-1")).ReturnsAsync(
            new RateLimit { ApiKeyId = "key-1", RequestsPerUnit = 100, Unit = RateLimitUnit.Hour, CurrentRequestCount = 58 });
        var context = CreateContext(authorization: "Bearer   token-value  ");

        // Act
        await RunAsync(context);

        // Assert
        _authMock.Verify(a => a.AuthenticateAsync("token-value", It.IsAny<string?>()), Times.Once);
        _nextCalled.Should().BeTrue();
        context.Response.Headers["X-RateLimit-Limit"].ToString().Should().Be("100");
        context.Response.Headers["X-RateLimit-Remaining"].ToString().Should().Be("42");
        context.Items["ApiKey"].Should().BeOfType<ApiKey>();
        context.Items["ConsumerId"].Should().Be("consumer-1");
        _usageMock.Verify(u => u.RecordUsageAsync(
            It.Is<UsageRecord>(r => r.ApiKeyId == "key-1" && r.ConsumerId == "consumer-1")), Times.Once);
        _rateLimitMock.Verify(r => r.RecordRequestAsync("key-1"), Times.Once);
    }

    [Theory]
    [InlineData(AuthenticationFailureReason.ApiKeyNotFound, StatusCodes.Status401Unauthorized)]
    [InlineData(AuthenticationFailureReason.ServiceUnavailable, StatusCodes.Status503ServiceUnavailable)]
    public async Task InvokeAsync_RejectsRequest_WhenAuthenticationFails(AuthenticationFailureReason reason, int expectedStatus)
    {
        // Arrange
        _authMock.Setup(a => a.AuthenticateAsync("bad-key", It.IsAny<string?>()))
            .ReturnsAsync(AuthenticationResult.FailureResult(reason));
        var context = CreateContext(apiKeyHeader: "bad-key");

        // Act
        await RunAsync(context);

        // Assert
        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(false, StatusCodes.Status429TooManyRequests)]
    [InlineData(true, StatusCodes.Status503ServiceUnavailable)]
    public async Task InvokeAsync_MapsRateLimitFailures_ToStatusCode(bool storeUnavailable, int expectedStatus)
    {
        // Arrange
        SetupValidKey("key-1");
        Exception failure = storeUnavailable
            ? new KeyStoreUnavailableException("Rate limit store is down")
            : new RateLimitExceededException("key-1", 100, 3600);
        _rateLimitMock.Setup(r => r.CheckLimitAsync("key-1")).ThrowsAsync(failure);
        var context = CreateContext(apiKeyHeader: "valid-key");

        // Act
        await RunAsync(context);

        // Assert
        _nextCalled.Should().BeFalse();
        context.Response.StatusCode.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task InvokeAsync_Returns500_WhenNextThrowsUnexpectedException()
    {
        // Arrange
        SetupValidKey("key-1");
        var context = CreateContext(apiKeyHeader: "valid-key");
        RequestDelegate next = _ => throw new InvalidOperationException("boom");

        // Act
        await RunAsync(context, next);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        _usageMock.Verify(u => u.RecordUsageAsync(It.IsAny<UsageRecord>()), Times.Never);
    }

    [Fact]
    public async Task UseApiKeyAuthentication_RegistersMiddleware_ThatResolvesScopedServicesPerRequest()
    {
        // Arrange
        _authMock.Setup(a => a.AuthenticateAsync("bad-key", It.IsAny<string?>()))
            .ReturnsAsync(AuthenticationResult.FailureResult(AuthenticationFailureReason.ApiKeyNotFound));
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<ApiKeyAuthenticationMiddleware>>(NullLogger<ApiKeyAuthenticationMiddleware>.Instance);
        services.AddSingleton(_authMock.Object);
        services.AddSingleton(_usageMock.Object);
        services.AddSingleton(_rateLimitMock.Object);
        services.AddSingleton(_quotaMock.Object);
        services.AddSingleton(_alertMock.Object);
        var app = new ApplicationBuilder(services.BuildServiceProvider());
        app.UseApiKeyAuthentication();
        app.Run(_ => { _nextCalled = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        context.Request.Headers["X-API-Key"] = "bad-key";

        // Act
        await app.Build()(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        _nextCalled.Should().BeFalse();
    }
}

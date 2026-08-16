using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ApiKeyGateway.Controllers;

namespace api_key_gateway.Tests;

public sealed class StatsControllerTests
{
    private static StatsController CreateController(string apiKeyId = "test-key")
    {
        var logger = NullLogger<StatsController>.Instance;
        var controller = new StatsController(logger);

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[] { new Claim("api_key_id", apiKeyId) },
                "TestAuth"
            )
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Fact]
    public void GetUsageStatistics_ReturnsDailyStats_WhenPeriodIsDay()
    {
        var controller = CreateController();

        var result = controller.GetUsageStatistics("day") as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        dynamic data = result.Value;
        Assert.Equal("last 24 hours", (string)data.period);
        Assert.Equal(4500, (int)data.requests);
    }

    [Fact]
    public void GetUsageStatistics_ReturnsHourlyStats_WhenPeriodIsHour()
    {
        var controller = CreateController();

        var result = controller.GetUsageStatistics("hour") as OkObjectResult;

        Assert.NotNull(result);
        dynamic data = result.Value;
        Assert.Equal("last 1 hour", (string)data.period);
        Assert.Equal(450, (int)data.requests);
    }

    [Fact]
    public void GetUsageStatistics_ReturnsMonthlyStats_WhenPeriodIsMonth()
    {
        var controller = CreateController();

        var result = controller.GetUsageStatistics("month") as OkObjectResult;

        Assert.NotNull(result);
        dynamic data = result.Value;
        Assert.Equal("last 30 days", (string)data.period);
        Assert.Equal(45000, (int)data.requests);
    }

    [Fact]
    public void GetUsageStatistics_ReturnsDailyStats_WhenPeriodIsUnknown()
    {
        var controller = CreateController();

        var result = controller.GetUsageStatistics("unknown") as OkObjectResult;

        Assert.NotNull(result);
        dynamic data = result.Value;
        Assert.Equal("last 24 hours", (string)data.period);
    }

    [Fact]
    public void GetUsageStatistics_Throws_WhenPeriodIsNull()
    {
        var controller = CreateController();

        Assert.Throws<NullReferenceException>(() => controller.GetUsageStatistics(null));
    }

    [Fact]
    public void GetRateLimitStatus_ReturnsOkWithStatus()
    {
        var controller = CreateController();

        var result = controller.GetRateLimitStatus() as OkObjectResult;

        Assert.NotNull(result);
        dynamic data = result.Value;
        Assert.Equal("ok", (string)data.status);
        Assert.Equal("test-key", (string)data.apiKeyId);
    }

    [Fact]
    public void GetEndpointStatistics_ReturnsEndpointsArray()
    {
        var controller = CreateController();

        var result = controller.GetEndpointStatistics() as OkObjectResult;

        Assert.NotNull(result);
        dynamic data = result.Value;
        Assert.Equal("test-key", (string)data.apiKeyId);
        var endpoints = data.endpoints as object[];
        Assert.NotNull(endpoints);
        Assert.Equal(3, endpoints.Length);
    }

    [Fact]
    public void GetRecentActivity_ReturnsLimitedRequests()
    {
        var controller = CreateController();

        var result = controller.GetRecentActivity(10) as OkObjectResult;

        Assert.NotNull(result);
        dynamic data = result.Value;
        Assert.Equal("test-key", (string)data.apiKeyId);
        var requests = data.recentRequests as object[];
        Assert.NotNull(requests);
        Assert.Equal(3, requests.Length);
    }

    [Fact]
    public void GetQuotaStatus_ReturnsQuotaInfo()
    {
        var controller = CreateController();

        var result = controller.GetQuotaStatus() as OkObjectResult;

        Assert.NotNull(result);
        dynamic data = result.Value;
        Assert.Equal("test-key", (string)data.apiKeyId);
        Assert.Equal("pro", (string)data.quotaType);
        Assert.Equal(10000, (int)data.limits.requestsPerDay);
    }
}

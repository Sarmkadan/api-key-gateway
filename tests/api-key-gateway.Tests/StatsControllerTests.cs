using System;
using System.Security.Claims;
using System.Text.Json;
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

    private static JsonElement GetJsonValue(OkObjectResult result)
    {
        var json = JsonSerializer.Serialize(result.Value);
        return JsonDocument.Parse(json).RootElement;
    }

    [Fact]
    public void GetUsageStatistics_ReturnsDailyStats_WhenPeriodIsDay()
    {
        var controller = CreateController();
        var result = controller.GetUsageStatistics("day") as OkObjectResult;
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        var data = GetJsonValue(result);
        Assert.Equal("last 24 hours", data.GetProperty("period").GetString());
        Assert.Equal(4500, data.GetProperty("requests").GetInt32());
    }

    [Fact]
    public void GetUsageStatistics_ReturnsHourlyStats_WhenPeriodIsHour()
    {
        var controller = CreateController();
        var result = controller.GetUsageStatistics("hour") as OkObjectResult;
        Assert.NotNull(result);
        var data = GetJsonValue(result);
        Assert.Equal("last 1 hour", data.GetProperty("period").GetString());
        Assert.Equal(450, data.GetProperty("requests").GetInt32());
    }

    [Fact]
    public void GetUsageStatistics_ReturnsMonthlyStats_WhenPeriodIsMonth()
    {
        var controller = CreateController();
        var result = controller.GetUsageStatistics("month") as OkObjectResult;
        Assert.NotNull(result);
        var data = GetJsonValue(result);
        Assert.Equal("last 30 days", data.GetProperty("period").GetString());
        Assert.Equal(45000, data.GetProperty("requests").GetInt32());
    }

    [Fact]
    public void GetUsageStatistics_ReturnsDailyStats_WhenPeriodIsUnknown()
    {
        var controller = CreateController();
        var result = controller.GetUsageStatistics("unknown") as OkObjectResult;
        Assert.NotNull(result);
        var data = GetJsonValue(result);
        Assert.Equal("last 24 hours", data.GetProperty("period").GetString());
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
        var data = GetJsonValue(result);
        Assert.Equal("ok", data.GetProperty("status").GetString());
        Assert.Equal("test-key", data.GetProperty("apiKeyId").GetString());
    }

    [Fact]
    public void GetEndpointStatistics_ReturnsEndpointsArray()
    {
        var controller = CreateController();
        var result = controller.GetEndpointStatistics() as OkObjectResult;
        Assert.NotNull(result);
        var data = GetJsonValue(result);
        Assert.Equal("test-key", data.GetProperty("apiKeyId").GetString());
        var endpoints = data.GetProperty("endpoints");
        Assert.Equal(3, endpoints.GetArrayLength());
    }

    [Fact]
    public void GetRecentActivity_ReturnsLimitedRequests()
    {
        var controller = CreateController();
        var result = controller.GetRecentActivity(10) as OkObjectResult;
        Assert.NotNull(result);
        var data = GetJsonValue(result);
        Assert.Equal("test-key", data.GetProperty("apiKeyId").GetString());
        var requests = data.GetProperty("recentRequests");
        Assert.Equal(3, requests.GetArrayLength());
    }

    [Fact]
    public void GetQuotaStatus_ReturnsQuotaInfo()
    {
        var controller = CreateController();
        var result = controller.GetQuotaStatus() as OkObjectResult;
        Assert.NotNull(result);
        var data = GetJsonValue(result);
        Assert.Equal("test-key", data.GetProperty("apiKeyId").GetString());
        Assert.Equal("pro", data.GetProperty("quotaType").GetString());
        Assert.Equal(10000, data.GetProperty("limits").GetProperty("requestsPerDay").GetInt32());
    }
}

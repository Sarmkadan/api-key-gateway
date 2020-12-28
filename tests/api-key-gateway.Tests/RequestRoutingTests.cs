// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
using Xunit;
using System.Net;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Tests for request routing with encoded path segments
/// </summary>
public class RequestRoutingTests
{
    [Fact]
    public async Task Request_WithEncodedPathSegments_ShouldRouteCorrectly()
    {
        // Arrange: a key whose ID contains a slash, reachable only if the
        // router decodes %2F into the {id} route value instead of splitting
        // the path into two segments (which would 404).
        const string keyIdWithSlash = "tenant/key-42";
        string? observedId = null;

        var apiKeyServiceMock = new Mock<IApiKeyService>();
        apiKeyServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<string>()))
            .Callback<string>(id => observedId = id)
            .ReturnsAsync(new ApiKey
            {
                Id = keyIdWithSlash,
                ConsumerId = "consumer-1",
                Name = "Encoded Segment Key",
                Status = ApiKeyStatus.Active,
                CreatedAt = DateTime.UtcNow
            });

        using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped(_ => apiKeyServiceMock.Object);
                });
            });

        using var client = application.CreateClient();

        // Act: %2F is an encoded forward slash inside a single path segment.
        var response = await client.GetAsync("/api/apikeys/tenant%2Fkey-42");

        // Assert: the request routed to GetKeyById as a single segment instead of
        // being split into two path segments (which would produce a router 404).
        // ASP.NET Core intentionally leaves %2F encoded in route values, so the
        // observed id must round-trip to the original slash-containing id.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        apiKeyServiceMock.Verify(s => s.GetByIdAsync(It.IsAny<string>()), Times.Once);
        Assert.NotNull(observedId);
        Assert.Equal(keyIdWithSlash, Uri.UnescapeDataString(observedId));
    }
}

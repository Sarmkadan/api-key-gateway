// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
using Xunit;
using System.Net;
using System.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ApiKeyGateway.Tests;

/// <summary>
/// Tests for request routing with encoded path segments
/// </summary>
public class RequestRoutingTests
{
    [Fact]
    public async Task Request_WithEncodedPathSegments_ShouldRouteCorrectly()
    {
        // Arrange
        var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Add any required test services here
                });
            });

        var client = application.CreateClient();

        // Create a request with encoded path segments
        var encodedPath = "/api/test%2Fpath"; // %2F is encoded forward slash
        var request = new HttpRequestMessage(HttpMethod.Get, encodedPath);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        // Should not return 404 - should properly route to the endpoint
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
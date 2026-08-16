// =============================================================================
// Tests for ApiKeyGateway.Utilities.HttpClientFactory
// =============================================================================

using System;
using System.Linq;
using System.Net.Http;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests
{
    public class HttpClientFactoryTests
    {
        [Fact]
        public void CreateClient_DefaultSettings_ReturnsConfiguredClient()
        {
            // Act
            using var client = HttpClientFactory.CreateClient();

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(30), client.Timeout);

            Assert.True(client.DefaultRequestHeaders.TryGetValues("User-Agent", out var uaValues));
            Assert.Single(uaValues);
            Assert.Equal("ApiKeyGateway/1.0", uaValues.First());

            Assert.True(client.DefaultRequestHeaders.TryGetValues("Accept-Encoding", out var encValues));
            Assert.Single(encValues);
            Assert.Equal("gzip, deflate", encValues.First());
        }

        [Fact]
        public void CreateClient_CustomUserAgent_SetsHeader()
        {
            // Arrange
            const string customAgent = "MyApp/2.0";

            // Act
            using var client = HttpClientFactory.CreateClient(customAgent);

            // Assert
            Assert.True(client.DefaultRequestHeaders.TryGetValues("User-Agent", out var uaValues));
            Assert.Single(uaValues);
            Assert.Equal(customAgent, uaValues.First());
        }

        [Fact]
        public void CreateClient_NullUserAgent_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => HttpClientFactory.CreateClient(null!));
        }

        [Fact]
        public void CreateWebhookClient_ReturnsClientWithWebhookSettings()
        {
            // Act
            using var client = HttpClientFactory.CreateWebhookClient();

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(60), client.Timeout);

            Assert.True(client.DefaultRequestHeaders.TryGetValues("User-Agent", out var uaValues));
            Assert.Single(uaValues);
            Assert.Equal("ApiKeyGateway-WebhookDelivery/1.0", uaValues.First());
        }

        [Fact]
        public void CreateExternalApiClient_WithApiName_SetsUserAgentAndTimeout()
        {
            // Arrange
            const string apiName = "GitHub";

            // Act
            using var client = HttpClientFactory.CreateExternalApiClient(apiName);

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(45), client.Timeout);

            Assert.True(client.DefaultRequestHeaders.TryGetValues("User-Agent", out var uaValues));
            Assert.Single(uaValues);
            Assert.Equal($"ApiKeyGateway/{apiName}/1.0", uaValues.First());
        }

        [Fact]
        public void CreateExternalApiClient_NullApiName_ProducesUserAgentWithEmptyName()
        {
            // Act
            using var client = HttpClientFactory.CreateExternalApiClient(null!);

            // Assert
            Assert.True(client.DefaultRequestHeaders.TryGetValues("User-Agent", out var uaValues));
            Assert.Single(uaValues);
            // When apiName is null, string interpolation yields an empty segment.
            Assert.Equal("ApiKeyGateway//1.0", uaValues.First());
        }

        [Fact]
        public void CreateClient_MultipleCalls_ReturnDistinctInstances()
        {
            // Act
            using var client1 = HttpClientFactory.CreateClient();
            using var client2 = HttpClientFactory.CreateClient();

            // Assert
            Assert.NotSame(client1, client2);
        }
    }
}

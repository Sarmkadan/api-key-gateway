// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Test suite for WebhookManager
// =============================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using ApiKeyGateway.Domain.Exceptions;
using ApiKeyGateway.Integration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace api_key_gateway.Tests
{
    public class WebhookManagerTests
    {
        private readonly WebhookManager _manager;

        public WebhookManagerTests()
        {
            // Use a null logger to avoid noisy output during tests
            _manager = new WebhookManager(NullLogger<WebhookManager>.Instance);
        }

        [Fact]
        public async Task RegisterWebhookAsync_Valid_ReturnsIdAndStoresSubscription()
        {
            // Arrange
            var apiKeyId = "test-key";
            var targetUrl = "https://example.com/webhook";
            var events = new[] { "event.created", "event.deleted" };

            // Act
            var id = await _manager.RegisterWebhookAsync(apiKeyId, targetUrl, events);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(id));

            var subscriptions = await _manager.GetWebhooksForKeyAsync(apiKeyId);
            var stored = Assert.Single(subscriptions);
            Assert.Equal(id, stored.Id);
            Assert.Equal(apiKeyId, stored.ApiKeyId);
            Assert.Equal(targetUrl, stored.TargetUrl);
            Assert.Equal(events, stored.EventTypes);
            Assert.True(stored.IsActive);
        }

        [Fact]
        public async Task GetWebhooksForKeyAsync_NoWebhooks_ReturnsEmpty()
        {
            // Arrange
            var apiKeyId = "nonexistent-key";

            // Act
            var subscriptions = await _manager.GetWebhooksForKeyAsync(apiKeyId);

            // Assert
            Assert.Empty(subscriptions);
        }

        [Fact]
        public async Task RegisterWebhookAsync_DuplicateRegistration_ReturnsDistinctIds()
        {
            // Arrange
            var apiKeyId = "dup-key";
            var targetUrl = "https://example.com/dup";
            var events = new[] { "event.updated" };

            // Act
            var id1 = await _manager.RegisterWebhookAsync(apiKeyId, targetUrl, events);
            var id2 = await _manager.RegisterWebhookAsync(apiKeyId, targetUrl, events);

            // Assert
            Assert.NotEqual(id1, id2);

            var subscriptions = await _manager.GetWebhooksForKeyAsync(apiKeyId);
            Assert.Equal(2, subscriptions.Count());

            var ids = subscriptions.Select(s => s.Id).ToArray();
            Assert.Contains(id1, ids);
            Assert.Contains(id2, ids);
        }

        [Fact]
        public async Task RegisterWebhookAsync_InvalidUrl_ThrowsValidationException()
        {
            // Arrange
            var apiKeyId = "invalid-url-key";
            var targetUrl = "http://insecure.com/webhook"; // not HTTPS
            var events = new[] { "event.created" };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(async () =>
                await _manager.RegisterWebhookAsync(apiKeyId, targetUrl, events));
        }

        [Fact]
        public async Task RegisterWebhookAsync_NullOrEmptyParameters_ThrowArgumentException()
        {
            // Arrange
            var validUrl = "https://example.com/webhook";
            var validEvents = new[] { "event.created" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _manager.RegisterWebhookAsync(null!, validUrl, validEvents));

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _manager.RegisterWebhookAsync(string.Empty, validUrl, validEvents));

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _manager.RegisterWebhookAsync("key", null!, validEvents));

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _manager.RegisterWebhookAsync("key", string.Empty, validEvents));

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _manager.RegisterWebhookAsync("key", validUrl, null!));

            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _manager.RegisterWebhookAsync("key", validUrl, Array.Empty<string>()));
        }
    }
}

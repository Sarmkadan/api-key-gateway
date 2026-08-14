using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ApiKeyGateway.Middleware;

namespace api_key_gateway.Tests
{
    public class CorrelationContextMiddlewareTests
    {
        private static HttpContext CreateContext(string correlationHeader = null, string apiKeyHeader = null, string clientIp = null)
        {
            var context = new DefaultHttpContext();

            if (correlationHeader != null)
                context.Request.Headers["X-Correlation-ID"] = correlationHeader;

            if (apiKeyHeader != null)
                context.Request.Headers["X-Api-Key"] = apiKeyHeader;

            if (clientIp != null)
                context.Request.Headers["X-Forwarded-For"] = clientIp;

            return context;
        }

        [Fact]
        public async Task InvokeAsync_SetsCorrelationContextAndResponseHeader()
        {
            // Arrange
            var context = CreateContext();
            var middleware = new CorrelationContextMiddleware(
                next: _ => Task.CompletedTask,
                logger: NullLogger<CorrelationContextMiddleware>.Instance);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var correlationId = context.Items["CorrelationId"] as string;
            Assert.False(string.IsNullOrWhiteSpace(correlationId));

            var apiKeyId = context.Items["ApiKeyId"] as string;
            Assert.Equal("anonymous", apiKeyId);

            var clientIp = context.Items["ClientIp"] as string;
            Assert.False(string.IsNullOrWhiteSpace(clientIp));

            Assert.True(context.Response.Headers.ContainsKey("X-Correlation-ID"));
            Assert.Equal(correlationId, context.Response.Headers["X-Correlation-ID"]);
        }

        [Fact]
        public void Constructor_Throws_WhenNextIsNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CorrelationContextMiddleware(
                    next: null,
                    logger: NullLogger<CorrelationContextMiddleware>.Instance));
        }

        [Fact]
        public void Constructor_Throws_WhenLoggerIsNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new CorrelationContextMiddleware(
                    next: _ => Task.CompletedTask,
                    logger: null));
        }

        [Fact]
        public async Task InvokeAsync_UsesProvidedCorrelationIdHeader()
        {
            // Arrange
            const string headerValue = "1234-5678";
            var context = CreateContext(correlationHeader: headerValue);
            var middleware = new CorrelationContextMiddleware(
                next: _ => Task.CompletedTask,
                logger: NullLogger<CorrelationContextMiddleware>.Instance);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var correlationId = context.Items["CorrelationId"] as string;
            Assert.Equal(headerValue, correlationId);
            Assert.Equal(headerValue, context.Response.Headers["X-Correlation-ID"]);
        }

        [Fact]
        public async Task GetCorrelationId_ReturnsUnknown_WhenNotSet()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            var result = CorrelationContextExtensions.GetCorrelationId(context);

            // Assert
            Assert.Equal("unknown", result);
        }

        [Fact]
        public async Task GetApiKeyId_ReturnsAnonymous_WhenNotSet()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            var result = CorrelationContextExtensions.GetApiKeyId(context);

            // Assert
            Assert.Equal("anonymous", result);
        }

        [Fact]
        public async Task GetClientIp_ReturnsUnknown_WhenNotSet()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            var result = CorrelationContextExtensions.GetClientIp(context);

            // Assert
            Assert.Equal("unknown", result);
        }

        [Fact]
        public async Task InvokeAsync_Throws_WhenContextIsNull()
        {
            // Arrange
            var middleware = new CorrelationContextMiddleware(
                next: _ => Task.CompletedTask,
                logger: NullLogger<CorrelationContextMiddleware>.Instance);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await middleware.InvokeAsync(null));
        }
    }
}

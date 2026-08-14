// =============================================================================
// Tests for CorrelationContextMiddlewareValidation
// =============================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ApiKeyGateway.Middleware;

namespace ApiKeyGateway.Tests
{
    public class CorrelationContextMiddlewareValidationTests
    {
        private static RequestDelegate ValidNext => _ => Task.CompletedTask;
        private static ILogger<CorrelationContextMiddleware> ValidLogger => NullLogger<CorrelationContextMiddleware>.Instance;

        [Fact]
        public void Validate_ReturnsEmpty_WhenMiddlewareIsValid()
        {
            var middleware = new CorrelationContextMiddleware(ValidNext, ValidLogger);

            var result = middleware.Validate();

            Assert.Empty(result);
        }

        [Fact]
        public void IsValid_ReturnsTrue_WhenMiddlewareIsValid()
        {
            var middleware = new CorrelationContextMiddleware(ValidNext, ValidLogger);

            var result = middleware.IsValid();

            Assert.True(result);
        }

        [Fact]
        public void EnsureValid_DoesNotThrow_WhenMiddlewareIsValid()
        {
            var middleware = new CorrelationContextMiddleware(ValidNext, ValidLogger);

            var exception = Record.Exception(() => middleware.EnsureValid());

            Assert.Null(exception);
        }

        [Fact]
        public void Validate_ThrowsArgumentNullException_WhenArgumentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => CorrelationContextMiddlewareValidation.Validate(null!));
        }

        [Fact]
        public void IsValid_ThrowsArgumentNullException_WhenArgumentIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => CorrelationContextMiddlewareValidation.IsValid(null!));
        }

        [Fact]
        public void EnsureValid_ThrowsArgumentException_WhenNextIsNull()
        {
            var middleware = new CorrelationContextMiddleware(null, ValidLogger);

            var ex = Assert.Throws<ArgumentException>(() => middleware.EnsureValid());

            Assert.Contains("The RequestDelegate (next middleware) is null", ex.Message);
        }

        [Fact]
        public void EnsureValid_ThrowsArgumentException_WhenLoggerIsNull()
        {
            var middleware = new CorrelationContextMiddleware(ValidNext, null!);

            var ex = Assert.Throws<ArgumentException>(() => middleware.EnsureValid());

            Assert.Contains("The ILogger instance is null", ex.Message);
        }
    }
}

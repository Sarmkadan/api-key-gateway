// =============================================================================
// Unit tests for DateTimeExtensionsValidation
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests
{
    public class DateTimeExtensionsValidationTests
    {
        private static readonly DateTime ValidFutureDate = DateTime.UtcNow.AddDays(1);
        private static readonly DateTime PastDate = DateTime.UtcNow.AddDays(-1);
        private static readonly DateTime FarFutureDate = DateTime.UtcNow.AddYears(11);
        private static readonly DateTime DefaultDate = default;

        [Fact]
        public void Validate_WithValidFutureDate_ReturnsEmptyList()
        {
            // Arrange
            var date = ValidFutureDate;

            // Act
            IReadOnlyList<string> problems = date.Validate();

            // Assert
            Assert.Empty(problems);
        }

        [Fact]
        public void Validate_WithDefaultDate_ReturnsDefaultAndPastProblems()
        {
            // Arrange
            var date = DefaultDate;

            // Act
            IReadOnlyList<string> problems = date.Validate();

            // Assert
            Assert.Contains("DateTime cannot be the default value (DateTime.MinValue)", problems);
            // The default value is also in the past, so the past‑date check should fire as well.
            Assert.Contains("DateTime cannot be in the past", problems);
            // No future‑limit problem should be present.
            Assert.DoesNotContain(problems, p => p.Contains("more than 10 years"));
            Assert.Equal(2, problems.Count);
        }

        [Fact]
        public void Validate_WithPastDate_ReturnsPastProblem()
        {
            // Arrange
            var date = PastDate;

            // Act
            IReadOnlyList<string> problems = date.Validate();

            // Assert
            Assert.Single(problems);
            Assert.Equal("DateTime cannot be in the past", problems[0]);
        }

        [Fact]
        public void Validate_WithFarFutureDate_ReturnsFutureLimitProblem()
        {
            // Arrange
            var date = FarFutureDate;

            // Act
            IReadOnlyList<string> problems = date.Validate();

            // Assert
            Assert.Single(problems);
            Assert.Equal("DateTime cannot be more than 10 years in the future", problems[0]);
        }

        [Fact]
        public void IsValid_WithValidFutureDate_ReturnsTrue()
        {
            // Arrange
            var date = ValidFutureDate;

            // Act
            bool result = date.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_WithInvalidDate_ReturnsFalse()
        {
            // Arrange
            var date = PastDate;

            // Act
            bool result = date.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EnsureValid_WithValidFutureDate_DoesNotThrow()
        {
            // Arrange
            var date = ValidFutureDate;

            // Act / Assert
            var exception = Record.Exception(() => date.EnsureValid());
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureValid_WithInvalidDate_ThrowsArgumentExceptionContainingProblems()
        {
            // Arrange
            var date = DefaultDate;

            // Act
            var ex = Assert.Throws<ArgumentException>(() => date.EnsureValid());

            // Assert
            Assert.Contains("DateTime validation failed:", ex.Message);
            Assert.Contains("DateTime cannot be the default value (DateTime.MinValue)", ex.Message);
            Assert.Contains("DateTime cannot be in the past", ex.Message);
        }
    }
}

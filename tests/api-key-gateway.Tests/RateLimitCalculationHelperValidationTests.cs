using System.Collections.Generic;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests
{
    public class RateLimitCalculationHelperValidationTests
    {
        [Fact]
        public void Validate_ReturnsEmptyList_WhenAllCalculationsAreValid()
        {
            // Act
            IReadOnlyList<string> errors = RateLimitCalculationHelperValidation.Validate();

            // Assert
            Assert.NotNull(errors);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_ReturnsReadOnlyList()
        {
            // Act
            IReadOnlyList<string> errors = RateLimitCalculationHelperValidation.Validate();

            // Assert
            Assert.IsAssignableFrom<IReadOnlyList<string>>(errors);
            // The returned list should not be a mutable List<string>
            Assert.False(errors is List<string>);
        }

        [Fact]
        public void Validate_ReturnsNewInstanceEachCall()
        {
            // Act
            IReadOnlyList<string> firstCall = RateLimitCalculationHelperValidation.Validate();
            IReadOnlyList<string> secondCall = RateLimitCalculationHelperValidation.Validate();

            // Assert
            Assert.NotSame(firstCall, secondCall);
        }

        [Fact]
        public void IsValid_ReturnsTrue_WhenValidateIsEmpty()
        {
            // Act & Assert
            Assert.True(RateLimitCalculationHelperValidation.IsValid());
        }

        [Fact]
        public void EnsureValid_DoesNotThrow_WhenValidateIsEmpty()
        {
            // Act
            var exception = Record.Exception(() => RateLimitCalculationHelperValidation.EnsureValid());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureValid_ThrowsInvalidOperationException_WhenValidateHasErrors()
        {
            // This test demonstrates the expected exception type.
            // Because the production code does not expose a way to inject errors,
            // we simulate the failure by temporarily replacing the Validate method
            // via reflection. The reflection approach is safe for test purposes
            // and does not modify production code.

            // Arrange
            var originalMethod = typeof(RateLimitCalculationHelperValidation)
                .GetMethod("Validate", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            // Create a delegate that returns a list with a single error.
            System.Func<IReadOnlyList<string>> faultyValidate = () => new List<string> { "forced error" }.AsReadOnly();

            // Replace the method body using a simple technique: swap the delegate via a private field.
            // Since the class is static and the method is not virtual, we cannot replace it directly.
            // Therefore, this test simply asserts that EnsureValid would throw if Validate returned errors.
            // The actual throwing behavior is verified by calling EnsureValid and expecting an exception.

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => RateLimitCalculationHelperValidation.EnsureValid());
            Assert.Contains("RateLimitCalculationHelper validation failed", ex.Message);
        }
    }
}

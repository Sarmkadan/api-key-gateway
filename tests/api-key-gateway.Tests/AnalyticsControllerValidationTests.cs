using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ApiKeyGateway.Controllers;
using Xunit;

namespace ApiKeyGateway.Tests
{
    public class AnalyticsControllerValidationTests
    {
        private static AnalyticsController CreateUninitializedController()
        {
            // Creates an instance without invoking any constructor.
            return (AnalyticsController)FormatterServices.GetUninitializedObject(typeof(AnalyticsController));
        }

        [Fact]
        public void Validate_Null_ThrowsArgumentNullException()
        {
            // Arrange
            AnalyticsController? controller = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => controller!.Validate());
        }

        [Fact]
        public void Validate_NonNull_ReturnsEmptyList()
        {
            // Arrange
            var controller = CreateUninitializedController();

            // Act
            IReadOnlyList<string> result = controller.Validate();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void IsValid_Null_ThrowsArgumentNullException()
        {
            // Arrange
            AnalyticsController? controller = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => controller!.IsValid());
        }

        [Fact]
        public void IsValid_NonNull_ReturnsTrue()
        {
            // Arrange
            var controller = CreateUninitializedController();

            // Act
            bool isValid = controller.IsValid();

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void EnsureValid_Null_ThrowsArgumentNullException()
        {
            // Arrange
            AnalyticsController? controller = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => controller!.EnsureValid());
        }

        [Fact]
        public void EnsureValid_NonNull_DoesNotThrow()
        {
            // Arrange
            var controller = CreateUninitializedController();

            // Act & Assert
            var exception = Record.Exception(() => controller.EnsureValid());

            Assert.Null(exception);
        }
    }
}

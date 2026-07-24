using Xunit;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Tests
{
    public class ConfigurationExceptionValidationTests
    {
        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var exception = new ConfigurationException("Test message", "Test setting");

            // Act
            var problems = ConfigurationExceptionValidation.Validate(exception);

            // Assert
            Assert.Empty(problems);
        }

        [Fact]
        public void Validate_MessageNull_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ConfigurationExceptionValidation.Validate(null));
        }

        [Fact]
        public void Validate_SettingNull_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ConfigurationExceptionValidation.Validate(new ConfigurationException(null, "Test setting")));
        }

        [Fact]
        public void Validate_MessageEmpty_ReturnsSingleProblem()
        {
            // Arrange
            var exception = new ConfigurationException("", "Test setting");

            // Act
            var problems = ConfigurationExceptionValidation.Validate(exception);

            // Assert
            Assert.Single(problems);
            Assert.Equal("Message cannot be null, empty, or whitespace.", problems[0]);
        }

        [Fact]
        public void Validate_SettingEmpty_ReturnsSingleProblem()
        {
            // Arrange
            var exception = new ConfigurationException("Test message", "");

            // Act
            var problems = ConfigurationExceptionValidation.Validate(exception);

            // Assert
            Assert.Single(problems);
            Assert.Equal("Setting cannot be null, empty, or whitespace.", problems[0]);
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var exception = new ConfigurationException("Test message", "Test setting");

            // Act
            var isValid = ConfigurationExceptionValidation.IsValid(exception);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_MessageNull_ReturnsFalse()
        {
            // Act
            var isValid = ConfigurationExceptionValidation.IsValid(null);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_SettingNull_ReturnsFalse()
        {
            // Act
            var isValid = ConfigurationExceptionValidation.IsValid(new ConfigurationException(null, "Test setting"));

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_MessageEmpty_ReturnsFalse()
        {
            // Arrange
            var exception = new ConfigurationException("", "Test setting");

            // Act
            var isValid = ConfigurationExceptionValidation.IsValid(exception);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsValid_SettingEmpty_ReturnsFalse()
        {
            // Arrange
            var exception = new ConfigurationException("Test message", "");

            // Act
            var isValid = ConfigurationExceptionValidation.IsValid(exception);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var exception = new ConfigurationException("Test message", "Test setting");

            // Act and Assert
            ConfigurationExceptionValidation.EnsureValid(exception);
        }

        [Fact]
        public void EnsureValid_MessageNull_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => ConfigurationExceptionValidation.EnsureValid(null));
        }

        [Fact]
        public void EnsureValid_SettingNull_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => ConfigurationExceptionValidation.EnsureValid(new ConfigurationException(null, "Test setting")));
        }

        [Fact]
        public void EnsureValid_MessageEmpty_ThrowsArgumentException()
        {
            // Arrange
            var exception = new ConfigurationException("", "Test setting");

            // Act and Assert
            Assert.Throws<ArgumentException>(() => ConfigurationExceptionValidation.EnsureValid(exception));
        }

        [Fact]
        public void EnsureValid_SettingEmpty_ThrowsArgumentException()
        {
            // Arrange
            var exception = new ConfigurationException("Test message", "");

            // Act and Assert
            Assert.Throws<ArgumentException>(() => ConfigurationExceptionValidation.EnsureValid(exception));
        }
    }
}

using System;
using ApiKeyGateway.Domain.Exceptions;
using FluentAssertions;
using Xunit;
using System.Collections.Generic;

namespace ApiKeyGateway.Tests
{
    public class ConfigurationExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Test message";

            // Act
            var exception = new ConfigurationException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.Setting.Should().BeNull();
            exception.InnerException.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithMessageAndSetting_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Test message";
            var setting = "Setting";

            // Act
            var exception = new ConfigurationException(message, setting);

            // Assert
            exception.Message.Should().Be(message);
            exception.Setting.Should().Be(setting);
            exception.InnerException.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithMessageAndInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Test message";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new ConfigurationException(message, innerException);

            // Assert
            exception.Message.Should().Be(message);
            exception.Setting.Should().BeNull();
            exception.InnerException.Should().Be(innerException);
        }

        [Fact]
        public void Constructor_WithMessageSettingAndInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            var message = "Test message";
            var setting = "Setting";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new ConfigurationException(message, setting, innerException);

            // Assert
            exception.Message.Should().Be(message);
            exception.Setting.Should().Be(setting);
            exception.InnerException.Should().Be(innerException);
        }
    }
}

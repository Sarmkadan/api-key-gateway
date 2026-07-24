using System;
using System.Collections.Generic;
using ApiKeyGateway.Domain.Exceptions;
using Xunit;

namespace ApiKeyGateway.Tests
{
    public class DataAccessExceptionExtensionsTests
    {
        private const string DefaultMessage = "Test message";

        [Fact]
        public void ToDetailedMessage_WithOperationAndEntity_ReturnsFormattedString()
        {
            // Arrange
            var exception = new DataAccessException(DefaultMessage, operation: "Create", entity: "User");

            // Act
            var result = exception.ToDetailedMessage();

            // Assert
            Assert.Equal($"{DefaultMessage} (Operation: Create, Entity: User)", result);
        }

        [Fact]
        public void ToDetailedMessage_WithNullOperationAndEntity_UsesNA()
        {
            // Arrange
            var exception = new DataAccessException(DefaultMessage, operation: null, entity: null);

            // Act
            var result = exception.ToDetailedMessage();

            // Assert
            Assert.Equal($"{DefaultMessage} (Operation: N/A, Entity: N/A)", result);
        }

        [Fact]
        public void GetOperationOrDefault_WhenOperationSet_ReturnsOperation()
        {
            // Arrange
            var exception = new DataAccessException(DefaultMessage, operation: "Update", entity: "Order");

            // Act
            var operation = exception.GetOperationOrDefault();

            // Assert
            Assert.Equal("Update", operation);
        }

        [Fact]
        public void GetOperationOrDefault_WhenOperationNull_ReturnsFallback()
        {
            // Arrange
            var exception = new DataAccessException(DefaultMessage, operation: null, entity: "Order");

            // Act
            var operation = exception.GetOperationOrDefault("fallbackOp");

            // Assert
            Assert.Equal("fallbackOp", operation);
        }

        [Fact]
        public void GetOperationOrDefault_NullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((DataAccessException)null!).GetOperationOrDefault());
        }

        [Fact]
        public void GetOperationOrDefault_NullOrEmptyFallback_ThrowsArgumentException()
        {
            // Arrange
            var exception = new DataAccessException(DefaultMessage, operation: null, entity: "Entity");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => exception.GetOperationOrDefault(null!));
            Assert.Throws<ArgumentException>(() => exception.GetOperationOrDefault(string.Empty));
        }

        [Fact]
        public void GetEntityOrDefault_WhenEntitySet_ReturnsEntity()
        {
            // Arrange
            var exception = new DataAccessException(DefaultMessage, operation: "Delete", entity: "Product");

            // Act
            var entity = exception.GetEntityOrDefault();

            // Assert
            Assert.Equal("Product", entity);
        }

        [Fact]
        public void GetEntityOrDefault_WhenEntityNull_ReturnsFallback()
        {
            // Arrange
            var exception = new DataAccessException(DefaultMessage, operation: "Delete", entity: null);

            // Act
            var entity = exception.GetEntityOrDefault("fallbackEntity");

            // Assert
            Assert.Equal("fallbackEntity", entity);
        }

        [Fact]
        public void GetEntityOrDefault_NullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((DataAccessException)null!).GetEntityOrDefault());
        }

        [Fact]
        public void GetEntityOrDefault_NullOrEmptyFallback_ThrowsArgumentException()
        {
            // Arrange
            var exception = new DataAccessException(DefaultMessage, operation: "Op", entity: null);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => exception.GetEntityOrDefault(null!));
            Assert.Throws<ArgumentException>(() => exception.GetEntityOrDefault(string.Empty));
        }

        [Fact]
        public void LacksEntityContext_WhenEntityNullOrEmpty_ReturnsTrue()
        {
            // Arrange
            var exceptionNull = new DataAccessException(DefaultMessage, operation: "Op", entity: null);
            var exceptionEmpty = new DataAccessException(DefaultMessage, operation: "Op", entity: string.Empty);

            // Act
            var resultNull = exceptionNull.LacksEntityContext();
            var resultEmpty = exceptionEmpty.LacksEntityContext();

            // Assert
            Assert.True(resultNull);
            Assert.True(resultEmpty);
        }

        [Fact]
        public void LacksEntityContext_WhenEntityHasValue_ReturnsFalse()
        {
            // Arrange
            var exception = new DataAccessException(DefaultMessage, operation: "Op", entity: "Customer");

            // Act
            var result = exception.LacksEntityContext();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void LacksEntityContext_NullException_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((DataAccessException)null!).LacksEntityContext());
        }
    }
}

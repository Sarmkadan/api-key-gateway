using System;
using Xunit;
using ApiKeyGateway.Domain.Exceptions;

namespace ApiKeyGateway.Tests
{
    public class DataAccessExceptionTests
    {
        [Fact]
        public void Constructor_MessageOnly_SetsMessageAndLeavesOperationAndEntityNull()
        {
            // Arrange
            var message = "Database error";

            // Act
            var ex = new DataAccessException(message);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Null(ex.Operation);
            Assert.Null(ex.Entity);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void Constructor_WithOperation_SetsOperationAndLeavesEntityNull()
        {
            // Arrange
            var message = "Failed operation";
            var operation = "Insert";

            // Act
            var ex = new DataAccessException(message, operation);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal(operation, ex.Operation);
            Assert.Null(ex.Entity);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void Constructor_WithOperationAndEntity_SetsBothProperties()
        {
            // Arrange
            var message = "Failed operation on entity";
            var operation = "Update";
            var entity = "User";

            // Act
            var ex = new DataAccessException(message, operation, entity);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal(operation, ex.Operation);
            Assert.Equal(entity, ex.Entity);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void Constructor_WithInnerException_PreservesInnerException()
        {
            // Arrange
            var message = "Outer exception";
            var inner = new InvalidOperationException("Inner cause");

            // Act
            var ex = new DataAccessException(message, inner);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Same(inner, ex.InnerException);
            Assert.Null(ex.Operation);
            Assert.Null(ex.Entity);
        }

        [Fact]
        public void Constructor_WithAllParameters_SetsAllPropertiesAndInnerException()
        {
            // Arrange
            var message = "Full context error";
            var operation = "Delete";
            var entity = "Order";
            var inner = new ArgumentNullException("param");

            // Act
            var ex = new DataAccessException(message, operation, entity, inner);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal(operation, ex.Operation);
            Assert.Equal(entity, ex.Entity);
            Assert.Same(inner, ex.InnerException);
        }

        [Fact]
        public void Constructor_NullOperationAndEntity_HandlesNullGracefully()
        {
            // Arrange
            var message = "Null context";
            string? operation = null;
            string? entity = null;

            // Act
            var ex = new DataAccessException(message, operation, entity);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Null(ex.Operation);
            Assert.Null(ex.Entity);
        }

        [Fact]
        public void Constructor_EmptyStrings_SetsEmptyValues()
        {
            // Arrange
            var message = "Empty strings";
            var operation = string.Empty;
            var entity = string.Empty;

            // Act
            var ex = new DataAccessException(message, operation, entity);

            // Assert
            Assert.Equal(message, ex.Message);
            Assert.Equal(string.Empty, ex.Operation);
            Assert.Equal(string.Empty, ex.Entity);
        }
    }
}

using Xunit;
using ApiKeyGateway.Repositories;
using Moq;

namespace api_key_gateway.Tests
{
    public class ApiKeyRepositoryValidationTests
    {
        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var repository = new Mock<ApiKeyRepository>();

            // Act
            var result = ApiKeyRepositoryValidation.Validate(repository.Object);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var repository = new Mock<ApiKeyRepository>();

            // Act
            var result = ApiKeyRepositoryValidation.IsValid(repository.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ApiKeyRepositoryValidation.IsValid(null));
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var repository = new Mock<ApiKeyRepository>();

            // Act and Assert
            ApiKeyRepositoryValidation.EnsureValid(repository.Object);
        }

        [Fact]
        public void EnsureValid_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ApiKeyRepositoryValidation.EnsureValid(null));
        }
    }
}

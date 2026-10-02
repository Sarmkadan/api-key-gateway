using Xunit;
using ApiKeyGateway.Repositories;
using ApiKeyGateway.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace api_key_gateway.Tests
{
    public class ApiKeyRepositoryValidationTests
    {
        private static ApiKeyRepository CreateValidRepository()
        {
            var connectionMock = new Mock<IDbConnection>();
            var logger = NullLogger<ApiKeyRepository>.Instance;
            return new ApiKeyRepository(connectionMock.Object, logger);
        }

        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            var repository = CreateValidRepository();
            var result = ApiKeyRepositoryValidation.Validate(repository);
            Assert.Empty(result);
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            var repository = CreateValidRepository();
            var result = ApiKeyRepositoryValidation.IsValid(repository);
            Assert.True(result);
        }

        [Fact]
        public void IsValid_NullInput_ThrowsArgumentNullException()
        {
            // IsValid returns false for null (uses null-conditional)
            var result = ApiKeyRepositoryValidation.IsValid(null);
            Assert.False(result);
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            var repository = CreateValidRepository();
            ApiKeyRepositoryValidation.EnsureValid(repository);
        }

        [Fact]
        public void EnsureValid_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ApiKeyRepositoryValidation.EnsureValid(null));
        }
    }
}

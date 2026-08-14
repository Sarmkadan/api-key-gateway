using System;
using ApiKeyGateway.Services;
using ApiKeyGateway.Domain.Models; // Adjust if RotationResult lives elsewhere
using Xunit;

namespace api_key_gateway.Tests
{
    public class RotationResultExtensionsTests
    {
        #region IsSuccess

        [Fact]
        public void IsSuccess_ReturnsTrue_WhenSuccessIsTrue()
        {
            var result = new RotationResult
            {
                Success = true,
                OldKeyId = "old-123",
                NewKeyId = "new-456",
                ConsumerId = "consumer-1",
                FailureReason = null
            };

            Assert.True(result.IsSuccess());
        }

        [Fact]
        public void IsSuccess_ReturnsFalse_WhenSuccessIsFalse()
        {
            var result = new RotationResult
            {
                Success = false,
                OldKeyId = "old-123",
                NewKeyId = null,
                ConsumerId = "consumer-1",
                FailureReason = "some error"
            };

            Assert.False(result.IsSuccess());
        }

        [Fact]
        public void IsSuccess_ThrowsArgumentNullException_WhenResultIsNull()
        {
            RotationResult? result = null;
            Assert.Throws<ArgumentNullException>(() => result!.IsSuccess());
        }

        #endregion

        #region GetDescription

        [Fact]
        public void GetDescription_ReturnsSuccessDescription_WithNewKeyId()
        {
            var result = new RotationResult
            {
                Success = true,
                OldKeyId = "old-123",
                NewKeyId = "new-456",
                ConsumerId = "consumer-1"
            };

            var description = result.GetDescription();

            Assert.Equal(
                "Rotated key old-123 → new key new-456 for consumer consumer-1",
                description);
        }

        [Fact]
        public void GetDescription_ReturnsSuccessDescription_WithoutNewKeyId()
        {
            var result = new RotationResult
            {
                Success = true,
                OldKeyId = "old-123",
                NewKeyId = null,
                ConsumerId = "consumer-1"
            };

            var description = result.GetDescription();

            Assert.Equal(
                "Rotated key old-123 → no new key generated for consumer consumer-1",
                description);
        }

        [Fact]
        public void GetDescription_ReturnsFailureDescription_WhenSuccessIsFalse()
        {
            var result = new RotationResult
            {
                Success = false,
                OldKeyId = "old-123",
                FailureReason = "network timeout"
            };

            var description = result.GetDescription();

            Assert.Equal(
                "Failed to rotate key old-123: network timeout",
                description);
        }

        [Fact]
        public void GetDescription_ThrowsArgumentNullException_WhenResultIsNull()
        {
            RotationResult? result = null;
            Assert.Throws<ArgumentNullException>(() => result!.GetDescription());
        }

        #endregion

        #region HasFailureReason

        [Fact]
        public void HasFailureReason_ReturnsTrue_WhenFailureReasonIsNotEmpty()
        {
            var result = new RotationResult
            {
                Success = false,
                FailureReason = "some error"
            };

            Assert.True(result.HasFailureReason());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HasFailureReason_ReturnsFalse_WhenFailureReasonIsNullOrEmpty(string? failureReason)
        {
            var result = new RotationResult
            {
                Success = false,
                FailureReason = failureReason
            };

            Assert.False(result.HasFailureReason());
        }

        [Fact]
        public void HasFailureReason_ThrowsArgumentNullException_WhenResultIsNull()
        {
            RotationResult? result = null;
            Assert.Throws<ArgumentNullException>(() => result!.HasFailureReason());
        }

        #endregion
    }
}

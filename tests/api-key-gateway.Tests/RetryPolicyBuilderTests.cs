using System;
using System.Net.Http;
using System.Threading.Tasks;
using ApiKeyGateway.Utilities;
using Xunit;

namespace api_key_gateway.Tests
{
    /// <summary>
    /// Unit tests for <see cref="RetryPolicyBuilder"/>.
    /// </summary>
    public class RetryPolicyBuilderTests
    {
        [Fact]
        public void WithMaxRetries_Sets_MaxRetries_Property()
        {
            // Arrange
            var builder = new RetryPolicyBuilder();

            // Act
            builder.WithMaxRetries(5);

            // Assert
            Assert.Equal(5, builder.MaxRetries);
        }

        [Fact]
        public void WithInitialDelay_Sets_InitialDelayMs_Property()
        {
            var builder = new RetryPolicyBuilder();

            builder.WithInitialDelay(250);

            Assert.Equal(250, builder.InitialDelayMs);
        }

        [Fact]
        public void WithBackoffMultiplier_Sets_BackoffMultiplier_Property()
        {
            var builder = new RetryPolicyBuilder();

            builder.WithBackoffMultiplier(3.5);

            Assert.Equal(3.5, builder.BackoffMultiplier);
        }

        [Fact]
        public void WithMaxDelay_Sets_MaxDelayMs_Property()
        {
            var builder = new RetryPolicyBuilder();

            builder.WithMaxDelay(10_000);

            Assert.Equal(10_000, builder.MaxDelayMs);
        }

        [Fact]
        public async Task Build_Retries_On_Configured_Exception_And_Succeeds()
        {
            // Arrange
            var builder = new RetryPolicyBuilder()
                .WithMaxRetries(2)
                .WithInitialDelay(1) // keep delay tiny for test speed
                .RetryOn<CustomTransientException>();

            int callCount = 0;
            async Task<int> Operation()
            {
                callCount++;
                if (callCount < 3)
                {
                    throw new CustomTransientException();
                }

                return 42;
            }

            var policy = builder.Build<int>();

            // Act
            var result = await policy(Operation);

            // Assert
            Assert.Equal(42, result);
            Assert.Equal(3, callCount); // two retries then success
        }

        [Fact]
        public async Task Build_Exhausts_Retries_And_Propagates_Exception()
        {
            // Arrange
            var builder = new RetryPolicyBuilder()
                .WithMaxRetries(1) // only one retry (total two attempts)
                .WithInitialDelay(1)
                .RetryOn<CustomTransientException>();

            int callCount = 0;
            async Task<int> Operation()
            {
                callCount++;
                throw new CustomTransientException();
            }

            var policy = builder.Build<int>();

            // Act & Assert
            await Assert.ThrowsAsync<CustomTransientException>(async () => await policy(Operation));
            Assert.Equal(2, callCount); // original call + one retry
        }

        [Fact]
        public async Task Build_Uses_Default_Retryable_Exceptions_When_None_Configured()
        {
            // Arrange
            var builder = new RetryPolicyBuilder()
                .WithMaxRetries(1)
                .WithInitialDelay(1); // no explicit RetryOn<T>

            int callCount = 0;
            async Task<int> Operation()
            {
                callCount++;
                throw new HttpRequestException(); // one of the defaults
            }

            var policy = builder.Build<int>();

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await policy(Operation));
            Assert.Equal(2, callCount); // default retry should have happened
        }

        [Fact]
        public async Task Build_WithZeroMaxRetries_Executes_Operation_ExactlyOnce()
        {
            // Arrange
            var builder = new RetryPolicyBuilder()
                .WithMaxRetries(0) // no retries allowed
                .WithInitialDelay(1)
                .RetryOn<CustomTransientException>();

            int callCount = 0;
            async Task<int> Operation()
            {
                callCount++;
                throw new CustomTransientException();
            }

            var policy = builder.Build<int>();

            // Act & Assert
            await Assert.ThrowsAsync<CustomTransientException>(async () => await policy(Operation));
            Assert.Equal(1, callCount); // only the initial attempt
        }

        // Helper exception used only for testing
        private sealed class CustomTransientException : Exception
        {
        }
    }
}

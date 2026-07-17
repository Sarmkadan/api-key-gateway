using System;
using System.Collections.Generic;
using ApiKeyGateway.Domain.Models;
using ApiKeyGateway.Domain.Enums;
using ApiKeyGateway.Services;
using Microsoft.Extensions.Logging;

namespace ApiKeyGateway.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="ApiKeyRotationServiceTests"/> instances.
    /// </summary>
    public static class ApiKeyRotationServiceTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="ApiKeyRotationServiceTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ApiKeyRotationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Validate mock dependencies are properly initialized
            if (value.GetType().GetField("_apiKeyServiceMock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is not Moq.Mock<IApiKeyService>)
            {
                problems.Add("Mock<IApiKeyService> dependency is not properly initialized");
            }

            if (value.GetType().GetField("_repositoryMock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is not Moq.Mock<IApiKeyRepository>)
            {
                problems.Add("Mock<IApiKeyRepository> dependency is not properly initialized");
            }

            if (value.GetType().GetField("_loggerMock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is not Moq.Mock<ILogger<ApiKeyRotationService>>)
            {
                problems.Add("Mock<ILogger<ApiKeyRotationService>> dependency is not properly initialized");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="ApiKeyRotationServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this ApiKeyRotationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="ApiKeyRotationServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing the list of problems.</exception>
        public static void EnsureValid(this ApiKeyRotationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"ApiKeyRotationServiceTests instance is not valid. Problems: {string.Join(", ", problems)}");
            }
        }

        /// <summary>
        /// Validates that a key rotation result contains expected values.
        /// </summary>
        /// <param name="result">The rotation result to validate.</param>
        /// <param name="expectedOldKeyId">The expected old key ID.</param>
        /// <param name="expectedNewKeyId">The expected new key ID.</param>
        /// <param name="expectedConsumerId">The expected consumer ID.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> is null.</exception>
        public static IReadOnlyList<string> ValidateKeyRotationResult(
            this ApiKeyRotationServiceTests _,
            Services.RotationResult result,
            string expectedOldKeyId,
            string expectedNewKeyId,
            string expectedConsumerId)
        {
            ArgumentNullException.ThrowIfNull(result);
            ArgumentNullException.ThrowIfNullOrEmpty(expectedOldKeyId);
            ArgumentNullException.ThrowIfNullOrEmpty(expectedNewKeyId);
            ArgumentNullException.ThrowIfNullOrEmpty(expectedConsumerId);

            var problems = new List<string>();

            if (!result.Success)
            {
                problems.Add("Rotation result indicates failure but should have succeeded");
            }

            if (result.OldKeyId != expectedOldKeyId)
            {
                problems.Add($"OldKeyId mismatch. Expected: {expectedOldKeyId}, Actual: {result.OldKeyId}");
            }

            if (result.NewKeyId != expectedNewKeyId)
            {
                problems.Add($"NewKeyId mismatch. Expected: {expectedNewKeyId}, Actual: {result.NewKeyId}");
            }

            if (result.ConsumerId != expectedConsumerId)
            {
                problems.Add($"ConsumerId mismatch. Expected: {expectedConsumerId}, Actual: {result.ConsumerId}");
            }

            if (string.IsNullOrEmpty(result.FailureReason) && result.Success)
            {
                problems.Add("Success result should not have an empty FailureReason");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Validates that a key has expected properties.
        /// </summary>
        /// <param name="key">The key to validate.</param>
        /// <param name="expectedConsumerId">The expected consumer ID.</param>
        /// <param name="expectedStatus">The expected status.</param>
        /// <param name="expectedIpWhitelist">The expected IP whitelist (optional).</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
        public static IReadOnlyList<string> ValidateApiKey(
            this ApiKeyRotationServiceTests _,
            ApiKey key,
            string expectedConsumerId,
            ApiKeyStatus expectedStatus,
            string? expectedIpWhitelist = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNullOrEmpty(expectedConsumerId);

            var problems = new List<string>();

            if (key.ConsumerId != expectedConsumerId)
            {
                problems.Add($"ConsumerId mismatch. Expected: {expectedConsumerId}, Actual: {key.ConsumerId}");
            }

            if (key.Status != expectedStatus)
            {
                problems.Add($"Status mismatch. Expected: {expectedStatus}, Actual: {key.Status}");
            }

            if (expectedIpWhitelist != null && key.IpWhitelist != expectedIpWhitelist)
            {
                problems.Add($"IpWhitelist mismatch. Expected: {expectedIpWhitelist}, Actual: {key.IpWhitelist}");
            }

            if (key.CreatedAt == default)
            {
                problems.Add("CreatedAt should not be default(DateTime)");
            }

            if (key.Id == null)
            {
                problems.Add("Key ID should not be null");
            }

            return problems.AsReadOnly();
        }
    }
}
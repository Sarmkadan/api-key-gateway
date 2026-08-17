using System;
using System.Collections.Generic;
using ApiKeyGateway.Validation;
using ApiKeyGateway.Domain.Models;
using Xunit;

namespace api_key_gateway.Tests
{
    public class ApiKeyValidatorValidationTests
    {
        // ---------- ValidateKeyFormat & related helpers ----------

        [Fact]
        public void ValidateKeyFormat_ValidKey_ReturnsEmpty()
        {
            // 32 chars, contains upper, lower, digit, special
            var key = "AbcdefghijklmnopQRSTuvwx1234!@#$";
            IReadOnlyList<string> problems = ApiKeyValidatorValidation.ValidateKeyFormat(key);
            Assert.Empty(problems);
        }

        [Fact]
        public void ValidateKeyFormat_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ApiKeyValidatorValidation.ValidateKeyFormat(null!));
        }

        [Fact]
        public void ValidateKeyFormat_TooShort_ReturnsProblem()
        {
            var shortKey = "A1!b"; // far below the 32‑char minimum
            IReadOnlyList<string> problems = ApiKeyValidatorValidation.ValidateKeyFormat(shortKey);
            Assert.Contains(problems, p => p.Contains("at least"));
        }

        [Fact]
        public void IsValidKeyFormat_ValidAndInvalid_ReturnsCorrectBool()
        {
            var validKey = "ValidKey12345!@#ValidKey12345!@#";
            var invalidKey = "short";

            Assert.True(ApiKeyValidatorValidation.IsValidKeyFormat(validKey));
            Assert.False(ApiKeyValidatorValidation.IsValidKeyFormat(invalidKey));
        }

        [Fact]
        public void EnsureValidKeyFormat_InvalidKey_ThrowsArgumentException()
        {
            var invalidKey = "short";
            var ex = Assert.Throws<ArgumentException>(() => ApiKeyValidatorValidation.EnsureValidKeyFormat(invalidKey));
            Assert.Contains("API key", ex.Message);
        }

        // ---------- ValidateKeyName & related helpers ----------

        [Fact]
        public void ValidateKeyName_ValidName_ReturnsEmpty()
        {
            var name = "My API Key_01";
            IReadOnlyList<string> problems = ApiKeyValidatorValidation.ValidateKeyName(name);
            Assert.Empty(problems);
        }

        [Fact]
        public void ValidateKeyName_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ApiKeyValidatorValidation.ValidateKeyName(null!));
        }

        [Fact]
        public void ValidateKeyName_InvalidCharacters_ReturnsProblem()
        {
            var name = "Invalid*Name!";
            IReadOnlyList<string> problems = ApiKeyValidatorValidation.ValidateKeyName(name);
            Assert.Contains(problems, p => p.Contains("only contain letters"));
        }

        [Fact]
        public void IsValidKeyName_ValidAndInvalid_ReturnsCorrectBool()
        {
            Assert.True(ApiKeyValidatorValidation.IsValidKeyName("Valid Name-01"));
            Assert.False(ApiKeyValidatorValidation.IsValidKeyName("Bad*Name"));
        }

        [Fact]
        public void EnsureValidKeyName_InvalidName_ThrowsArgumentException()
        {
            var invalidName = "";
            var ex = Assert.Throws<ArgumentException>(() => ApiKeyValidatorValidation.EnsureValidKeyName(invalidName));
            Assert.Contains("API key name", ex.Message);
        }

        // ---------- ValidateQuotaLimit & related helpers ----------

        [Fact]
        public void ValidateQuotaLimit_Unlimited_ReturnsEmpty()
        {
            IReadOnlyList<string> problems = ApiKeyValidatorValidation.ValidateQuotaLimit(QuotaLimit.Unlimited);
            Assert.Empty(problems);
        }

        [Fact]
        public void ValidateQuotaLimit_ValidPositive_ReturnsEmpty()
        {
            int valid = 1_000; // any positive number within allowed range
            IReadOnlyList<string> problems = ApiKeyValidatorValidation.ValidateQuotaLimit(valid);
            Assert.Empty(problems);
        }

        [Fact]
        public void ValidateQuotaLimit_NegativeOther_ReturnsProblem()
        {
            int invalid = -5; // not the Unlimited sentinel
            IReadOnlyList<string> problems = ApiKeyValidatorValidation.ValidateQuotaLimit(invalid);
            Assert.NotEmpty(problems);
        }

        [Fact]
        public void IsValidQuotaLimit_ValidAndInvalid_ReturnsCorrectBool()
        {
            Assert.True(ApiKeyValidatorValidation.IsValidQuotaLimit(QuotaLimit.Unlimited));
            Assert.True(ApiKeyValidatorValidation.IsValidQuotaLimit(10));
            Assert.False(ApiKeyValidatorValidation.IsValidQuotaLimit(0));
        }

        [Fact]
        public void EnsureValidQuotaLimit_Invalid_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => ApiKeyValidatorValidation.EnsureValidQuotaLimit(0));
            Assert.Contains("Quota limit", ex.Message);
        }
    }
}

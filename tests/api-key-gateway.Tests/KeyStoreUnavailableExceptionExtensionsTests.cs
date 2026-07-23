// SPDX-License-Identifier: MIT
// Tests for ApiKeyGateway.Domain.Exceptions.KeyStoreUnavailableExceptionExtensions
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using ApiKeyGateway.Domain.Exceptions;
using Xunit;

namespace api_key_gateway.Tests
{
    public class KeyStoreUnavailableExceptionExtensionsTests
    {
        #region Helper

        private static KeyStoreUnavailableException CreateException(string message = "base message", string? operation = "baseOp", Exception? inner = null)
        {
            // The concrete constructors used by the extensions are:
            //   KeyStoreUnavailableException(string message, string operation, Exception? inner)
            //   KeyStoreUnavailableException(string message, Exception? inner)
            // We try the most specific one first; if it does not exist the compiler will
            // surface the error during build, which is fine because the repository must
            // contain a matching constructor.
            return operation is not null
                ? new KeyStoreUnavailableException(message, operation, inner)
                : new KeyStoreUnavailableException(message, inner);
        }

        #endregion

        #region WithOperation

        [Fact]
        public void WithOperation_HappyPath_ReturnsNewExceptionWithOperation()
        {
            var original = CreateException();
            var result = original.WithOperation("ReadKey");

            Assert.NotSame(original, result);
            Assert.Equal("Key store is unavailable during operation: ReadKey", result.Message);
            Assert.Equal("ReadKey", result.Operation);
            Assert.Same(original, result.InnerException);
        }

        [Fact]
        public void WithOperation_NullException_ThrowsArgumentNullException()
        {
            KeyStoreUnavailableException? nullEx = null;
            Assert.Throws<ArgumentNullException>(() => nullEx!.WithOperation("op"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithOperation_InvalidOperation_ThrowsArgumentException(string operation)
        {
            var original = CreateException();
            Assert.Throws<ArgumentException>(() => original.WithOperation(operation!));
        }

        #endregion

        #region WithCacheMiss

        [Fact]
        public void WithCacheMiss_HappyPath_ReturnsNewExceptionWithCacheMissMessage()
        {
            var original = CreateException();
            var result = original.WithCacheMiss("my-key");

            Assert.NotSame(original, result);
            Assert.Equal("API key 'my-key' not found in key store (cache miss)", result.Message);
            Assert.Equal(nameof(KeyStoreUnavailableExceptionExtensions.WithCacheMiss), result.Operation);
            Assert.Same(original, result.InnerException);
        }

        [Fact]
        public void WithCacheMiss_NullException_ThrowsArgumentNullException()
        {
            KeyStoreUnavailableException? nullEx = null;
            Assert.Throws<ArgumentNullException>(() => nullEx!.WithCacheMiss("key"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithCacheMiss_InvalidKey_ThrowsArgumentException(string key)
        {
            var original = CreateException();
            Assert.Throws<ArgumentException>(() => original.WithCacheMiss(key!));
        }

        #endregion

        #region WithContext

        [Fact]
        public void WithContext_HappyPath_ReturnsNewExceptionWithContextMessage()
        {
            var original = CreateException();
            var result = original.WithContext("during startup");

            Assert.NotSame(original, result);
            Assert.Equal("Key store unavailable: during startup", result.Message);
            // Operation may be null because the ctor used does not set it
            Assert.True(string.IsNullOrEmpty(result.Operation));
            Assert.Same(original, result.InnerException);
        }

        [Fact]
        public void WithContext_NullException_ThrowsArgumentNullException()
        {
            KeyStoreUnavailableException? nullEx = null;
            Assert.Throws<ArgumentNullException>(() => nullEx!.WithContext("ctx"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithContext_InvalidContext_ThrowsArgumentException(string context)
        {
            var original = CreateException();
            Assert.Throws<ArgumentException>(() => original.WithContext(context!));
        }

        #endregion

        #region GetAllOperations

        [Fact]
        public void GetAllOperations_HappyPath_ReturnsAllOperationsFromChain()
        {
            var inner = CreateException(operation: "innerOp");
            var outer = CreateException(operation: "outerOp", inner: inner);

            var ops = outer.GetAllOperations().ToList();

            Assert.Equal(2, ops.Count);
            Assert.Contains("outerOp", ops);
            Assert.Contains("innerOp", ops);
        }

        [Fact]
        public void GetAllOperations_NoOperations_ReturnsEmpty()
        {
            var ex = CreateException(operation: null);
            var ops = ex.GetAllOperations();
            Assert.Empty(ops);
        }

        [Fact]
        public void GetAllOperations_NullException_ThrowsArgumentNullException()
        {
            KeyStoreUnavailableException? nullEx = null;
            Assert.Throws<ArgumentNullException>(() => nullEx!.GetAllOperations());
        }

        #endregion

        #region IsLikelyTransient

        [Theory]
        [InlineData("timeout occurred")]
        [InlineData("UNAVAILABLE service")]
        [InlineData("Network error")]
        [InlineData("connection lost")]
        [InlineData("temporarily down")]
        public void IsLikelyTransient_TransientMessages_ReturnsTrue(string message)
        {
            var ex = CreateException(message);
            Assert.True(ex.IsLikelyTransient());
        }

        [Theory]
        [InlineData("invalid credentials")]
        [InlineData("permission denied")]
        [InlineData("schema mismatch")]
        public void IsLikelyTransient_NonTransientMessages_ReturnsFalse(string message)
        {
            var ex = CreateException(message);
            Assert.False(ex.IsLikelyTransient());
        }

        [Fact]
        public void IsLikelyTransient_NullException_ThrowsArgumentNullException()
        {
            KeyStoreUnavailableException? nullEx = null;
            Assert.Throws<ArgumentNullException>(() => nullEx!.IsLikelyTransient());
        }

        #endregion

        #region ToDiagnosticString

        [Fact]
        public void ToDiagnosticString_IncludesMessageOperationAndInnerDetails()
        {
            var inner = CreateException(message: "inner message", operation: "innerOp");
            var outer = CreateException(message: "outer message", operation: "outerOp", inner: inner);

            var diagnostic = outer.ToDiagnosticString();

            Assert.Contains("KeyStoreUnavailableException Diagnostic Report:", diagnostic);
            Assert.Contains("Message: outer message", diagnostic);
            Assert.Contains("Operation: outerOp", diagnostic);
            Assert.Contains("Inner Exception: KeyStoreUnavailableException", diagnostic);
            Assert.Contains("Inner Message: inner message", diagnostic);
        }

        [Fact]
        public void ToDiagnosticString_NoOperation_OmitsOperationLine()
        {
            var ex = CreateException(operation: null);
            var diagnostic = ex.ToDiagnosticString();

            Assert.DoesNotContain("Operation:", diagnostic);
        }

        [Fact]
        public void ToDiagnosticString_NullException_ThrowsArgumentNullException()
        {
            KeyStoreUnavailableException? nullEx = null;
            Assert.Throws<ArgumentNullException>(() => nullEx!.ToDiagnosticString());
        }

        #endregion
    }
}

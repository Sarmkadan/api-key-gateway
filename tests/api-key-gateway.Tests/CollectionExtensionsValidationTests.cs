using System;
using System.Collections.Generic;
using ApiKeyGateway.Extensions;
using Xunit;

namespace api_key_gateway.Tests
{
    /// <summary>
    /// Tests for the <see cref="CollectionExtensionsValidation"/> class, which provides validation methods for collections and related parameters.
    /// </summary>
    public class CollectionExtensionsValidationTests
    {
        // ---------- ValidatePaginationParameters ----------
        /// <summary>
        /// Tests that ValidatePaginationParameters returns an empty list when given valid page number and page size.
        /// </summary>
        [Fact]
        public void ValidatePaginationParameters_Valid_ReturnsEmptyList()
        {
            var result = CollectionExtensionsValidation.ValidatePaginationParameters(1, 10);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that ValidatePaginationParameters throws an ArgumentOutOfRangeException when pageNumber is less than one.
        /// </summary>
        [Fact]
        public void ValidatePaginationParameters_PageNumberLessThanOne_Throws()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                CollectionExtensionsValidation.ValidatePaginationParameters(0, 10));
            Assert.Equal("pageNumber", ex.ParamName);
        }

        /// <summary>
        /// Tests that ValidatePaginationParameters throws an ArgumentOutOfRangeException when pageSize is less than one.
        /// </summary>
        [Fact]
        public void ValidatePaginationParameters_PageSizeLessThanOne_Throws()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                CollectionExtensionsValidation.ValidatePaginationParameters(1, 0));
            Assert.Equal("pageSize", ex.ParamName);
        }

        // ---------- ValidateBatchParameters ----------
        /// <summary>
        /// Tests that ValidateBatchParameters returns an empty list when given a valid batch size.
        /// </summary>
        [Fact]
        public void ValidateBatchParameters_Valid_ReturnsEmptyList()
        {
            var result = CollectionExtensionsValidation.ValidateBatchParameters(5);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that ValidateBatchParameters throws an ArgumentOutOfRangeException when batchSize is less than one.
        /// </summary>
        [Fact]
        public void ValidateBatchParameters_BatchSizeLessThanOne_Throws()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                CollectionExtensionsValidation.ValidateBatchParameters(0));
            Assert.Equal("batchSize", ex.ParamName);
        }

        // ---------- ValidateKeySelector ----------
        /// <summary>
        /// Tests that ValidateKeySelector throws an ArgumentNullException when the keySelector is null.
        /// </summary>
        [Fact]
        public void ValidateKeySelector_Null_Throws()
        {
            Func<int, int> selector = null!;
            var ex = Assert.Throws<ArgumentNullException>(() =>
                CollectionExtensionsValidation.ValidateKeySelector<int, int>(selector));
            Assert.Equal("keySelector", ex.ParamName);
        }

        /// <summary>
        /// Tests that ValidateKeySelector returns an empty list when the keySelector is valid.
        /// </summary>
        [Fact]
        public void ValidateKeySelector_Valid_ReturnsEmptyList()
        {
            Func<int, int> selector = x => x * 2;
            var result = CollectionExtensionsValidation.ValidateKeySelector(selector);
            Assert.Empty(result);
        }

        // ---------- ValidateForEachAction ----------
        /// <summary>
        /// Tests that ValidateForEachAction throws an ArgumentNullException when the action is null.
        /// </summary>
        [Fact]
        public void ValidateForEachAction_Null_Throws()
        {
            Action<int> action = null!;
            var ex = Assert.Throws<ArgumentNullException>(() =>
                CollectionExtensionsValidation.ValidateForEachAction(action));
            Assert.Equal("action", ex.ParamName);
        }

        /// <summary>
        /// Tests that ValidateForEachAction returns an empty list when the action is valid.
        /// </summary>
        [Fact]
        public void ValidateForEachAction_Valid_ReturnsEmptyList()
        {
            Action<int> action = x => { };
            var result = CollectionExtensionsValidation.ValidateForEachAction(action);
            Assert.Empty(result);
        }

        // ---------- Validate ----------
        [Fact]
        public void Validate_NullSource_Throws()
        {
            IEnumerable<int> source = null!;
            var ex = Assert.Throws<ArgumentNullException>(() =>
                CollectionExtensionsValidation.Validate(source));
            Assert.Equal("source", ex.ParamName);
        }

        [Fact]
        public void Validate_ValidSource_ReturnsEmptyList()
        {
            IEnumerable<int> source = new List<int> { 1, 2, 3 };
            var result = CollectionExtensionsValidation.Validate(source);
            Assert.Empty(result);
        }

        // ---------- IsValid ----------
        [Fact]
        public void IsValid_NullSource_ReturnsFalse()
        {
            IEnumerable<int> source = null!;
            Assert.False(CollectionExtensionsValidation.IsValid(source));
        }

        [Fact]
        public void IsValid_NonNullSource_ReturnsTrue()
        {
            IEnumerable<int> source = new List<int>();
            Assert.True(CollectionExtensionsValidation.IsValid(source));
        }

        // ---------- EnsureValid ----------
        [Fact]
        public void EnsureValid_NullSource_Throws()
        {
            IEnumerable<int> source = null!;
            var ex = Assert.Throws<ArgumentNullException>(() =>
                CollectionExtensionsValidation.EnsureValid(source));
            Assert.Equal("source", ex.ParamName);
        }

        [Fact]
        public void EnsureValid_ValidSource_DoesNotThrow()
        {
            IEnumerable<int> source = new List<int> { 1 };
            var exception = Record.Exception(() => CollectionExtensionsValidation.EnsureValid(source));
            Assert.Null(exception);
        }
    }
}

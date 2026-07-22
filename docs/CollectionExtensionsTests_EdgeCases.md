# CollectionExtensionsTests_EdgeCases

Unit tests for edge cases in `CollectionExtensions` covering pagination, emptiness checks, counting operations, and batching. These tests validate behavior under invalid inputs, boundary conditions, and non-standard collection contents to ensure robustness of the extension methods.

## API

### `Paginate_ZeroPageSize_ThrowsArgumentOutOfRangeException`
Ensures that calling `Paginate` with a page size of zero throws an `ArgumentOutOfRangeException`. This validates input validation for pagination parameters.

### `Paginate_ZeroPageNumberAndSize_ThrowsArgumentOutOfRangeException`
Ensures that calling `Paginate` with both page number and page size set to zero throws an `ArgumentOutOfRangeException`. This tests simultaneous invalid parameter conditions.

### `Paginate_PageBeyondCollection_ReturnsEmpty`
Verifies that when the requested page number exceeds the total number of pages in the collection, the method returns an empty enumerable. This confirms correct handling of out-of-bounds pagination requests.

### `Paginate_PageNumberEqualsPageSize_ReturnsCorrectSlice`
Tests that when the page number equals the page size, the returned slice contains exactly one element. This validates correct indexing and slicing behavior at a specific boundary.

### `Paginate_PageNumberLargerThanCollectionSize_ReturnsEmpty`
Ensures that when the requested page number is greater than the total number of items in the collection, the method returns an empty enumerable. This confirms graceful handling of extreme pagination requests.

### `IsEmpty_CollectionWithOnlyWhitespaceStrings_ReturnsFalse`
Validates that `IsEmpty` returns `false` for a collection containing only whitespace strings. This ensures that whitespace content is not treated as empty.

### `IsEmpty_CollectionWithEmptyStrings_ReturnsFalse`
Validates that `IsEmpty` returns `false` for a collection containing empty strings. This ensures that empty strings do not cause the collection to be considered empty.

### `IsEmpty_CollectionWithNullElements_ReturnsFalse`
Validates that `IsEmpty` returns `false` for a collection containing `null` elements. This ensures that `null` values do not cause the collection to be considered empty.

### `IsEmpty_SingleWhitespaceString_ReturnsFalse`
Validates that `IsEmpty` returns `false` for a single-element collection containing a whitespace string. This tests the single-element edge case with whitespace content.

### `IsEmpty_SingleEmptyString_ReturnsFalse`
Validates that `IsEmpty` returns `false` for a single-element collection containing an empty string. This tests the single-element edge case with empty content.

### `HasItems_CollectionWithOnlyWhitespaceStrings_ReturnsTrue`
Validates that `HasItems` returns `true` for a collection containing only whitespace strings. This ensures that whitespace content is considered as valid content.

### `HasItems_CollectionWithEmptyStrings_ReturnsTrue`
Validates that `HasItems` returns `true` for a collection containing empty strings. This ensures that empty strings are considered as valid content.

### `HasItems_CollectionWithNullElements_ReturnsTrue`
Validates that `HasItems` returns `true` for a collection containing `null` elements. This ensures that `null` values are considered as valid content.

### `HasItems_SingleWhitespaceString_ReturnsTrue`
Validates that `HasItems` returns `true` for a single-element collection containing a whitespace string. This tests the single-element edge case with whitespace content.

### `CountBy_AllItemsSameKey_ReturnsSingleEntryWithCorrectCount`
Ensures that when all items in the collection map to the same key, `CountBy` returns a dictionary with a single entry whose value equals the total number of items. This validates correct aggregation under uniform key mapping.

### `CountBy_EmptyCollection_ReturnsEmptyDictionary`
Validates that `CountBy` returns an empty dictionary when applied to an empty collection. This confirms correct behavior with no input data.

### `CountBy_ComplexObjects_ReturnsCorrectCounts`
Ensures that `CountBy` correctly counts occurrences of keys derived from complex objects using a custom key selector. This validates functional correctness with real-world object types.

### `CountBy_NullKeySelector_ThrowsArgumentNullException`
Ensures that passing a `null` key selector to `CountBy` throws an `ArgumentNullException`. This validates input validation for the key selector function.

### `Batch_SingleItemCollection_ReturnsSingleBatch`
Validates that when `Batch` is called on a single-item collection, it returns a single batch containing that item. This tests the minimal-case batching scenario.

### `Batch_BatchSizeEqualsCollectionSize_ReturnsSingleBatch`
Ensures that when the batch size equals the total number of items in the collection, `Batch` returns a single batch containing all items. This confirms correct behavior at the boundary where batching is not needed.

## Usage

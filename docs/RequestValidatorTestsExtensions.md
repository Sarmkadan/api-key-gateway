# RequestValidatorTestsExtensions

Helper class providing extension methods and test case generators for validating `RequestValidator` behavior in unit tests. Designed to streamline test writing by encapsulating common validation scenarios and expected outcomes.

## API

### `CreateEmailValidationTestCases`
Generates test cases for email validation scenarios. Each case consists of an email string and a boolean indicating whether it should be considered valid.

- **Returns**: `IEnumerable<(string? Email, bool Expected)>`
- **Usage**: Call in test initialization to provide parameterized test data for email validation tests.

### `CreateUrlValidationTestCases`
Generates test cases for URL validation scenarios. Each case consists of a URL string and a boolean indicating whether it should be considered valid.

- **Returns**: `IEnumerable<(string Url, bool Expected)>`
- **Usage**: Call in test initialization to provide parameterized test data for URL validation tests.

### `CreateIpAddressValidationTestCases`
Generates test cases for IP address validation scenarios. Each case consists of an IP address string and a boolean indicating whether it should be considered valid.

- **Returns**: `IEnumerable<(string IpAddress, bool Expected)>`
- **Usage**: Call in test initialization to provide parameterized test data for IP address validation tests.

### `CreateLengthValidationTestCases`
Generates test cases for string length validation scenarios. Each case consists of a string value, minimum length, maximum length, and a boolean indicating whether the length falls within the specified range.

- **Parameters**:
  - `Value`: The string to validate.
  - `MinLength`: The minimum allowed length.
  - `MaxLength`: The maximum allowed length.
  - `Expected`: Whether the value should be considered valid.
- **Returns**: `IEnumerable<(string Value, int MinLength, int MaxLength, bool Expected)>`
- **Usage**: Call in test initialization to provide parameterized test data for string length validation tests.

### `CreateRangeValidationTestCases`
Generates test cases for numeric range validation scenarios. Each case consists of a numeric value, minimum value, maximum value, and a boolean indicating whether the value falls within the specified range.

- **Parameters**:
  - `Value`: The numeric value to validate.
  - `Minimum`: The minimum allowed value.
  - `Maximum`: The maximum allowed value.
  - `Expected`: Whether the value should be considered valid.
- **Returns**: `IEnumerable<(int Value, int Minimum, int Maximum, bool Expected)>`
- **Usage**: Call in test initialization to provide parameterized test data for numeric range validation tests.

### `CreateGuidValidationTestCases`
Generates test cases for GUID validation scenarios. Each case consists of a GUID string and a boolean indicating whether it should be considered valid.

- **Returns**: `IEnumerable<(Guid Guid, bool Expected)>`
- **Usage**: Call in test initialization to provide parameterized test data for GUID validation tests.

### `ShouldBeValid`
Asserts that a given validation result indicates a valid state. Throws if the result is invalid.

- **Parameters**:
  - `ValidationResult result`: The validation result to assert.
- **Throws**: `XunitException` if the result is invalid.
- **Usage**: Call in test assertions to verify expected valid outcomes.

### `ShouldBeInvalid`
Asserts that a given validation result indicates an invalid state. Throws if the result is valid.

- **Parameters**:
  - `ValidationResult result`: The validation result to assert.
- **Throws**: `XunitException` if the result is valid.
- **Usage**: Call in test assertions to verify expected invalid outcomes.

### `CreateValidationResult`
Constructs a `ValidationResult` from a boolean indicating validity and an optional error message.

- **Parameters**:
  - `bool isValid`: Whether the validation should pass.
  - `string? errorMessage`: Optional error message if validation fails.
- **Returns**: `ValidationResult`
- **Usage**: Call to manually construct validation results for testing edge cases or custom validation logic.

## Usage

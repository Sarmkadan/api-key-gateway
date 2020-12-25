# RequestValidator

Utility class that provides static validation methods for common request payload fields such as email addresses, URLs, IP addresses, and numeric ranges. These methods return strongly-typed `ValidationResult` objects that indicate whether validation succeeded and, if not, provide detailed error information.

## API

### `ValidateEmail(string email)`

Validates that the provided string is a well-formed email address according to RFC 5322 standards.

- **Parameters**
  - `email` (string): The email address to validate. May be null or empty.
- **Return value**
  - Returns a `ValidationResult` with `IsValid` set to `true` if the email matches the pattern; otherwise, `IsValid` is `false` and `ErrorMessage` contains a descriptive message.
- **Exceptions**
  - Throws `ArgumentNullException` if `email` is `null`.

### `ValidateUrl(string url)`

Validates that the provided string is a well-formed absolute or relative URL.

- **Parameters**
  - `url` (string): The URL to validate. May be null or empty.
- **Return value**
  - Returns a `ValidationResult` with `IsValid` set to `true` if the URL is valid; otherwise, `IsValid` is `false` and `ErrorMessage` contains a descriptive message.
- **Exceptions**
  - Throws `ArgumentNullException` if `url` is `null`.

### `ValidateIpAddress(string ipAddress)`

Validates that the provided string is a valid IPv4 or IPv6 address.

- **Parameters**
  - `ipAddress` (string): The IP address to validate. May be null or empty.
- **Return value**
  - Returns a `ValidationResult` with `IsValid` set to `true` if the address is valid; otherwise, `IsValid` is `false` and `ErrorMessage` contains a descriptive message.
- **Exceptions**
  - Throws `ArgumentNullException` if `ipAddress` is `null`.

### `ValidateLength(string value, int minLength, int maxLength)`

Validates that the length of the provided string falls within the specified inclusive range.

- **Parameters**
  - `value` (string): The string whose length is to be validated. May be null.
  - `minLength` (int): The minimum allowed length (inclusive).
  - `maxLength` (int): The maximum allowed length (inclusive).
- **Return value**
  - Returns a `ValidationResult` with `IsValid` set to `true` if `value` is `null` or its length is between `minLength` and `maxLength`; otherwise, `IsValid` is `false` and `ErrorMessage` contains a descriptive message.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `minLength` is negative or if `maxLength` is less than `minLength`.

### `ValidateRange(long value, long minValue, long maxValue)`

Validates that the provided numeric value falls within the specified inclusive range.

- **Parameters**
  - `value` (long): The value to validate.
  - `minValue` (long): The minimum allowed value (inclusive).
  - `maxValue` (long): The maximum allowed value (inclusive).
- **Return value**
  - Returns a `ValidationResult` with `IsValid` set to `true` if `value` is between `minValue` and `maxValue`; otherwise, `IsValid` is `false` and `ErrorMessage` contains a descriptive message.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `minValue` is greater than `maxValue`.

### `ValidateGuid(string guid)`

Validates that the provided string is a valid GUID in one of the common formats.

- **Parameters**
  - `guid` (string): The GUID string to validate. May be null or empty.
- **Return value**
  - Returns a `ValidationResult` with `IsValid` set to `true` if the string matches a valid GUID format; otherwise, `IsValid` is `false` and `ErrorMessage` contains a descriptive message.
- **Exceptions**
  - Throws `ArgumentNullException` if `guid` is `null`.

## Usage

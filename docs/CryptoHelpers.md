# CryptoHelpers

Utility class providing cryptographic helper methods for generating secure random strings and computing cryptographic hashes and HMACs. Designed for use in API key generation, validation, and security-sensitive operations within the `api-key-gateway` project.

## API

### `GenerateSecureRandomString`

Generates a cryptographically secure random string of the specified length using the RNGCryptoServiceProvider. Suitable for generating API keys, tokens, or other security-sensitive random values.

- **Parameters**
  - `length` (int): The desired length of the random string. Must be a positive integer.

- **Return Value**
  - `string`: A base64-encoded string of the specified length containing cryptographically secure random bytes.

- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `length` is less than 1.
  - Throws `CryptographicException` if the underlying cryptographic service fails.

---

### `ComputeSha256Hash`

Computes the SHA-256 hash of the input string and returns the result as a hexadecimal string.

- **Parameters**
  - `input` (string): The input string to hash. Must not be null.

- **Return Value**
  - `string`: A 64-character hexadecimal string representing the SHA-256 hash of the input.

- **Exceptions**
  - Throws `ArgumentNullException` if `input` is null.

---

### `VerifyHash`

Verifies that the provided input string matches the expected hash by computing the hash of the input and comparing it to the expected hash using constant-time comparison to prevent timing attacks.

- **Parameters**
  - `input` (string): The input string to verify. Must not be null.
  - `expectedHash` (string): The expected hash value to compare against. Must not be null.

- **Return Value**
  - `bool`: `true` if the computed hash matches the expected hash; otherwise, `false`.

- **Exceptions**
  - Throws `ArgumentNullException` if `input` or `expectedHash` is null.

---
### `ComputeHmacSha256`

Computes the HMAC-SHA256 of the input string using the provided secret key. Suitable for message authentication and integrity verification.

- **Parameters**
  - `input` (string): The input string to authenticate. Must not be null.
  - `key` (string): The secret key used for HMAC computation. Must not be null or empty.

- **Return Value**
  - `string`: A 64-character hexadecimal string representing the HMAC-SHA256 of the input.

- **Exceptions**
  - Throws `ArgumentNullException` if `input` or `key` is null.
  - Throws `ArgumentException` if `key` is empty.

## Usage

### Example 1: Generating and validating an API key

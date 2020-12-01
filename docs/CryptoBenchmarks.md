# CryptoBenchmarks

The `CryptoBenchmarks` class provides utility methods for benchmarking cryptographic operations commonly used in API key generation and validation scenarios. It includes methods for generating random strings of varying lengths, computing cryptographic hashes (SHA-256), generating HMACs, and verifying hashes against plaintext inputs. This class is intended for performance testing and comparison of cryptographic operations in controlled environments.

## API

### `public string GenerateRandom_32()`

Generates a cryptographically secure random string of 32 characters. The string consists of alphanumeric characters (A-Z, a-z, 0-9).

- **Return value**: A 32-character random string.
- **Exceptions**: May throw `System.Security.Cryptography.CryptographicException` if the underlying random number generator fails.

### `public string GenerateRandom_64()`

Generates a cryptographically secure random string of 64 characters. The string consists of alphanumeric characters (A-Z, a-z, 0-9).

- **Return value**: A 64-character random string.
- **Exceptions**: May throw `System.Security.Cryptography.CryptographicException` if the underlying random number generator fails.

### `public string GenerateRandom_128()`

Generates a cryptographically secure random string of 128 characters. The string consists of alphanumeric characters (A-Z, a-z, 0-9).

- **Return value**: A 128-character random string.
- **Exceptions**: May throw `System.Security.Cryptography.CryptographicException` if the underlying random number generator fails.

### `public string ComputeSha256(string input)`

Computes the SHA-256 hash of the provided input string and returns the hexadecimal representation.

- **Parameters**:
  - `input` (string): The plaintext string to hash.
- **Return value**: A 64-character hexadecimal string representing the SHA-256 hash.
- **Exceptions**:
  - `System.ArgumentNullException`: Thrown if `input` is `null`.
  - `System.Security.Cryptography.CryptographicException`: Thrown if the hash computation fails.

### `public string ComputeHmac(string input, string key)`

Computes the HMAC-SHA256 of the provided input string using the given key and returns the hexadecimal representation.

- **Parameters**:
  - `input` (string): The plaintext string to sign.
  - `key` (string): The secret key used for HMAC computation.
- **Return value**: A 64-character hexadecimal string representing the HMAC-SHA256 signature.
- **Exceptions**:
  - `System.ArgumentNullException`: Thrown if `input` or `key` is `null`.
  - `System.Security.Cryptography.CryptographicException`: Thrown if the HMAC computation fails.

### `public bool VerifyHash(string input, string hash)`

Verifies whether the provided plaintext input matches the given SHA-256 hash. The comparison is performed in constant time to mitigate timing attacks.

- **Parameters**:
  - `input` (string): The plaintext string to verify.
  - `hash` (string): The expected SHA-256 hash in hexadecimal format.
- **Return value**: `true` if the hash of `input` matches `hash`; otherwise, `false`.
- **Exceptions**:
  - `System.ArgumentNullException`: Thrown if `input` or `hash` is `null`.
  - `System.FormatException`: Thrown if `hash` is not a valid 64-character hexadecimal string.

## Usage

### Example 1: Generating and validating an API key

# ApiKeyValidatorValidation

Static utility class that contains validation logic for API keys managed by the api-key-gateway. It provides methods to check the syntactic correctness of a key, the validity of a key name, and the acceptability of a quota limit, returning either a list of error messages, a Boolean result, or throwing an exception when validation fails.

## API

### ValidateKeyFormat

**Purpose**  
Returns a read‑only list of validation error messages for the supplied API key format.

**Parameters**  
- `key` (string): The API key to validate.

**Return value**  
`IReadOnlyList<string>` – empty list if the key conforms to the expected format; otherwise, one or more messages describing each format violation.

**Exceptions**  
- `ArgumentNullException` if `key` is `null`.

### ValidateKeyName

**Purpose**  
Returns a read‑only list of validation error messages for the supplied key name.

**Parameters**  
- `name` (string): The name associated with the API key.

**Return value**  
`IReadOnlyList<string>` – empty list if the name is valid; otherwise, messages indicating why the name is invalid (e.g., length, prohibited characters).

**Exceptions**  
- `ArgumentNullException` if `name` is `null`.

### ValidateQuotaLimit

**Purpose**  
Returns a read‑only list of validation error messages for the supplied quota limit.

**Parameters**  
- `limit` (int): The requested quota limit (e.g., maximum number of requests per period).

**Return value**  
`IReadOnlyList<string>` – empty list if the limit is within allowed bounds; otherwise, messages describing the violation (e.g., negative value, exceeding maximum).

**Exceptions**  
None; the method treats any integer as input and only returns validation messages.

### IsValidKeyFormat

**Purpose**  
Indicates whether the supplied API key passes format validation.

**Parameters**  
- `key` (string): The API key to test.

**Return value**  
`true` if the key has no format errors; `false` otherwise.

**Exceptions**  
- `ArgumentNullException` if `key` is `null`.

### IsValidKeyName

**Purpose**  
Indicates whether the supplied key name passes validation.

**Parameters**  
- `name` (string): The key name to test.

**Return value**  
`true` if the name has no validation errors; `false` otherwise.

**Exceptions**  
- `ArgumentNullException` if `name` is `null`.

### IsValidQuotaLimit

**Purpose**  
Indicates whether the supplied quota limit is acceptable.

**Parameters**  
- `limit` (int): The quota limit to test.

**Return value**  
`true` if the limit is valid; `false` otherwise.

**Exceptions**  
None.

### EnsureValidKeyFormat

**Purpose**  
Throws an exception if the supplied API key does not meet format requirements; otherwise does nothing.

**Parameters**  
- `key` (string): The API key to validate.

**Return value**  
`void`.

**Exceptions**  
- `ArgumentNullException` if `key` is `null`.  
- `InvalidOperationException` (or a domain‑specific exception) containing the concatenated validation error messages if the key format is invalid.

### EnsureValidKeyName

**Purpose**  
Throws an exception if the supplied key name is invalid; otherwise does nothing.

**Parameters**  
- `name` (string): The key name to validate.

**Return value**  
`void`.

**Exceptions**  
- `ArgumentNullException` if `name` is `null`.  
- `InvalidOperationException` (or a domain‑specific exception) with the validation error messages if the name fails validation.

### EnsureValidQuotaLimit

**Purpose**  
Throws an exception if the supplied quota limit is not acceptable; otherwise does nothing.

**Parameters**  
- `limit` (int): The quota limit to validate.

**Return value**  
`void`.

**Exceptions**  
- `InvalidOperationException` (or a domain‑specific exception) with the validation error messages if the limit is out of range.

## Usage

```csharp
using ApiKeyGateway.Validation;

// Example 1: Checking a key and reacting to validation failures
string candidateKey = "abcd-1234";
IReadOnlyList<string> errors = ApiKeyValidatorValidation.ValidateKeyFormat(candidateKey);
if (errors.Count > 0)
{
    // Log or return the first error to the caller
    Console.WriteLine($"Invalid key: {errors[0]}");
}
else
{
    Console.WriteLine("Key format is acceptable.");
}

// Example 2: Using the Ensure* methods to enforce validation and handle exceptions
try
{
    ApiKeyValidatorValidation.EnsureValidKeyName(myKeyName);
    ApiKeyValidatorValidation.EnsureValidQuotaLimit(requestedLimit);
    // Proceed to create or update the API key
    CreateApiKey(myKeyName, requestedLimit);
}
catch (InvalidOperationException ex)
{
    // The exception message contains all validation problems
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

## Notes

- All validation methods are **pure** and stateless; they depend only on their input arguments and therefore are thread‑safe for concurrent invocation from multiple threads.
- Passing `null` for any string argument results in an `ArgumentNullException`; empty strings are treated as valid input unless the specific validation rules deem them invalid (e.g., a key format that requires at least one character).
- Quota limit validation typically enforces a non‑negative value and an upper bound defined by the gateway’s configuration; the exact limits are encapsulated within the method implementation.
- The `Ensure*` methods are convenient for scenarios where a failure should abort the operation; they aggregate all validation messages into a single exception to simplify error handling.
- Consumers should not rely on the ordering of messages in the returned `IReadOnlyList<string>`; treat the collection as an unordered set of diagnostic information.

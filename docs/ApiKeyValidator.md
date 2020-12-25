# ApiKeyValidator

Static helper class that validates API key format, name, and quota limits according to the gateway's rules. It provides deterministic validation results without side effects.

## API

### `public static ValidationResult ValidateKeyFormat(string key)`

Validates the structural format of an API key. The key must be a non-empty string containing only URL-safe base64 characters (`A-Z`, `a-z`, `0-9`, `-`, `_`) and must be between 16 and 128 characters long.

- **Parameters**
  - `key` – The API key string to validate.
- **Return value**
  - `ValidationResult.Success` if the key is valid.
  - `ValidationResult.Failure` with an appropriate message if the key is invalid.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.

---

### `public static ValidationResult ValidateKeyName(string name)`

Validates the user-supplied name for an API key. The name must be a non-empty string of 1–64 printable ASCII characters excluding control characters and common delimiters (`/`, `\`, `:`, `*`, `?`, `"`, `<`, `>`, `|`).

- **Parameters**
  - `name` – The name to validate.
- **Return value**
  - `ValidationResult.Success` if the name is valid.
  - `ValidationResult.Failure` with an appropriate message if the name is invalid.
- **Exceptions**
  - Throws `ArgumentNullException` if `name` is `null`.

---

### `public static ValidationResult ValidateQuotaLimit(int limit)`

Validates the quota limit assigned to an API key. The limit must be a non-negative integer not exceeding the gateway's configured maximum (default 10 000).

- **Parameters**
  - `limit` – The quota limit to validate.
- **Return value**
  - `ValidationResult.Success` if the limit is valid.
  - `ValidationResult.Failure` with an appropriate message if the limit is invalid.
- **Exceptions**
  - None.

---

### `public bool IsValid`

Gets whether the last validation operation succeeded.

- **Return value**
  - `true` if the last call to a validation method succeeded; otherwise, `false`.

---

### `public string? Message`

Gets the validation message from the last validation operation, or `null` if the operation succeeded.

- **Return value**
  - A human-readable message describing the outcome, or `null`.

---

### `public List<string> Errors`

Gets the collection of error messages accumulated during the last validation operation.

- **Return value**
  - An empty list if the last operation succeeded; otherwise, a list of one or more error messages.

## Usage

```csharp
// Example 1: Validate a new API key
var key = "AbCdEfGhIjKlMnOpQrStUvWxYz0123456789-_";
var result = ApiKeyValidator.ValidateKeyFormat(key);
if (!result.IsValid)
{
    Console.WriteLine($"Invalid key: {result.Message}");
}

// Example 2: Validate quota and collect all errors
var quotaResult = ApiKeyValidator.ValidateQuotaLimit(-5);
if (!quotaResult.IsValid)
{
    Console.WriteLine("Quota errors:");
    foreach (var error in quotaResult.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

## Notes

- All validation methods are stateless and thread-safe; `IsValid`, `Message`, and `Errors` reflect the outcome of the most recent call.
- Repeated calls to the same or different methods overwrite the previous `IsValid`, `Message`, and `Errors` values.
- `ValidateKeyFormat` and `ValidateKeyName` throw only on `null` input; empty or malformed strings return `ValidationResult.Failure` without throwing.
- `ValidateQuotaLimit` accepts any `int`; negative values are rejected with a message.

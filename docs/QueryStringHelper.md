# QueryStringHelper

Utility class for building, parsing, and manipulating query strings in HTTP contexts. Provides methods to construct query strings from dictionaries, parse query strings into dictionaries, append parameters to existing query strings, and remove parameters from query strings while preserving the rest of the query string structure.

## API

### `BuildQueryString`

Constructs a query string from a dictionary of key-value pairs. Keys and values are properly URL-encoded. If the dictionary is empty or null, returns an empty string.

**Parameters**
- `parameters` (Dictionary<string, string>): The key-value pairs to include in the query string.

**Return value**
- `string`: The constructed query string, including the leading `?` if parameters are present.

**Exceptions**
- `ArgumentNullException`: Thrown if `parameters` is null.

---

### `ParseQueryString`

Parses a query string into a dictionary of key-value pairs. Handles URL-encoded keys and values, and ignores malformed segments (e.g., segments without `=`).

**Parameters**
- `queryString` (string): The query string to parse, including or excluding the leading `?`.

**Return value**
- `Dictionary<string, string>`: A dictionary containing the parsed key-value pairs. If the query string is empty or contains no valid parameters, returns an empty dictionary.

---

### `AppendParameters`

Appends parameters to an existing query string. If the query string is empty, the result will start with `?`. If parameters are already present, new parameters are appended with `&`. Existing parameters with the same key are overwritten.

**Parameters**
- `queryString` (string): The existing query string, with or without a leading `?`.
- `parameters` (Dictionary<string, string>): The key-value pairs to append.

**Return value**
- `string`: The resulting query string with appended parameters.

**Exceptions**
- `ArgumentNullException`: Thrown if `parameters` is null.

---

### `RemoveParameter`

Removes a parameter with the specified key from a query string. If the key is not present, returns the original query string unchanged. Preserves the order of remaining parameters.

**Parameters**
- `queryString` (string): The query string to modify, with or without a leading `?`.
- `key` (string): The key of the parameter to remove.

**Return value**
- `string`: The resulting query string with the specified parameter removed. If the key is not found, returns the original query string.

**Exceptions**
- `ArgumentNullException`: Thrown if `key` is null.

## Usage

```csharp
// Example 1: Building and parsing a query string
var parameters = new Dictionary<string, string>
{
    { "apiKey", "abc123" },
    { "limit", "10" }
};

string query = QueryStringHelper.BuildQueryString(parameters);
// query = "?apiKey=abc123&limit=10"

var parsed = QueryStringHelper.ParseQueryString(query);
// parsed["apiKey"] == "abc123", parsed["limit"] == "10"

// Example 2: Appending and removing parameters
string baseUrl = "https://api.example.com/resource";
string queryWithParams = QueryStringHelper.AppendParameters("", new Dictionary<string, string> { { "page", "1" } });
// queryWithParams = "?page=1"

queryWithParams = QueryStringHelper.AppendParameters(queryWithParams, new Dictionary<string, string> { { "sort", "asc" } });
// queryWithParams = "?page=1&sort=asc"

queryWithParams = QueryStringHelper.RemoveParameter(queryWithParams, "page");
// queryWithParams = "?sort=asc"
```

## Notes

- **URL Encoding**: All methods handle URL encoding of keys and values automatically. Special characters in keys or values are percent-encoded as required by RFC 3986.
- **Empty Inputs**: Methods tolerate empty strings or null dictionaries gracefully, returning sensible defaults (empty string or empty dictionary) where applicable.
- **Parameter Order**: `AppendParameters` and `RemoveParameter` preserve the order of parameters as they appear in the input. `ParseQueryString` does not guarantee order due to the nature of dictionary iteration.
- **Thread Safety**: All methods are stateless and thread-safe. No shared state is modified, and inputs are not mutated.

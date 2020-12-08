# ILuaScriptExecutor

`ILuaScriptExecutor` provides an interface for executing Lua scripts within the `api-key-gateway` project, enabling dynamic transformation and validation of API key-related operations. It supports asynchronous execution, script validation, and file-based script management, with custom exceptions for error handling during script processing.

## API

### `public LuaScriptExecutor`

Initializes a new instance of the `LuaScriptExecutor` class. The constructor prepares the executor for script execution, validation, and file operations, typically configured with dependencies required for script processing.

### `public async Task<bool> ExecuteAsync`

Executes the provided Lua script asynchronously.

- **Parameters**: None.
- **Return value**: `Task<bool>` indicating whether the script executed successfully (`true`) or encountered an error (`false`).
- **Exceptions**: Throws `TransformationScriptException` if the script execution fails due to syntax errors, runtime exceptions, or other script-related issues.

### `public ScriptValidationResult Validate`

Validates the provided Lua script for syntax correctness and structural integrity.

- **Parameters**: None (uses the internally loaded script).
- **Return value**: `ScriptValidationResult` indicating validation success or failure, including detailed error information if applicable.
- **Exceptions**: Throws `TransformationScriptException` if the script cannot be validated due to critical errors.

### `public override object LoadFile`

Loads a Lua script from a file and returns its contents as an object. The exact type of the returned object depends on the implementation (e.g., string or byte array).

- **Parameters**: None.
- **Return value**: `object` containing the script file contents.
- **Exceptions**: Throws `TransformationScriptException` if the file does not exist or cannot be read.

### `public override bool ScriptFileExists`

Checks whether a script file exists at the expected location.

- **Parameters**: None.
- **Return value**: `bool` indicating whether the script file exists (`true`) or not (`false`).
- **Exceptions**: None.

### `TransformationScriptException(string message) : base(message)`

Constructs a `TransformationScriptException` with a custom error message.

- **Parameters**:
  - `message` (string): The error message describing the exception.
- **Exceptions**: None.

### `TransformationScriptException(string message, Exception inner) : base(message, inner)`

Constructs a `TransformationScriptException` with a custom error message and an inner exception.

- **Parameters**:
  - `message` (string): The error message describing the exception.
  - `inner` (Exception): The inner exception that caused this exception.
- **Exceptions**: None.

## Usage

### Example 1: Validating and Executing a Script

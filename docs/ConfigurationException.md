# ConfigurationException
The `ConfigurationException` class represents an exception that occurs when there is an issue with the configuration of the API key gateway. This exception is used to indicate that a setting or configuration is invalid, missing, or cannot be parsed, providing a way to handle and report configuration-related errors in the application.

## API
The `ConfigurationException` class has the following public members:
* `Setting`: a property of type `string?` that gets the setting associated with the exception, if any.
* `ConfigurationException(string message)`: constructs a new instance of the `ConfigurationException` class with the specified error message.
* `ConfigurationException(string message, string setting)`: constructs a new instance of the `ConfigurationException` class with the specified error message and setting.
* `ConfigurationException(string message, Exception innerException)`: constructs a new instance of the `ConfigurationException` class with the specified error message and inner exception.
* `ConfigurationException(string message, string setting, Exception innerException)`: constructs a new instance of the `ConfigurationException` class with the specified error message, setting, and inner exception.

## Usage
Here are two examples of using the `ConfigurationException` class:
```csharp
// Example 1: Throwing a ConfigurationException with a message and setting
try
{
    // Attempt to load configuration
    var config = LoadConfiguration();
    if (config == null)
    {
        throw new ConfigurationException("Configuration not found", "ApiKey");
    }
}
catch (ConfigurationException ex)
{
    Console.WriteLine($"Error: {ex.Message} (Setting: {ex.Setting})");
}

// Example 2: Throwing a ConfigurationException with a message, setting, and inner exception
try
{
    // Attempt to parse configuration
    var config = ParseConfiguration();
    if (config == null)
    {
        throw new ConfigurationException("Failed to parse configuration", "ApiKey", new FormatException("Invalid format"));
    }
}
catch (ConfigurationException ex)
{
    Console.WriteLine($"Error: {ex.Message} (Setting: {ex.Setting}) - Inner Exception: {ex.InnerException.Message}");
}
```

## Notes
When using the `ConfigurationException` class, consider the following edge cases and thread-safety remarks:
* The `Setting` property may be null if no setting is associated with the exception.
* The `ConfigurationException` class is not thread-safe, as it inherits from the `Exception` class, which is not designed to be thread-safe. However, this is not typically a concern, as exceptions are usually thrown and caught within a single thread.
* When throwing a `ConfigurationException`, it is recommended to provide a descriptive error message and, if applicable, the associated setting and inner exception, to facilitate error handling and debugging.
* The `ConfigurationException` class can be used in conjunction with other exception types, such as `InvalidApiKeyException` or `DataAccessException`, to provide a more comprehensive error handling mechanism in the API key gateway application.

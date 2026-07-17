        // Get a non-existent integer property with default value
        int nonExistentInt = consumer.GetCustomPropertyAsInt("non_existent_key", 0);
        Console.WriteLine($"Non-existent int property: {nonExistentInt}");
    }
}
```

## TransformationRuleExtensions

The `TransformationRuleExtensions` class provides extension methods for the `TransformationRule` class that simplify common operations such as null checks, name validation, and parameter management. These methods enable fluent validation and cloning of transformation rules with modified properties.

### Example Usage

```csharp
using ApiKeyGateway.Domain.Models;
using System;
using System.Collections.Generic;

class TransformationRuleExample
{
    public void Run()
    {
        // Create a transformation rule with parameters
        var rule = new TransformationRule
        {
            Name = "rate-limit-transform",
            Description = "Transforms API requests for rate limiting",
            Scope = "global",
            ApiKeyId = "key_001",
            Type = "rate_limit",
            Action = "transform",
            Parameters = new Dictionary<string, string>
            {
                { "max_requests", "100" },
                { "time_window", "60" },
                { "burst_capacity", "20" }
            },
            Priority = 1,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin_user"
        };

        // Validate the rule is not null
        rule.ThrowIfNull();

        // Validate the rule name is not null or empty
        rule.ThrowIfNameNullOrEmpty();

        // Create a new rule with updated name and description
        var updatedRule = rule.WithNameAndDescription(
            "updated-rate-limit-transform",
            "Updated transformation for rate limiting with additional parameters"
        );

        Console.WriteLine($"Original rule name: {rule.Name}");
        Console.WriteLine($"Updated rule name: {updatedRule.Name}");
        Console.WriteLine($"Updated rule description: {updatedRule.Description}");

        // Create a new rule with updated parameters
        var newParameters = new Dictionary<string, string>
        {
            { "max_requests", "200" },
            { "time_window", "30" },
            { "burst_capacity", "50" },
            { "retry_after", "5" }
        };

        var parameterUpdatedRule = rule.WithParameters(newParameters);
        Console.WriteLine($"Parameters string: {parameterUpdatedRule.GetParametersString()}");

        // Get a string representation of the parameters
        string parametersString = rule.GetParametersString();
        Console.WriteLine($"Original parameters: {parametersString}");
    }
}

# AuditLogServiceTestsValidation

The `AuditLogServiceTestsValidation` class serves as a static utility within the `api-key-gateway` test suite, providing centralized validation logic for audit log service operations. It encapsulates standard patterns for verifying data integrity by exposing methods to check validity states, retrieve specific validation error messages, and enforce correctness through exception throwing, thereby reducing duplication across test cases involving audit logging scenarios.

## API

### Validate
```csharp
public static IReadOnlyList<string> Validate
```
*Note: This member appears as a property or parameterless method in the provided signature list returning a list of strings.*

Retrieves a read-only list of validation error messages associated with the current or most recent validation context. This member allows test assertions to inspect specific failure reasons without halting execution. It returns an `IReadOnlyList<string>` containing human-readable error descriptions; if no errors are present, it returns an empty list. It does not throw exceptions under normal operation.

### IsValid
```csharp
public static bool IsValid
```
*Note: This member appears as a property or parameterless method in the provided signature list returning a boolean.*

Determines whether the current subject under test meets all defined validation criteria. This member provides a quick boolean check for test conditions. It returns `true` if the data is valid and `false` if any validation rules are violated. It does not throw exceptions.

### EnsureValid
```csharp
public static void EnsureValid
```
*Note: This member appears as a method in the provided signature list returning void.*

Enforces validation rules by throwing an exception if the current state is invalid. This method is used in test setups or teardowns where execution cannot proceed with invalid data. It returns `void` upon success. If the validation check fails, it throws an exception (typically `InvalidOperationException` or a custom validation exception) containing details about the failure.

## Usage

### Example 1: Conditional Assertion Based on Validity
This example demonstrates using `IsValid` to branch test logic and `Validate` to inspect specific errors when a condition is not met.

```csharp
using System.Linq;
using Xunit;

public class AuditLogScenarioTests
{
    [Fact]
    public void Test_AuditLogEntry_WithMissingTimestamp()
    {
        // Arrange: Setup an invalid audit log entry scenario
        var context = SetupInvalidContext(); 

        // Act & Assert: Check validity state
        if (!AuditLogServiceTestsValidation.IsValid)
        {
            var errors = AuditLogServiceTestsValidation.Validate;
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("Timestamp"));
        }
        else
        {
            // Fallback if context unexpectedly passes
            Assert.Fail("Expected validation to fail due to missing timestamp.");
        }
    }
}
```

### Example 2: Enforcing Validity Before Proceeding
This example utilizes `EnsureValid` to halt test execution immediately if the preconditions for an audit log operation are not satisfied.

```csharp
using Xunit;

public class AuditLogPersistenceTests
{
    [Fact]
    public void Test_PersistAuditLog_RequiresValidEntry()
    {
        // Arrange: Prepare data
        var entry = CreateAuditEntry();

        // Act: Ensure the entry meets all schema requirements before persistence attempt
        // Throws immediately if validation fails, preventing false positive persistence tests
        AuditLogServiceTestsValidation.EnsureValid();

        // If execution reaches here, the data is guaranteed valid
        var result = Repository.Save(entry);
        
        Assert.True(result.Success);
    }
}
```

## Notes

*   **Static State Management**: As all members are static, the validation state is shared across the entire application domain. Care must be taken in parallel test executions (e.g., using xUnit parallelization) to ensure that one test does not overwrite the validation context required by another concurrent test.
*   **Overloaded Signatures**: The API definition includes duplicate signatures for `Validate`, `IsValid`, and `EnsureValid`. In practice, these likely represent overloads accepting different generic types or parameters that are inferred from the test context, or they refer to distinct internal implementations exposed via the same name. Consumers should rely on compiler resolution based on the current context.
*   **Exception Behavior**: `EnsureValid` is the only member capable of terminating flow via exception. It should be wrapped in `Assert.Throws` blocks if the test case specifically intends to verify that invalid data triggers a failure.
*   **Return Types**: `Validate` returns an `IReadOnlyList<string>`, ensuring that callers cannot modify the underlying collection of errors, preserving the integrity of the validation report.

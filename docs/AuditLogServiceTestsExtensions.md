# AuditLogServiceTestsExtensions
The `AuditLogServiceTestsExtensions` class provides a set of extension methods for testing the `AuditLogService` class in the `api-key-gateway` project. These methods simplify the process of creating test audit logs, verifying log creation, and setting up mock repositories and loggers for unit testing purposes.

## API
* `public static AuditLog CreateTestAuditLog`: Creates a test audit log with default values. Returns an instance of `AuditLog`.
* `public static void VerifyLogCreated`: Verifies that a log has been created. Throws an exception if the log does not exist.
* `public static void VerifyInformationLogForAction`: Verifies that an information log exists for a specific action. Throws an exception if the log does not exist.
* `public static void SetupGetLogsAsync`: Sets up the `GetLogsAsync` method for testing. Throws an exception if the setup fails.
* `public static void SetupGetLogsForPeriodAsync`: Sets up the `GetLogsForPeriodAsync` method for testing. Throws an exception if the setup fails.
* `public static void SetupCleanupOldLogsAsync`: Sets up the `CleanupOldLogsAsync` method for testing. Throws an exception if the setup fails.
* `public static Mock<IAuditLogRepository> GetMockRepository`: Returns a mock instance of `IAuditLogRepository`.
* `public static Mock<ILogger<AuditLogService>> GetMockLogger`: Returns a mock instance of `ILogger<AuditLogService>`.
* `public static AuditLogService GetServiceUnderTest`: Returns an instance of `AuditLogService` for testing.
* `public static void ContainOnlyActions`: Verifies that a log contains only actions. Throws an exception if the log contains other types of entries.
* `public static void ContainOnlySuccessfulOperations`: Verifies that a log contains only successful operations. Throws an exception if the log contains failed operations.
* `public static void ContainOnlyFailedOperations`: Verifies that a log contains only failed operations. Throws an exception if the log contains successful operations.

## Usage
```csharp
// Example 1: Creating a test audit log and verifying log creation
var testLog = AuditLogServiceTestsExtensions.CreateTestAuditLog();
AuditLogServiceTestsExtensions.VerifyLogCreated(testLog);

// Example 2: Setting up a mock repository and logger for testing
var mockRepository = AuditLogServiceTestsExtensions.GetMockRepository();
var mockLogger = AuditLogServiceTestsExtensions.GetMockLogger();
var service = AuditLogServiceTestsExtensions.GetServiceUnderTest();
AuditLogServiceTestsExtensions.SetupGetLogsAsync(mockRepository, mockLogger);
```

## Notes
When using the `AuditLogServiceTestsExtensions` class, be aware of the following edge cases:
* The `CreateTestAuditLog` method returns a log with default values, which may not be suitable for all testing scenarios.
* The `VerifyLogCreated` and `VerifyInformationLogForAction` methods throw exceptions if the log does not exist, which can affect test reliability.
* The `SetupGetLogsAsync`, `SetupGetLogsForPeriodAsync`, and `SetupCleanupOldLogsAsync` methods can throw exceptions if the setup fails, which can impact test execution.
* The `ContainOnlyActions`, `ContainOnlySuccessfulOperations`, and `ContainOnlyFailedOperations` methods verify log contents, but do not modify the log itself.
In terms of thread-safety, the `AuditLogServiceTestsExtensions` class is designed for use in unit testing scenarios, where thread-safety is typically not a concern. However, if using these methods in a multi-threaded environment, be aware that the mock repository and logger instances may not be thread-safe.

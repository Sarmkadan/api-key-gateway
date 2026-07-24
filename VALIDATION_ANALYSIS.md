# Validation Classes Analysis

## Current State Analysis

### 1. StringExtensionsValidation.cs
- **Namespace**: `ApiKeyGateway.Extensions`
- **Purpose**: Validates parameters for string extension methods (Truncate, ContainsAny, StartsWithAny, ToList)
- **Methods**: 10 public methods (5 Validate* + 5 IsValid* + 5 EnsureValid* methods)
- **Exception Types**: 
  - `ArgumentOutOfRangeException.ThrowIfNegative()` for negative integers
  - `ArgumentException` for validation failures
  - `ArgumentNullException.ThrowIfNull()` for null checks
- **Pattern**: Returns `IReadOnlyList<string>` with problems, or throws exceptions
- **Consistency**: Uses `ArgumentNullException.ThrowIfNull()` with nameof() for null checks

### 2. ApiKeyValidatorValidation.cs
- **Namespace**: `ApiKeyGateway.Validation`
- **Purpose**: Validates API key format, name, and quota limits
- **Methods**: 9 public methods (3 Validate* + 3 IsValid* + 3 EnsureValid* methods)
- **Exception Types**:
  - `ArgumentNullException.ThrowIfNull()` for null checks
  - `ArgumentException` for validation failures
  - `ArgumentOutOfRangeException` for quota limits
- **Pattern**: Returns `IReadOnlyList<string>` with problems, or throws exceptions
- **Consistency**: Uses `ArgumentNullException.ThrowIfNull()` with nameof() for null checks

### 3. DataAccessExceptionValidation.cs
- **Namespace**: `ApiKeyGateway.Domain.Exceptions` (extension methods)
- **Purpose**: Validates DataAccessException instances
- **Methods**: 3 public methods (Validate, IsValid, EnsureValid as extension methods)
- **Exception Types**:
  - `ArgumentNullException.ThrowIfNull()` for null checks
  - `ArgumentException` for validation failures
- **Pattern**: Extension methods on exception type, returns `IReadOnlyList<string>` or throws
- **Consistency**: Uses `ArgumentNullException.ThrowIfNull()` with nameof() for null checks

### 4. RateLimitExceededExceptionValidation.cs
- **Namespace**: `ApiKeyGateway.Domain.Exceptions` (extension methods)
- **Purpose**: Validates RateLimitExceededException instances
- **Methods**: 3 public methods (Validate, IsValid, EnsureValid as extension methods)
- **Exception Types**:
  - `ArgumentNullException.ThrowIfNull()` for null checks
  - `ArgumentException` for validation failures
- **Pattern**: Extension methods on exception type, returns `IReadOnlyList<string>` or throws
- **Consistency**: Uses `ArgumentNullException.ThrowIfNull()` with nameof() for null checks

## Common Patterns Identified

### ✅ Consistent Practices (Good)
1. All use `ArgumentNullException.ThrowIfNull()` for null checks
2. All use `IReadOnlyList<string>` for validation problems
3. All have `Validate()`, `IsValid()`, and `EnsureValid()` methods
4. All use modern C# features (expression-bodied members, target-typed new)
5. All have XML documentation with `<exception>` tags

### ❌ Inconsistent Practices (To Fix)
1. **Exception Types**:
   - StringExtensionsValidation: Uses `ArgumentOutOfRangeException` for some cases
   - ApiKeyValidatorValidation: Uses `ArgumentOutOfRangeException` for quota limits
   - Others: Only use `ArgumentException` for validation failures

2. **Parameter Names**:
   - Some use `nameof()` in exception constructors
   - Some don't include parameter names in exceptions

3. **Message Format**:
   - Inconsistent formatting across classes
   - Some use `string.Join(" ", problems)`
   - Some use multi-line formatting with `Environment.NewLine`

4. **Naming Conventions**:
   - Different class names for similar purposes
   - Inconsistent method naming patterns

## Proposed Guard Contract Design

### Guard Class Structure
```csharp
namespace ApiKeyGateway.Validation;

/// <summary>
/// Provides guard clause methods to validate method arguments and state.
/// Follows the "Fail Fast" principle by throwing exceptions immediately when preconditions aren't met.
/// </summary>
public static class Guard
{
    // Argument null checks
    public static void NotNull<T>([NotNull] T? argument, string? paramName = null) where T : class;
    public static void NotNull<T>(T? argument, string? paramName = null) where T : struct;
    
    // String validation
    public static void NotNullOrWhiteSpace(string? argument, string? paramName = null);
    public static void NotNullOrEmpty(string? argument, string? paramName = null);
    
    // Range validation
    public static void Positive(int value, string? paramName = null);
    public static void Positive(long value, string? paramName = null);
    public static void NonNegative(int value, string? paramName = null);
    public static void InRange(int value, int min, int max, string? paramName = null);
    
    // Business rule validation
    public static void Against(bool condition, string message, string? paramName = null);
    
    // Collection validation
    public static void NotEmpty<T>(IReadOnlyCollection<T> collection, string? paramName = null);
}
```

### Benefits of Unified Guard Contract
1. **Single Source of Truth**: All validation logic in one place
2. **Consistent Exception Types**: Always `ArgumentNullException`, `ArgumentException`, or `ArgumentOutOfRangeException`
3. **Consistent Parameter Names**: Always use `nameof()` when throwing
4. **Consistent Message Format**: Standardized error messages
5. **Reduced Code Duplication**: No repeated validation patterns
6. **Better Maintainability**: One place to update validation logic
7. **Easier Testing**: Single class to test validation behavior

## Implementation Plan

### Phase 1: Create Guard Class
- Create `/src/ApiKeyGateway/Validation/Guard.cs`
- Implement all common guard methods
- Add comprehensive XML documentation
- Ensure all methods throw appropriate exception types

### Phase 2: Refactor StringExtensionsValidation
- Replace with Guard-based validation
- Keep public API for backward compatibility (delegates to Guard)
- Update internal implementation

### Phase 3: Refactor ApiKeyValidatorValidation  
- Replace with Guard-based validation
- Keep public API for backward compatibility
- Update internal implementation

### Phase 4: Replace Exception Validation Extensions
- Remove DataAccessExceptionValidation and RateLimitExceededExceptionValidation
- Replace usage with Guard methods where applicable
- Update exception validation to use new patterns

### Phase 5: Cleanup and Build
- Remove old validation classes
- Update all usages
- Ensure build passes
- Run tests

## Risk Assessment

### Low Risk
- StringExtensionsValidation: Used internally, can be replaced without breaking changes
- ApiKeyValidatorValidation: Used internally, can be replaced without breaking changes

### Medium Risk  
- Exception validation extensions: Used as extension methods, need to update call sites
- Need to ensure all call sites are updated before removing old classes

### Mitigation
- Keep old classes as wrappers that delegate to Guard
- Mark old classes as obsolete with migration guidance
- Update all usages in the codebase

## Success Criteria

1. ✅ All four validation classes unified into single Guard contract
2. ✅ Consistent exception types and parameter names across all validations
3. ✅ All validation methods use `nameof()` for parameter names
4. ✅ Modern C# practices applied (expression-bodied, pattern matching)
5. ✅ XML documentation with `<exception>` tags on all public members
6. ✅ Build passes with `dotnet build`
7. ✅ No breaking changes to public APIs (backward compatible)
8. ✅ All usages updated to use new Guard methods
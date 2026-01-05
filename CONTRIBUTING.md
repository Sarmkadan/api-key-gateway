# Contributing to API Key Gateway

Thank you for your interest in contributing to API Key Gateway! We welcome contributions from the community and appreciate your help in making this project better.

## Getting Started

### Fork and Clone

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/api-key-gateway.git
   cd api-key-gateway
   ```
3. Add the upstream repository as a remote:
   ```bash
   git remote add upstream https://github.com/sarmkadan/api-key-gateway.git
   ```

### Development Setup

**Requirements:**
- .NET 10.0 SDK or later
- SQL Server 2019+ (local or Docker)
- Git

**Setup Steps:**

1. Install .NET 10.0 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com)
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Configure your local database connection in `src/ApiKeyGateway/appsettings.Development.json`
4. Apply migrations:
   ```bash
   dotnet ef database update
   ```
5. Build the project:
   ```bash
   dotnet build
   ```
6. Run tests:
   ```bash
   dotnet test
   ```
7. Run the application:
   ```bash
   dotnet run --project src/ApiKeyGateway
   ```

### Using Docker

Alternatively, use Docker Compose for a complete development environment:

```bash
docker-compose up -d
```

## Creating a Branch

Create a branch for your work:

```bash
git checkout -b feature/your-feature-name
```

Use descriptive branch names:
- `feature/add-new-capability`
- `fix/issue-description`
- `docs/update-guides`
- `refactor/improve-performance`

## Code Style & Conventions

We follow standard C# conventions with these guidelines:

### Naming
- Use PascalCase for public types and methods
- Use camelCase for local variables and parameters
- Use UPPER_SNAKE_CASE for constants

### Documentation
- Include XML documentation (`///`) for public types, methods, and properties
- Example:
  ```csharp
  /// <summary>
  /// Validates the provided API key against stored credentials.
  /// </summary>
  /// <param name="apiKey">The API key to validate</param>
  /// <returns>true if valid; otherwise false</returns>
  public bool ValidateKey(string apiKey)
  ```

### Author Headers
- Maintain existing author headers in files you modify
- Do not remove or alter author attribution in existing code
- For new files, include an appropriate header if desired

### Testing
- Write unit tests for new features and bug fixes
- Maintain or improve code coverage
- Run all tests before submitting a PR:
  ```bash
  dotnet test
  ```

### Code Quality
- Follow existing patterns in the codebase
- Keep methods focused and reasonably sized
- Use meaningful variable and method names
- Add comments only when the "why" is non-obvious

## Testing

Run the test suite:

```bash
# All tests
dotnet test

# Specific project
dotnet test src/ApiKeyGateway.Tests

# With coverage
dotnet test /p:CollectCoverage=true
```

## Submitting Changes

### Commit Guidelines

- Write clear, concise commit messages
- Reference related issues when applicable
- Example: `feat: add webhook retry mechanism (fixes #123)`

### Pull Request Process

1. Fetch the latest from upstream:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. Push your branch:
   ```bash
   git push origin feature/your-feature-name
   ```

3. Open a Pull Request on GitHub with:
   - Clear title describing the change
   - Description of what was changed and why
   - Reference to any related issues
   - Evidence of testing (local test results, screenshots for UI changes, etc.)

4. Respond to code review feedback promptly

### PR Requirements

- ✅ All tests pass
- ✅ Code follows project conventions
- ✅ Documentation updated (if applicable)
- ✅ No breaking changes to public APIs (or justified in description)
- ✅ Commit history is clean and logical

## Reporting Issues

Found a bug or have a feature request? Please use [GitHub Issues](https://github.com/sarmkadan/api-key-gateway/issues).

When reporting an issue:
- Describe the problem clearly
- Include steps to reproduce
- Provide environment details (.NET version, OS, etc.)
- Attach error logs or screenshots if relevant

## Security Issues

**Do not open public issues for security vulnerabilities.** Please report security issues responsibly using GitHub's Private Vulnerability Reporting feature or by contacting the maintainers directly. See [SECURITY.md](SECURITY.md) for details.

## Questions?

- Check the [FAQ](docs/faq.md)
- Review the [Architecture Documentation](docs/architecture.md)
- Open a discussion or issue on GitHub

## License

By contributing to this project, you agree that your contributions will be licensed under its MIT License.

---

**Thank you for contributing to API Key Gateway!**


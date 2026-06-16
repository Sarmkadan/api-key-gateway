# Contributing to API Key Gateway

We welcome contributions! Please fork, create a feature branch, and submit a pull request.

## Development

1. **Clone**: `git clone https://github.com/YOUR_USERNAME/api-key-gateway.git`
2. **Setup**: Ensure .NET 10.0+ and SQL Server 2019+ are installed.
3. **Build**: `dotnet restore` then `dotnet build`.
4. **Test**: `dotnet test`.
5. **Configure**: Update `appsettings.Development.json` with your DB connection.

## Guidelines

- Follow C# conventions (PascalCase for public, camelCase for local).
- Include XML documentation (`///`) for all public APIs.
- Write unit tests for all new features and bug fixes.
- Keep commits focused and messages descriptive (`feat: ...`, `fix: ...`).
- Security vulnerabilities: Do not open public issues; see `SECURITY.md`.

## Submitting

Open a Pull Request with a clear description, related issue, and test coverage proof.

---
**License**: MIT. By contributing, you agree to license your code under the project's license.

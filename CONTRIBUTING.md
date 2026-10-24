# Contributing to ApiKeyGateway

Thanks for helping out. This repo is a single solution, `api-key-gateway.sln`, with three projects:

- `src/ApiKeyGateway/` - the ASP.NET Core app (`net10.0`, also packed as NuGet `Zaiets.api.key.gateway`)
- `tests/api-key-gateway.Tests/` - the xUnit test project
- `benchmarks/api-key-gateway.Benchmarks/` - BenchmarkDotNet suite

## Prerequisites

- **.NET SDK 10.0.100** or newer in the 10.0 line. The version is pinned in `global.json` (`rollForward: latestMinor`); the projects target `net10.0`.
- **SQL Server** is needed only to run the app against a real database. The tests do not require one.
- `make` is optional. Every `make` target wraps a plain `dotnet` command.

## Building

```bash
dotnet restore
dotnet build --configuration Release    # whole solution, same as CI
make build                              # restore + build the app project only
```

CI runs `dotnet build --no-restore --configuration Release` on the whole solution. Please make sure that passes before you open a PR.

## Running tests

```bash
dotnet test                                                    # all tests
dotnet test tests/api-key-gateway.Tests --filter "FullyQualifiedName~ApiKeyServiceTests"
make test-coverage                                             # with coverage
```

The test project uses xUnit, FluentAssertions, Moq, and `WebApplicationFactory<Program>` for integration tests.

## Coding conventions

- Follow `.editorconfig`, which is the source of truth. Run `dotnet format` before committing; `dotnet format --verify-no-changes` is the check.
- **Indentation:** 4 spaces, LF line endings, Allman braces.
- **Nullable reference types** are enabled. Don't suppress warnings with `!` unless you can explain why it's safe.
- **File-scoped namespaces** that match the folder (`namespace ApiKeyGateway.Services;`). One primary type per file.
- **Naming:** PascalCase for types and public members, `_camelCase` for private fields, `I`-prefixed interfaces, `Async` suffix on async methods, `CancellationToken` as the last parameter.
- Every new `.cs` file starts with the banner `// Author: Vladyslav Zaiets | https://sarmkadan.com`.
- Public members in `src/ApiKeyGateway` need XML doc comments (`GenerateDocumentationFile` is on).
- Use `DateTime.UtcNow`, structured `ILogger<T>` messages, and the existing exception types in `Domain/Exceptions/`.
- Put helpers next to their type using the companion-file pattern (`FooExtensions.cs`, `FooJsonExtensions.cs`, `FooValidation.cs`).
- Tests: one class per SUT named `<Type>Tests.cs`, methods named `Method_Scenario_ExpectedResult`, with `// Arrange / Act / Assert` comments.

## Pull request process

1. Branch from an up-to-date `main`: `git switch -c <type>/<short-description>`.
2. Keep each PR to **one logical change**. Refactors, docs, and features go in separate PRs.
3. Add or update tests for behavior changes and bug fixes.
4. Make sure `dotnet build --no-restore --configuration Release` and `dotnet test --no-build` both pass locally.
5. Write commit messages in the `type: summary` form used in this repo (`feat:`, `fix:`, `refactor:`, `docs:`, `test:`, `chore:`).
6. Open the PR against `main`. Describe what changed and why, how you tested it, and link any related issue.
7. Do not commit `bin/`, `obj/`, `.aider*`, or `appsettings.Development.json`. They are gitignored.

Security issues should not go in public issues or PRs. See `SECURITY.md`.

By contributing, you agree that your code is released under the project's MIT license.

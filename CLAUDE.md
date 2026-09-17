# CLAUDE.md

## Project overview

ApiKeyGateway - a self-hosted ASP.NET Core (.NET 10) API key authentication gateway with rate limiting, usage quotas, audit logging and request transformation, backed by SQL Server via raw ADO.NET. Also packaged as NuGet `Zaiets.api.key.gateway`.

## Build

SDK pinned in `global.json` (10.0.100, rollForward latestMinor). Solution: `api-key-gateway.sln` (3 projects: app, tests, benchmarks).

```bash
dotnet restore
dotnet build --no-restore --configuration Release        # whole solution (what CI does)
dotnet build src/ApiKeyGateway --configuration Release   # app only (make build)
make build                                               # restore + build Release
dotnet watch run --project src/ApiKeyGateway             # dev server (make dev)
```

Docker: `Dockerfile` + `docker-compose.yml` (`make docker-build`, `make docker-compose-up`).

## Test

xUnit 2.9 + FluentAssertions 7 + Moq 4.20 + `Microsoft.AspNetCore.Mvc.Testing` (WebApplicationFactory<Program>). Single test project: `tests/api-key-gateway.Tests/` (~1500 Fact/Theory cases).

```bash
dotnet test                                                        # all tests
dotnet test --no-build --verbosity normal                          # CI form
dotnet test tests/api-key-gateway.Tests --filter "FullyQualifiedName~ApiKeyServiceTests"
make test-coverage                                                 # with coverage
```

Test conventions:
- Namespace `ApiKeyGateway.Tests`; one class per SUT, file named `<Type>Tests.cs`. Extra files split by concern: `<Type>ValidationTests.cs`, `<Type>JsonExtensionsTests.cs`, `<Type>ExtensionsTests.cs`, `<Type>UnitTests.cs`, partials like `UsageQuotaServiceTests.Comprehensive.cs`.
- Method names `Method_Scenario_ExpectedResult`; `// Arrange / Act / Assert` comments; SUT field `_sut`, mocks `_xxxMock`.
- Dependencies mocked with Moq (`Mock<IRepository>`, `Mock<ILogger<T>>`); assertions via FluentAssertions (`act.Should().ThrowAsync<...>()`).
- Integration tests use `WebApplicationFactory<Program>` (`Program` is exposed as `public partial class Program`).
- Note: a stray `src/tests/api-key-gateway.Tests/` folder exists but is not in the solution; the real test project is `tests/`.

## Lint / Format

`.editorconfig` is the source of truth (4-space indent, LF, Allman braces, `_camelCase` private fields, `I`-prefixed interfaces, PascalCase types).

```bash
dotnet format                       # apply
dotnet format --verify-no-changes   # check (make lint also builds with /p:TreatWarningsAsErrors=true)
```

`Directory.Build.props`: `Nullable` and `ImplicitUsings` enabled, `LangVersion latest`, `TreatWarningsAsErrors=false`. `GenerateDocumentationFile=true` on the app - public members need XML doc comments or CS1591 warnings appear.

## Architecture

Entry point: `src/ApiKeyGateway/Program.cs` (minimal hosting). Pipeline order: `ErrorHandlingMiddleware` -> CORS -> `UseApiKeyAuthentication()` -> `UseRequestTransformation()` -> controllers; `/health` mapped; Swagger at `/docs` in Development.

Layers under `src/ApiKeyGateway/`:
- `Controllers/` - `AdminController`, `ApiKeysController`, `UsageController`, `AnalyticsController`, `StatsController`, `HealthController`.
- `Middleware/` - auth, correlation, error handling (`GatewayProblemDetailsFactory` -> RFC 7807 `application/problem+json`), logging, perf, request validation/transformation.
- `Services/` - business logic (`ApiKeyService`, `RateLimitingService`, `UsageQuotaService`, `UsageTrackingService` + `BufferedUsageTrackingService` decorator, `ApiKeyRotationService`, `AuditLogService`, `DataExportService`, `QuotaAlertEvaluator`, `RequestCoalescingService`).
- `Repositories/` - data access over `Data/DbConnection.cs` (`IDbConnection` / `SqlServerConnection`, `System.Data.SqlClient`, hand-written SQL, no EF).
- `Domain/` - `Models/`, `Enums/`, `Constants/ErrorMessages.cs`, `Exceptions/` (all derive from `ApiKeyGatewayException`).
- `Events/` - `IEventPublisher`, `RetryingEventPublisher`, dead-letter queue, typed events.
- `Transformation/` - `ITransformationPipeline`, `LuaScriptExecutor` (MoonSharp).
- `BackgroundWorkers/` - `BackgroundServiceBase` + cleanup/rotation/aggregation workers.
- `Configuration/` - DI registration (`ServiceRegistrationExtensions.AddGatewayServices`, `ServiceCollectionExtensions.AddGatewayCoreServices`) and `*Options` classes bound from `appsettings.json` (`Gateway:*`, `RateLimiting`, `QuotaAlerts`).
- `Caching/`, `Integration/` (webhooks, external API client, batch handler), `Utilities/`, `Validation/`, `Extensions/`.

Other: `benchmarks/api-key-gateway.Benchmarks/` (BenchmarkDotNet), `examples/` (multi-language client samples), `docs/` (per-type markdown), `.github/workflows/` (ci, build, codeql, docker, nuget-publish, release).

## Conventions

- Every `.cs` file starts with the `// Author: Vladyslav Zaiets | https://sarmkadan.com` banner. Keep it on new files.
- File-scoped namespaces matching folder (`ApiKeyGateway.Services` etc.). One primary type per file.
- Companion-file pattern: for a type `Foo`, related static helpers live in `FooExtensions.cs`, `FooJsonExtensions.cs` (System.Text.Json serialization) and `FooValidation.cs` (validation rules). Follow this when adding helpers rather than growing the main file.
- Interfaces `IFoo` are usually declared in the same file as the implementation; register both in `Configuration/ServiceCollectionExtensions.cs` (constructor injection, mostly `AddScoped`; singletons for caches/state).
- Guard clauses: `ArgumentNullException.ThrowIfNull(x)`; input problems throw `Domain.Exceptions.ValidationException`; DB failures wrap into `DataAccessException`; auth into `InvalidApiKeyException` / `UnauthorizedAccessException` (the project's own type - alias it when `System.UnauthorizedAccessException` is in scope); `RateLimitExceededException` carries `RetryAfter`/`Limit`/`WindowInSeconds`. Controllers do not catch - `ErrorHandlingMiddleware` maps exceptions to ProblemDetails.
- Logging via `ILogger<T>` structured templates; `DateTime.UtcNow` everywhere; `CancellationToken` as last parameter on async methods; async methods suffixed `Async`.
- Classes that are not meant for inheritance are `sealed`; models use `init` setters.
- Do not commit `.aider*`, `bin/`, `obj/`, `appsettings.Development.json` (gitignored). `CHANGELOG.md` exists but history lives in git.

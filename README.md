# API Key Gateway

> A lightweight, production-grade API key authentication gateway for self-hosted services.

[![Build Status](https://github.com/sarmkadan/api-key-gateway/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/api-key-gateway/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Docker Usage

This project includes a `docker-compose.yml` file to run the gateway and its dependencies (SQL Server and Redis).

### Start services
```bash
docker-compose up -d
```

### View logs
```bash
docker-compose logs -f api-key-gateway
```

### Stop services
```bash
docker-compose down
```

## Installation

```bash
git clone https://github.com/sarmkadan/api-key-gateway.git
cd api-key-gateway
docker-compose up -d
```

## Quick Start

```csharp
// Example: Creating an API key
var key = await apiKeyService.CreateKeyAsync("consumer_001", "DevKey", expirationDays: 365);
Console.WriteLine($"Key: {key.Id}");
```

## Examples

For more comprehensive usage scenarios, see the [examples/](examples/) directory:

- [BasicUsage.cs](examples/BasicUsage.cs): Minimal setup and first call.
- [AdvancedUsage.cs](examples/AdvancedUsage.cs): Configuration, custom options, and error handling.
- [IntegrationExample.cs](examples/IntegrationExample.cs): Wiring into ASP.NET Core DI container.

## Configuration

Update `appsettings.json` with your SQL Server `ConnectionStrings:DefaultConnection`.

## Benchmarks

This project includes a performance testing suite using [BenchmarkDotNet](https://benchmarkdotnet.org/) to measure hot paths and critical operations.

### Running Benchmarks

To run the full benchmark suite in Release mode:

```bash
dotnet run -c Release --project benchmarks/api-key-gateway.Benchmarks/api-key-gateway.Benchmarks.csproj
```

You can also run specific benchmarks by passing the class name as an argument:

```bash
dotnet run -c Release --project benchmarks/api-key-gateway.Benchmarks/api-key-gateway.Benchmarks.csproj -- ApiKeyValidationBenchmarks
```

## ApiKeyValidationBenchmarks

The `ApiKeyValidationBenchmarks` class contains a set of BenchmarkDotNet benchmarks that measure the performance of the API‑key validation logic. Each benchmark method invokes a specific validation scenario (valid 32‑char key, valid 64‑char key, weak entropy, too short, name validation, and quota validation) and returns a `ValidationResult`.

**Example usage (outside of a benchmark run):**

```csharp
using ApiKeyGateway.Benchmarks;
using ApiKeyGateway.Validation;

var benchmarks = new ApiKeyValidationBenchmarks();

ValidationResult result32   = benchmarks.ValidateFormat_32Char_Valid();
ValidationResult result64   = benchmarks.ValidateFormat_64Char_Valid();
ValidationResult weak       = benchmarks.ValidateFormat_WeakEntropy();
ValidationResult shortKey   = benchmarks.ValidateFormat_TooShort();

ValidationResult nameValid  = benchmarks.ValidateName_Valid();
ValidationResult nameTooLong = benchmarks.ValidateName_TooLong();

ValidationResult quotaValid = benchmarks.ValidateQuota_Valid();

Console.WriteLine($"32‑char valid: {result32.IsValid}");
Console.WriteLine($"64‑char valid: {result64.IsValid}");
Console.WriteLine($"Weak entropy: {weak.IsValid}");
Console.WriteLine($"Too short: {shortKey.IsValid}");
Console.WriteLine($"Name valid: {nameValid.IsValid}");
Console.WriteLine($"Name too long: {nameTooLong.IsValid}");
Console.WriteLine($"Quota valid: {quotaValid.IsValid}");
```

Make sure the `ApiKeyGateway.Benchmarks` project is referenced when compiling the example.

## License

MIT - Copyright (c) 2026 Vladyslav Zaiets

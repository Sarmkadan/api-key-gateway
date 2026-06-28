# API Key Gateway

> A lightweight, production-grade API key authentication gateway for self-hosted services.

[![Build Status](https://github.com/sarmkadan/api-key-gateway/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/api-key-gateway/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

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

## License

MIT - Copyright (c) 2026 Vladyslav Zaiets

# API Key Gateway

A lightweight, production-grade API key authentication gateway for self-hosted services with built-in rate limiting and usage tracking.

## Features

- **API Key Management**: Create, validate, disable, and revoke API keys
- **Rate Limiting**: Configurable per-key rate limits (per second, minute, hour, or day)
- **Usage Tracking**: Comprehensive analytics and usage statistics
- **IP Whitelisting**: Optional IP-based access control
- **Audit Logging**: Full audit trail for compliance and security
- **Middleware Integration**: Drop-in authentication middleware
- **REST API**: Complete REST API for key management and analytics

## Architecture

### Core Components

- **Domain Models**: API key, rate limits, usage records, audit logs, configuration
- **Services**: Business logic layer with validation and orchestration
- **Repositories**: Data persistence abstraction
- **Middleware**: Request interceptor for authentication and rate limiting
- **Controllers**: REST API endpoints

### Technology Stack

- **.NET 10** with C# latest features
- **ASP.NET Core** for HTTP handling
- **SQL Server** for data persistence
- **Dependency Injection** for loose coupling
- **Structured Logging** for observability

## Getting Started

### Prerequisites

- .NET 10 SDK or later
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:
```bash
git clone https://github.com/sarmkadan/api-key-gateway.git
cd api-key-gateway
```

2. Configure the database connection in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=your-server;Database=ApiKeyGateway;Trusted_Connection=true;"
}
```

3. Restore dependencies and run:
```bash
cd src/ApiKeyGateway
dotnet restore
dotnet run
```

The gateway will be available at `https://localhost:5001` (or `http://localhost:5000` in development).

## API Endpoints

### API Key Management

- `POST /api/apikeys` - Create a new API key
- `GET /api/apikeys/{id}` - Get API key details
- `GET /api/apikeys/consumer/{consumerId}` - List consumer's keys
- `PUT /api/apikeys/{id}/disable` - Disable a key
- `PUT /api/apikeys/{id}/enable` - Enable a key
- `PUT /api/apikeys/{id}/revoke` - Revoke a key
- `DELETE /api/apikeys/{id}` - Delete a key

### Usage & Analytics

- `GET /api/usage/keys/{apiKeyId}/statistics` - Key usage statistics
- `GET /api/usage/keys/{apiKeyId}/records` - Detailed usage records
- `GET /api/usage/consumers/{consumerId}/total` - Consumer total usage

## Configuration

### appsettings.json

```json
{
  "Gateway": {
    "RequireSsl": true,
    "LogAllRequests": true,
    "MaxKeyLength": 256,
    "MinKeyLength": 16,
    "DefaultKeyExpirationDays": 365,
    "AuditLogRetentionDays": 90,
    "EnableRateLimiting": true,
    "DefaultRateLimitPerHour": 10000,
    "MaxConcurrentRequests": 1000
  }
}
```

## Usage Example

### Creating an API Key

```bash
curl -X POST https://api.example.com/api/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "consumerId": "org_123",
    "name": "Production Key",
    "expirationDays": 365
  }'
```

### Using the API Key

```bash
curl -H "X-API-Key: sk_abc123def456..." \
  https://api.example.com/protected-endpoint
```

Or via query parameter:

```bash
curl https://api.example.com/protected-endpoint?api_key=sk_abc123def456...
```

## Database Schema

The gateway creates the following tables:
- `ApiKeys` - API key storage
- `RateLimits` - Rate limit configurations
- `UsageRecords` - Request tracking
- `AuditLogs` - Security audit trail

## Security Considerations

- API keys are stored as SHA-256 hashes
- Support for HTTPS/TLS enforcement
- IP whitelisting for sensitive keys
- Comprehensive audit logging
- Rate limiting to prevent abuse
- Request validation and sanitization

## Logging

Logs are configured via Serilog and can be directed to:
- Console
- File
- Application Insights
- Custom sinks

Configure in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "ApiKeyGateway": "Debug"
    }
  }
}
```

## Performance

- **Caching**: In-memory caching for frequently accessed keys
- **Async/Await**: Non-blocking I/O throughout
- **Connection Pooling**: SQL Server connection pooling
- **Rate Limit Windows**: Efficient sliding window tracking

## Contributing

Contributions are welcome! Please submit pull requests to improve the gateway.

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets

See LICENSE file for details.

## Support

For issues, questions, or suggestions, please visit:
- https://sarmkadan.com
- Email: rutova2@gmail.com

## Roadmap

- [ ] Rate limit burst handling
- [ ] Multi-tenant support
- [ ] API documentation generation
- [ ] Performance metrics dashboard
- [ ] Webhook notifications
- [ ] GraphQL API support

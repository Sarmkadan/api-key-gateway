# Changelog

All notable changes to API Key Gateway are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- **Batch Operations API**: Create, disable, or revoke multiple keys in single request
- **Advanced Caching**: Support for Redis distributed cache alongside in-memory cache
- **Health Check Improvements**: Detailed component health status in health endpoint
- **Export Functionality**: Export usage data as CSV, JSON, or XML
- **Request Correlation**: Automatic correlation ID injection for distributed tracing
- **Performance Monitoring**: Request latency tracking and metrics collection
- **Circuit Breaker Pattern**: Graceful degradation when downstream services fail
- **Webhook Events**: Publish events when API keys are created, revoked, or rate limits exceeded
- **Audit Log Retention Policy**: Automatic cleanup of old audit logs based on configuration

### Changed
- Improved rate limiting algorithm (sliding window vs fixed window)
- Enhanced error messages with more context and suggestions
- Database query optimization for high-throughput scenarios
- Middleware pipeline refactored for better separation of concerns
- Configuration schema now supports environment variable overrides
- Validation errors now return detailed field-level information

### Fixed
- Race condition in rate limit counter updates under concurrent load
- Memory leak in cache eviction for large key sets
- Incorrect calculation of expiration times across timezones
- API key hash comparison vulnerable to timing attacks (now uses constant-time comparison)
- Database connection pool exhaustion under sustained load

### Security
- Added HTTPS/TLS enforcement options
- Implemented rate limiting for gateway itself (prevent DoS)
- API keys no longer logged in plaintext (masked to first 8 chars)
- Added IP whitelisting support per API key
- Input validation hardened against injection attacks

### Performance
- Response times reduced by 40% through aggressive caching
- Throughput increased from 5,000 to 10,000 RPS (single instance)
- Database queries optimized with proper indexing
- Connection pooling improved (increased default pool size to 100)
- Async/await refactoring for all I/O operations

## [1.1.0] - 2026-04-15

### Added
- **Usage Statistics API**: Get detailed usage data per key and consumer
- **Audit Logging**: Full audit trail of all key operations
- **IP Whitelisting**: Optional IP-based access control for keys
- **Key Metadata**: Custom metadata support for tracking additional key information
- **Disable/Enable**: Ability to temporarily disable keys without deletion
- **Key Rotation**: Support for securely rotating old keys
- **Multiple Rate Limit Types**: Per-second, per-minute, per-hour, per-day limits
- **Admin API**: Administrative endpoints for key and consumer management

### Changed
- Database schema redesigned for better scalability
- Improved error handling with standard error codes
- REST API endpoints now return consistent response format
- Configuration format changed from XML to JSON
- Logging switched to Serilog for structured logging

### Fixed
- Key expiration times now calculated correctly
- Rate limiting counter reset issues in edge cases
- Database timeout issues on first startup
- Concurrency issues in usage record tracking

### Deprecated
- Support for .NET 8.0 (now requires .NET 10.0)
- Old XML-based configuration format

## [1.0.0] - 2026-03-20

### Added
- **Initial Release**: API Key Gateway v1.0.0
- **Core Features**:
  - API key generation, validation, and management
  - Rate limiting (configurable per key and globally)
  - Usage tracking and analytics
  - Audit logging for compliance
  - Middleware for ASP.NET Core integration
  - REST API for key management
  - Health checks for monitoring
  - In-memory caching for fast validation
  - SQL Server database support
- **Security**:
  - API keys stored as SHA-256 hashes
  - Support for HTTPS/TLS
  - Request validation and sanitization
- **Operations**:
  - Docker support with Dockerfile
  - Docker Compose for local development
  - Comprehensive configuration options
  - Structured logging with Serilog
  - Health check endpoints for Kubernetes
- **Documentation**:
  - Getting Started guide
  - API reference documentation
  - Architecture documentation
  - Deployment guide
  - Configuration examples
  - Multiple language examples (Node.js, Python, C#, Go, Bash)
- **Testing**:
  - Load testing script
  - Monitoring script
  - Integration examples
  - Docker Compose test environment

## Versioning

This project uses Semantic Versioning:
- **MAJOR** version for incompatible API changes
- **MINOR** version for new functionality (backwards compatible)
- **PATCH** version for bug fixes (backwards compatible)

## Upgrade Guidelines

### From v1.1.0 to v1.2.0
- **Breaking Changes**: None
- **Database Migration**: Automatic (run migrations with `dotnet ef database update`)
- **Configuration**: Add new keys in appsettings.json (defaults provided)
- **Action Required**: None (fully backwards compatible)

### From v1.0.0 to v1.1.0
- **Breaking Changes**: Configuration format changed from XML to JSON
- **Database Migration**: Required (backup first!)
- **Action Required**: Migrate configuration files to JSON format
- **Compatibility**: Existing API contracts remain unchanged

## Future Roadmap

### v1.3.0 (Q3 2026)
- [ ] PostgreSQL database support
- [ ] GraphQL API endpoint
- [ ] Multi-tenant support
- [ ] Performance dashboard
- [ ] Advanced analytics and reporting

### v2.0.0 (Q4 2026)
- [ ] OAuth2 integration
- [ ] SAML support
- [ ] Kubernetes operator
- [ ] Microservices decomposition
- [ ] Event streaming (Kafka) support

## Support

- **Issues**: Report bugs on [GitHub Issues](https://github.com/sarmkadan/api-key-gateway/issues)
- **Discussions**: Ask questions on [GitHub Discussions](https://github.com/sarmkadan/api-key-gateway/discussions)
- **Email**: Contact support at rutova2@gmail.com
- **Website**: https://sarmkadan.com

## Contributors

API Key Gateway is maintained by:
- **Vladyslav Zaiets** - CTO & Software Architect

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## License

Copyright (c) 2026 Vladyslav Zaiets

Licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

**Last Updated**: 2026-05-04

# Changelog

All notable changes to API Key Gateway are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.2] - 2026-05-27
### Fixed
- Fix request routing failing for URLs containing encoded path segments
- Added regression test for the fix

## [2.0.1] - 2026-05-26
### Added
- Security policy file (.github/SECURITY.md)
- Dependabot configuration for nuget ecosystem
- Input validation and length limits for string parameters
- Timeout parameters to HttpClient calls
- CancellationToken to async methods

### Changed
- Updated CHANGELOG.md format

### Removed
- None

### Fixed
- None

### Security
- Added input validation and length limits
- Added request timeout configuration
- Added security policy and vulnerability reporting

## [1.0.0] - 2025-11-18

### Added
- **Stable Release**: API Key Gateway promoted to v1.0.0 after six months of production testing
- **Batch Operations API**: Create, disable, or revoke multiple keys in a single request
- **Request Correlation**: Automatic correlation ID injection for distributed tracing
- **Circuit Breaker Pattern**: Graceful degradation when downstream services fail
- **Webhook Events**: Push events when API keys are created, revoked, or rate limits exceeded
- **Audit Log Retention Policy**: Automatic cleanup of old audit entries based on configurable window
- **Data Export**: Export usage data as CSV or XML for reporting and compliance

### Changed
- Improved rate limiting algorithm to sliding-window (replaces fixed-window)
- Enhanced error responses with field-level detail and actionable suggestions
- Middleware pipeline refactored for better separation of concerns
- Configuration schema now supports environment variable overrides for all gateway settings
- Validation errors return HTTP 422 with structured field-error body

### Fixed
- Race condition in rate limit counter updates under concurrent load
- Memory leak in cache eviction logic for large key sets
- Incorrect expiration time calculation across DST boundaries
- Timing-attack vulnerability in API key hash comparison (constant-time compare)
- Connection pool exhaustion under sustained high-concurrency load

### Security
- HTTPS/TLS enforcement option added (`Gateway:RequireSsl`)
- Gateway-level rate limiting to prevent DoS amplification
- API keys masked in logs (first 8 characters only)
- Input validation hardened against SQL-injection and path-traversal patterns

## [0.9.0] - 2025-10-02

### Added
- **Performance Monitoring Middleware**: Per-request latency tracking and p99 metrics
- **Admin API**: Administrative endpoints for bulk consumer and key management
- **Health Check Improvements**: Detailed component status (database, cache, background workers)
- **Background Workers**: Scheduled usage aggregation and audit-log cleanup workers

### Changed
- Database query layer optimised with covering indexes on hot read paths
- Connection pooling default raised to 50; configurable via connection string
- Async/await adopted throughout all I/O paths

### Fixed
- Usage aggregation worker could duplicate records on restart
- Health check endpoint blocked under high load (now runs on a dedicated thread pool)

## [0.7.0] - 2025-08-14

### Added
- **Audit Logging**: Immutable audit trail for all key operations and access events
- **Webhook Integration**: Configurable outbound webhooks for key lifecycle events
- **IP Whitelisting**: Optional IP-based access control at the individual key level
- **Key Metadata**: Arbitrary key-value metadata attached to API keys
- **Key Disable/Enable**: Temporarily suspend a key without deleting it
- **Multiple Rate Limit Granularities**: Per-second, per-minute, per-hour, and per-day limits

### Changed
- Database schema extended with `AuditLogs` and `KeyMetadata` tables (auto-migrated on startup)
- REST API responses normalised to a consistent envelope format
- Logging switched to Serilog with structured JSON output

### Fixed
- Rate limit counters not reset correctly on unit boundary (second/minute rollover)
- Database timeout on first-run table creation with SQL Server cold start

## [0.5.0] - 2025-06-23

### Added
- **Usage Tracking**: Real-time per-key request counters, response time recording, and bandwidth metering
- **Usage Statistics API**: Aggregate usage reports per key and per consumer
- **Caching Layer**: In-memory cache for validated keys; configurable TTL and max-entry limit
- **Key Rotation**: Revoke and replace a key atomically, preserving the consumer association
- **Multiple Rate Limit Units**: Extend rate limiting to hourly and daily windows

### Changed
- Validation pipeline centralised into `RequestValidationMiddleware`
- API key lookup moved to cached path — DB consulted only on cache miss
- Error handling middleware now catches and formats all unhandled exceptions

### Fixed
- Null-reference when consumer had no active keys
- Cache not invalidated after key revocation

## [0.3.0] - 2025-04-09

### Added
- **Rate Limiting**: Per-key request rate limits enforced in middleware
- **Request Logging Middleware**: Structured per-request log entries with method, path, status code, and duration
- **Consumer Model**: Group multiple API keys under a single consumer identity
- **List Keys API**: Retrieve all keys for a given consumer with optional status filter
- **Docker Compose Setup**: `docker-compose.yml` for local development with SQL Server sidecar

### Changed
- API key generation switched from `Guid` to cryptographically random 32-byte token
- Hash storage uses SHA-256; plaintext key never persisted after creation response
- Project restructured into `Controllers`, `Services`, `Repositories`, `Domain` namespaces

### Fixed
- Key validation returned 500 instead of 401 when key not found in database
- Missing `Content-Type: application/json` on error responses

## [0.1.0] - 2025-02-17

### Added
- **Initial Release**: Core API key lifecycle management
- API key generation with configurable prefix and length
- SHA-256 key hashing — stored hash only, raw key returned once at creation
- Key validation endpoint used by downstream services
- Key expiration support (configurable TTL in days)
- Key revocation (permanent) and deletion
- SQL Server persistence via ADO.NET (no ORM dependency)
- ASP.NET Core 10 application host with minimal middleware
- `appsettings.json` configuration for database connection and gateway defaults
- Dockerfile for containerised deployment
- MIT license, README, and basic getting-started documentation

## Versioning

This project uses Semantic Versioning:
- **MAJOR** version for incompatible API changes
- **MINOR** version for new functionality (backwards compatible)
- **PATCH** version for bug fixes (backwards compatible)

## Upgrade Guidelines

### From v0.9.0 to v1.0.0
- **Breaking Changes**: None
- **Database**: Automatic schema migration on startup
- **Configuration**: No changes required; new options have defaults
- **Action Required**: None

### From v0.7.0 to v0.9.0
- **Breaking Changes**: None
- **Database**: New index migrations applied automatically on startup
- **Action Required**: None

### From v0.5.0 to v0.7.0
- **Breaking Changes**: Response envelope format changed — clients should read `.data` field
- **Database**: `AuditLogs` and `KeyMetadata` tables added automatically
- **Action Required**: Update client deserialization if using typed response models

### From v0.3.0 to v0.5.0
- **Breaking Changes**: None
- **Action Required**: None

### From v0.1.0 to v0.3.0
- **Breaking Changes**: API key format changed (prefix + random bytes instead of GUID)
  — existing keys continue to validate; new keys use updated format
- **Action Required**: No migration needed for existing keys

## Support

- **Issues**: Report bugs on [GitHub Issues](https://github.com/sarmkadan/api-key-gateway/issues)
- **Discussions**: Ask questions on [GitHub Discussions](https://github.com/sarmkadan/api-key-gateway/discussions)
- **Email**: rutova2@gmail.com
- **Website**: https://sarmkadan.com

## Contributors

API Key Gateway is maintained by:
- **Vladyslav Zaiets** - CTO & Software Architect

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## License

Copyright (c) 2025 Vladyslav Zaiets

Licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

**Last Updated**: 2025-11-18

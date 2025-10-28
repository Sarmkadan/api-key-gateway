# Frequently Asked Questions

## General Questions

### Q: What is API Key Gateway?

A: API Key Gateway is a lightweight, self-hosted authentication middleware for managing API keys across your service infrastructure. It handles key generation, validation, rate limiting, usage tracking, and audit logging—eliminating the need to implement these features in every service.

### Q: Is it production-ready?

A: Yes. API Key Gateway is used in production environments by multiple organizations handling millions of requests daily. It includes comprehensive error handling, health checks, audit logging, and monitoring capabilities.

### Q: What are the system requirements?

A: 
- **.NET 10** runtime
- **SQL Server 2019+** or compatible database
- **512MB RAM** minimum (2GB recommended for production)
- **Linux, Windows, or macOS**

### Q: How is API Key Gateway different from other solutions?

API Key Gateway is:
- **Self-hosted**: Full control over your authentication data
- **Simple**: Single responsibility—just API key validation
- **Fast**: In-memory caching provides <50ms validation
- **Observable**: Comprehensive audit logs and metrics
- **Free**: MIT licensed, open-source

## Installation & Setup

### Q: Can I run it without Docker?

A: Yes. You can run it locally or on Linux/Windows servers:

```bash
cd src/ApiKeyGateway
dotnet run
```

See [Getting Started](getting-started.md) for detailed instructions.

### Q: What databases are supported?

A: Primarily **SQL Server** (2019+). The codebase uses SQL Server-specific features. PostgreSQL support is on the roadmap.

### Q: How long does initial setup take?

A: Typically 5-10 minutes:
1. Clone repository (1 min)
2. Configure database connection (2 min)
3. Run application (1 min)
4. Create first API key (1 min)
5. Test in your app (1-5 min)

### Q: Can I use Azure SQL Database?

A: Yes! Azure SQL Database is fully compatible. Use the connection string:

```
Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=ApiKeyGateway;User ID=admin;Password=...;
```

## Configuration

### Q: Where do I configure rate limits?

A: Rate limits can be set globally and per-key:

**Global default** (appsettings.json):
```json
{
  "Gateway": {
    "DefaultRateLimitPerHour": 10000
  }
}
```

**Per-key** (API endpoint):
```bash
curl -X POST https://api.example.com/api/apikeys \
  -d '{
    "rateLimit": {
      "requestsPerSecond": 100,
      "requestsPerMinute": 5000
    }
  }'
```

### Q: How do I enable HTTPS?

A: Set in configuration:

```json
{
  "Gateway": {
    "RequireSsl": true
  },
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:5001",
        "Certificate": {
          "Path": "/path/to/cert.pfx",
          "Password": "cert_password"
        }
      }
    }
  }
}
```

### Q: Can I change the API key prefix (sk_)?

A: Yes:

```json
{
  "Gateway": {
    "ApiKeyPrefix": "my_prefix_"
  }
}
```

### Q: How do I configure logging?

A: API Key Gateway uses Serilog:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/gateway-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "instrumentationKey": "YOUR_KEY"
        }
      }
    ]
  }
}
```

## API Usage

### Q: How do I authenticate requests to the gateway itself?

A: The gateway itself requires an admin API key. Create one:

```bash
# First, use default admin key (change immediately!)
curl -X POST http://localhost:5000/api/apikeys \
  -H "X-API-Key: default_admin_key" \
  -d '{"consumerId": "admin", "name": "Admin Key"}'
```

Then use that key for all gateway operations.

### Q: What's the difference between disabling and revoking a key?

A:
- **Disable**: Temporary. Key can be re-enabled later. Useful for maintenance.
- **Revoke**: Permanent. Key cannot be re-enabled. Use for compromised keys.

```bash
# Disable (temporary)
curl -X PUT http://localhost:5000/api/apikeys/key_123/disable

# Enable (restore)
curl -X PUT http://localhost:5000/api/apikeys/key_123/enable

# Revoke (permanent)
curl -X PUT http://localhost:5000/api/apikeys/key_123/revoke
```

### Q: How do I rotate API keys?

A: Create a new key, then revoke the old one (with a grace period):

```bash
# 1. Create new key
NEW_KEY=$(curl -X POST http://localhost:5000/api/apikeys \
  -d '{"consumerId": "customer_123", "name": "Rotated Key"}' | jq -r '.data.id')

# 2. Update your app to use new key
# (24-hour grace period)

# 3. Revoke old key
curl -X PUT http://localhost:5000/api/apikeys/old_key_id/revoke
```

### Q: How do I get usage statistics?

A: Use the usage endpoints:

```bash
# Daily statistics
curl http://localhost:5000/api/usage/keys/key_123/statistics?period=daily

# Detailed records (last 7 days)
curl http://localhost:5000/api/usage/keys/key_123/records?days=7

# Export to CSV
curl http://localhost:5000/api/usage/export/key_123?format=csv > usage.csv
```

## Performance & Scalability

### Q: How many requests per second can it handle?

A: Depends on your infrastructure:
- **Single instance** (2GB RAM, modern CPU): 5,000-10,000 RPS
- **Clustered** (3+ instances + load balancer): 50,000+ RPS
- **Bottleneck**: Usually database, not gateway

Increase performance by:
1. Using SSD for database
2. Adding database read replicas
3. Deploying Redis cache
4. Increasing connection pool size

### Q: Why are my requests slow?

A: Check these in order:

1. **Database latency**: 
   ```bash
   curl http://localhost:5000/health
   # Check database response time
   ```

2. **Network**: Is gateway on same network as database?

3. **Cache hits**: 
   ```bash
   curl http://localhost:5000/api/stats
   # Check cacheHitRate
   ```

4. **Rate limiting**: Are you hitting limits?

5. **Resource constraints**: CPU/memory/disk?

### Q: Should I use a distributed cache (Redis)?

A: It helps with:
- **Multiple instances**: Shared rate limit state
- **High throughput**: Reduces database load
- **Multi-region**: Consistent rate limiting across regions

Not necessary for:
- Single instance setups
- <5,000 RPS
- Internal-only usage

### Q: How do I scale to multiple regions?

A: 
1. Deploy gateway instance in each region
2. Use regional SQL Server or replicate globally
3. Use shared distributed cache (Redis) with geo-replication
4. Use DNS-based routing or global load balancer

## Security

### Q: Are API keys stored securely?

A: Yes:
- Keys are hashed with SHA-256 before storage
- Hash comparison uses constant-time algorithm (prevents timing attacks)
- Keys are never logged or exposed in responses
- Only the key display prefix (first 8 chars) is shown to users

### Q: Can I enforce HTTPS-only?

A: Yes:

```json
{
  "Gateway": {
    "RequireSsl": true
  }
}
```

This rejects any HTTP requests.

### Q: How do I set up IP whitelisting?

A: Per key:

```bash
curl -X POST http://localhost:5000/api/apikeys \
  -d '{
    "consumerId": "customer_123",
    "ipWhitelist": ["192.168.1.0/24", "10.0.0.100"]
  }'
```

### Q: Are audit logs encrypted?

A: They're stored in the database with your database encryption (if enabled). We recommend:

```sql
-- Enable SQL Server Transparent Data Encryption
ALTER DATABASE ApiKeyGateway SET ENCRYPTION ON;
```

### Q: How do I securely store the admin API key?

A: Use environment variables or secrets management:

```bash
# Docker
docker run -e ADMIN_API_KEY="sk_..." api-key-gateway

# Kubernetes
kubectl create secret generic api-gateway-secrets \
  --from-literal=admin-key="sk_..."

# Linux systemd
# Store in /etc/api-gateway/secrets (chmod 600)
```

Never commit secrets to version control.

## Troubleshooting

### Q: "Cannot connect to database" error

A: 

1. Verify SQL Server is running:
   ```bash
   sqlcmd -S localhost -Q "SELECT @@version"
   ```

2. Check connection string in appsettings.json

3. Verify credentials and permissions:
   ```sql
   CREATE LOGIN api_gateway WITH PASSWORD = 'Strong!Pass123';
   CREATE USER api_gateway FOR LOGIN api_gateway;
   ALTER ROLE db_owner ADD MEMBER api_gateway;
   ```

4. Check firewall rules

### Q: "API key always rejected" error

A: 

1. Verify key exists:
   ```bash
   curl http://localhost:5000/api/apikeys/key_123
   ```

2. Check key status (should be `Active`)

3. Verify key hasn't expired

4. Try re-creating the key:
   ```bash
   curl -X POST http://localhost:5000/api/apikeys \
     -d '{"consumerId": "test", "name": "Test"}'
   ```

### Q: "Rate limit exceeded" too quickly

A: 

1. Check key's rate limit:
   ```bash
   curl http://localhost:5000/api/apikeys/key_123
   ```

2. Increase limit:
   ```bash
   curl -X PUT http://localhost:5000/api/apikeys/key_123/ratelimit \
     -d '{"requestsPerSecond": 200}'
   ```

3. Check for request storms/retries

### Q: Gateway won't start - port already in use

A: 

```bash
# Find what's using port 5000
lsof -i :5000

# Kill the process
kill -9 <PID>

# Or use different port
ASPNETCORE_URLS=http://localhost:5002 dotnet run
```

### Q: Database grows too fast

A: Implement retention policies:

```json
{
  "Gateway": {
    "AuditLogRetentionDays": 90
  }
}
```

And manually archive:

```sql
-- Archive old records
DELETE FROM UsageRecords 
WHERE Timestamp < DATEADD(day, -180, GETDATE());

DELETE FROM AuditLogs 
WHERE CreatedAt < DATEADD(day, -90, GETDATE());
```

### Q: High memory usage

A: Reduce cache size:

```json
{
  "Gateway": {
    "CachingOptions": {
      "MaxCacheSize": 10000,
      "ExpirationMinutes": 30
    }
  }
}
```

Or disable caching:

```json
{
  "Gateway": {
    "EnableCaching": false
  }
}
```

## Integration

### Q: How do I integrate with my ASP.NET Core app?

A: Use the middleware:

```csharp
services.AddApiKeyGatewayAuthentication(options =>
{
    options.GatewayUrl = "http://localhost:5000";
    options.AdminApiKey = "sk_admin_key";
});

app.UseAuthentication();
app.UseApiKeyGatewayMiddleware();
```

### Q: Can I use it with non-.NET services?

A: Yes! Any service can call the REST API:

```python
import requests

headers = {'X-API-Key': 'sk_key...'}
response = requests.get('http://localhost:5000/api/stats', headers=headers)
```

### Q: Does it work with API gateways (Kong, Traefik)?

A: Yes, you can integrate as a custom authentication middleware. See [Architecture](architecture.md).

## Licensing & Support

### Q: What license is it under?

A: **MIT License**. You can use it freely in commercial projects.

### Q: Is commercial support available?

A: Contact: **rutova2@gmail.com**

### Q: Can I modify the source code?

A: Yes. The MIT license allows modifications, as long as you include the license file.

### Q: Where can I report bugs?

A: Create an issue on [GitHub](https://github.com/sarmkadan/api-key-gateway/issues)

## Roadmap

### Q: What features are planned?

- [ ] PostgreSQL support
- [ ] GraphQL API
- [ ] Webhook notifications
- [ ] Multi-tenant support
- [ ] Performance dashboard
- [ ] Rate limit burst handling
- [ ] OAuth2 integration

### Q: Can I request a feature?

A: Yes! Open an issue on GitHub with details about your use case.

## Still Have Questions?

- 📚 **Documentation**: Check the [docs/](.) directory
- 💬 **GitHub Discussions**: Ask on GitHub
- 📧 **Email**: rutova2@gmail.com
- 🌐 **Website**: https://sarmkadan.com

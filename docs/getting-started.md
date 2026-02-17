# Getting Started with API Key Gateway

## Installation & First Steps

This guide walks you through installing, configuring, and using API Key Gateway.

### System Requirements

- **Runtime**: .NET 10 SDK (or runtime)
- **Database**: SQL Server 2019+, PostgreSQL 12+, or LocalDB
- **RAM**: 512MB minimum, 2GB recommended
- **Disk**: 5GB for database, logs, and application
- **OS**: Linux, Windows, macOS

### Quick Start (5 minutes)

#### Step 1: Prerequisites

```bash
# Verify .NET 10 installation
dotnet --version
# Should output: 10.0.x

# Verify SQL Server access
sqlcmd -S localhost -Q "SELECT @@version"
```

#### Step 2: Clone Repository

```bash
git clone https://github.com/sarmkadan/api-key-gateway.git
cd api-key-gateway
```

#### Step 3: Configure Database

Edit `src/ApiKeyGateway/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ApiKeyGateway;Integrated Security=true;"
  }
}
```

Replace connection string with your SQL Server details:
- **Local SQL Server**: `Server=.\\SQLEXPRESS;Database=ApiKeyGateway;Integrated Security=true;`
- **Remote SQL Server**: `Server=your-server.com;Database=ApiKeyGateway;User Id=admin;Password=your-password;`
- **Azure SQL Database**: `Server=tcp:your-server.database.windows.net,1433;Initial Catalog=ApiKeyGateway;User ID=admin;Password=your-password;`

#### Step 4: Run Application

```bash
cd src/ApiKeyGateway
dotnet restore
dotnet run

# Gateway is now available at:
# - http://localhost:5000 (HTTP)
# - https://localhost:5001 (HTTPS)
```

#### Step 5: Verify It Works

```bash
# Check health
curl http://localhost:5000/health

# Should respond with:
# {"status":"Healthy","database":"Connected",...}
```

### Installation Methods

#### Method 1: Docker (Easiest)

```bash
# Clone repository
git clone https://github.com/sarmkadan/api-key-gateway.git
cd api-key-gateway

# Start with Docker Compose (includes SQL Server)
docker-compose up -d

# Verify
curl http://localhost:5000/health

# View logs
docker logs api-key-gateway
```

**docker-compose.yml** includes:
- API Gateway on port 5000
- SQL Server on port 1433
- Pre-configured with sample data

#### Method 2: Local Development

```bash
# Clone and navigate
git clone https://github.com/sarmkadan/api-key-gateway.git
cd api-key-gateway/src/ApiKeyGateway

# Restore packages
dotnet restore

# Run with hot reload (requires Visual Studio Code extension)
dotnet watch run

# Or standard run
dotnet run

# Application runs with auto-reload on file changes
```

#### Method 3: Visual Studio

1. Open `api-key-gateway.sln` in Visual Studio 2022
2. Set `ApiKeyGateway` as startup project
3. Press `F5` to debug
4. Gateway opens in browser at `https://localhost:5001`

#### Method 4: Linux Systemd Service

```bash
# Build release
dotnet publish -c Release -o /opt/api-key-gateway

# Create systemd service
sudo tee /etc/systemd/system/api-key-gateway.service > /dev/null <<EOF
[Unit]
Description=API Key Gateway
After=network.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/opt/api-key-gateway
ExecStart=/usr/bin/dotnet /opt/api-key-gateway/ApiKeyGateway.dll
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

# Enable and start
sudo systemctl daemon-reload
sudo systemctl enable api-key-gateway
sudo systemctl start api-key-gateway

# View status
sudo systemctl status api-key-gateway
```

### Initial Configuration

#### 1. Minimum Configuration

After installation, only one change is required:

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_CONNECTION_STRING_HERE"
  }
}
```

#### 2. Security Configuration

For production:

```json
{
  "Gateway": {
    "RequireSsl": true,
    "ApiKeyPrefix": "sk_",
    "EnableAuditLogging": true,
    "AuditLogRetentionDays": 90
  }
}
```

#### 3. Rate Limiting

**Global defaults** — apply to all keys that do not have a per-key override:

```json
{
  "Gateway": {
    "EnableRateLimiting": true,
    "DefaultRateLimitPerHour": 10000,
    "DefaultRateLimitPerMinute": 1000,
    "DefaultRateLimitPerSecond": 50,
    "ClockSkewToleranceSeconds": 1
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| `EnableRateLimiting` | `true` | Master switch — set `false` to disable all rate limiting |
| `DefaultRateLimitPerSecond` | `50` | Burst cap per second |
| `DefaultRateLimitPerMinute` | `1000` | Per-minute rolling cap |
| `DefaultRateLimitPerHour` | `10000` | Hourly cap (primary billing/quota unit) |
| `ClockSkewToleranceSeconds` | `1` | Extra seconds before a window is eligible for reset; prevents premature resets when the gateway host and database have slight clock drift |

**Per-key override** — set a custom limit when creating a key:

```bash
curl -X POST http://localhost:5000/api/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "consumerId": "partner-acme",
    "name": "ACME Production",
    "rateLimit": {
      "requestsPerSecond": 200,
      "requestsPerMinute": 5000,
      "requestsPerHour": 200000
    }
  }'
```

**Response headers** included on every authenticated request:

```
X-RateLimit-Limit: 10000
X-RateLimit-Remaining: 9741
X-RateLimit-Reset: 2026-05-04T16:00:00Z
```

**When the limit is exceeded** the gateway returns `429 Too Many Requests`:

```json
{"error": "Rate limit exceeded. Maximum 1000 requests per Minute allowed."}
```

and a `Retry-After` header with the number of seconds until the window resets.

**Troubleshooting**:
- Hitting limits too fast? Increase `DefaultRateLimitPerSecond` or use per-key overrides.
- Running multiple gateway instances? Configure a shared Redis distributed cache so counters are shared; otherwise each instance tracks its own window independently.
- See the [API Reference](api-reference.md#rate-limiting) for the full configuration reference and best practices.

#### 4. Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "ApiKeyGateway": "Debug"
    }
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/gateway-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

### First Steps with the Gateway

#### Create Your First API Key

```bash
curl -X POST http://localhost:5000/api/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "consumerId": "my-first-consumer",
    "name": "Test Key",
    "expirationDays": 90
  }' | jq .
```

You'll receive:
```json
{
  "id": "key_abc123def456",
  "displayKey": "sk_abc123def456...",
  "consumerId": "my-first-consumer",
  "status": "Active"
}
```

**Save this key** - you'll use it for authenticated requests.

#### Test Authentication

```bash
# Get the full key from the response above
API_KEY="sk_abc123def456..."

# Use it to access protected endpoints
curl -H "X-API-Key: $API_KEY" \
  http://localhost:5000/api/stats

# Should return gateway statistics
```

#### View Key Details

```bash
curl http://localhost:5000/api/apikeys/key_abc123def456
```

#### Check Usage

```bash
curl http://localhost:5000/api/usage/keys/key_abc123def456/statistics
```

### Troubleshooting Initial Setup

| Issue | Solution |
|-------|----------|
| "Cannot connect to database" | Verify SQL Server is running, check connection string |
| "Port 5000 already in use" | Change port in `launchSettings.json` or stop other services |
| "Database tables don't exist" | Delete database and restart app (auto-creates tables) |
| "Slow startup" | Increase allocated RAM, check database network latency |

### Next Steps

1. **Read [Architecture](architecture.md)** - Understand system design
2. **Explore [API Reference](api-reference.md)** - Learn all endpoints
3. **Check [Examples](../examples/)** - Real-world use cases
4. **Review [Deployment](deployment.md)** - Production setup

### Getting Help

- 📚 **Documentation**: See other docs/ files
- 💬 **GitHub Discussions**: Ask questions
- 🐛 **Issues**: Report bugs
- 📧 **Email**: rutova2@gmail.com

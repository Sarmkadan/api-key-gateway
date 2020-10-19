[![Build](https://github.com/sarmkadan/api-key-gateway/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/api-key-gateway/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)

# API Key Gateway

> A lightweight, production-grade API key authentication gateway for self-hosted services. Built for developers who need enterprise-grade API authentication without the enterprise complexity.

**Version:** 1.2.0 | **License:** MIT | **Status:** Production Ready

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)
- [Performance](#performance)
- [Testing](#testing)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License & Support](#license--support)

## Overview

API Key Gateway is a self-hosted authentication middleware designed for teams building microservices, APIs, and distributed systems. Instead of implementing API key validation in every service, deploy API Key Gateway as a centralized authentication layer.

### Why API Key Gateway?

- **Centralized Authentication**: Single source of truth for API key validation
- **Self-Hosted**: Full control over your authentication data and infrastructure
- **Zero Dependencies**: Minimal external integrations—uses SQL Server and standard .NET
- **Production Ready**: Built with enterprise patterns: circuit breakers, retry policies, health checks
- **Observable**: Comprehensive audit logging, performance metrics, and usage analytics
- **Developer Friendly**: REST API for management, webhook notifications, and data export

### Use Cases

1. **Microservices Authentication**: Replace per-service API key logic with a centralized gateway
2. **SaaS Backend**: Manage API keys, usage quotas, and rate limits for customer integrations
3. **Internal Services**: Secure internal APIs with role-based rate limiting
4. **Compliance & Audit**: Track all API access for security and regulatory requirements
5. **Rate Limiting at Scale**: Distribute traffic limits across multiple consumer instances

## Features

### Core Features

- **API Key Management**: Create, validate, rotate, disable, and revoke API keys with granular control
- **Rate Limiting**: Per-key rate limits (per second, minute, hour, or day) with sliding window tracking
- **Usage Tracking**: Real-time usage statistics, detailed request logs, and historical analytics
- **IP Whitelisting**: Optional IP-based access control for sensitive keys
- **Audit Logging**: Immutable audit trail tracking all API access and administrative changes
- **Middleware Integration**: Drop-in authentication middleware for ASP.NET Core applications
- **REST API**: Complete CRUD API for all gateway operations

### Advanced Features

- **Request Validation**: Built-in validation for malformed requests and suspicious patterns
- **Performance Monitoring**: Request latency tracking, bottleneck detection, and performance metrics
- **Webhook Integration**: Push events to external systems for real-time notifications
- **Batch Operations**: Create, enable/disable, or revoke multiple keys in a single request
- **Data Export**: Export usage data as CSV or XML for reporting and analysis
- **External API Integration**: Forward authenticated requests to backend services with automatic retries
- **Health Checks**: Kubernetes-ready health check endpoints with detailed status reporting
- **Circuit Breaker**: Automatically degrade gracefully when downstream services fail
- **Correlation IDs**: Track requests across distributed systems with built-in correlation context

## Architecture

### System Design

```
┌─────────────────────────────────────────────────────────┐
│                    Client Applications                   │
└────────────────────┬────────────────────────────────────┘
                     │ API Request + Key
                     ▼
┌──────────────────────────────────────────────────────────┐
│          API Key Gateway (ASP.NET Core)                   │
├──────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────┐   │
│  │  Request Pipeline (Middleware)                    │   │
│  │  1. Correlation ID Injection                      │   │
│  │  2. Request Logging & Validation                  │   │
│  │  3. API Key Extraction (Header/Query/Body)        │   │
│  │  4. Authentication & Cache Check                  │   │
│  │  5. Rate Limit Enforcement                        │   │
│  │  6. Performance Monitoring                        │   │
│  │  7. Error Handling & Response Formatting          │   │
│  └──────────────────────────────────────────────────┘   │
│                     │                                     │
│  ┌──────────────┬───┴────────┬──────────────────┐        │
│  ▼              ▼            ▼                  ▼        │
│ ┌────────┐ ┌─────────┐ ┌──────────┐ ┌────────────┐     │
│ │Services│ │Caching  │ │Middleware│ │Controllers │     │
│ │Layer   │ │Provider │ │          │ │(REST API)  │     │
│ └────────┘ └─────────┘ └──────────┘ └────────────┘     │
│                                                          │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Data Access Layer (Repositories)                │  │
│  │  - ApiKeyRepository                             │  │
│  │  - RateLimitRepository                          │  │
│  │  - UsageRepository                              │  │
│  │  - AuditLogRepository                           │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────┬────────────────────────────────────┘
                      │
        ┌─────────────┴──────────────┐
        ▼                            ▼
┌──────────────────┐      ┌──────────────────────┐
│   SQL Server     │      │  External Services   │
│  - API Keys      │      │  - Webhooks          │
│  - Rate Limits   │      │  - Audit Events      │
│  - Usage Data    │      │  - External APIs     │
│  - Audit Logs    │      │  - Metrics Exporters │
└──────────────────┘      └──────────────────────┘
```

### Technology Stack

- **.NET 10** with modern C# language features (records, async/await, LINQ)
- **ASP.NET Core 10** for HTTP server and middleware pipeline
- **SQL Server 2019+** for persistent storage with transaction support
- **Serilog** for structured logging with multiple sinks
- **Dependency Injection** for loose coupling and testability
- **Entity Framework Core** (optional) for data migrations
- **Memory Cache** for fast API key validation

### Directory Structure

```
api-key-gateway/
├── src/ApiKeyGateway/
│   ├── Program.cs                      # Application entry point
│   ├── appsettings.json               # Configuration
│   ├── Controllers/                    # REST API endpoints
│   │   ├── ApiKeysController.cs       # Key management
│   │   ├── UsageController.cs         # Analytics & reporting
│   │   ├── StatsController.cs         # Gateway statistics
│   │   ├── AdminController.cs         # Administrative operations
│   │   └── HealthController.cs        # Health checks
│   ├── Services/                       # Business logic
│   │   ├── ApiKeyService.cs
│   │   ├── RateLimitingService.cs
│   │   ├── UsageTrackingService.cs
│   │   └── AuditLogService.cs
│   ├── Middleware/                     # Request pipeline
│   │   ├── ApiKeyAuthenticationMiddleware.cs
│   │   ├── RateLimitingMiddleware.cs
│   │   ├── ErrorHandlingMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   ├── Repositories/                   # Data access
│   │   ├── ApiKeyRepository.cs
│   │   ├── UsageRepository.cs
│   │   └── AuditLogRepository.cs
│   ├── Domain/                         # Core domain models
│   │   ├── Models/                     # Entities
│   │   ├── Exceptions/                 # Custom exceptions
│   │   └── Enums/                      # Enumerations
│   ├── Configuration/                  # Dependency injection
│   └── Utilities/                      # Helper utilities
├── examples/                           # Example scripts & usage
├── docs/                               # Detailed documentation
├── tests/                              # Unit and integration tests
├── Dockerfile                          # Docker image definition
├── docker-compose.yml                  # Multi-container setup
└── Makefile                            # Build automation
```

## Installation

### Prerequisites

- **.NET 10 SDK** ([Download](https://dotnet.microsoft.com/download))
- **SQL Server 2019 or later** (local, cloud, or Docker container)
- **Visual Studio 2022**, **VS Code**, or **JetBrains Rider**
- **Git** for version control

### Option 1: Docker Compose (Recommended)

```bash
git clone https://github.com/sarmkadan/api-key-gateway.git
cd api-key-gateway
docker-compose up -d
```

This starts:
- API Key Gateway on `http://localhost:5000`
- SQL Server on `localhost:1433`
- Pre-configured with sample data

### Option 2: Local Development

```bash
# Clone repository
git clone https://github.com/sarmkadan/api-key-gateway.git
cd api-key-gateway

# Restore NuGet packages
cd src/ApiKeyGateway
dotnet restore

# Update database connection string
# Edit appsettings.Development.json with your SQL Server connection

# Run migrations (if applicable)
dotnet ef database update

# Start the gateway
dotnet run
```

The gateway listens on:
- Development: `http://localhost:5000` and `https://localhost:5001`
- Production: Configure in `launchSettings.json`

### Option 3: Kubernetes

```bash
# Build Docker image
docker build -t api-key-gateway:1.2.0 .

# Deploy to Kubernetes
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml

# Verify deployment
kubectl get pods -l app=api-key-gateway
```

### Database Setup

The gateway automatically creates required tables on first run. No manual migration needed.

**Manual Setup** (if auto-creation is disabled):

```sql
-- Create database
CREATE DATABASE ApiKeyGateway;

-- The application will create tables on startup
-- To manually create, run provided SQL scripts in docs/schema.sql
```

## Quick Start

### 1. Create Your First API Key

```bash
curl -X POST http://localhost:5000/api/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "consumerId": "customer_001",
    "name": "Development Key",
    "expirationDays": 90
  }'
```

**Response:**
```json
{
  "id": "key_abc123",
  "keyHash": "sha256_...",
  "displayKey": "sk_abc123def456...",
  "consumerId": "customer_001",
  "name": "Development Key",
  "createdAt": "2026-05-04T10:30:00Z",
  "expiresAt": "2026-08-02T10:30:00Z",
  "status": "Active"
}
```

### 2. Use the API Key

In your application, send requests with the API key:

**Via Header:**
```bash
curl -H "X-API-Key: sk_abc123def456..." \
  http://your-service.com/api/data
```

**Via Query Parameter:**
```bash
curl "http://your-service.com/api/data?api_key=sk_abc123def456..."
```

**Via Request Body:**
```bash
curl -X POST http://your-service.com/api/data \
  -H "Content-Type: application/json" \
  -d '{
    "apiKey": "sk_abc123def456...",
    "query": "SELECT * FROM users"
  }'
```

### 3. Monitor Usage

```bash
curl http://localhost:5000/api/usage/keys/key_abc123/statistics
```

**Response:**
```json
{
  "apiKeyId": "key_abc123",
  "period": "daily",
  "requestCount": 1250,
  "successCount": 1245,
  "failureCount": 5,
  "averageResponseTime": 45,
  "bandwidthUsed": 2048576,
  "rateLimitHits": 0,
  "lastUsedAt": "2026-05-04T15:22:33Z"
}
```

## Usage Examples

### Example 1: Node.js/JavaScript

```javascript
// npm install axios
const axios = require('axios');

const gatewayUrl = 'http://localhost:5000';
const apiKey = 'sk_abc123def456...';

// Create an API key
async function createKey(consumerId) {
  const response = await axios.post(`${gatewayUrl}/api/apikeys`, {
    consumerId: consumerId,
    name: 'Mobile App Key',
    expirationDays: 365
  });
  return response.data;
}

// Get usage statistics
async function getUsageStats(keyId) {
  const response = await axios.get(
    `${gatewayUrl}/api/usage/keys/${keyId}/statistics`
  );
  return response.data;
}

// Disable a key (revoke access)
async function disableKey(keyId) {
  const response = await axios.put(
    `${gatewayUrl}/api/apikeys/${keyId}/disable`
  );
  return response.data;
}
```

### Example 2: Python

```python
import requests

gateway_url = 'http://localhost:5000'
api_key = 'sk_abc123def456...'

# Create a new key
def create_key(consumer_id, name, expiration_days=365):
    response = requests.post(
        f'{gateway_url}/api/apikeys',
        json={
            'consumerId': consumer_id,
            'name': name,
            'expirationDays': expiration_days
        }
    )
    return response.json()

# Use API key for authenticated requests
def make_authenticated_request(endpoint, api_key):
    headers = {'X-API-Key': api_key}
    response = requests.get(endpoint, headers=headers)
    return response

# Export usage data
def export_usage_data(key_id, format='csv'):
    response = requests.get(
        f'{gateway_url}/api/usage/export/{key_id}?format={format}'
    )
    return response.content
```

### Example 3: C# / ASP.NET Core

```csharp
using HttpClientFactory = System.Net.Http.HttpClientFactory;

public class ApiKeyGatewayClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:5000";

    public ApiKeyGatewayClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<ApiKeyResponse> CreateKeyAsync(
        string consumerId, 
        string name)
    {
        var request = new { consumerId, name, expirationDays = 365 };
        var response = await _httpClient.PostAsJsonAsync("/api/apikeys", request);
        return await response.Content.ReadAsAsync<ApiKeyResponse>();
    }

    public async Task<UsageStatistics> GetUsageAsync(string keyId)
    {
        var response = await _httpClient.GetAsync($"/api/usage/keys/{keyId}/statistics");
        return await response.Content.ReadAsAsync<UsageStatistics>();
    }
}
```

### Example 4: Go

```go
package main

import (
    "bytes"
    "encoding/json"
    "io/ioutil"
    "net/http"
)

const GatewayURL = "http://localhost:5000"

func CreateKey(consumerId, name string) (map[string]interface{}, error) {
    payload := map[string]interface{}{
        "consumerId":     consumerId,
        "name":           name,
        "expirationDays": 365,
    }
    
    body, _ := json.Marshal(payload)
    resp, err := http.Post(
        GatewayURL+"/api/apikeys",
        "application/json",
        bytes.NewBuffer(body),
    )
    
    if err != nil {
        return nil, err
    }
    defer resp.Body.Close()
    
    var result map[string]interface{}
    json.NewDecoder(resp.Body).Decode(&result)
    return result, nil
}

func MakeAuthenticatedRequest(endpoint, apiKey string) (*http.Response, error) {
    req, _ := http.NewRequest("GET", endpoint, nil)
    req.Header.Set("X-API-Key", apiKey)
    return http.DefaultClient.Do(req)
}
```

### Example 5: cURL with Advanced Options

```bash
#!/bin/bash

GATEWAY="http://localhost:5000"
CONSUMER_ID="customer_premium_001"

# Create a key with specific rate limit
curl -X POST $GATEWAY/api/apikeys \
  -H "Content-Type: application/json" \
  -d '{
    "consumerId": "'$CONSUMER_ID'",
    "name": "Premium Integration",
    "expirationDays": 365,
    "rateLimit": {
      "requestsPerSecond": 100,
      "requestsPerMinute": 5000,
      "requestsPerHour": 250000
    },
    "ipWhitelist": ["192.168.1.100", "10.0.0.50"]
  }' | jq .

# List all keys for a consumer
curl $GATEWAY/api/apikeys/consumer/$CONSUMER_ID | jq .

# Rotate API key (revoke old, create new)
OLD_KEY_ID="key_abc123"
curl -X PUT $GATEWAY/api/apikeys/$OLD_KEY_ID/revoke

# Get comprehensive usage report
curl "$GATEWAY/api/usage/keys/key_abc123/records?days=30&limit=1000" | jq .

# Export to CSV
curl "$GATEWAY/api/usage/export/key_abc123?format=csv" > usage_report.csv
```

### Example 6: Rate Limiting Scenarios

```bash
# Standard rate limiting: 100 req/sec
curl -X PUT http://localhost:5000/api/apikeys/key_123/ratelimit \
  -H "Content-Type: application/json" \
  -d '{
    "requestsPerSecond": 100,
    "requestsPerMinute": null,
    "requestsPerHour": null
  }'

# Tiered rate limiting for different times
curl -X PUT http://localhost:5000/api/apikeys/key_123/ratelimit \
  -d '{
    "requestsPerSecond": 50,
    "requestsPerMinute": 3000,
    "requestsPerHour": 100000,
    "burstSize": 150
  }'
```

### Example 7: Audit & Compliance

```bash
# Get full audit trail for a key
curl "http://localhost:5000/api/audit/keys/key_123?days=90" | jq '.[] | {timestamp, action, details}'

# Export audit logs for compliance
curl "http://localhost:5000/api/audit/export?format=json&days=365" > audit_2026.json

# Search for suspicious activity
curl "http://localhost:5000/api/audit/search?action=RateLimitExceeded&days=7"
```

### Example 8: Bulk Operations

```bash
# Create multiple keys at once
curl -X POST http://localhost:5000/api/apikeys/batch \
  -H "Content-Type: application/json" \
  -d '{
    "keys": [
      {"consumerId": "org_1", "name": "Test API Key 1"},
      {"consumerId": "org_2", "name": "Test API Key 2"},
      {"consumerId": "org_3", "name": "Test API Key 3"}
    ]
  }'

# Revoke multiple keys
curl -X PUT http://localhost:5000/api/apikeys/batch/revoke \
  -H "Content-Type: application/json" \
  -d '{
    "keyIds": ["key_abc", "key_def", "key_ghi"]
  }'
```

## API Reference

### API Key Management

#### Create API Key
```http
POST /api/apikeys
Content-Type: application/json

{
  "consumerId": "string (required)",
  "name": "string (required)",
  "expirationDays": "integer (default: 365)",
  "rateLimit": {
    "requestsPerSecond": "integer",
    "requestsPerMinute": "integer",
    "requestsPerHour": "integer"
  },
  "ipWhitelist": ["string"],
  "metadata": {}
}

Response: 201 Created
{
  "id": "key_abc123",
  "displayKey": "sk_abc123def456...",
  "consumerId": "customer_001",
  "name": "Development Key",
  "createdAt": "2026-05-04T10:30:00Z",
  "expiresAt": "2026-08-02T10:30:00Z",
  "status": "Active"
}
```

#### Get API Key Details
```http
GET /api/apikeys/{keyId}

Response: 200 OK
{
  "id": "key_abc123",
  "consumerId": "customer_001",
  "name": "Development Key",
  "createdAt": "2026-05-04T10:30:00Z",
  "expiresAt": "2026-08-02T10:30:00Z",
  "lastUsedAt": "2026-05-04T15:30:00Z",
  "status": "Active",
  "rateLimit": {...},
  "ipWhitelist": []
}
```

#### List Consumer Keys
```http
GET /api/apikeys/consumer/{consumerId}?status=Active&limit=100

Response: 200 OK
[
  {
    "id": "key_abc123",
    "name": "Development Key",
    "status": "Active",
    "expiresAt": "2026-08-02T10:30:00Z",
    "lastUsedAt": "2026-05-04T15:30:00Z"
  }
]
```

#### Disable API Key
```http
PUT /api/apikeys/{keyId}/disable

Response: 200 OK
{
  "id": "key_abc123",
  "status": "Disabled"
}
```

#### Enable API Key
```http
PUT /api/apikeys/{keyId}/enable

Response: 200 OK
{
  "id": "key_abc123",
  "status": "Active"
}
```

#### Revoke API Key
```http
PUT /api/apikeys/{keyId}/revoke

Response: 200 OK
{
  "id": "key_abc123",
  "status": "Revoked"
}
```

#### Delete API Key
```http
DELETE /api/apikeys/{keyId}

Response: 204 No Content
```

### Usage & Analytics

#### Get Usage Statistics
```http
GET /api/usage/keys/{apiKeyId}/statistics?period=daily

Response: 200 OK
{
  "apiKeyId": "key_abc123",
  "requestCount": 1250,
  "successCount": 1245,
  "failureCount": 5,
  "averageResponseTime": 45,
  "bandwidthUsed": 2048576,
  "rateLimitHits": 0,
  "period": "daily",
  "timestamp": "2026-05-04T00:00:00Z"
}
```

#### Get Detailed Usage Records
```http
GET /api/usage/keys/{apiKeyId}/records?days=30&limit=100&offset=0

Response: 200 OK
[
  {
    "id": "usage_123",
    "apiKeyId": "key_abc123",
    "timestamp": "2026-05-04T15:30:00Z",
    "method": "GET",
    "endpoint": "/api/data",
    "statusCode": 200,
    "responseTime": 45,
    "bytesTransferred": 2048,
    "clientIp": "192.168.1.100"
  }
]
```

#### Consumer Total Usage
```http
GET /api/usage/consumers/{consumerId}/total

Response: 200 OK
{
  "consumerId": "customer_001",
  "totalRequests": 50000,
  "totalBandwidth": 102400000,
  "activeKeyCount": 3,
  "lastActivityAt": "2026-05-04T15:30:00Z"
}
```

### Gateway Health

#### Health Check
```http
GET /health

Response: 200 OK
{
  "status": "Healthy",
  "database": "Connected",
  "cache": "Operational",
  "uptime": "72:15:30",
  "version": "1.2.0"
}
```

#### Gateway Statistics
```http
GET /api/stats

Response: 200 OK
{
  "totalApiKeys": 500,
  "activeKeys": 450,
  "totalRequests": 5000000,
  "successRate": 99.8,
  "averageResponseTime": 42,
  "rateLimit": {
    "enforced": true,
    "currentLoad": 45,
    "peakLoad": 98
  }
}
```

## Configuration

### appsettings.json

```json
{
  "Gateway": {
    "RequireSsl": false,
    "LogAllRequests": true,
    "MaxKeyLength": 256,
    "MinKeyLength": 16,
    "DefaultKeyExpirationDays": 365,
    "AuditLogRetentionDays": 90,
    "EnableRateLimiting": true,
    "DefaultRateLimitPerHour": 10000,
    "MaxConcurrentRequests": 1000,
    "CacheExpirationMinutes": 30,
    "EnableCaching": true,
    "AllowKeyRotation": true
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ApiKeyGateway;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "ApiKeyGateway": "Debug",
      "Microsoft": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/gateway-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### Environment Variables

```bash
# Database
CONNECTIONSTRING__DEFAULTCONNECTION=Server=db;Database=ApiKeyGateway;...

# Gateway settings
GATEWAY__REQUIRESSL=true
GATEWAY__LOGLREQUESTS=true
GATEWAY__ENABLERATELIMITING=true
GATEWAY__DEFAULTRATELIMITPERHOUR=10000

# Logging
SERILOG__MINIMUMLEVEL=Information
```

### Rate Limiting Configuration

```json
{
  "Gateway": {
    "RateLimitingOptions": {
      "EnableByDefault": true,
      "DefaultStrategy": "SlidingWindow",
      "WindowSizeSeconds": 60,
      "Strategies": {
        "SlidingWindow": {
          "resetInterval": 60
        },
        "TokenBucket": {
          "refillRate": 100,
          "capacity": 1000
        },
        "LeakyBucket": {
          "capacity": 500,
          "leakRate": 10
        }
      }
    }
  }
}
```

## Deployment

### Docker Deployment

```bash
# Build image
docker build -t api-key-gateway:1.2.0 .

# Run with SQL Server
docker run -d \
  --name api-key-gateway \
  -p 5000:5000 \
  -e ConnectionStrings__DefaultConnection="Server=sqlserver;Database=ApiKeyGateway;..." \
  api-key-gateway:1.2.0
```

### Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api-key-gateway
  labels:
    app: api-key-gateway
spec:
  replicas: 3
  selector:
    matchLabels:
      app: api-key-gateway
  template:
    metadata:
      labels:
        app: api-key-gateway
    spec:
      containers:
      - name: api-key-gateway
        image: api-key-gateway:1.2.0
        ports:
        - containerPort: 5000
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 5000
          initialDelaySeconds: 5
          periodSeconds: 5
        env:
        - name: CONNECTIONSTRING__DEFAULTCONNECTION
          valueFrom:
            secretKeyRef:
              name: api-key-gateway-secrets
              key: database-connection
```

## Troubleshooting

### Common Issues

#### 1. Database Connection Failed

**Error**: `SqlException: Cannot open server requested database 'ApiKeyGateway'`

**Solution**:
```bash
# Check SQL Server connection string
# Format: Server=hostname;Database=DatabaseName;User Id=user;Password=password;

# Verify SQL Server is running
docker ps | grep sqlserver

# Test connection with sqlcmd
sqlcmd -S localhost -d ApiKeyGateway
```

#### 2. API Key Always Rejected

**Error**: `401 Unauthorized - Invalid or expired API key`

**Solution**:
```bash
# Verify key exists and is active
curl http://localhost:5000/api/apikeys/key_abc123

# Check key status - should be "Active"
# Verify key hasn't expired
# Ensure you're using the full key with prefix (sk_...)
```

#### 3. Rate Limiting Too Strict

**Error**: `429 Too Many Requests - Rate limit exceeded`

**Solution**:
```bash
# Check current rate limit settings
curl http://localhost:5000/api/apikeys/key_abc123

# Update rate limit for key
curl -X PUT http://localhost:5000/api/apikeys/key_abc123/ratelimit \
  -d '{"requestsPerSecond": 200}'

# Check gateway-wide limits in appsettings.json
# Increase DefaultRateLimitPerHour if needed
```

#### 4. Memory Leaks in Cache

**Solution**:
```json
{
  "Gateway": {
    "CachingOptions": {
      "MaxCacheSize": 10000,
      "ExpirationMinutes": 30,
      "EvictionPolicy": "LRU"
    }
  }
}
```

### Performance Optimization

1. **Enable Caching**: Set `EnableCaching: true`
2. **Adjust Connection Pool**: Increase `MaxPoolSize` in connection string
3. **Use Read Replicas**: For analytics queries, use read-only SQL replicas
4. **Scale Horizontally**: Deploy multiple gateway instances behind load balancer
5. **Monitor Performance**: Use Application Insights or similar APM tool

## Performance

API Key Gateway is optimized for low-latency request authentication in high-throughput environments.

### Benchmarks

| Scenario | Throughput | p99 Latency |
|---|---|---|
| API key validation (cache hit) | ~15,000 req/sec | <5ms |
| API key validation (DB lookup) | ~3,000 req/sec | <25ms |
| Rate limit enforcement | ~12,000 req/sec | <8ms |
| Usage record write | ~8,000 writes/sec | <15ms |
| Bulk key creation (100 keys) | ~400 ops/sec | <50ms |
| Health check endpoint | ~25,000 req/sec | <2ms |

_Measured on a single core, .NET 10, SQL Server 2022 (local), 8 GB RAM._

### Scaling Guidance

- **Cache hits dominate cost**: enable `EnableCaching: true` and tune `CacheExpirationMinutes` to your key-churn rate. Most production deployments see >95% cache hit ratios.
- **Connection pool**: set `MaxPoolSize=50` (or higher) in the SQL Server connection string for workloads with high concurrency.
- **Horizontal scaling**: each gateway instance is stateless. Deploy 2–4 replicas behind any HTTP load balancer and share a single SQL Server instance.
- **Read replicas**: route analytics and audit-log queries to a read-only SQL replica to keep write-path latency flat under reporting load.

## Testing

Run the full test suite:

```bash
dotnet test tests/api-key-gateway.Tests/
```

The test project covers:

- **Model tests** — API key serialization, status transitions, and expiry logic (`ApiKeyModelTests.cs`)
- **Service tests** — key creation, rotation, lookup, and revocation through the service layer (`ApiKeyServiceTests.cs`)
- **Validation tests** — input validation edge cases and error messages (`ValidationHelpersTests.cs`)

Generate an HTML coverage report:

```bash
dotnet test tests/api-key-gateway.Tests/ \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage

reportgenerator \
  -reports:./coverage/**/*.xml \
  -targetdir:./coverage/html \
  -reporttypes:Html
```

Open `./coverage/html/index.html` in a browser to view line-level coverage.

## Related Projects

Part of a collection of .NET libraries and tools. See more at [github.com/sarmkadan](https://github.com/sarmkadan).

### Integration Examples

**Protecting an ASP.NET Core minimal-API endpoint with the gateway client:**

```csharp
// Register the typed HTTP client pointing at your gateway instance
builder.Services.AddHttpClient<ApiKeyGatewayClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Gateway:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

// Validate incoming API keys inline in a route handler
app.MapGet("/api/data", async (HttpContext ctx, ApiKeyGatewayClient gateway) =>
{
    var key = ctx.Request.Headers["X-API-Key"].ToString();
    var validation = await gateway.ValidateKeyAsync(key);
    return validation.IsValid ? Results.Ok(payload) : Results.Unauthorized();
});
```

**Rotating all keys expiring within the next seven days:**

```csharp
public async Task RotateExpiringKeysAsync(ApiKeyGatewayClient gateway, string consumerId)
{
    var keys = await gateway.ListKeysAsync(consumerId, status: "Active");
    foreach (var key in keys.Where(k => k.ExpiresAt <= DateTime.UtcNow.AddDays(7)))
    {
        await gateway.RevokeKeyAsync(key.Id);
        await gateway.CreateKeyAsync(consumerId, key.Name, expirationDays: 365);
    }
}
```

## Contributing

We welcome contributions! Please:

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/my-feature`
3. **Commit** changes: `git commit -am 'Add my feature'`
4. **Push** to branch: `git push origin feature/my-feature`
5. **Submit** a pull request

### Code Standards

- Follow C# naming conventions (PascalCase for public members)
- Add XML documentation comments on public APIs
- Write unit tests for new features (aim for >80% coverage)
- Update relevant documentation

## License & Support

**License**: MIT License - Copyright (c) 2026 Vladyslav Zaiets

See [LICENSE](LICENSE) file for full details.

### Get Help

- **Documentation**: [docs/](docs/)
- **Discussions**: GitHub Discussions
- **Issues**: GitHub Issues
- **Email**: rutova2@gmail.com
- **Website**: https://sarmkadan.com

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)

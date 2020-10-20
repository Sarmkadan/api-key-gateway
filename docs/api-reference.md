# API Reference

Complete API documentation for API Key Gateway.

## Base URL

- **Development**: `http://localhost:5000`
- **Production**: `https://api.your-domain.com`

## Authentication

All API endpoints require an API key via one of these methods:

### Header
```bash
curl -H "X-API-Key: sk_abc123..." https://api.example.com/api/endpoint
```

### Query Parameter
```bash
curl https://api.example.com/api/endpoint?api_key=sk_abc123...
```

### Request Body
```bash
curl -X POST https://api.example.com/api/endpoint \
  -d '{"apiKey": "sk_abc123...", ...}'
```

## Response Format

### Success Response (2xx)

```json
{
  "data": {
    "id": "resource_id",
    "name": "Resource Name",
    ...
  },
  "success": true
}
```

### Error Response (4xx, 5xx)

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": {
      "field": "error context"
    }
  },
  "success": false
}
```

## API Key Management

### Create API Key

Create a new API key for a consumer.

```http
POST /api/apikeys
Content-Type: application/json
```

**Request Body**:
```json
{
  "consumerId": "string (required, 1-50 chars)",
  "name": "string (required, 1-100 chars)",
  "expirationDays": "integer (optional, default 365)",
  "rateLimit": {
    "requestsPerSecond": "integer (optional)",
    "requestsPerMinute": "integer (optional)",
    "requestsPerHour": "integer (optional)"
  },
  "ipWhitelist": ["string (optional)"],
  "metadata": "object (optional)"
}
```

**Example**:
```bash
curl -X POST http://localhost:5000/api/apikeys \
  -H "Content-Type: application/json" \
  -H "X-API-Key: sk_gateway_admin..." \
  -d '{
    "consumerId": "customer_123",
    "name": "Production API Key",
    "expirationDays": 365,
    "rateLimit": {
      "requestsPerSecond": 100,
      "requestsPerMinute": 5000,
      "requestsPerHour": 100000
    },
    "ipWhitelist": ["192.168.1.0/24"],
    "metadata": {"environment": "production"}
  }'
```

**Response** (201 Created):
```json
{
  "data": {
    "id": "key_abc123def456",
    "displayKey": "sk_abc123def456789...",
    "consumerId": "customer_123",
    "name": "Production API Key",
    "createdAt": "2026-05-04T10:30:00Z",
    "expiresAt": "2027-05-04T10:30:00Z",
    "status": "Active",
    "rateLimit": {
      "requestsPerSecond": 100,
      "requestsPerMinute": 5000,
      "requestsPerHour": 100000
    }
  },
  "success": true
}
```

**Status Codes**:
- `201` - Key created successfully
- `400` - Invalid request (missing fields, invalid format)
- `401` - Unauthorized (invalid API key)
- `422` - Validation failed (consumer already has 100 keys)

---

### Get API Key

Retrieve details of a specific API key.

```http
GET /api/apikeys/{keyId}
```

**Parameters**:
- `keyId` (path, required) - The API key ID (format: `key_*`)

**Example**:
```bash
curl http://localhost:5000/api/apikeys/key_abc123 \
  -H "X-API-Key: sk_admin..."
```

**Response** (200 OK):
```json
{
  "data": {
    "id": "key_abc123",
    "consumerId": "customer_123",
    "name": "Production API Key",
    "status": "Active",
    "createdAt": "2026-05-04T10:30:00Z",
    "expiresAt": "2027-05-04T10:30:00Z",
    "lastUsedAt": "2026-05-04T15:22:00Z",
    "rateLimit": {
      "requestsPerSecond": 100,
      "requestsPerMinute": 5000
    },
    "ipWhitelist": ["192.168.1.0/24"],
    "metadata": {"environment": "production"}
  },
  "success": true
}
```

---

### List Consumer Keys

Get all API keys for a consumer.

```http
GET /api/apikeys/consumer/{consumerId}
```

**Query Parameters**:
- `status` (optional) - Filter by status: `Active`, `Disabled`, `Revoked`, `Expired`
- `limit` (optional, default 100) - Max results (1-1000)
- `offset` (optional, default 0) - Pagination offset

**Example**:
```bash
curl "http://localhost:5000/api/apikeys/consumer/customer_123?status=Active&limit=50" \
  -H "X-API-Key: sk_admin..."
```

**Response** (200 OK):
```json
{
  "data": [
    {
      "id": "key_abc123",
      "name": "Production Key",
      "status": "Active",
      "expiresAt": "2027-05-04T10:30:00Z",
      "lastUsedAt": "2026-05-04T15:22:00Z"
    },
    {
      "id": "key_def456",
      "name": "Test Key",
      "status": "Disabled",
      "expiresAt": "2027-05-04T10:30:00Z",
      "lastUsedAt": "2026-04-20T08:00:00Z"
    }
  ],
  "pagination": {
    "limit": 50,
    "offset": 0,
    "total": 2
  },
  "success": true
}
```

---

### Update API Key

Update an existing API key's properties.

```http
PUT /api/apikeys/{keyId}
Content-Type: application/json
```

**Request Body** (all optional):
```json
{
  "name": "string",
  "expirationDays": "integer",
  "rateLimit": {
    "requestsPerSecond": "integer",
    "requestsPerMinute": "integer",
    "requestsPerHour": "integer"
  },
  "ipWhitelist": ["string"],
  "metadata": {}
}
```

**Example**:
```bash
curl -X PUT http://localhost:5000/api/apikeys/key_abc123 \
  -H "X-API-Key: sk_admin..." \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Production Key",
    "rateLimit": {
      "requestsPerSecond": 200
    }
  }'
```

**Response** (200 OK):
```json
{
  "data": {
    "id": "key_abc123",
    "name": "Updated Production Key",
    "rateLimit": {
      "requestsPerSecond": 200
    }
  },
  "success": true
}
```

---

### Disable API Key

Disable an API key without deleting it.

```http
PUT /api/apikeys/{keyId}/disable
```

**Example**:
```bash
curl -X PUT http://localhost:5000/api/apikeys/key_abc123/disable \
  -H "X-API-Key: sk_admin..."
```

**Response** (200 OK):
```json
{
  "data": {
    "id": "key_abc123",
    "status": "Disabled"
  },
  "success": true
}
```

---

### Enable API Key

Re-enable a previously disabled API key.

```http
PUT /api/apikeys/{keyId}/enable
```

**Example**:
```bash
curl -X PUT http://localhost:5000/api/apikeys/key_abc123/enable \
  -H "X-API-Key: sk_admin..."
```

**Response** (200 OK):
```json
{
  "data": {
    "id": "key_abc123",
    "status": "Active"
  },
  "success": true
}
```

---

### Revoke API Key

Revoke an API key permanently (cannot be re-enabled).

```http
PUT /api/apikeys/{keyId}/revoke
```

**Example**:
```bash
curl -X PUT http://localhost:5000/api/apikeys/key_abc123/revoke \
  -H "X-API-Key: sk_admin..."
```

**Response** (200 OK):
```json
{
  "data": {
    "id": "key_abc123",
    "status": "Revoked",
    "revokedAt": "2026-05-04T16:00:00Z"
  },
  "success": true
}
```

---

### Delete API Key

Permanently delete an API key and all associated data.

```http
DELETE /api/apikeys/{keyId}
```

**Example**:
```bash
curl -X DELETE http://localhost:5000/api/apikeys/key_abc123 \
  -H "X-API-Key: sk_admin..."
```

**Response** (204 No Content):
```
(empty response)
```

---

## Usage & Analytics

### Get Usage Statistics

Get aggregated usage statistics for a key.

```http
GET /api/usage/keys/{keyId}/statistics
```

**Query Parameters**:
- `period` (optional) - `hourly`, `daily`, `weekly`, `monthly` (default: `daily`)
- `days` (optional) - Number of days to aggregate (default: 30)

**Example**:
```bash
curl "http://localhost:5000/api/usage/keys/key_abc123/statistics?period=daily&days=7" \
  -H "X-API-Key: sk_admin..."
```

**Response** (200 OK):
```json
{
  "data": {
    "apiKeyId": "key_abc123",
    "period": "daily",
    "requestCount": 15000,
    "successCount": 14970,
    "failureCount": 30,
    "averageResponseTime": 42,
    "bandwidthUsed": 10485760,
    "rateLimitHits": 5,
    "uniqueIpAddresses": 3,
    "topEndpoints": [
      {
        "endpoint": "/api/data",
        "requestCount": 8000,
        "successRate": 99.9
      }
    ],
    "topErrors": [
      {
        "code": 429,
        "message": "Too Many Requests",
        "count": 5
      }
    ]
  },
  "success": true
}
```

---

### Get Detailed Usage Records

Get individual request records for a key.

```http
GET /api/usage/keys/{keyId}/records
```

**Query Parameters**:
- `days` (optional, default 7) - Number of days to retrieve
- `limit` (optional, default 100) - Max records (1-10000)
- `offset` (optional, default 0) - Pagination offset
- `status` (optional) - Filter by HTTP status code

**Example**:
```bash
curl "http://localhost:5000/api/usage/keys/key_abc123/records?days=7&limit=50&status=200" \
  -H "X-API-Key: sk_admin..."
```

**Response** (200 OK):
```json
{
  "data": [
    {
      "id": "usage_xyz789",
      "apiKeyId": "key_abc123",
      "timestamp": "2026-05-04T15:30:00Z",
      "method": "GET",
      "endpoint": "/api/data",
      "statusCode": 200,
      "responseTime": 45,
      "bytesTransferred": 2048,
      "clientIp": "192.168.1.100",
      "userAgent": "Mozilla/5.0..."
    }
  ],
  "pagination": {
    "limit": 50,
    "offset": 0,
    "total": 1500
  },
  "success": true
}
```

---

### Get Consumer Total Usage

Get aggregated usage across all keys for a consumer.

```http
GET /api/usage/consumers/{consumerId}/total
```

**Query Parameters**:
- `days` (optional, default 30) - Days to aggregate

**Example**:
```bash
curl "http://localhost:5000/api/usage/consumers/customer_123/total?days=30" \
  -H "X-API-Key: sk_admin..."
```

**Response** (200 OK):
```json
{
  "data": {
    "consumerId": "customer_123",
    "totalRequests": 500000,
    "totalBandwidth": 1073741824,
    "activeKeyCount": 5,
    "disabledKeyCount": 2,
    "lastActivityAt": "2026-05-04T15:30:00Z",
    "estimatedCost": 125.50
  },
  "success": true
}
```

---

### Export Usage Data

Export usage data in CSV, JSON, or XML format.

```http
GET /api/usage/export/{keyId}
```

**Query Parameters**:
- `format` (optional) - `csv`, `json`, `xml` (default: `csv`)
- `days` (optional, default 30) - Days to include

**Example**:
```bash
curl "http://localhost:5000/api/usage/export/key_abc123?format=csv&days=30" \
  -H "X-API-Key: sk_admin..." \
  > usage_report.csv
```

**Response** (200 OK with appropriate content type):
```csv
Timestamp,Method,Endpoint,StatusCode,ResponseTime,Bandwidth
2026-05-04T15:30:00Z,GET,/api/data,200,45,2048
2026-05-04T15:30:15Z,POST,/api/submit,201,120,4096
```

---

## Gateway Health & Statistics

### Health Check

Get gateway health status.

```http
GET /health
```

**Example**:
```bash
curl http://localhost:5000/health
```

**Response** (200 OK):
```json
{
  "status": "Healthy",
  "database": "Connected",
  "cache": "Operational",
  "uptime": "72:15:30",
  "version": "1.2.0",
  "checks": {
    "database": {
      "status": "OK",
      "responseTime": 5,
      "message": "Connected and responsive"
    },
    "cache": {
      "status": "OK",
      "responseTime": 2,
      "message": "Cache operational"
    }
  }
}
```

---

### Gateway Statistics

Get overall gateway metrics and statistics.

```http
GET /api/stats
```

**Example**:
```bash
curl http://localhost:5000/api/stats \
  -H "X-API-Key: sk_admin..."
```

**Response** (200 OK):
```json
{
  "data": {
    "totalApiKeys": 500,
    "activeKeys": 450,
    "disabledKeys": 40,
    "expiredKeys": 10,
    "totalRequests": 5000000,
    "requestsToday": 150000,
    "successRate": 99.8,
    "averageResponseTime": 42,
    "peakRequestsPerSecond": 2500,
    "currentRequestsPerSecond": 1200,
    "cacheHitRate": 94.2,
    "databaseConnections": {
      "active": 45,
      "poolSize": 100
    },
    "topConsumers": [
      {
        "consumerId": "customer_001",
        "requests": 250000,
        "percentage": 5.0
      }
    ],
    "systemInfo": {
      "uptime": "72:15:30",
      "memoryUsage": 512,
      "version": "1.2.0"
    }
  },
  "success": true
}
```

---

## Batch Operations

### Batch Create Keys

Create multiple API keys in a single request.

```http
POST /api/apikeys/batch
Content-Type: application/json
```

**Request Body**:
```json
{
  "keys": [
    {
      "consumerId": "customer_1",
      "name": "Key 1"
    },
    {
      "consumerId": "customer_2",
      "name": "Key 2"
    }
  ]
}
```

**Response** (201 Created):
```json
{
  "data": {
    "created": 2,
    "failed": 0,
    "keys": [
      {
        "consumerId": "customer_1",
        "id": "key_123",
        "displayKey": "sk_..."
      }
    ]
  },
  "success": true
}
```

---

### Batch Revoke Keys

Revoke multiple API keys at once.

```http
PUT /api/apikeys/batch/revoke
Content-Type: application/json
```

**Request Body**:
```json
{
  "keyIds": ["key_abc123", "key_def456"]
}
```

**Response** (200 OK):
```json
{
  "data": {
    "revoked": 2,
    "failed": 0
  },
  "success": true
}
```

---

## Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| INVALID_API_KEY | 401 | API key not found or incorrect |
| API_KEY_EXPIRED | 401 | Key has expired |
| API_KEY_DISABLED | 403 | Key is disabled |
| RATE_LIMIT_EXCEEDED | 429 | Rate limit quota exceeded |
| INVALID_REQUEST | 400 | Malformed or invalid request |
| VALIDATION_FAILED | 422 | Business rule validation failed |
| RESOURCE_NOT_FOUND | 404 | Requested resource does not exist |
| CONFLICT | 409 | Request conflicts with existing data |
| INTERNAL_ERROR | 500 | Unexpected server error |

---

## Rate Limiting

The gateway itself is rate-limited to prevent abuse:

- **Default**: 10,000 requests/hour per API key
- **Limits are per key**: Each key has independent quota
- **Sliding Window**: Limits reset continuously (not on hour boundary)
- **Response**: 429 Too Many Requests when exceeded

**Check remaining quota**:
```
HTTP/1.1 200 OK
X-RateLimit-Limit: 10000
X-RateLimit-Remaining: 9999
X-RateLimit-Reset: 2026-05-04T16:00:00Z
```

---

## Pagination

List endpoints support pagination:

```http
GET /api/apikeys/consumer/{consumerId}?limit=50&offset=100
```

**Response includes**:
```json
{
  "data": [...],
  "pagination": {
    "limit": 50,
    "offset": 100,
    "total": 1000
  }
}
```

---

## Filtering & Sorting

### Status Filter

```bash
GET /api/apikeys/consumer/cust_123?status=Active
```

Valid statuses: `Active`, `Disabled`, `Revoked`, `Expired`

### Date Range Filter

```bash
GET /api/usage/keys/key_123/records?startDate=2026-05-01&endDate=2026-05-04
```

---

## Webhooks (Future)

Future API versions will support webhooks for real-time events:

```
POST /webhooks/subscribe
{
  "url": "https://example.com/webhook",
  "events": ["key_created", "key_revoked", "rate_limit_exceeded"]
}
```

See [Deployment](deployment.md) for webhook configuration.

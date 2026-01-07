# Deployment Guide

Production deployment best practices and configuration for API Key Gateway.

## Pre-Deployment Checklist

- [ ] Database backup strategy defined
- [ ] HTTPS/TLS certificates obtained and configured
- [ ] Load balancer configured (if multi-instance)
- [ ] Monitoring and alerting set up
- [ ] Audit logging configured
- [ ] Rate limiting policies defined
- [ ] Admin API keys created for automation
- [ ] Disaster recovery plan documented
- [ ] Performance tested under expected load

## Deployment Methods

### Method 1: Docker Container (Recommended)

#### Prerequisites

```bash
# Check Docker installation
docker --version
# Docker version 20.10+

# Check Docker Compose
docker-compose --version
# Docker Compose version 2.0+
```

#### Single Instance Deployment

```bash
# Build the image
docker build -t api-key-gateway:1.2.0 .

# Run with SQL Server
docker run -d \
  --name api-key-gateway \
  --restart unless-stopped \
  -p 5000:5000 \
  -p 5001:5001 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Server=db.example.com;Database=ApiKeyGateway;User Id=sa;Password=Strong!Pass123;" \
  -e GATEWAY__REQUIRESSL=true \
  -e Serilog__MinimumLevel=Information \
  --log-driver json-file \
  --log-opt max-size=10m \
  --log-opt max-file=5 \
  api-key-gateway:1.2.0
```

#### Multi-Instance with Load Balancer

```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  nginx:
    image: nginx:latest
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    depends_on:
      - gateway1
      - gateway2
      - gateway3

  gateway1:
    image: api-key-gateway:1.2.0
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=ApiKeyGateway;..."
    depends_on:
      - sqlserver

  gateway2:
    image: api-key-gateway:1.2.0
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=ApiKeyGateway;..."
    depends_on:
      - sqlserver

  gateway3:
    image: api-key-gateway:1.2.0
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Server=sqlserver;Database=ApiKeyGateway;..."
    depends_on:
      - sqlserver

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "Strong!Pass123"
      ACCEPT_EULA: Y
    volumes:
      - sqldata:/var/opt/mssql/data

  redis:
    image: redis:latest
    ports:
      - "6379:6379"

volumes:
  sqldata:
```

```bash
# Deploy
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f gateway1
```

### Method 2: Kubernetes Deployment

#### Create Secrets

```bash
kubectl create namespace api-gateway

# Create secret for database connection
kubectl create secret generic api-gateway-secrets \
  --from-literal=database-connection="Server=sql.example.com;Database=ApiKeyGateway;..." \
  -n api-gateway

# Create secret for SSL certificates
kubectl create secret tls api-gateway-tls \
  --cert=cert.pem \
  --key=key.pem \
  -n api-gateway
```

#### Deployment Configuration

```yaml
# api-gateway-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: api-gateway
  namespace: api-gateway
  labels:
    app: api-gateway
    version: "1.2.0"
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  selector:
    matchLabels:
      app: api-gateway
  template:
    metadata:
      labels:
        app: api-gateway
    spec:
      serviceAccountName: api-gateway
      containers:
      - name: api-gateway
        image: api-key-gateway:1.2.0
        imagePullPolicy: Always
        
        ports:
        - name: http
          containerPort: 5000
          protocol: TCP
        - name: https
          containerPort: 5001
          protocol: TCP
        
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:5000;https://+:5001"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: api-gateway-secrets
              key: database-connection
        - name: GATEWAY__REQUIRESSL
          value: "true"
        - name: Serilog__MinimumLevel
          value: "Information"
        
        resources:
          requests:
            cpu: "250m"
            memory: "512Mi"
          limits:
            cpu: "500m"
            memory: "1Gi"
        
        livenessProbe:
          httpGet:
            path: /health
            port: http
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        
        readinessProbe:
          httpGet:
            path: /health/ready
            port: http
          initialDelaySeconds: 5
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 2
        
        volumeMounts:
        - name: logs
          mountPath: /app/logs
        - name: ssl
          mountPath: /app/ssl
          readOnly: true
      
      volumes:
      - name: logs
        emptyDir: {}
      - name: ssl
        secret:
          secretName: api-gateway-tls

---
apiVersion: v1
kind: Service
metadata:
  name: api-gateway
  namespace: api-gateway
  labels:
    app: api-gateway
spec:
  type: LoadBalancer
  ports:
  - name: http
    port: 80
    targetPort: 5000
  - name: https
    port: 443
    targetPort: 5001
  selector:
    app: api-gateway

---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: api-gateway
  namespace: api-gateway
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: api-gateway
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

```bash
# Deploy to Kubernetes
kubectl apply -f api-gateway-deployment.yaml

# Monitor deployment
kubectl get pods -n api-gateway
kubectl logs -n api-gateway -l app=api-gateway -f

# Get external IP
kubectl get service api-gateway -n api-gateway
```

### Method 3: Linux Systemd Service

```bash
# Build release
cd src/ApiKeyGateway
dotnet publish -c Release -o /opt/api-key-gateway

# Create service user
sudo useradd -m -s /bin/false api-gateway

# Create systemd service file
sudo tee /etc/systemd/system/api-key-gateway.service > /dev/null <<'EOF'
[Unit]
Description=API Key Gateway
After=network.target sql-server.service
StartLimitIntervalSec=60
StartLimitBurst=3

[Service]
Type=notify
User=api-gateway
WorkingDirectory=/opt/api-key-gateway
ExecStart=/usr/bin/dotnet /opt/api-key-gateway/ApiKeyGateway.dll
Restart=on-failure
RestartSec=10
StandardOutput=journal
StandardError=journal

Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://0.0.0.0:5000;https://0.0.0.0:5001"
Environment="ConnectionStrings__DefaultConnection=Server=localhost;Database=ApiKeyGateway;..."

# Security hardening
NoNewPrivileges=yes
PrivateTmp=yes
ProtectSystem=strict
ProtectHome=yes
ReadWritePaths=/opt/api-key-gateway/logs

[Install]
WantedBy=multi-user.target
EOF

# Enable and start
sudo systemctl daemon-reload
sudo systemctl enable api-key-gateway
sudo systemctl start api-key-gateway

# Monitor
sudo systemctl status api-key-gateway
sudo journalctl -u api-key-gateway -f
```

## Configuration for Production

### appsettings.Production.json

```json
{
  "ASPNETCORE_ENVIRONMENT": "Production",
  "Gateway": {
    "RequireSsl": true,
    "LogAllRequests": true,
    "EnableRateLimiting": true,
    "EnableCaching": true,
    "CacheExpirationMinutes": 60,
    "DefaultRateLimitPerHour": 10000,
    "DefaultRateLimitPerMinute": 500,
    "MaxConcurrentRequests": 5000,
    "AuditLogRetentionDays": 180,
    "DefaultKeyExpirationDays": 365,
    "ApiKeyPrefix": "sk_"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=db-prod.example.com;Database=ApiKeyGateway;User Id=api_user;Password=...;Connection Timeout=30;Pooling=true;Max Pool Size=100;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "ApiKeyGateway": "Information",
      "Microsoft": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/api-gateway/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90,
          "formatter": "Serilog.Formatting.Json.JsonFormatter"
        }
      },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "instrumentationKey": "YOUR_APP_INSIGHTS_KEY"
        }
      }
    ]
  },
  "ApplicationInsights": {
    "InstrumentationKey": "YOUR_APP_INSIGHTS_KEY"
  }
}
```

## Security Hardening

### 1. HTTPS/TLS Configuration

```csharp
// In Program.cs
services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 5001;
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
});

app.UseHsts(options =>
{
    options.MaxAge(TimeSpan.FromDays(365));
    options.IncludeSubdomains();
    options.Preload();
});

app.UseHttpsRedirection();
```

### 2. CORS Configuration

```json
{
  "Cors": {
    "AllowedOrigins": ["https://app.example.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Content-Type", "X-API-Key"],
    "AllowCredentials": false
  }
}
```

### 3. API Key Rotation

```bash
# Create new key
curl -X POST https://api.example.com/api/apikeys \
  -H "X-API-Key: admin_key" \
  -d '{"consumerId": "customer_123", "name": "New Prod Key"}'

# Revoke old key (after 24-hour grace period)
curl -X PUT https://api.example.com/api/apikeys/old_key_id/revoke
```

## Monitoring & Alerting

### Health Checks

```bash
# Gateway health
curl https://api.example.com/health

# Kubernetes probes (already configured)
livenessProbe: GET /health (30s)
readinessProbe: GET /health/ready (5s)
```

### Metrics to Monitor

1. **Response Time**: 50th, 95th, 99th percentiles
2. **Error Rate**: 4xx and 5xx error percentages
3. **Throughput**: Requests per second
4. **Cache Hit Rate**: % cache hits vs total requests
5. **Database Connections**: Active and queued connections
6. **Memory Usage**: Process memory and GC collections
7. **Disk Space**: Log file growth

### Application Insights Setup

```csharp
// In Program.cs
services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);
```

```json
// appsettings.json
{
  "ApplicationInsights": {
    "InstrumentationKey": "YOUR_KEY_HERE"
  }
}
```

### Prometheus Metrics (Optional)

```csharp
services.AddPrometheusMetrics();

app.MapPrometheusScrapingEndpoint("/metrics");
```

## Backup & Recovery

### Database Backup Strategy

```bash
# Daily backup to Azure Blob Storage
az sql db export \
  --resource-group mygroup \
  --server myserver \
  --name ApiKeyGateway \
  --admin-user "sa" \
  --admin-password "password" \
  --storage-key "key" \
  --storage-uri "https://mystg.blob.core.windows.net" \
  --bacpac-name "backup-$(date +%Y%m%d).bacpac"
```

### Audit Log Archive

```bash
# Monthly export to cold storage
SELECT * INTO ApiKeyGateway_Audit_2026_05 
FROM AuditLogs 
WHERE CreatedAt >= '2026-05-01' AND CreatedAt < '2026-06-01';

-- Archive to blob storage
```

## Disaster Recovery

### RTO/RPO Targets

- **RTO** (Recovery Time Objective): 1 hour
- **RPO** (Recovery Point Objective): 15 minutes

### Failover Procedure

1. Detect failure (health check fails for 2 minutes)
2. DNS failover to standby region
3. Restore database from latest backup
4. Bring up new gateway instances
5. Update consumers with new endpoint

## Scaling Strategies

### Horizontal Scaling

Deploy multiple gateway instances:

```
Load Balancer
  ├─ Gateway Instance 1
  ├─ Gateway Instance 2
  ├─ Gateway Instance 3
  └─ Gateway Instance N
     └─ Shared SQL Server
     └─ Shared Redis Cache
```

### Database Scaling

```
Write Operations → Primary Database
Read Operations → Read Replicas
Analytics → Separate Analytics Database
```

### Caching Strategy

```
L1: In-memory (per instance, 30 min TTL)
L2: Redis (distributed, shared across instances)
L3: Database (source of truth)
```

## Performance Tuning

### Database Optimization

```sql
-- Create indexes for fast lookups
CREATE INDEX idx_apikey_consumerId ON ApiKeys(ConsumerId);
CREATE INDEX idx_ratelimit_keyId ON RateLimits(ApiKeyId);
CREATE INDEX idx_usage_timestamp ON UsageRecords(Timestamp);

-- Statistics update
UPDATE STATISTICS ApiKeys;
UPDATE STATISTICS UsageRecords;
```

### Connection Pool Tuning

```
Connection String: 
  Pooling=true;
  Max Pool Size=100;
  Min Pool Size=10;
  Connection Timeout=30;
```

### Cache Configuration

```json
{
  "Gateway": {
    "CachingOptions": {
      "MaxCacheSize": 50000,
      "ExpirationMinutes": 60,
      "EvictionPolicy": "LRU"
    }
  }
}
```

## Maintenance Operations

### Rolling Updates

```bash
# Kubernetes
kubectl rollout restart deployment/api-gateway -n api-gateway

# Docker Compose
docker-compose up -d --no-deps --build gateway1
docker-compose up -d --no-deps --build gateway2
docker-compose up -d --no-deps --build gateway3
```

### Database Maintenance

```sql
-- Rebuild fragmented indexes
ALTER INDEX ALL ON ApiKeys REBUILD;

-- Update statistics
UPDATE STATISTICS ApiKeys;

-- Archive old audit logs
DELETE FROM AuditLogs 
WHERE CreatedAt < DATEADD(day, -180, GETDATE());
```

## Support & Troubleshooting

For production issues, see [FAQ](faq.md) or contact `rutova2@gmail.com`.

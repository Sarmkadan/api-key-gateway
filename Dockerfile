# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10 AS builder

WORKDIR /build

# Copy project files
COPY src/ApiKeyGateway/ApiKeyGateway.csproj ./

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ApiKeyGateway/ .

# Build application
RUN dotnet build -c Release --no-restore

# Publish application
RUN dotnet publish -c Release -o /app --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10

LABEL maintainer="Vladyslav Zaiets <rutova2@gmail.com>"
LABEL org.opencontainers.image.title="API Key Gateway"
LABEL org.opencontainers.image.description="Lightweight API key authentication gateway for self-hosted services"
LABEL org.opencontainers.image.source="https://github.com/sarmkadan/api-key-gateway"

WORKDIR /app

# Copy published application from builder
COPY --from=builder /app .

# Create non-root user for security
RUN useradd -m -u 1001 -s /bin/false api-gateway && \
    chown -R api-gateway:api-gateway /app

USER api-gateway

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD dotnet /app/ApiKeyGateway.dll --check-health || exit 1

# Expose ports
EXPOSE 8080

# Environment configuration
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start application
ENTRYPOINT ["dotnet", "ApiKeyGateway.dll"]

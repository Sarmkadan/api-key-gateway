# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Makefile for API Key Gateway
# Common build and deployment tasks
#
# Usage:
#   make help              - Show this help message
#   make build             - Build the project
#   make test              - Run tests
#   make clean             - Clean build artifacts
#   make docker-build      - Build Docker image
#   make docker-run        - Run Docker container
#   make dev               - Start development environment
#   make format            - Format code
#   make lint              - Run code analysis

.PHONY: help build restore clean test run dev docker-build docker-run docker-stop \
        format lint publish deploy stop logs

# Variables
PROJECT_DIR := src/ApiKeyGateway
TESTS_DIR := tests
DOCKER_IMAGE := api-key-gateway
DOCKER_CONTAINER := api-key-gateway
DOTNET_VERSION := 10
CONFIGURATION := Release

# Colors for output
RED := \033[0;31m
GREEN := \033[0;32m
BLUE := \033[0;34m
YELLOW := \033[1;33m
NC := \033[0m # No Color

# Default target
.DEFAULT_GOAL := help

# ============================================================================
# Help
# ============================================================================

help:
	@echo "$(BLUE)API Key Gateway - Build Automation$(NC)"
	@echo ""
	@echo "$(GREEN)Available targets:$(NC)"
	@echo ""
	@echo "  $(YELLOW)Development$(NC)"
	@echo "    make build              - Build the project (Release)"
	@echo "    make restore            - Restore NuGet packages"
	@echo "    make clean              - Clean build artifacts"
	@echo "    make dev                - Start development server (watch mode)"
	@echo "    make format             - Format code with dotnet format"
	@echo "    make lint               - Run code analysis and style checks"
	@echo ""
	@echo "  $(YELLOW)Testing$(NC)"
	@echo "    make test               - Run all unit tests"
	@echo "    make test-coverage      - Run tests with code coverage"
	@echo "    make test-watch         - Run tests in watch mode"
	@echo ""
	@echo "  $(YELLOW)Docker$(NC)"
	@echo "    make docker-build       - Build Docker image"
	@echo "    make docker-run         - Run Docker container"
	@echo "    make docker-stop        - Stop Docker container"
	@echo "    make docker-logs        - View Docker container logs"
	@echo "    make docker-compose-up  - Start with Docker Compose"
	@echo "    make docker-compose-down- Stop Docker Compose services"
	@echo ""
	@echo "  $(YELLOW)Utilities$(NC)"
	@echo "    make logs               - View application logs"
	@echo "    make db-migrate         - Run database migrations"
	@echo "    make db-clean           - Drop and recreate database"
	@echo ""
	@echo "$(GREEN)Examples:$(NC)"
	@echo "  make build test docker-build docker-run"
	@echo ""

# ============================================================================
# Core Build Targets
# ============================================================================

build: restore
	@echo "$(BLUE)Building project...$(NC)"
	@dotnet build $(PROJECT_DIR) --configuration $(CONFIGURATION) --no-restore
	@echo "$(GREEN)✅ Build completed$(NC)"

restore:
	@echo "$(BLUE)Restoring dependencies...$(NC)"
	@dotnet restore
	@echo "$(GREEN)✅ Dependencies restored$(NC)"

clean:
	@echo "$(BLUE)Cleaning build artifacts...$(NC)"
	@dotnet clean $(PROJECT_DIR)
	@rm -rf $(PROJECT_DIR)/bin $(PROJECT_DIR)/obj
	@rm -rf $(TESTS_DIR)/bin $(TESTS_DIR)/obj
	@rm -rf .coverage
	@echo "$(GREEN)✅ Clean completed$(NC)"

# ============================================================================
# Development Targets
# ============================================================================

dev: restore
	@echo "$(BLUE)Starting development server (watch mode)...$(NC)"
	@echo "$(YELLOW)Press Ctrl+C to stop$(NC)"
	@cd $(PROJECT_DIR) && dotnet watch run --no-restore

format:
	@echo "$(BLUE)Formatting code...$(NC)"
	@dotnet format --verify-no-changes --verbosity diagnostic || dotnet format
	@echo "$(GREEN)✅ Code formatted$(NC)"

lint:
	@echo "$(BLUE)Running code analysis...$(NC)"
	@dotnet build $(PROJECT_DIR) /p:TreatWarningsAsErrors=true --no-restore
	@if dotnet format --verify-no-changes --verbosity quiet >/dev/null 2>&1; then \
		echo "$(GREEN)✅ Code style OK$(NC)"; \
	else \
		echo "$(YELLOW)⚠️  Code style issues found$(NC)"; \
		echo "   Run: make format"; \
	fi

# ============================================================================
# Testing Targets
# ============================================================================

test: restore
	@echo "$(BLUE)Running unit tests...$(NC)"
	@dotnet test $(TESTS_DIR) --configuration $(CONFIGURATION) --no-restore \
		--logger "console;verbosity=normal"
	@echo "$(GREEN)✅ Tests completed$(NC)"

test-coverage: restore
	@echo "$(BLUE)Running tests with code coverage...$(NC)"
	@dotnet test $(TESTS_DIR) --configuration $(CONFIGURATION) --no-restore \
		--collect:"XPlat Code Coverage" \
		--logger "trx;LogFileName=test-results.trx"
	@echo "$(GREEN)✅ Coverage report generated in $(TESTS_DIR)/TestResults$(NC)"

test-watch: restore
	@echo "$(BLUE)Running tests in watch mode...$(NC)"
	@echo "$(YELLOW)Press Ctrl+C to stop$(NC)"
	@cd $(TESTS_DIR) && dotnet watch test

# ============================================================================
# Docker Targets
# ============================================================================

docker-build:
	@echo "$(BLUE)Building Docker image: $(DOCKER_IMAGE)...$(NC)"
	@docker build -t $(DOCKER_IMAGE):latest -t $(DOCKER_IMAGE):$(shell git describe --tags 2>/dev/null || echo "dev") .
	@echo "$(GREEN)✅ Docker image built$(NC)"
	@docker images | grep $(DOCKER_IMAGE)

docker-run: docker-build
	@echo "$(BLUE)Running Docker container...$(NC)"
	@docker run -d \
		--name $(DOCKER_CONTAINER) \
		-p 5000:5000 \
		-p 5001:5001 \
		-e ASPNETCORE_ENVIRONMENT=Development \
		$(DOCKER_IMAGE):latest
	@echo "$(GREEN)✅ Container started: $(DOCKER_CONTAINER)$(NC)"
	@echo "   Access at: http://localhost:5000"
	@echo "   View logs: make docker-logs"

docker-stop:
	@echo "$(BLUE)Stopping Docker container...$(NC)"
	@docker stop $(DOCKER_CONTAINER) || true
	@docker rm $(DOCKER_CONTAINER) || true
	@echo "$(GREEN)✅ Container stopped$(NC)"

docker-logs:
	@echo "$(BLUE)Docker container logs:$(NC)"
	@docker logs -f $(DOCKER_CONTAINER)

docker-compose-up:
	@echo "$(BLUE)Starting services with Docker Compose...$(NC)"
	@docker-compose up -d
	@echo "$(GREEN)✅ Services started$(NC)"
	@echo ""
	@echo "   Gateway:  http://localhost:5000"
	@echo "   DB:       localhost:1433"
	@echo "   Adminer:  http://localhost:8080"

docker-compose-down:
	@echo "$(BLUE)Stopping Docker Compose services...$(NC)"
	@docker-compose down
	@echo "$(GREEN)✅ Services stopped$(NC)"

docker-compose-logs:
	@docker-compose logs -f

# ============================================================================
# Database Targets
# ============================================================================

db-migrate:
	@echo "$(BLUE)Running database migrations...$(NC)"
	@cd $(PROJECT_DIR) && dotnet ef database update
	@echo "$(GREEN)✅ Migrations applied$(NC)"

db-clean:
	@echo "$(YELLOW)⚠️  This will drop and recreate the database$(NC)"
	@read -p "Continue? (y/n) " -n 1 -r; \
	echo; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		cd $(PROJECT_DIR) && dotnet ef database drop --force && dotnet ef database update; \
		echo "$(GREEN)✅ Database reset$(NC)"; \
	else \
		echo "Cancelled"; \
	fi

# ============================================================================
# Utility Targets
# ============================================================================

logs:
	@echo "$(BLUE)Application logs:$(NC)"
	@tail -f logs/*.log 2>/dev/null || echo "No log files found"

version:
	@echo "$(BLUE)Version information:$(NC)"
	@dotnet --version
	@echo "Project: $$(grep '<TargetFramework>' $(PROJECT_DIR)/$(PROJECT_DIR).csproj | sed -e 's/.*<TargetFramework>//;s/<\/TargetFramework>.*//')"

publish:
	@echo "$(BLUE)Publishing release build...$(NC)"
	@dotnet publish $(PROJECT_DIR) -c $(CONFIGURATION) -o ./publish
	@echo "$(GREEN)✅ Published to ./publish$(NC)"

install-tools:
	@echo "$(BLUE)Installing .NET tools...$(NC)"
	@dotnet tool update -g dotnet-format || dotnet tool install -g dotnet-format
	@dotnet tool update -g dotnet-ef || dotnet tool install -g dotnet-ef
	@echo "$(GREEN)✅ Tools installed$(NC)"

# ============================================================================
# Composite Targets
# ============================================================================

all: clean build test lint
	@echo "$(GREEN)✅ All checks passed$(NC)"

ci: restore build test lint
	@echo "$(GREEN)✅ CI pipeline completed$(NC)"

demo: docker-compose-up
	@echo "$(GREEN)✅ Demo environment ready$(NC)"
	@echo ""
	@sleep 3
	@echo "Testing gateway..."
	@curl -s http://localhost:5000/health | jq . || echo "Gateway not ready yet, wait a moment"

# ============================================================================
# Info Targets
# ============================================================================

info:
	@echo "$(BLUE)Project Information:$(NC)"
	@echo "  Name:           API Key Gateway"
	@echo "  Version:        $$(grep '<Version>' $(PROJECT_DIR)/$(PROJECT_DIR).csproj | sed -e 's/.*<Version>//;s/<\/Version>.*//')"
	@echo "  .NET Version:   $(DOTNET_VERSION)"
	@echo "  Configuration:  $(CONFIGURATION)"
	@echo "  Project Dir:    $(PROJECT_DIR)"
	@echo "  Tests Dir:      $(TESTS_DIR)"

.PHONY: info

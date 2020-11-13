#!/bin/bash
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# API Key Gateway - Quick Operations Script
# Common cURL operations for managing API keys
#
# Usage:
#   ./04-bash-quick-operations.sh
#
# Environment variables:
#   GATEWAY_URL - Base URL (default: http://localhost:5000)
#   ADMIN_KEY - Admin API key (default: admin_key_example)

set -e

# Configuration
GATEWAY_URL="${GATEWAY_URL:-http://localhost:5000}"
ADMIN_KEY="${ADMIN_KEY:-admin_key_example}"
CONSUMER_ID="bash_demo_$(date +%s)"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Utility function for printing
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

log_step() {
    echo -e "\n${YELLOW}$1${NC}"
}

# Function to make API calls
api_call() {
    local method=$1
    local endpoint=$2
    local data=$3

    if [ -z "$data" ]; then
        curl -s -X "$method" \
            -H "X-API-Key: $ADMIN_KEY" \
            -H "Content-Type: application/json" \
            "$GATEWAY_URL$endpoint"
    else
        curl -s -X "$method" \
            -H "X-API-Key: $ADMIN_KEY" \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$GATEWAY_URL$endpoint"
    fi
}

# Function to extract JSON value
get_json_value() {
    echo "$1" | grep -o "\"$2\":\"[^\"]*\"" | cut -d'"' -f4
}

main() {
    echo -e "${YELLOW}🚀 API Key Gateway - Bash Operations Script${NC}\n"

    # Check gateway connectivity
    log_info "Checking gateway connectivity..."
    if ! curl -s "$GATEWAY_URL/health" > /dev/null; then
        log_error "Cannot connect to gateway at $GATEWAY_URL"
        exit 1
    fi
    log_success "Gateway is online\n"

    # Step 1: Create an API key
    log_step "1️⃣  Creating API key..."
    create_response=$(api_call POST "/api/apikeys" "{
        \"consumerId\": \"$CONSUMER_ID\",
        \"name\": \"Bash Demo Key\",
        \"expirationDays\": 90,
        \"rateLimit\": {
            \"requestsPerSecond\": 100,
            \"requestsPerMinute\": 5000
        }
    }")

    # Extract key ID from response
    key_id=$(echo "$create_response" | jq -r '.data.id')
    display_key=$(echo "$create_response" | jq -r '.data.displayKey')

    if [ -z "$key_id" ] || [ "$key_id" = "null" ]; then
        log_error "Failed to create API key"
        echo "Response: $create_response"
        exit 1
    fi

    log_success "Key created: $key_id"
    log_info "Display key: $display_key"

    # Step 2: Verify key was created
    log_step "2️⃣  Verifying key details..."
    details=$(api_call GET "/api/apikeys/$key_id")
    status=$(echo "$details" | jq -r '.data.status')
    expires=$(echo "$details" | jq -r '.data.expiresAt')

    log_success "Key status: $status"
    log_info "Expires: $expires"

    # Step 3: List keys for consumer
    log_step "3️⃣  Listing all keys for consumer..."
    keys_list=$(api_call GET "/api/apikeys/consumer/$CONSUMER_ID")
    key_count=$(echo "$keys_list" | jq '.data | length')

    log_success "Found $key_count key(s)"
    echo "$keys_list" | jq '.data[] | {id: .id, name: .name, status: .status}' | head -20

    # Step 4: Update key rate limit
    log_step "4️⃣  Updating rate limit..."
    api_call PUT "/api/apikeys/$key_id" "{
        \"rateLimit\": {
            \"requestsPerSecond\": 200
        }
    }" > /dev/null

    log_success "Rate limit updated to 200 req/sec"

    # Step 5: Check gateway statistics
    log_step "5️⃣  Getting gateway statistics..."
    stats=$(api_call GET "/api/stats")
    total_keys=$(echo "$stats" | jq '.data.totalApiKeys')
    active_keys=$(echo "$stats" | jq '.data.activeKeys')
    success_rate=$(echo "$stats" | jq '.data.successRate')

    log_success "Total API keys: $total_keys"
    log_info "Active keys: $active_keys"
    log_info "Success rate: $success_rate%"

    # Step 6: Get usage statistics
    log_step "6️⃣  Getting usage statistics..."
    usage=$(api_call GET "/api/usage/keys/$key_id/statistics?period=daily")
    requests=$(echo "$usage" | jq '.data.requestCount // 0')
    avg_time=$(echo "$usage" | jq '.data.averageResponseTime // 0')

    log_success "Requests: $requests"
    log_info "Avg response time: ${avg_time}ms"

    # Step 7: Test rate limiting (simulate)
    log_step "7️⃣  Simulating requests..."
    for i in {1..3}; do
        log_info "Request $i..."
        # This would be a real authenticated request in practice
        sleep 1
    done
    log_success "Simulation complete"

    # Step 8: Export usage data
    log_step "8️⃣  Exporting usage data..."
    csv_file="/tmp/usage_${key_id}.csv"
    curl -s -H "X-API-Key: $ADMIN_KEY" \
        "$GATEWAY_URL/api/usage/export/$key_id?format=csv" > "$csv_file"

    if [ -s "$csv_file" ]; then
        log_success "Usage data exported to $csv_file"
        log_info "Preview:"
        head -5 "$csv_file" | sed 's/^/   /'
    else
        log_info "No usage data available yet (new key)"
    fi

    # Step 9: Disable key (temporary)
    log_step "9️⃣  Disabling key (temporary)..."
    api_call PUT "/api/apikeys/$key_id/disable" > /dev/null
    log_success "Key disabled"

    # Step 10: Re-enable key
    log_step "🔟 Re-enabling key..."
    api_call PUT "/api/apikeys/$key_id/enable" > /dev/null
    log_success "Key enabled"

    # Step 11: Revoke key (permanent)
    log_step "1️⃣1️⃣  Revoking key (permanent)..."
    api_call PUT "/api/apikeys/$key_id/revoke" > /dev/null
    log_success "Key revoked"

    # Step 12: Verify revocation
    log_step "1️⃣2️⃣  Verifying revocation..."
    final_status=$(api_call GET "/api/apikeys/$key_id" | jq -r '.data.status')
    log_success "Final status: $final_status"

    # Summary
    log_step "📊 Summary"
    echo "   Consumer ID: $CONSUMER_ID"
    echo "   Key ID: $key_id"
    echo "   Display Key: $display_key"
    echo "   Initial Status: $status"
    echo "   Final Status: $final_status"

    log_step "✨ Example completed successfully!"
}

# Run main function
main "$@"

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"time"
)

// Service demonstrates integrating API Key Gateway with a Go service
// Creates API keys and validates them for incoming requests

const (
	gatewayURL = "http://localhost:5000"
	adminKey   = "admin_key_example"
)

// CreateKeyRequest is the payload for creating an API key
type CreateKeyRequest struct {
	ConsumerID     string `json:"consumerId"`
	Name           string `json:"name"`
	ExpirationDays int    `json:"expirationDays"`
	RateLimit      *RateLimit `json:"rateLimit,omitempty"`
}

// RateLimit defines rate limiting rules
type RateLimit struct {
	RequestsPerSecond int `json:"requestsPerSecond,omitempty"`
	RequestsPerMinute int `json:"requestsPerMinute,omitempty"`
	RequestsPerHour   int `json:"requestsPerHour,omitempty"`
}

// APIKeyResponse is the gateway's response when creating a key
type APIKeyResponse struct {
	Data APIKeyData `json:"data"`
}

// APIKeyData contains the API key details
type APIKeyData struct {
	ID          string    `json:"id"`
	DisplayKey  string    `json:"displayKey"`
	ConsumerID  string    `json:"consumerId"`
	Name        string    `json:"name"`
	Status      string    `json:"status"`
	CreatedAt   time.Time `json:"createdAt"`
	ExpiresAt   time.Time `json:"expiresAt"`
	RateLimit   RateLimit `json:"rateLimit"`
}

// GatewayClient provides methods to interact with API Key Gateway
type GatewayClient struct {
	baseURL  string
	adminKey string
	client   *http.Client
}

// NewGatewayClient creates a new gateway client
func NewGatewayClient(baseURL, adminKey string) *GatewayClient {
	return &GatewayClient{
		baseURL:  baseURL,
		adminKey: adminKey,
		client: &http.Client{
			Timeout: 10 * time.Second,
		},
	}
}

// CreateKey creates a new API key
func (gc *GatewayClient) CreateKey(consumerID, name string) (*APIKeyData, error) {
	req := CreateKeyRequest{
		ConsumerID:     consumerID,
		Name:           name,
		ExpirationDays: 365,
		RateLimit: &RateLimit{
			RequestsPerSecond: 100,
			RequestsPerMinute: 5000,
			RequestsPerHour:   100000,
		},
	}

	body, err := json.Marshal(req)
	if err != nil {
		return nil, err
	}

	httpReq, err := http.NewRequest("POST", fmt.Sprintf("%s/api/apikeys", gc.baseURL), bytes.NewReader(body))
	if err != nil {
		return nil, err
	}

	httpReq.Header.Set("X-API-Key", gc.adminKey)
	httpReq.Header.Set("Content-Type", "application/json")

	resp, err := gc.client.Do(httpReq)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusCreated {
		bodyBytes, _ := io.ReadAll(resp.Body)
		return nil, fmt.Errorf("failed to create key: %d %s", resp.StatusCode, string(bodyBytes))
	}

	var keyResp APIKeyResponse
	if err := json.NewDecoder(resp.Body).Decode(&keyResp); err != nil {
		return nil, err
	}

	return &keyResp.Data, nil
}

// GetKeyStats retrieves usage statistics for a key
func (gc *GatewayClient) GetKeyStats(keyID string) (map[string]interface{}, error) {
	httpReq, err := http.NewRequest("GET",
		fmt.Sprintf("%s/api/usage/keys/%s/statistics", gc.baseURL, keyID), nil)
	if err != nil {
		return nil, err
	}

	httpReq.Header.Set("X-API-Key", gc.adminKey)

	resp, err := gc.client.Do(httpReq)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("failed to get stats: %d", resp.StatusCode)
	}

	var result map[string]interface{}
	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		return nil, err
	}

	return result, nil
}

// DisableKey temporarily disables an API key
func (gc *GatewayClient) DisableKey(keyID string) error {
	httpReq, err := http.NewRequest("PUT",
		fmt.Sprintf("%s/api/apikeys/%s/disable", gc.baseURL, keyID), nil)
	if err != nil {
		return err
	}

	httpReq.Header.Set("X-API-Key", gc.adminKey)

	resp, err := gc.client.Do(httpReq)
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return fmt.Errorf("failed to disable key: %d", resp.StatusCode)
	}

	return nil
}

// RotateKey creates a new key and disables the old one
func (gc *GatewayClient) RotateKey(consumerID, oldKeyID, keyName string) (*APIKeyData, error) {
	log.Printf("🔄 Rotating key for consumer: %s", consumerID)

	// Create new key
	newKey, err := gc.CreateKey(consumerID, fmt.Sprintf("%s (rotated)", keyName))
	if err != nil {
		return nil, fmt.Errorf("failed to create new key: %w", err)
	}

	log.Printf("✅ Created new key: %s", newKey.ID)

	// Disable old key
	if err := gc.DisableKey(oldKeyID); err != nil {
		log.Printf("⚠️  Warning: failed to disable old key: %v", err)
	} else {
		log.Printf("✅ Disabled old key: %s", oldKeyID)
	}

	return newKey, nil
}

// ServiceWithKeyValidation demonstrates a service that validates API keys
type ServiceWithKeyValidation struct {
	gatewayClient *GatewayClient
}

// NewServiceWithKeyValidation creates a new service
func NewServiceWithKeyValidation(gatewayClient *GatewayClient) *ServiceWithKeyValidation {
	return &ServiceWithKeyValidation{
		gatewayClient: gatewayClient,
	}
}

// ValidateKeyMiddleware is a middleware that validates API keys
func (s *ServiceWithKeyValidation) ValidateKeyMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		// In a real scenario, you'd call the gateway to validate the key
		// For this example, we just log it
		apiKey := r.Header.Get("X-API-Key")
		if apiKey == "" {
			http.Error(w, "Missing API key", http.StatusUnauthorized)
			return
		}

		log.Printf("📝 Request with API key: %s (first 8 chars)", apiKey[:min(8, len(apiKey))])
		next.ServeHTTP(w, r)
	})
}

// HandleDataRequest handles a sample API request
func (s *ServiceWithKeyValidation) HandleDataRequest(w http.ResponseWriter, r *http.Request) {
	response := map[string]interface{}{
		"status":  "ok",
		"message": "Data retrieved successfully",
		"data": map[string]interface{}{
			"items": []string{"item1", "item2", "item3"},
		},
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(response)
}

func min(a, b int) int {
	if a < b {
		return a
	}
	return b
}

func main() {
	fmt.Println("🚀 API Key Gateway - Go Service Integration Example\n")

	// Create gateway client
	client := NewGatewayClient(gatewayURL, adminKey)

	// Create a unique consumer ID
	consumerID := fmt.Sprintf("go_demo_%d", time.Now().Unix())

	// Step 1: Create API key
	fmt.Println("1️⃣  Creating API key...")
	key, err := client.CreateKey(consumerID, "Go Service Key")
	if err != nil {
		log.Fatalf("❌ Failed to create key: %v", err)
	}
	fmt.Printf("✅ Key created: %s\n", key.ID)
	fmt.Printf("   Display key: %s\n", key.DisplayKey)
	fmt.Printf("   Status: %s\n", key.Status)
	fmt.Printf("   Expires: %s\n\n", key.ExpiresAt.Format("2006-01-02"))

	// Step 2: Get statistics
	fmt.Println("2️⃣  Getting key statistics...")
	stats, err := client.GetKeyStats(key.ID)
	if err != nil {
		log.Fatalf("❌ Failed to get stats: %v", err)
	}
	fmt.Printf("✅ Statistics retrieved\n")
	statsByte, _ := json.MarshalIndent(stats, "   ", "  ")
	fmt.Println(string(statsByte))
	fmt.Println()

	// Step 3: Demonstrate key rotation
	fmt.Println("3️⃣  Rotating key...")
	newKey, err := client.RotateKey(consumerID, key.ID, "Go Service Key")
	if err != nil {
		log.Fatalf("❌ Failed to rotate key: %v", err)
	}
	fmt.Printf("✅ New key created: %s\n\n", newKey.ID)

	// Step 4: Demonstrate HTTP service with key validation
	fmt.Println("4️⃣  Starting sample HTTP service...")
	service := NewServiceWithKeyValidation(client)

	// Register routes
	mux := http.NewServeMux()
	mux.HandleFunc("/api/data", service.ValidateKeyMiddleware(
		http.HandlerFunc(service.HandleDataRequest)))

	fmt.Println("✅ Service ready at http://localhost:8080/api/data")
	fmt.Printf("   Try: curl -H 'X-API-Key: %s' http://localhost:8080/api/data\n", newKey.DisplayKey)

	// Server setup (non-blocking for demo)
	go func() {
		if err := http.ListenAndServe(":8080", mux); err != nil && err != http.ErrServerClosed {
			log.Printf("⚠️  Server error: %v", err)
		}
	}()

	// Give server time to start
	time.Sleep(1 * time.Second)

	// Step 5: Make a test request to the service
	fmt.Println("\n5️⃣  Testing service with API key...")
	testReq, _ := http.NewRequest("GET", "http://localhost:8080/api/data", nil)
	testReq.Header.Set("X-API-Key", newKey.DisplayKey)

	client.client.Timeout = 5 * time.Second
	testResp, err := client.client.Do(testReq)
	if err == nil && testResp.StatusCode == http.StatusOK {
		var testResult map[string]interface{}
		json.NewDecoder(testResp.Body).Decode(&testResult)
		testResp.Body.Close()
		fmt.Printf("✅ Service responded successfully\n")
	} else {
		fmt.Printf("ℹ️  Service test (expected timeout in standalone mode)\n")
	}

	// Summary
	fmt.Println("\n📊 Example Summary")
	fmt.Printf("   Consumer ID: %s\n", consumerID)
	fmt.Printf("   Original Key ID: %s\n", key.ID)
	fmt.Printf("   Rotated Key ID: %s\n", newKey.ID)
	fmt.Printf("   New Key Display: %s\n", newKey.DisplayKey)

	fmt.Println("\n✨ Example completed successfully!")
}

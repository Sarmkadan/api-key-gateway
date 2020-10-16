#!/usr/bin/env node
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

/**
 * Basic Node.js example using API Key Gateway
 * Creates an API key and uses it to authenticate requests
 *
 * Requirements:
 *   npm install axios
 *
 * Usage:
 *   node 01-nodejs-basic.js
 */

const axios = require('axios');

// Configuration
const GATEWAY_URL = process.env.GATEWAY_URL || 'http://localhost:5000';
const ADMIN_KEY = process.env.ADMIN_KEY || 'admin_key_example';

// Create axios instance with default headers
const gateway = axios.create({
  baseURL: GATEWAY_URL,
  headers: {
    'X-API-Key': ADMIN_KEY,
    'Content-Type': 'application/json'
  }
});

async function main() {
  console.log('🚀 API Key Gateway Node.js Example\n');

  try {
    // Step 1: Create an API key
    console.log('1️⃣  Creating new API key...');
    const createResponse = await gateway.post('/api/apikeys', {
      consumerId: `customer_${Date.now()}`,
      name: 'Node.js Example Key',
      expirationDays: 90,
      rateLimit: {
        requestsPerSecond: 50,
        requestsPerMinute: 2000
      }
    });

    const apiKey = createResponse.data.data;
    console.log(`✅ Key created: ${apiKey.id}`);
    console.log(`   Display key: ${apiKey.displayKey}`);
    console.log(`   Consumer ID: ${apiKey.consumerId}`);
    console.log(`   Expires: ${apiKey.expiresAt}\n`);

    // Step 2: Retrieve key details
    console.log('2️⃣  Getting key details...');
    const detailsResponse = await gateway.get(`/api/apikeys/${apiKey.id}`);
    const keyDetails = detailsResponse.data.data;
    console.log(`✅ Status: ${keyDetails.status}`);
    console.log(`   Created: ${keyDetails.createdAt}\n`);

    // Step 3: Update rate limit
    console.log('3️⃣  Updating rate limit...');
    const updateResponse = await gateway.put(`/api/apikeys/${apiKey.id}`, {
      rateLimit: {
        requestsPerSecond: 100
      }
    });
    console.log(`✅ Rate limit updated\n`);

    // Step 4: Check usage
    console.log('4️⃣  Checking usage statistics...');
    const statsResponse = await gateway.get(
      `/api/usage/keys/${apiKey.id}/statistics`
    );
    const stats = statsResponse.data.data;
    console.log(`✅ Total requests: ${stats.requestCount}`);
    console.log(`   Success rate: ${((stats.successCount / stats.requestCount) * 100).toFixed(2)}%\n`);

    // Step 5: Disable key
    console.log('5️⃣  Disabling key (temporary)...');
    await gateway.put(`/api/apikeys/${apiKey.id}/disable`);
    console.log(`✅ Key disabled\n`);

    // Step 6: Re-enable key
    console.log('6️⃣  Re-enabling key...');
    await gateway.put(`/api/apikeys/${apiKey.id}/enable`);
    console.log(`✅ Key enabled\n`);

    // Step 7: Revoke key (permanent)
    console.log('7️⃣  Revoking key (permanent)...');
    await gateway.put(`/api/apikeys/${apiKey.id}/revoke`);
    console.log(`✅ Key revoked\n`);

    console.log('✨ Example completed successfully!');

  } catch (error) {
    console.error('❌ Error:', error.response?.data || error.message);
    process.exit(1);
  }
}

// Run the example
main();

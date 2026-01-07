#!/usr/bin/env python3
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

"""
Python example: Track API usage and export reports
Demonstrates usage statistics, filtering, and data export

Requirements:
    pip install requests

Usage:
    python 02-python-usage-tracking.py
"""

import requests
import json
import os
from datetime import datetime, timedelta

class GatewayClient:
    def __init__(self, base_url=None, admin_key=None):
        self.base_url = base_url or os.getenv('GATEWAY_URL', 'http://localhost:5000')
        self.admin_key = admin_key or os.getenv('ADMIN_KEY', 'admin_key_example')
        self.session = requests.Session()
        self.session.headers.update({
            'X-API-Key': self.admin_key,
            'Content-Type': 'application/json'
        })

    def create_key(self, consumer_id, name, rate_limit=None):
        payload = {
            'consumerId': consumer_id,
            'name': name,
            'expirationDays': 365
        }
        if rate_limit:
            payload['rateLimit'] = rate_limit

        response = self.session.post(f'{self.base_url}/api/apikeys', json=payload)
        return response.json()['data']

    def get_usage_statistics(self, key_id, period='daily', days=30):
        params = {'period': period, 'days': days}
        response = self.session.get(
            f'{self.base_url}/api/usage/keys/{key_id}/statistics',
            params=params
        )
        return response.json()['data']

    def get_usage_records(self, key_id, days=7, limit=100):
        params = {'days': days, 'limit': limit}
        response = self.session.get(
            f'{self.base_url}/api/usage/keys/{key_id}/records',
            params=params
        )
        return response.json()['data']

    def get_consumer_total_usage(self, consumer_id, days=30):
        params = {'days': days}
        response = self.session.get(
            f'{self.base_url}/api/usage/consumers/{consumer_id}/total',
            params=params
        )
        return response.json()['data']

    def export_usage_data(self, key_id, format='csv', days=30):
        params = {'format': format, 'days': days}
        response = self.session.get(
            f'{self.base_url}/api/usage/export/{key_id}',
            params=params
        )
        return response.content

def main():
    print('🚀 API Key Gateway - Usage Tracking Example\n')

    client = GatewayClient()

    try:
        # Create test key
        print('1️⃣  Creating API key...')
        consumer_id = f'tracking_demo_{int(datetime.now().timestamp())}'
        api_key = client.create_key(
            consumer_id=consumer_id,
            name='Usage Tracking Demo',
            rate_limit={
                'requestsPerSecond': 100,
                'requestsPerMinute': 5000
            }
        )
        key_id = api_key['id']
        print(f'✅ Created key: {key_id}\n')

        # Get statistics
        print('2️⃣  Getting usage statistics...')
        stats = client.get_usage_statistics(key_id, period='daily', days=30)
        print(f'✅ Total requests: {stats.get("requestCount", 0)}')
        print(f'   Success rate: {stats.get("successCount", 0)} / {stats.get("requestCount", 0)}')
        print(f'   Avg response time: {stats.get("averageResponseTime", 0)}ms')
        print(f'   Bandwidth: {stats.get("bandwidthUsed", 0)} bytes\n')

        # Get detailed records
        print('3️⃣  Getting usage records (last 7 days)...')
        records = client.get_usage_records(key_id, days=7, limit=50)
        if records:
            print(f'✅ Retrieved {len(records)} records:')
            for record in records[:3]:  # Show first 3
                print(f'   - {record.get("timestamp")}: '
                      f'{record.get("method")} {record.get("endpoint")} '
                      f'({record.get("statusCode")})')
            if len(records) > 3:
                print(f'   ... and {len(records) - 3} more')
        else:
            print('✅ No records yet (new key)')
        print()

        # Get consumer total usage
        print('4️⃣  Getting consumer total usage...')
        consumer_usage = client.get_consumer_total_usage(consumer_id, days=30)
        print(f'✅ Total requests by consumer: {consumer_usage.get("totalRequests", 0)}')
        print(f'   Active keys: {consumer_usage.get("activeKeyCount", 0)}')
        print(f'   Bandwidth: {consumer_usage.get("totalBandwidth", 0)} bytes\n')

        # Export usage data
        print('5️⃣  Exporting usage data to CSV...')
        csv_data = client.export_usage_data(key_id, format='csv', days=30)

        # Save to file
        export_file = f'usage_report_{key_id}.csv'
        with open(export_file, 'wb') as f:
            f.write(csv_data)
        print(f'✅ Exported to {export_file}\n')

        # Show usage patterns
        print('6️⃣  Usage analysis...')
        if records:
            # Count by status code
            status_codes = {}
            for record in records:
                status = record.get('statusCode')
                status_codes[status] = status_codes.get(status, 0) + 1

            print('✅ Requests by status code:')
            for status, count in sorted(status_codes.items()):
                print(f'   HTTP {status}: {count} requests')

            # Count by endpoint
            endpoints = {}
            for record in records:
                endpoint = record.get('endpoint', 'unknown')
                endpoints[endpoint] = endpoints.get(endpoint, 0) + 1

            print('\n✅ Top endpoints:')
            for endpoint, count in sorted(endpoints.items(),
                                         key=lambda x: x[1],
                                         reverse=True)[:5]:
                print(f'   {endpoint}: {count} requests')
        print()

        print('✨ Example completed successfully!')
        print(f'📊 Key ID for reference: {key_id}')

    except requests.exceptions.RequestException as e:
        print(f'❌ Request error: {e}')
        if hasattr(e.response, 'json'):
            print(f'   Details: {e.response.json()}')
    except Exception as e:
        print(f'❌ Error: {e}')

if __name__ == '__main__':
    main()

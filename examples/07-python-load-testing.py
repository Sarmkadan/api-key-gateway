#!/usr/bin/env python3
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

"""
Load testing script for API Key Gateway
Tests rate limiting, throughput, and response times

Requirements:
    pip install requests concurrent-futures

Usage:
    python 07-python-load-testing.py --concurrent 10 --duration 30
"""

import requests
import time
import statistics
import argparse
import threading
from concurrent.futures import ThreadPoolExecutor, as_completed
from datetime import datetime
import json
import sys

class LoadTester:
    def __init__(self, gateway_url, api_key, concurrent_users=5, duration_seconds=30):
        self.gateway_url = gateway_url
        self.api_key = api_key
        self.concurrent_users = concurrent_users
        self.duration_seconds = duration_seconds
        self.results = {
            'total': 0,
            'success': 0,
            'failed': 0,
            'rate_limited': 0,
            'response_times': [],
            'errors': []
        }
        self.lock = threading.Lock()
        self.start_time = None
        self.end_time = None

    def make_request(self):
        # Create a simple GET request to API
        try:
            start = time.time()
            response = requests.get(
                f'{self.gateway_url}/api/stats',
                headers={'X-API-Key': self.api_key},
                timeout=5
            )
            elapsed = time.time() - start

            with self.lock:
                self.results['total'] += 1
                self.results['response_times'].append(elapsed * 1000)  # Convert to ms

                if response.status_code == 200:
                    self.results['success'] += 1
                elif response.status_code == 429:  # Rate limited
                    self.results['rate_limited'] += 1
                    self.results['failed'] += 1
                else:
                    self.results['failed'] += 1
                    self.results['errors'].append(f"HTTP {response.status_code}")

            return True
        except requests.exceptions.Timeout:
            with self.lock:
                self.results['total'] += 1
                self.results['failed'] += 1
                self.results['errors'].append("Timeout")
            return False
        except Exception as e:
            with self.lock:
                self.results['total'] += 1
                self.results['failed'] += 1
                self.results['errors'].append(str(e))
            return False

    def worker(self):
        # Worker thread - make requests until time is up
        while time.time() - self.start_time < self.duration_seconds:
            self.make_request()

    def run(self):
        print(f'🚀 Load Testing API Key Gateway\n')
        print(f'⚙️  Configuration:')
        print(f'   Gateway: {self.gateway_url}')
        print(f'   Concurrent Users: {self.concurrent_users}')
        print(f'   Duration: {self.duration_seconds} seconds\n')

        self.start_time = time.time()

        # Create thread pool and launch workers
        with ThreadPoolExecutor(max_workers=self.concurrent_users) as executor:
            futures = [
                executor.submit(self.worker)
                for _ in range(self.concurrent_users)
            ]

            # Progress bar
            while time.time() - self.start_time < self.duration_seconds:
                elapsed = time.time() - self.start_time
                progress = (elapsed / self.duration_seconds) * 100
                bar_length = 40
                filled = int(bar_length * elapsed / self.duration_seconds)
                bar = '█' * filled + '░' * (bar_length - filled)
                print(f'\r⏳ Progress: [{bar}] {progress:.1f}% ({self.results["total"]} requests)',
                      end='', flush=True)
                time.sleep(0.5)

            # Wait for all workers to complete
            for future in futures:
                future.result()

        self.end_time = time.time()
        print('\n')
        self.print_results()

    def print_results(self):
        print(f'📊 Load Test Results\n')

        # Summary statistics
        total_time = self.end_time - self.start_time
        rps = self.results['total'] / total_time if total_time > 0 else 0

        print(f'⏱️  Duration: {total_time:.2f} seconds')
        print(f'📈 Total Requests: {self.results["total"]}')
        print(f'✅ Successful: {self.results["success"]}')
        print(f'❌ Failed: {self.results["failed"]}')
        print(f'⚠️  Rate Limited (429): {self.results["rate_limited"]}')
        print(f'📊 Throughput: {rps:.2f} req/sec\n')

        # Response time statistics
        if self.results['response_times']:
            response_times = self.results['response_times']
            print(f'⏲️  Response Times:')
            print(f'   Min: {min(response_times):.2f}ms')
            print(f'   Max: {max(response_times):.2f}ms')
            print(f'   Mean: {statistics.mean(response_times):.2f}ms')
            print(f'   Median: {statistics.median(response_times):.2f}ms')
            print(f'   StdDev: {statistics.stdev(response_times):.2f}ms' if len(response_times) > 1 else '')

            # Percentiles
            sorted_times = sorted(response_times)
            p50 = sorted_times[int(len(sorted_times) * 0.50)]
            p95 = sorted_times[int(len(sorted_times) * 0.95)]
            p99 = sorted_times[int(len(sorted_times) * 0.99)]

            print(f'   P50: {p50:.2f}ms')
            print(f'   P95: {p95:.2f}ms')
            print(f'   P99: {p99:.2f}ms\n')

        # Error details
        if self.results['errors']:
            print(f'🐛 Error Distribution:')
            error_counts = {}
            for error in self.results['errors']:
                error_counts[error] = error_counts.get(error, 0) + 1

            for error, count in sorted(error_counts.items(), key=lambda x: x[1], reverse=True):
                percentage = (count / self.results['total']) * 100
                print(f'   {error}: {count} ({percentage:.1f}%)')
            print()

        # Success rate
        success_rate = (self.results['success'] / self.results['total'] * 100) if self.results['total'] > 0 else 0
        print(f'✨ Success Rate: {success_rate:.2f}%')

        # Performance verdict
        print(f'\n🎯 Performance Analysis:')
        if rps > 1000:
            print(f'   ⭐⭐⭐ Excellent throughput ({rps:.0f} req/sec)')
        elif rps > 500:
            print(f'   ⭐⭐ Good throughput ({rps:.0f} req/sec)')
        elif rps > 100:
            print(f'   ⭐ Acceptable throughput ({rps:.0f} req/sec)')
        else:
            print(f'   ⚠️  Low throughput ({rps:.0f} req/sec) - check gateway resources')

        if statistics.mean(self.results['response_times']) < 50:
            print(f'   ⭐⭐⭐ Fast response times (< 50ms)')
        elif statistics.mean(self.results['response_times']) < 100:
            print(f'   ⭐⭐ Good response times (< 100ms)')
        else:
            print(f'   ⚠️  Slow response times (> 100ms)')

        if success_rate > 99:
            print(f'   ⭐⭐⭐ High reliability ({success_rate:.2f}%)')
        elif success_rate > 95:
            print(f'   ⭐⭐ Good reliability ({success_rate:.2f}%)')
        else:
            print(f'   ⚠️  Low reliability ({success_rate:.2f}%) - investigate errors')

def main():
    parser = argparse.ArgumentParser(description='Load test API Key Gateway')
    parser.add_argument('--gateway-url', default='http://localhost:5000',
                        help='Gateway URL (default: http://localhost:5000)')
    parser.add_argument('--api-key', default='admin_key_example',
                        help='API key for authentication')
    parser.add_argument('--concurrent', type=int, default=10,
                        help='Number of concurrent users (default: 10)')
    parser.add_argument('--duration', type=int, default=30,
                        help='Test duration in seconds (default: 30)')

    args = parser.parse_args()

    try:
        tester = LoadTester(
            gateway_url=args.gateway_url,
            api_key=args.api_key,
            concurrent_users=args.concurrent,
            duration_seconds=args.duration
        )
        tester.run()

    except KeyboardInterrupt:
        print('\n\n⚠️  Test interrupted by user')
        sys.exit(1)
    except Exception as e:
        print(f'\n❌ Error: {e}')
        sys.exit(1)

if __name__ == '__main__':
    main()

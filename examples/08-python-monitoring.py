#!/usr/bin/env python3
# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

"""
Monitoring script for API Key Gateway
Tracks health, performance, and alerts on issues

Requirements:
    pip install requests

Usage:
    python 08-python-monitoring.py --interval 60 --duration 600
"""

import requests
import time
import statistics
import argparse
import sys
from datetime import datetime
from collections import deque

class GatewayMonitor:
    def __init__(self, gateway_url, api_key, check_interval=60, max_samples=100):
        self.gateway_url = gateway_url
        self.api_key = api_key
        self.check_interval = check_interval
        self.max_samples = max_samples

        # Metrics tracking
        self.response_times = deque(maxlen=max_samples)
        self.success_counts = deque(maxlen=max_samples)
        self.error_counts = deque(maxlen=max_samples)
        self.health_status = deque(maxlen=max_samples)

        # Thresholds
        self.thresholds = {
            'avg_response_time_ms': 100,
            'error_rate_percent': 5,
            'health_check_timeout': 10
        }

    def check_health(self):
        try:
            start = time.time()
            response = requests.get(
                f'{self.gateway_url}/health',
                timeout=self.thresholds['health_check_timeout']
            )
            elapsed = time.time() - start

            if response.status_code == 200:
                data = response.json()
                status = data.get('status', 'Unknown')
                db_status = data.get('database', 'Unknown')

                self.response_times.append(elapsed * 1000)  # ms
                self.health_status.append(status)

                return {
                    'healthy': True,
                    'status': status,
                    'database': db_status,
                    'response_time': elapsed,
                    'checks': data.get('checks', {})
                }
            else:
                return {'healthy': False, 'error': f'HTTP {response.status_code}'}

        except requests.exceptions.Timeout:
            return {'healthy': False, 'error': 'Health check timeout'}
        except Exception as e:
            return {'healthy': False, 'error': str(e)}

    def get_gateway_stats(self):
        try:
            response = requests.get(
                f'{self.gateway_url}/api/stats',
                headers={'X-API-Key': self.api_key},
                timeout=5
            )

            if response.status_code == 200:
                return response.json().get('data', {})
            else:
                return None

        except Exception as e:
            print(f'⚠️  Failed to get stats: {e}')
            return None

    def check_api_keys(self):
        try:
            response = requests.get(
                f'{self.gateway_url}/api/stats',
                headers={'X-API-Key': self.api_key},
                timeout=5
            )

            if response.status_code == 200:
                data = response.json().get('data', {})
                return {
                    'total': data.get('totalApiKeys', 0),
                    'active': data.get('activeKeys', 0),
                    'disabled': data.get('disabledKeys', 0),
                    'expired': data.get('expiredKeys', 0)
                }
            return None

        except Exception:
            return None

    def check_performance(self):
        try:
            response = requests.get(
                f'{self.gateway_url}/api/stats',
                headers={'X-API-Key': self.api_key},
                timeout=5
            )

            if response.status_code == 200:
                data = response.json().get('data', {})
                return {
                    'avg_response_time': data.get('averageResponseTime', 0),
                    'throughput': data.get('currentRequestsPerSecond', 0),
                    'cache_hit_rate': data.get('cacheHitRate', 0),
                    'success_rate': data.get('successRate', 100)
                }
            return None

        except Exception:
            return None

    def print_header(self, title):
        print(f'\n{"="*60}')
        print(f'  {title}')
        print(f'{"="*60}')

    def print_metric(self, name, value, unit='', threshold=None, good_if_above=True):
        if threshold is not None:
            if good_if_above:
                status = '✅' if value >= threshold else '⚠️'
            else:
                status = '✅' if value <= threshold else '⚠️'
        else:
            status = '📊'

        print(f'{status} {name}: {value}{unit}')

    def run_check(self):
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        print(f'\n⏰ Check at {timestamp}')

        # Health check
        self.print_header('🏥 Health Status')
        health = self.check_health()

        if health.get('healthy'):
            self.print_metric('Gateway Status', health['status'])
            self.print_metric('Database', health['database'])
            self.print_metric('Response Time', f"{health['response_time']*1000:.2f}", ' ms',
                            self.thresholds['avg_response_time_ms'], False)

            # Additional checks
            if 'checks' in health:
                for check_name, check_info in health['checks'].items():
                    status = check_info.get('status', 'Unknown')
                    emoji = '✅' if status == 'OK' else '⚠️'
                    print(f'{emoji} {check_name}: {status}')
        else:
            print(f'❌ Gateway Unhealthy: {health.get("error", "Unknown error")}')
            return False

        # API Key Statistics
        keys = self.check_api_keys()
        if keys:
            self.print_header('🔑 API Key Statistics')
            self.print_metric('Total Keys', keys['total'])
            self.print_metric('Active', keys['active'])
            self.print_metric('Disabled', keys['disabled'])
            self.print_metric('Expired', keys['expired'])

        # Performance Metrics
        perf = self.check_performance()
        if perf:
            self.print_header('⚡ Performance Metrics')
            self.print_metric('Avg Response Time', f"{perf['avg_response_time']:.2f}", ' ms',
                            self.thresholds['avg_response_time_ms'], False)
            self.print_metric('Throughput', f"{perf['throughput']:.0f}", ' req/sec')
            self.print_metric('Cache Hit Rate', f"{perf['cache_hit_rate']:.1f}", '%')
            self.print_metric('Success Rate', f"{perf['success_rate']:.2f}", '%',
                            100 - self.thresholds['error_rate_percent'], True)

        # Response time analysis
        if self.response_times:
            self.print_header('⏲️  Response Time Analysis')
            avg = statistics.mean(self.response_times)
            print(f'📊 Last {len(self.response_times)} checks:')
            self.print_metric('  Min', f"{min(self.response_times):.2f}", ' ms')
            self.print_metric('  Max', f"{max(self.response_times):.2f}", ' ms')
            self.print_metric('  Avg', f"{avg:.2f}", ' ms',
                            self.thresholds['avg_response_time_ms'], False)
            if len(self.response_times) > 1:
                self.print_metric('  StdDev', f"{statistics.stdev(self.response_times):.2f}", ' ms')

        return True

    def run_continuous(self, duration_seconds=None):
        print(f'🚀 Starting continuous monitoring')
        print(f'   Gateway: {self.gateway_url}')
        print(f'   Check interval: {self.check_interval} seconds')
        if duration_seconds:
            print(f'   Duration: {duration_seconds} seconds')
        print(f'   Press Ctrl+C to stop\n')

        start_time = time.time()
        check_count = 0

        try:
            while True:
                if duration_seconds and (time.time() - start_time) > duration_seconds:
                    break

                check_count += 1
                success = self.run_check()

                if success:
                    print(f'\n✅ Check #{check_count} completed')
                else:
                    print(f'\n⚠️  Check #{check_count} failed')

                if duration_seconds and (time.time() - start_time) < duration_seconds:
                    time.sleep(self.check_interval)

        except KeyboardInterrupt:
            print('\n\n⏹️  Monitoring stopped by user')
        except Exception as e:
            print(f'\n❌ Error: {e}')

        self.print_summary(check_count)

    def print_summary(self, total_checks):
        self.print_header('📈 Monitoring Summary')
        print(f'Total checks performed: {total_checks}')
        print(f'Samples collected: {len(self.response_times)}')

        if self.response_times:
            avg_response = statistics.mean(self.response_times)
            max_response = max(self.response_times)
            min_response = min(self.response_times)

            print(f'\nResponse Time Stats:')
            print(f'  Average: {avg_response:.2f} ms')
            print(f'  Min: {min_response:.2f} ms')
            print(f'  Max: {max_response:.2f} ms')

            if min_response < 50 and max_response < 100 and avg_response < 75:
                print(f'\n✨ Excellent performance!')
            elif min_response < 100 and max_response < 200 and avg_response < 150:
                print(f'\n👍 Good performance')
            else:
                print(f'\n⚠️  Consider investigating performance')

def main():
    parser = argparse.ArgumentParser(description='Monitor API Key Gateway')
    parser.add_argument('--gateway-url', default='http://localhost:5000',
                        help='Gateway URL (default: http://localhost:5000)')
    parser.add_argument('--api-key', default='admin_key_example',
                        help='API key for authentication')
    parser.add_argument('--interval', type=int, default=60,
                        help='Check interval in seconds (default: 60)')
    parser.add_argument('--duration', type=int, default=None,
                        help='Total monitoring duration in seconds (optional)')

    args = parser.parse_args()

    try:
        monitor = GatewayMonitor(
            gateway_url=args.gateway_url,
            api_key=args.api_key,
            check_interval=args.interval
        )
        monitor.run_continuous(duration_seconds=args.duration)

    except KeyboardInterrupt:
        print('\n\n⚠️  Monitoring interrupted')
        sys.exit(0)
    except Exception as e:
        print(f'\n❌ Error: {e}')
        sys.exit(1)

if __name__ == '__main__':
    main()

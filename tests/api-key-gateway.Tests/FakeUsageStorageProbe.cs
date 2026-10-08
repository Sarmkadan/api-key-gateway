// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Services;

namespace ApiKeyGateway.Tests;

/// <summary>
/// In-memory stand-in for the usage store. Records how often it was pinged and
/// optionally runs a callback on each ping to simulate an outage or a hang.
/// </summary>
internal sealed class FakeUsageStorageProbe : IUsageStorageProbe
{
    private readonly Func<CancellationToken, Task>? _onPing;

    public FakeUsageStorageProbe(Func<CancellationToken, Task>? onPing = null)
    {
        _onPing = onPing;
    }

    public string ProviderName => "SqlServer";

    public int PingCount { get; private set; }

    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        PingCount++;

        if (_onPing != null)
        {
            await _onPing(cancellationToken);
        }
    }
}

using LightlessSyncShared.Metrics;
using LightlessSyncShared.Services;
using LightlessSyncShared.Utils.Configuration;
using Microsoft.AspNetCore.SignalR;
using System.Threading.RateLimiting;

namespace LightlessSyncServer.Hubs;

public sealed class ConcurrencyFilter : IHubFilter, IDisposable
{
    private ConcurrencyLimiter _limiter;
    private int _setLimit = 0;
    private readonly IConfigurationService<ServerConfiguration> _config;
    private readonly CancellationTokenSource _cts = new();

    private bool _disposed;

    public ConcurrencyFilter(IConfigurationService<ServerConfiguration> config, LightlessMetrics lightlessMetrics)
    {
        _config = config;
        _config.ConfigChangedEvent += OnConfigChange;

        RecreateLimiter();

        _ = Task.Run(async () =>
        {
            var token = _cts.Token;
            while (!token.IsCancellationRequested)
            {
                var stats = _limiter?.GetStatistics();
                if (stats != null)
                {
                    lightlessMetrics.SetGaugeTo(MetricsAPI.GaugeHubConcurrency, stats.CurrentAvailablePermits);
                    lightlessMetrics.SetGaugeTo(MetricsAPI.GaugeHubQueuedConcurrency, stats.CurrentQueuedCount);
                }
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        });
    }

    private void OnConfigChange(object sender, EventArgs e)
    {
        RecreateLimiter();
    }

    private void RecreateLimiter()
    {
        var newLimit = _config.GetValueOrDefault(nameof(ServerConfiguration.HubExecutionConcurrencyFilter), 50);

        if (newLimit == _setLimit && _limiter is not null)
        {
            return;
        }

        _setLimit = newLimit;
        _limiter?.Dispose();
        _limiter = new(new ConcurrencyLimiterOptions()
        {
            PermitLimit = newLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = newLimit * 100,
        });
    }

    public async ValueTask<object> InvokeMethodAsync(
    HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
    {
        if (string.Equals(invocationContext.HubMethodName, nameof(LightlessHub.CheckClientHealth), StringComparison.Ordinal))
        {
            return await next(invocationContext).ConfigureAwait(false);
        }

        var ct = invocationContext.Context.ConnectionAborted;
        RateLimitLease lease;
        try
        {
            lease = await _limiter.AcquireAsync(1, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }

        if (!lease.IsAcquired)
        {
            throw new HubException("Concurrency limit exceeded. Try again later.");
        }

        using (lease)
        {
            return await next(invocationContext).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cts.Cancel();
        _limiter?.Dispose();
        _config.ConfigChangedEvent -= OnConfigChange;
        _cts.Dispose();
    }
}

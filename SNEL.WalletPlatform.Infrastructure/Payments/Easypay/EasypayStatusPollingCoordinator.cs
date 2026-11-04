using System.Collections.Concurrent;
using System.Threading.Channels;
using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class EasypayStatusPollingCoordinator : IEasypayStatusPollingScheduler
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(
        new UnboundedChannelOptions { SingleReader = false, SingleWriter = false });

    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _cancellationSources = new();

    public ChannelReader<Guid> Reader => _channel.Reader;

    public void Schedule(Guid paymentId)
    {
        if (paymentId == Guid.Empty)
        {
            return;
        }

        if (_cancellationSources.TryRemove(paymentId, out var existing))
        {
            existing.Cancel();
            existing.Dispose();
        }

        _cancellationSources[paymentId] = new CancellationTokenSource();
        _channel.Writer.TryWrite(paymentId);
    }

    public void Cancel(Guid paymentId)
    {
        if (paymentId == Guid.Empty)
        {
            return;
        }

        if (_cancellationSources.TryRemove(paymentId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    public CancellationToken GetPollingCancellationToken(Guid paymentId, CancellationToken stoppingToken)
    {
        if (!_cancellationSources.TryGetValue(paymentId, out var cts))
        {
            return stoppingToken;
        }

        return CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, cts.Token).Token;
    }

    internal void Complete(Guid paymentId)
    {
        if (_cancellationSources.TryRemove(paymentId, out var cts))
        {
            cts.Dispose();
        }
    }
}

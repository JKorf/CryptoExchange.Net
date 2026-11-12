using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions.Features;

internal sealed class QueuedSubscriptionFeature<TUpdate> : ITradeSessionFeature
{
    private readonly object _writeLock = new();
    private readonly TradeSession _session;
    private readonly Func<Action<TUpdate>, CancellationToken, Task<WebSocketResult<UpdateSubscription>>> _starter;
    private readonly Func<TUpdate, Task> _handler;
    private readonly int? _maxQueuedItems;
    private readonly QueueFullBehavior? _fullBehavior;
    private ProcessQueue<TUpdate>? _queue;
    private UpdateSubscription? _subscription;
    private bool _accepting;

    internal QueuedSubscriptionFeature(
        TradeSession session,
        Func<Action<TUpdate>, CancellationToken, Task<WebSocketResult<UpdateSubscription>>> starter,
        Func<TUpdate, Task> handler,
        int? maxQueuedItems,
        QueueFullBehavior? fullBehavior)
    {
        _session = session;
        _starter = starter;
        _handler = handler;
        _maxQueuedItems = maxQueuedItems;
        _fullBehavior = fullBehavior;
    }

    public async Task<CallResult> StartAsync(CancellationToken ct)
    {
        var queue = new ProcessQueue<TUpdate>(
            update => _session.DispatchAsync(() => _handler(update)), _maxQueuedItems, _fullBehavior);
        _queue = queue;
        queue.Exception += _session.ReportException;
        await queue.StartAsync().ConfigureAwait(false);

        lock (_writeLock)
            _accepting = true;

        var result = await _starter(update => Enqueue(queue, update), ct).ConfigureAwait(false);
        if (!result.Success)
        {
            await StopAsync().ConfigureAwait(false);
            return CallResult.Fail(result.Error);
        }

        _subscription = result.Data;
        return CallResult.Ok();
    }

    private void Enqueue(ProcessQueue<TUpdate> queue, TUpdate update)
    {
        _session.Dispatch(() =>
        {
            lock (_writeLock)
            {
                if (_accepting)
                    queue.Write(update);
            }
        });
    }

    public async Task StopAsync()
    {
        lock (_writeLock)
            _accepting = false;

        var errors = new List<Exception>();
        if (_subscription != null)
        {
            try
            {
                await _subscription.CloseAsync().ConfigureAwait(false);
                _subscription = null;
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        }

        if (_queue != null)
        {
            try
            {
                await _queue.StopAsync(discardPending: true).ConfigureAwait(false);
                _queue.Exception -= _session.ReportException;
                _queue = null;
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        }

        if (errors.Count != 0)
            throw new AggregateException("Queued subscription cleanup failed.", errors);
    }

    public ValueTask DisposeAsync() => new ValueTask(StopAsync());
}

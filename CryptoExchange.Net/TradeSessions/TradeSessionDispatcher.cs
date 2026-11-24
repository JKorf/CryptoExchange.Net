using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions;

/// <summary>
/// Dispatcher from the host to the actual session
/// </summary>
internal sealed class TradeSessionDispatcher
{
    private readonly ILogger _logger;
    private readonly object _sync = new();
    private readonly AsyncLocal<bool> _executing = new();
    private bool _enabled;
    private int _activeCallbacks;
    private TaskCompletionSource<bool>? _drained;
    private readonly Queue<Func<Task>> _pending = new();

    internal bool ExternallyDriven { get; }

    internal bool IsExecuting => _executing.Value;

    internal TradeSessionDispatcher(ILogger logger, bool externallyDriven = false)
    {
        _logger = logger;
        ExternallyDriven = externallyDriven;
    }

    internal void ReportException(Exception error)
        => _logger.LogError(error, "Asynchronous symbol update handler failed");

    internal void Enable()
    {
        lock (_sync)
            _enabled = true;
    }

    internal Task SuspendAsync()
    {
        lock (_sync)
        {
            _enabled = false;
            _pending.Clear();
            return _activeCallbacks == 0 ? Task.CompletedTask : _drained!.Task;
        }
    }

    internal void Dispatch(Action callback)
    {
        if (ExternallyDriven)
        {
            Enqueue(() => { callback(); return Task.CompletedTask; });
            return;
        }

        if (!TryEnter(false))
            return;

        var previous = _executing.Value;
        _executing.Value = true;
        try
        {
            callback();
        }
        finally
        {
            _executing.Value = previous;
            Exit();
        }
    }

    internal Task DispatchAsync(Func<Task> callback)
    {
        if (!ExternallyDriven)
            return InvokeAsync(callback, false);

        Enqueue(callback);
        return Task.CompletedTask;
    }

    private void Enqueue(Func<Task> callback)
    {
        lock (_sync)
        {
            if (_enabled)
                _pending.Enqueue(callback);
        }
    }

    internal async Task ProcessPendingAsync(CancellationToken ct)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            Func<Task> callback;
            lock (_sync)
            {
                if (_pending.Count == 0)
                    return;
                callback = _pending.Dequeue();
            }

            await InvokeAsync(callback, false).ConfigureAwait(false);
        }
    }

    internal Task InvokeLifecycleAsync(Func<Task> callback) => InvokeAsync(callback, true);

    private async Task InvokeAsync(Func<Task> callback, bool lifecycle)
    {
        if (!TryEnter(lifecycle))
            return;

        var previous = _executing.Value;
        _executing.Value = true;
        try
        {
            await callback().ConfigureAwait(false);
        }
        finally
        {
            _executing.Value = previous;
            Exit();
        }
    }

    private bool TryEnter(bool lifecycle)
    {
        lock (_sync)
        {
            if (!_enabled && !lifecycle)
                return false;

            if (_activeCallbacks++ == 0)
                _drained = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            return true;
        }
    }

    private void Exit()
    {
        lock (_sync)
        {
            if (--_activeCallbacks == 0)
                _drained!.TrySetResult(true);
        }
    }
}

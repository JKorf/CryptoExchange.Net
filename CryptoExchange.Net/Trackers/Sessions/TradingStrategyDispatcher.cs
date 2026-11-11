using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sessions;

internal sealed class TradingStrategyDispatcher
{
    private readonly object _sync = new();
    private readonly AsyncLocal<bool> _executing = new();
    private bool _enabled;
    private int _activeCallbacks;
    private TaskCompletionSource<bool>? _drained;

    internal bool IsExecuting => _executing.Value;

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
            return _activeCallbacks == 0 ? Task.CompletedTask : _drained!.Task;
        }
    }

    internal void Dispatch(Action callback)
    {
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

    internal Task DispatchAsync(Func<Task> callback) => InvokeAsync(callback, false);

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

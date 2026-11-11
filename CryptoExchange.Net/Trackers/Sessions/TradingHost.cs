using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions.Features;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sessions;

/// <summary>
/// Trading host
/// </summary>
public sealed class TradingHost<TStrategy> : ITradingHost<TStrategy>
    where TStrategy : TradingStrategy
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly object _featuresLock = new();
    private readonly List<ITradingSessionFeature> _activeFeatures = new();
    private readonly TradingStrategyDispatcher _dispatcher = new();
    private readonly Func<ITradingSessionFeature>[] _factories;
    private readonly ILogger _logger;
    private CancellationTokenSource? _runCts;
    private CancellationTokenSource? _featuresCts;
    private CancellationTokenRegistration _lifetimeRegistration;
    private Task? _updateLoop;
    private volatile bool _started;
    private bool _strategyStarted;
    private bool _attached;
    private bool _disposed;

    public TStrategy Strategy { get; }
    /// <inheritdoc />
    public string Exchange { get; }
    /// <inheritdoc />
    public bool Started => _started;
    /// <inheritdoc />
    public TimeSpan UpdateLoopInterval { get; }

    internal TradingHost(
        TStrategy strategy,
        string exchange,
        ILogger logger,
        Func<ITradingSessionFeature>[] factories,
        TimeSpan updateLoopInterval)
    {
        Strategy = strategy;
        Exchange = exchange;
        _logger = logger;
        _factories = factories;
        UpdateLoopInterval = updateLoopInterval;
    }

    internal void Attach(TradingContext context)
    {
        Strategy.Attach(context, _dispatcher);
        _attached = true;
    }

    /// <inheritdoc />
    public async Task<CallResult> StartAsync(CancellationToken ct = default)
    {
        EnsureExternalLifecycle();
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TradingHost<TStrategy>));
            if (!_attached)
                throw new InvalidOperationException("Build the host before starting it.");
            if (_started)
                return CallResult.Ok();
            if (_activeFeatures.Count != 0)
                throw new InvalidOperationException("Retry StopAsync before restarting.");

            return await StartCoreAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<CallResult> StartCoreAsync(CancellationToken ct)
    {
        _runCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var runCts = _runCts;
        var token = runCts.Token;
        _featuresCts = new CancellationTokenSource();
        var featuresCts = _featuresCts;
        using var startupRegistration = ct.Register(() => featuresCts.Cancel());
        try
        {
            foreach (var create in _factories)
            {
                token.ThrowIfCancellationRequested();
                var feature = create()
                    ?? throw new InvalidOperationException("Feature factory returned null.");
                lock (_featuresLock)
                    _activeFeatures.Add(feature);

                var result = await feature.StartAsync(featuresCts.Token).ConfigureAwait(false);
                if (!result.Success)
                {
                    startupRegistration.Dispose();
                    return await FailStartupAsync(result).ConfigureAwait(false);
                }
            }

            _strategyStarted = true;
            await _dispatcher.InvokeLifecycleAsync(
                () => Strategy.InvokeStartedAsync(token)).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            startupRegistration.Dispose();

            _started = true;
            _dispatcher.Enable();
            if (UpdateLoopInterval > TimeSpan.Zero)
                _updateLoop = UpdateLoopAsync(token);

            _lifetimeRegistration = ct.Register(() => { _ = StopCanceledRunAsync(runCts); });
            return CallResult.Ok();
        }
        catch (OperationCanceledException)
        {
            startupRegistration.Dispose();
            return await FailStartupAsync(
                CallResult.Fail(new CancellationRequestedError())).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            startupRegistration.Dispose();
            return await FailStartupAsync(
                CallResult.Fail(new UnknownError("Strategy startup failed.", error))).ConfigureAwait(false);
        }
    }

    private async Task<CallResult> FailStartupAsync(CallResult failure)
    {
        try
        {
            await ShutdownCoreAsync(false).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            return CallResult.Fail(new UnknownError(
                $"Startup failed ({failure.Error}); cleanup also failed.", error));
        }
        return failure;
    }

    private async Task UpdateLoopAsync(CancellationToken ct)
    {
        await Task.Yield();
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _dispatcher.DispatchAsync(
                    () => Strategy.InvokeUpdateAsync(ct)).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Trading strategy update failed");
            }

            try
            {
                await Task.Delay(UpdateLoopInterval, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
        }
    }

    /// <inheritdoc/>
    public Task StopAsync() => ShutdownAsync(false);

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => new ValueTask(ShutdownAsync(true));

    private async Task ShutdownAsync(bool disposing)
    {
        EnsureExternalLifecycle();
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await ShutdownCoreAsync(disposing).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task ShutdownCoreAsync(bool disposing)
    {
        if (disposing)
            _disposed = true;

        _started = false;
        _lifetimeRegistration.Dispose();
        _lifetimeRegistration = default;
        var errors = new List<Exception>();

        var draining = _dispatcher.SuspendAsync();
        try
        {
            _runCts?.Cancel();
        }
        catch (Exception error)
        {
            errors.Add(error);
        }

        if (_updateLoop != null)
        {
            try
            {
                await _updateLoop.ConfigureAwait(false);
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        }
        await draining.ConfigureAwait(false);

        if (_strategyStarted)
        {
            _strategyStarted = false;
            try
            {
                await _dispatcher.InvokeLifecycleAsync(
                    () => Strategy.InvokeStoppingAsync(CancellationToken.None)).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        }

        try
        {
            _featuresCts?.Cancel();
        }
        catch (Exception error)
        {
            errors.Add(error);
        }

        errors.AddRange(await CloseFeaturesAsync().ConfigureAwait(false));
        _updateLoop = null;
        _runCts?.Dispose();
        _runCts = null;
        _featuresCts?.Dispose();
        _featuresCts = null;

        if (errors.Count != 0)
            throw new AggregateException("Trading host cleanup failed.", errors);
    }

    private async Task StopCanceledRunAsync(CancellationTokenSource runCts)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (ReferenceEquals(_runCts, runCts))
                await ShutdownCoreAsync(false).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Stopping a canceled trading host failed");
        }
        finally
        {
            _gate.Release();
        }
    }

    private void EnsureExternalLifecycle()
    {
        if (_dispatcher.IsExecuting)
        {
            throw new InvalidOperationException(
                "Start and stop the host outside strategy callbacks.");
        }
    }

    private async Task<List<Exception>> CloseFeaturesAsync()
    {
        var errors = new List<Exception>();
        for (var i = _activeFeatures.Count - 1; i >= 0; i--)
        {
            try
            {
                await _activeFeatures[i].DisposeAsync().ConfigureAwait(false);
                lock (_featuresLock)
                    _activeFeatures.RemoveAt(i);
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        }
        return errors;
    }

    private TFeature[] GetFeatures<TFeature>() where TFeature : class, ITradingSessionFeature
    {
        lock (_featuresLock)
            return _activeFeatures.OfType<TFeature>().ToArray();
    }

    internal IReadOnlyCollection<ISymbolOrderBook> GetOrderBooks()
        => GetFeatures<OrderBookFeature>().Select(feature => feature.OrderBook).ToArray();

    internal ISymbolOrderBook GetOrderBook(SharedSymbol symbol)
        => GetFeatures<OrderBookFeature>().SingleOrDefault(feature => feature.Symbol == symbol)?.OrderBook
            ?? throw new InvalidOperationException("No active order book exists for the requested symbol.");

    /// <summary>
    /// Retrieve the spot user data tracker for a user
    /// </summary>
    internal IUserSpotDataTracker GetSpotUserDataTracker(string userIdentifier)
        => GetFeatures<SpotUserDataFeature>()
            .SingleOrDefault(feature => feature.UserIdentifier == userIdentifier)?.Tracker
            ?? throw new InvalidOperationException("No active spot tracker exists for this user.");

    /// <summary>
    /// Retrieve the futures user data tracker for a user
    /// </summary>
    internal IUserFuturesDataTracker GetFuturesUserDataTracker(string userIdentifier, TradingMode mode)
        => GetFeatures<FuturesUserDataFeature>()
            .SingleOrDefault(feature => feature.UserIdentifier == userIdentifier && feature.TradingMode == mode)
            ?.Tracker
            ?? throw new InvalidOperationException("No active futures tracker exists for this user and trading mode.");
}

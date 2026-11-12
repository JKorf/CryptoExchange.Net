using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using CryptoExchange.Net.TradeSessions.Features;
using CryptoExchange.Net.TradeSessions.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions;

/// <summary>
/// Trade session host
/// </summary>
public sealed class TradeSessionHost<TTradeSession> : ITradeSessionHost<TTradeSession>
    where TTradeSession : TradeSession
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly object _featuresLock = new();
    private readonly List<ITradeSessionFeature> _activeFeatures = new();
    private readonly Dictionary<SharedSymbol, ISymbolOrderBook> _orderBooks = new();
    private readonly TradeSessionDispatcher _dispatcher;
    private readonly Func<ITradeSessionFeature>[] _factories;
    private readonly ILogger _logger;
    private CancellationTokenSource? _runCts;
    private CancellationTokenSource? _featuresCts;
    private CancellationTokenRegistration _lifetimeRegistration;
    private Task? _updateLoop;
    private volatile bool _started;
    private bool _sessionStarted;
    private bool _stoppedCallbackPending;
    private bool _attached;
    private bool _disposed;
    private string SessionName { get; } = typeof(TTradeSession).Name;

    /// <summary>
    /// The trade session instance
    /// </summary>
    public TTradeSession TradeSession { get; }
    /// <inheritdoc />
    public string Exchange { get; }
    /// <inheritdoc />
    public bool Started => _started;
    /// <inheritdoc />
    public TimeSpan UpdateLoopInterval { get; }

    internal TradeSessionHost(
        TTradeSession session,
        string exchange,
        ILogger logger,
        Func<ITradeSessionFeature>[] factories,
        TimeSpan updateLoopInterval)
    {
        TradeSession = session;
        Exchange = exchange;
        _logger = logger;
        _dispatcher = new TradeSessionDispatcher(logger);
        _factories = factories;
        UpdateLoopInterval = updateLoopInterval;
    }

    internal void Attach(TradeSessionContext context)
    {
        TradeSession.Attach(context, _dispatcher, _logger);
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
                throw new ObjectDisposedException(nameof(TradeSessionHost<TTradeSession>));
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
        _logger.LogInformation("Starting TradeSessionHost for {Session}", SessionName);
        _runCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var runCts = _runCts;
        var token = runCts.Token;
        _featuresCts = new CancellationTokenSource();
        var featuresCts = _featuresCts;
        using var startupRegistration = ct.Register(() => featuresCts.Cancel());
        try
        {
            token.ThrowIfCancellationRequested();
            _stoppedCallbackPending = true;
            await _dispatcher.InvokeLifecycleAsync(
                () => TradeSession.InvokeStartingAsync(token)).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            foreach (var create in _factories)
            {
                token.ThrowIfCancellationRequested();
                var feature = create()
                    ?? throw new InvalidOperationException("Feature factory returned null.");
                lock (_featuresLock)
                {
                    _activeFeatures.Add(feature);
                    if (feature is OrderBookFeature bookFeature)
                    {
                        if (_orderBooks.ContainsKey(bookFeature.Symbol))
                        {
                            throw new InvalidOperationException(
                                $"Order book tracking is already active for {bookFeature.Symbol}.");
                        }
                        _orderBooks.Add(bookFeature.Symbol, bookFeature.OrderBook);
                    }
                }

                var result = await feature.StartAsync(featuresCts.Token).ConfigureAwait(false);
                if (!result.Success)
                {
                    startupRegistration.Dispose();
                    return await FailStartupAsync(result).ConfigureAwait(false);
                }
            }

            _sessionStarted = true;
            await _dispatcher.InvokeLifecycleAsync(
                () => TradeSession.InvokeStartedAsync(token)).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            startupRegistration.Dispose();

            _started = true;
            _dispatcher.Enable();
            if (UpdateLoopInterval > TimeSpan.Zero)
                _updateLoop = UpdateLoopAsync(token);

            _lifetimeRegistration = ct.Register(() => { _ = StopCanceledRunAsync(runCts); });
            _logger.LogDebug("Started TradeSessionHost for {Session}", SessionName);
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
                CallResult.Fail(new UnknownError("Session startup failed.", error))).ConfigureAwait(false);
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
        finally
        {
            _logger.LogError("Failed to start TradeSessionHost for {Session}: {Error}", SessionName, failure.Error);
        }
        return failure;
    }

    private async Task UpdateLoopAsync(CancellationToken ct)
    {
        await Task.Yield();
        _logger.LogDebug("Starting trade session update loop for {Session} at {Interval} interval", SessionName, UpdateLoopInterval);
        while (!ct.IsCancellationRequested)
        {
            _logger.LogTrace("Triggering trade session update for {Session}", SessionName);
            try
            {
                await _dispatcher.DispatchAsync(
                    () => TradeSession.InvokeUpdateAsync(ct)).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Trade session update failed for {Session}", SessionName);
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
        _logger.LogInformation("Stopping TradeSessionHost for {Session}", SessionName);
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

        if (_sessionStarted)
        {
            _sessionStarted = false;
            try
            {
                await _dispatcher.InvokeLifecycleAsync(
                    () => TradeSession.InvokeStoppingAsync(CancellationToken.None)).ConfigureAwait(false);
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

        if (_stoppedCallbackPending && _activeFeatures.Count == 0)
        {
            _stoppedCallbackPending = false;
            try
            {
                await _dispatcher.InvokeLifecycleAsync(
                    () => TradeSession.InvokeStoppedAsync(CancellationToken.None)).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        }

        if (errors.Count != 0)
        {
            var ex = new AggregateException("Trade session host cleanup failed.", errors);
            _logger.LogError(ex, "Stopping TradeSessionHost for {Session} completed with errors", SessionName);
            throw ex;
        }

        _logger.LogDebug("Stopped TradeSessionHost for {Session}", SessionName);
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
            _logger.LogError(error, "Stopping a canceled trade session host failed");
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
                "Start and stop the host outside session callbacks.");
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
                {
                    if (_activeFeatures[i] is OrderBookFeature bookFeature
                        && _orderBooks.TryGetValue(bookFeature.Symbol, out var book)
                        && ReferenceEquals(book, bookFeature.OrderBook))
                    {
                        _orderBooks.Remove(bookFeature.Symbol);
                    }
                    _activeFeatures.RemoveAt(i);
                }
            }
            catch (Exception error)
            {
                errors.Add(error);
            }
        }
        return errors;
    }

    private TFeature[] GetFeatures<TFeature>() where TFeature : class, ITradeSessionFeature
    {
        lock (_featuresLock)
            return _activeFeatures.OfType<TFeature>().ToArray();
    }

    internal IReadOnlyDictionary<SharedSymbol, ISymbolOrderBook> GetOrderBooks()
    {
        lock (_featuresLock)
            return _orderBooks.ToDictionary(pair => pair.Key with { }, pair => pair.Value);
    }

    internal ISymbolOrderBook? GetOrderBook(SharedSymbol symbol)
    {
        lock (_featuresLock)
            return _orderBooks.TryGetValue(symbol, out var book) ? book : null;
    }

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

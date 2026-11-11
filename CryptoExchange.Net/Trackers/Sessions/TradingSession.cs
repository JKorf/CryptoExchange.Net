using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions;
using CryptoExchange.Net.Trackers.Sessions.Features;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using CryptoExchange.Net.Trackers.UserData.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Sessions;

/// <summary>
/// Trading session
/// </summary>
public abstract class TradingSession : ITradingSession
{
    private Task? _updateLoopProcess;
    private CancellationTokenSource? _cts;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly object _featuresLock = new();
    private readonly List<ITradingSessionFeature> _activeFeatures = new();
    private Func<ITradingSessionFeature>[]? _factories;
    private ILogger? _logger;
    private bool _disposed;

    /// <inheritdoc />
    public bool Started { get; private set; }
    /// <inheritdoc />
    public string Exchange { get; private set; } = "";
    /// <inheritdoc />
    public TimeSpan UpdateLoopInterval { get; private set; }

#warning TODO order book access
    protected IReadOnlyCollection<ISymbolOrderBook> OrderBooks
        => GetActiveFeatures<OrderBookFeature>().Select(feature => feature.OrderBook).ToArray();

    protected ISymbolOrderBook GetOrderBook(SharedSymbol symbol)
    {
        if (symbol == null)
            throw new ArgumentNullException(nameof(symbol));

        return GetActiveFeatures<OrderBookFeature>()
            .SingleOrDefault(feature => feature.Symbol == symbol)?.OrderBook
            ?? throw new InvalidOperationException("No active order book exists for the requested symbol.");
    }

    /// <summary>
    /// Retrieve the spot user data tracker for a user
    /// </summary>
    protected IUserSpotDataTracker GetSpotUserDataTracker(string userIdentifier)
    {
        return GetActiveFeatures<SpotUserDataFeature>()
            .SingleOrDefault(feature =>
                feature.UserIdentifier == userIdentifier)
            ?.Tracker
            ?? throw new InvalidOperationException(
                "No active spot tracker exists for this user.");
    }

    /// <summary>
    /// Retrieve the futures user data tracker for a user
    /// </summary>
    protected IUserFuturesDataTracker GetFuturesUserDataTracker(string userIdentifier, TradingMode tradingMode)
    {
        return GetActiveFeatures<FuturesUserDataFeature>()
            .SingleOrDefault(feature =>
                feature.UserIdentifier == userIdentifier &&
                feature.TradingMode == tradingMode)
            ?.Tracker
            ?? throw new InvalidOperationException(
                "No active futures tracker exists for this user and trading mode.");
    }

    private TFeature[] GetActiveFeatures<TFeature>() where TFeature : class, ITradingSessionFeature
    {
        lock (_featuresLock)
            return _activeFeatures.OfType<TFeature>().ToArray();
    }

    internal void Configure(
        string exchange,
        ILogger logger,
        Func<ITradingSessionFeature>[] factories,
        TimeSpan updateLoopInterval)
    {
        if (_factories != null || _disposed)
            throw new InvalidOperationException("Session has already been configured or disposed.");

        _logger = logger;
        _factories = factories;
        UpdateLoopInterval = updateLoopInterval;
        Exchange = exchange;
    }

    internal virtual void ConfigureClientProvider(ISharedUserClientProvider provider)
    { }
    internal virtual void ConfigureUsers(IReadOnlyCollection<TradingSessionUserDefinition> users)
    { }

    /// <inheritdoc />
    public async Task<CallResult> StartAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            if (_factories == null)
                throw new InvalidOperationException("Build the session before starting it.");
            if (Started)
                return CallResult.Ok();

            if (_activeFeatures.Count != 0)
                throw new InvalidOperationException("Retry StopAsync before restarting.");

            foreach (var create in _factories)
            {
                ct.ThrowIfCancellationRequested();
                var feature = create()
                    ?? throw new InvalidOperationException("Feature factory returned null.");
                lock (_featuresLock)
                    _activeFeatures.Add(feature);

                var result = await feature.StartAsync(ct).ConfigureAwait(false);
                if (!result.Success)
                {
                    var errors = await CloseActiveFeaturesAsync().ConfigureAwait(false);
                    return result;
                }
            }

            ct.ThrowIfCancellationRequested();

            Started = true;
            if (UpdateLoopInterval > TimeSpan.Zero)
            {
                _cts = new CancellationTokenSource();
                _updateLoopProcess = UpdateLoopAsync(_cts.Token);
            }

            return CallResult.Ok();
        }
        catch (Exception ex)
        {
            var errors = await CloseActiveFeaturesAsync().ConfigureAwait(false);
            if (ex is OperationCanceledException)
                return CallResult.Fail(new CancellationRequestedError());

            return CallResult.Fail(new UnknownError("Startup failed", ex));
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task UpdateLoopAsync(CancellationToken ct)
    {
        _logger?.LogDebug("Starting session update loop with {Interval} interval", UpdateLoopInterval);
        await Task.Yield();
        while (!ct.IsCancellationRequested)
        {
#warning extend with exact interval calculation to avoid drift
            try
            {
                try
                {
                    await UpdateAsync(ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Session update loop error");
                }

                if (ct.IsCancellationRequested)
                    break;

                await Task.Delay(UpdateLoopInterval, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    /// <summary>
    /// Update callback, only called when <see cref="UpdateLoopInterval"/> is set to a value greater than 0 by using <see cref="TradingSessionBuilder{TSession, TSelf}.AddUpdateLoop(TimeSpan)"/>.
    /// </summary>
    protected virtual Task UpdateAsync(CancellationToken ct) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task StopAsync() => ShutdownAsync(false);

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => new ValueTask(ShutdownAsync(true));

    private async Task ShutdownAsync(bool disposing)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (disposing)
                _disposed = true;

            Started = false;

            _cts?.Cancel();

            if (_updateLoopProcess != null)
                await _updateLoopProcess.ConfigureAwait(false);

            await CloseActiveFeaturesAsync().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<Exception>> CloseActiveFeaturesAsync()
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


}

/// <inheritdoc />
public abstract class TradingSession<TProvider> : TradingSession
    where TProvider : class, ISharedUserClientProvider
{
    private TProvider? _provider;

    /// <summary>
    /// The client instance provider
    /// </summary>
    protected TProvider ClientProvider => _provider
        ?? throw new InvalidOperationException(
            "Build the session before accessing its clients.");

    internal override void ConfigureClientProvider(
        ISharedUserClientProvider provider)
    {
        _provider = provider as TProvider
            ?? throw new ArgumentException(
                $"This session requires {typeof(TProvider).Name}.",
                nameof(provider));
    }
}

/// <inheritdoc />
public abstract class TradingSession<TProvider, TUser> : TradingSession<TProvider>
    where TProvider : class, ISharedUserClientProvider
    where TUser : TradingSessionUser
{
    private TradingSessionUsers<TUser>? _users;

    /// <summary>
    /// User access
    /// </summary>
    protected TradingSessionUsers<TUser> User => _users
        ?? throw new InvalidOperationException(
            "Build the session before accessing users.");

    internal override void ConfigureUsers(
        IReadOnlyCollection<TradingSessionUserDefinition> users)
    {
        _users = new TradingSessionUsers<TUser>(
            users.Select(CreateUser));
    }

    /// <summary>
    /// Create a session user
    /// </summary>
    protected abstract TUser CreateUser(TradingSessionUserDefinition definition);
}
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


public abstract class Session : IAsyncDisposable
{
    private Task? _updateLoopProcess;
    private CancellationTokenSource? _cts;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly object _featuresLock = new();
    private readonly List<ISessionFeature> _activeFeatures = new();
    private Func<ISessionFeature>[]? _factories;
    private ILogger _logger;
    public bool Started { get; private set; }
    private bool _disposed;

    public string Exchange { get; private set; } = "";

    public TimeSpan UpdateLoopInterval { get; set; }

    protected IReadOnlyCollection<ISymbolOrderBook> OrderBooks
        => GetActiveFeatures<OrderBookFeature>().Select(feature => feature.OrderBook).ToArray();

    protected IReadOnlyCollection<IUserSpotDataTracker> SpotUserDataTrackers =>
        GetActiveFeatures<SpotUserDataFeature>()
            .Select(feature => feature.Tracker)
            .Where(tracker => tracker != null)
            .Cast<IUserSpotDataTracker>()
            .ToArray();

    protected IReadOnlyCollection<IUserFuturesDataTracker> FuturesUserDataTrackers =>
        GetActiveFeatures<FuturesUserDataFeature>()
            .Select(feature => feature.Tracker)
            .Where(tracker => tracker != null)
            .Cast<IUserFuturesDataTracker>()
            .ToArray();

    protected ISymbolOrderBook GetOrderBook(SharedSymbol symbol)
    {
        if (symbol == null)
            throw new ArgumentNullException(nameof(symbol));

        return GetActiveFeatures<OrderBookFeature>()
            .SingleOrDefault(feature => feature.Symbol == symbol)?.OrderBook
            ?? throw new InvalidOperationException("No active order book exists for the requested symbol.");
    }

    protected IUserSpotDataTracker GetSpotUserDataTracker(
    string userIdentifier)
    {
        return GetActiveFeatures<SpotUserDataFeature>()
            .SingleOrDefault(feature =>
                feature.UserIdentifier == userIdentifier)
            ?.Tracker
            ?? throw new InvalidOperationException(
                "No active spot tracker exists for this user.");
    }

    protected IUserFuturesDataTracker GetFuturesUserDataTracker(
        string userIdentifier,
        TradingMode tradingMode)
    {
        return GetActiveFeatures<FuturesUserDataFeature>()
            .SingleOrDefault(feature =>
                feature.UserIdentifier == userIdentifier &&
                feature.TradingMode == tradingMode)
            ?.Tracker
            ?? throw new InvalidOperationException(
                "No active futures tracker exists for this user and trading mode.");
    }

    private TFeature[] GetActiveFeatures<TFeature>() where TFeature : class, ISessionFeature
    {
        lock (_featuresLock)
            return _activeFeatures.OfType<TFeature>().ToArray();
    }

    internal void Configure(
        string exchange,
        ILogger logger,
        Func<ISessionFeature>[] factories,
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
    internal virtual void ConfigureUsers(IReadOnlyCollection<SessionUserDefinition> users)
    { }

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
        _logger.LogDebug("Starting session update loop with {Interval} interval", UpdateLoopInterval);
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
                    _logger.LogError(ex, "Session update loop error");
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

    public virtual Task UpdateAsync(CancellationToken ct) => Task.CompletedTask;

    public Task StopAsync() => ShutdownAsync(false);

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

public abstract class Session<TProvider> : Session
    where TProvider : class, ISharedUserClientProvider
{
    private TProvider? _provider;

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

public abstract class Session<TProvider, TUser> : Session<TProvider>
    where TProvider : class, ISharedUserClientProvider
    where TUser : SessionUser
{
    private SessionUsers<TUser>? _users;

    protected SessionUsers<TUser> User => _users
        ?? throw new InvalidOperationException(
            "Build the session before accessing users.");

    internal override void ConfigureUsers(
        IReadOnlyCollection<SessionUserDefinition> users)
    {
        _users = new SessionUsers<TUser>(
            users.Select(CreateUser));
    }

    protected abstract TUser CreateUser(SessionUserDefinition definition);
}
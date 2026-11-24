using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.TradeSessions.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.TradeSessions;

/// <summary>
/// Trade session builder
/// </summary>
public class TradeSessionBuilder<TTradeSession, TSelf, TUser>
    where TTradeSession : TradeSession<TUser>
    where TSelf : TradeSessionBuilder<TTradeSession, TSelf, TUser>
    where TUser : class, ITradeSessionUser
{
    private readonly HashSet<string> _names = new();
    private readonly List<Func<ISharedUserClientProvider, Func<TTradeSession, ITradeSessionFeature>>> _features = new();
    private readonly Dictionary<string, TradeSessionUserDefinition> _users = new(StringComparer.Ordinal);
    private readonly ISharedUserClientProvider _clientProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Func<TradeSessionUserContext, TUser> _userFactory;
    private TimeSpan _updateLoopInterval;
    private Func<DateTime>? _externalClock;

    /// <summary>
    /// Use externally driven, serial callback processing and update scheduling with the supplied UTC clock.
    /// </summary>
    protected void UseExternalExecution(Func<DateTime> getUtcNow)
        => _externalClock = getUtcNow ?? throw new ArgumentNullException(nameof(getUtcNow));

    /// <summary>
    /// Register a user without configuring a data tracker.
    /// </summary>
    protected void RegisterUser(string userIdentifier, TradeEnvironment? environment = null)
    {
        if (string.IsNullOrWhiteSpace(userIdentifier))
            throw new ArgumentException("A user identifier is required.", nameof(userIdentifier));
        if (_users.ContainsKey(userIdentifier))
            throw new InvalidOperationException($"User '{userIdentifier}' is already configured.");
        _users.Add(userIdentifier, new TradeSessionUserDefinition(userIdentifier, environment));
    }

    /// <summary>
    /// ctor
    /// </summary>
    protected TradeSessionBuilder(
        ISharedUserClientProvider clientProvider,
        ILoggerFactory loggerFactory,
        Func<TradeSessionUserContext, TUser> userFactory)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _userFactory = userFactory ?? throw new ArgumentNullException(nameof(userFactory));
    }

    /// <summary>
    /// Add a symbol tracking configuration
    /// </summary>
    /// <param name="symbol">The symbol</param>
    /// <param name="configure">The symbol configurator</param>
    /// <param name="environment">Trading environment</param>
    protected TSelf AddSymbolTracking(
        SharedSymbol symbol,
        Action<TradeSessionSymbolBuilder<TTradeSession>> configure,
        TradeEnvironment? environment = null)
    {
        RegisterSymbol(new TradeSessionSymbolBuilder<TTradeSession>(symbol, environment), configure);
        return (TSelf)this;
    }

    /// <summary>
    /// Add the same symbol tracking configuration for each symbol
    /// </summary>
    /// <param name="symbols">The symbols</param>
    /// <param name="configure">The symbol configurator, called once for each symbol</param>
    /// <param name="environment">Trading environment</param>
    protected TSelf AddSymbolTracking(
        IEnumerable<SharedSymbol> symbols,
        Action<TradeSessionSymbolBuilder<TTradeSession>> configure,
        TradeEnvironment? environment = null)
    {
        RegisterSymbols(
            symbols,
            symbol => new TradeSessionSymbolBuilder<TTradeSession>(symbol, environment),
            configure);
        return (TSelf)this;
    }

    /// <summary>
    /// Add an update callback at a set interval
    /// </summary>
    /// <param name="updateInterval">The interval at which the
    /// <see cref="TradeSession.UpdateAsync(System.Threading.CancellationToken)"/> method should be called</param>
    public TSelf AddUpdateLoop(TimeSpan updateInterval)
    {
        if (updateInterval <= TimeSpan.Zero || updateInterval.TotalMilliseconds > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(updateInterval));

        _updateLoopInterval = updateInterval;
        return (TSelf)this;
    }

    /// <summary>
    /// Register a symbol config
    /// </summary>
    protected void RegisterSymbol<TBuilder>(TBuilder builder, Action<TBuilder> configure)
        where TBuilder : TradeSessionSymbolBuilder<TTradeSession, TBuilder>
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        configure(builder);
        _features.AddRange(builder.Features);
    }

    /// <summary>
    /// Register symbol configurations, committing them after every symbol has been configured
    /// </summary>
    /// <param name="symbols">The symbols</param>
    /// <param name="createBuilder">Creates a separate configuration builder for each symbol</param>
    /// <param name="configure">The symbol configurator, called once for each symbol</param>
    protected void RegisterSymbols<TBuilder>(
        IEnumerable<SharedSymbol> symbols,
        Func<SharedSymbol, TBuilder> createBuilder,
        Action<TBuilder> configure)
        where TBuilder : TradeSessionSymbolBuilder<TTradeSession, TBuilder>
    {
        if (symbols == null)
            throw new ArgumentNullException(nameof(symbols));
        if (createBuilder == null)
            throw new ArgumentNullException(nameof(createBuilder));
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var features = new List<Func<ISharedUserClientProvider, Func<TTradeSession, ITradeSessionFeature>>>();
        foreach (var symbol in symbols)
        {
            var builder = createBuilder(symbol);
            configure(builder);
            features.AddRange(builder.Features);
        }
        _features.AddRange(features);
    }

    /// <summary>
    /// Register a feature config
    /// </summary>
    protected void RegisterFeature(string name, Func<TTradeSession, ITradeSessionFeature> create)
    {
        if (create == null)
            throw new ArgumentNullException(nameof(create));
        if (!_names.Add(name))
            throw new InvalidOperationException($"Feature '{name}' was already configured.");

        _features.Add(_ => create);
    }

    /// <summary>
    /// Register a user config
    /// </summary>
    protected void RegisterUser<TEnvironment>(
        string userIdentifier,
        Action<TradeSessionUserBuilder<TTradeSession, TEnvironment>> configure,
        ITrackerFactory trackerFactory,
        TEnvironment? environment = null)
        where TEnvironment : TradeEnvironment
    {
        if (string.IsNullOrWhiteSpace(userIdentifier))
            throw new ArgumentException("A user identifier is required.", nameof(userIdentifier));
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));
        if (trackerFactory == null)
            throw new ArgumentNullException(nameof(trackerFactory));
        if (_users.ContainsKey(userIdentifier))
            throw new InvalidOperationException($"User '{userIdentifier}' is already configured.");

        var builder = new TradeSessionUserBuilder<TTradeSession, TEnvironment>(
            userIdentifier, environment, trackerFactory);
        configure(builder);
        foreach (var create in builder.Features.Values)
            _features.Add(_ => create);

        _users.Add(userIdentifier, new TradeSessionUserDefinition(userIdentifier, environment));
    }

    /// <summary>
    /// Build the trade session host with the provided configuration.
    /// </summary>
    /// <param name="factory">The factory function to create a new instance of <typeparamref name="TTradeSession"/>.</param>
    /// <returns>The configured trade session host which can be started</returns>
    public TradeSessionHost<TTradeSession> Build(Func<TTradeSession> factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var factories = _features.Select(bind => bind(_clientProvider)).ToArray();
        var session = factory() ?? throw new InvalidOperationException("Factory returned null.");
        var host = new TradeSessionHost<TTradeSession>(
            session,
            _clientProvider.ExchangeName,
            _loggerFactory.CreateLogger($"{_clientProvider.ExchangeName}.{typeof(TTradeSession).Name}"),
            factories.Select(create => new Func<ITradeSessionFeature>(() => create(session))).ToArray(),
            _updateLoopInterval,
            _externalClock != null);

        var users = new TradeSessionUsers<TUser>(_users.Values.Select(definition =>
            _userFactory(new TradeSessionUserContext(
                definition,
                () => host.GetSpotUserDataTracker(definition.UserIdentifier),
                mode => host.GetFuturesUserDataTracker(definition.UserIdentifier, mode)))));
        host.Attach(new TradeSessionContext<TUser>(
            _clientProvider.ExchangeName, users, host.GetOrderBooks, host.GetOrderBook, _externalClock));

        return host;
    }
}

/// <summary>
/// Symbol configuration builder
/// </summary>
public class TradeSessionSymbolBuilder<TTradeSession>
    : TradeSessionSymbolBuilder<TTradeSession, TradeSessionSymbolBuilder<TTradeSession>>,
        ITradeTrackingBuilder,
        ITickerTrackingBuilder,
        IKlineTrackingBuilder
    where TTradeSession : TradeSession
{
    internal TradeSessionSymbolBuilder(SharedSymbol symbol, TradeEnvironment? environment = null)
        : base(symbol, environment)
    {
    }
}

/// <summary>
/// Symbol configuration builder
/// </summary>
public abstract class TradeSessionSymbolBuilder<TTradeSession, TBuilder>
    where TTradeSession : TradeSession
    where TBuilder : TradeSessionSymbolBuilder<TTradeSession, TBuilder>
{
    private readonly HashSet<string> _names = new();

    /// <summary>
    /// The symbol
    /// </summary>
    public SharedSymbol Symbol { get; }
    /// <summary>
    /// The trade environment
    /// </summary>
    public TradeEnvironment? Environment { get; }

    internal List<Func<ISharedUserClientProvider, Func<TTradeSession, ITradeSessionFeature>>> Features { get; } = new();

    /// <summary>
    /// ctor
    /// </summary>
    protected TradeSessionSymbolBuilder(SharedSymbol symbol, TradeEnvironment? environment = null)
    {
        Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        Environment = environment;
    }

    /// <summary>
    /// Register a feature configuration
    /// </summary>
    protected TBuilder RegisterFeature(string name, Func<TTradeSession, ITradeSessionFeature> create)
    {
        if (create == null)
            throw new ArgumentNullException(nameof(create));

        return AddFeature(name, _ => create);
    }

    /// <summary>
    /// Add a feature
    /// </summary>
    internal TBuilder AddFeature(
        string name,
        Func<ISharedUserClientProvider, Func<TTradeSession, ITradeSessionFeature>> bind)
    {
        if (!_names.Add(name))
            throw new InvalidOperationException($"Feature '{name}' was already configured for this symbol.");

        Features.Add(bind);
        return (TBuilder)this;
    }
}

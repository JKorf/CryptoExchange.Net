using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.Sessions;

/// <summary>
/// Trading session builder
/// </summary>
public class TradingSessionBuilder<TStrategy, TSelf, TUser>
    where TStrategy : TradingStrategy<TUser>
    where TSelf : TradingSessionBuilder<TStrategy, TSelf, TUser>
    where TUser : class, ITradingUser
{
    private readonly HashSet<string> _names = new();
    private readonly List<Func<ISharedUserClientProvider, Func<TStrategy, ITradingSessionFeature>>> _features = new();
    private readonly Dictionary<string, TradingSessionUserDefinition> _users = new(StringComparer.Ordinal);
    private readonly ISharedUserClientProvider _clientProvider;
    private readonly ILogger _logger;
    private readonly Func<TradingSessionUserContext, TUser> _userFactory;
    private TimeSpan _updateLoopInterval;

    /// <summary>
    /// ctor
    /// </summary>
    protected TradingSessionBuilder(
        ISharedUserClientProvider clientProvider,
        ILogger logger,
        Func<TradingSessionUserContext, TUser> userFactory)
    {
        _clientProvider = clientProvider ?? throw new ArgumentNullException(nameof(clientProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userFactory = userFactory ?? throw new ArgumentNullException(nameof(userFactory));
    }

    /// <summary>
    /// Add a symbol tracking configuration
    /// </summary>
    /// <param name="symbol">The symbol</param>
    /// <param name="configure">The symbol configurator</param>
    /// <param name="environment">Trading environment</param>
    public TSelf AddSymbolTracking(
        SharedSymbol symbol,
        Action<TradingSessionSymbolBuilder<TStrategy>> configure,
        TradeEnvironment? environment = null)
    {
        RegisterSymbol(new TradingSessionSymbolBuilder<TStrategy>(symbol, environment), configure);
        return (TSelf)this;
    }

    /// <summary>
    /// Add an update callback at a set interval
    /// </summary>
    /// <param name="updateInterval">The interval at which the
    /// <see cref="TradingStrategy.UpdateAsync(System.Threading.CancellationToken)"/> method should be called</param>
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
        where TBuilder : TradingSessionSymbolBuilder<TStrategy, TBuilder>
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        configure(builder);
        _features.AddRange(builder.Features);
    }

    /// <summary>
    /// Register a feature config
    /// </summary>
    protected void RegisterFeature(string name, Func<TStrategy, ITradingSessionFeature> create)
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
        Action<TradingSessionUserBuilder<TStrategy, TEnvironment>> configure,
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

        var builder = new TradingSessionUserBuilder<TStrategy, TEnvironment>(
            userIdentifier, environment, trackerFactory);
        configure(builder);
        foreach (var create in builder.Features.Values)
            _features.Add(_ => create);

        _users.Add(userIdentifier, new TradingSessionUserDefinition(userIdentifier, environment));
    }

    /// <summary>
    /// Build the trading host with the provided configuration.
    /// </summary>
    /// <param name="factory">The factory function to create a new instance of <typeparamref name="TStrategy"/>.</param>
    /// <returns>The configured trading host which can be started</returns>
    public TradingHost<TStrategy> Build(Func<TStrategy> factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var factories = _features.Select(bind => bind(_clientProvider)).ToArray();
        var strategy = factory() ?? throw new InvalidOperationException("Factory returned null.");
        var host = new TradingHost<TStrategy>(
            strategy,
            _clientProvider.ExchangeName,
            _logger,
            factories.Select(create => new Func<ITradingSessionFeature>(() => create(strategy))).ToArray(),
            _updateLoopInterval);

        var users = new TradingSessionUsers<TUser>(_users.Values.Select(definition =>
            _userFactory(new TradingSessionUserContext(
                definition,
                () => host.GetSpotUserDataTracker(definition.UserIdentifier),
                mode => host.GetFuturesUserDataTracker(definition.UserIdentifier, mode)))));
        host.Attach(new TradingContext<TUser>(
            _clientProvider.ExchangeName, users, host.GetOrderBooks, host.GetOrderBook));

        return host;
    }
}

/// <summary>
/// Symbol configuration builder
/// </summary>
public class TradingSessionSymbolBuilder<TStrategy>
    : TradingSessionSymbolBuilder<TStrategy, TradingSessionSymbolBuilder<TStrategy>>
    where TStrategy : TradingStrategy
{
    internal TradingSessionSymbolBuilder(SharedSymbol symbol, TradeEnvironment? environment = null)
        : base(symbol, environment)
    {
    }
}

/// <summary>
/// Symbol configuration builder
/// </summary>
public abstract class TradingSessionSymbolBuilder<TStrategy, TBuilder>
    where TStrategy : TradingStrategy
    where TBuilder : TradingSessionSymbolBuilder<TStrategy, TBuilder>
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

    internal List<Func<ISharedUserClientProvider, Func<TStrategy, ITradingSessionFeature>>> Features { get; } = new();

    /// <summary>
    /// ctor
    /// </summary>
    protected TradingSessionSymbolBuilder(SharedSymbol symbol, TradeEnvironment? environment = null)
    {
        Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        Environment = environment;
    }

    /// <summary>
    /// Register a feature configuration
    /// </summary>
    protected TBuilder RegisterFeature(string name, Func<TStrategy, ITradingSessionFeature> create)
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
        Func<ISharedUserClientProvider, Func<TStrategy, ITradingSessionFeature>> bind)
    {
        if (!_names.Add(name))
            throw new InvalidOperationException($"Feature '{name}' was already configured for this symbol.");

        Features.Add(bind);
        return (TBuilder)this;
    }
}

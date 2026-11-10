using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Sessions;
using CryptoExchange.Net.Trackers.Sessions.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.Sessions;

public sealed record SessionUserDefinition(
    string UserIdentifier,
    TradeEnvironment? Environment);

public class SessionBuilder<TSession, TSelf>
    where TSession : Session
    where TSelf : SessionBuilder<TSession, TSelf>
{
    private readonly HashSet<string> _names = new();
    private readonly List<Func<ISharedUserClientProvider, Func<TSession, ISessionFeature>>> _features = new();
    private readonly ISharedUserClientProvider _clientProvider;
    private readonly ILogger _logger;
    private TimeSpan _updateLoopInterval;
    private readonly Dictionary<string, SessionUserDefinition> _users =
        new(StringComparer.Ordinal);

    protected SessionBuilder(
        ISharedUserClientProvider clientProvider,
        ILogger logger)
    {
        _clientProvider = clientProvider
            ?? throw new ArgumentNullException(nameof(clientProvider));
        _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));
    }

    public TSelf AddSymbolTracking(
        SharedSymbol symbol,
        Action<SymbolTrackingBuilder<TSession>> configure,
        TradeEnvironment? environment = null)
    {
        RegisterSymbol(new SymbolTrackingBuilder<TSession>(symbol, environment), configure);
        return (TSelf)this;
    }

    public TSelf AddUpdateLoop(TimeSpan updateInterval)
    {
        _updateLoopInterval = updateInterval;
        return (TSelf)this;
    }

    protected void RegisterSymbol<TBuilder>(TBuilder builder, Action<TBuilder> configure)
        where TBuilder : SymbolTrackingBuilder<TSession, TBuilder>
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        configure(builder);
        _features.AddRange(builder.Features);
    }

    protected void RegisterFeature(string name, Func<TSession, ISessionFeature> create)
    {
        if (create == null)
            throw new ArgumentNullException(nameof(create));
        if (!_names.Add(name))
            throw new InvalidOperationException($"Feature '{name}' was already configured.");

        _features.Add(_ => create);
    }

    protected void RegisterUser<TEnvironment>(
        string userIdentifier,
        Action<UserTrackingBuilder<TSession, TEnvironment>> configure,
        ITrackerFactory trackerFactory,
        TEnvironment? environment = null)
        where TEnvironment : TradeEnvironment
    {
        if (string.IsNullOrWhiteSpace(userIdentifier))
            throw new ArgumentException(
                "A user identifier is required.", nameof(userIdentifier));

        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        if (trackerFactory == null)
            throw new ArgumentNullException(nameof(trackerFactory));

        if (_users.ContainsKey(userIdentifier))
            throw new InvalidOperationException(
                $"User '{userIdentifier}' is already configured.");

        var builder = new UserTrackingBuilder<TSession, TEnvironment>(
            userIdentifier, environment, trackerFactory);

        configure(builder);

        // Transfer registrations after configuration succeeds.
        foreach (var create in builder.Features.Values)
            _features.Add(_ => create);

        _users.Add(
            userIdentifier,
            new SessionUserDefinition(userIdentifier, environment));
    }

    public TSession Build(Func<TSession> factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var factories = _features.Select(bind => bind(_clientProvider)).ToArray();
        var session = factory()
            ?? throw new InvalidOperationException("Factory returned null.");

        session.Configure(_clientProvider.ExchangeName, _logger, factories
            .Select(create => new Func<ISessionFeature>(() => create(session)))
            .ToArray(), _updateLoopInterval);

        session.ConfigureClientProvider(_clientProvider);
        session.ConfigureUsers(_users.Values.ToArray());

        return session;
    }
}

public class SymbolTrackingBuilder<TSession>
    : SymbolTrackingBuilder<TSession, SymbolTrackingBuilder<TSession>>
    where TSession : Session
{
    internal SymbolTrackingBuilder(SharedSymbol symbol, TradeEnvironment? environment = null) : base(symbol, environment) { }
}

public abstract class SymbolTrackingBuilder<TSession, TBuilder>
    where TSession : Session
    where TBuilder : SymbolTrackingBuilder<TSession, TBuilder>
{
    private readonly HashSet<string> _names = new();

    public SharedSymbol Symbol { get; }
    public TradeEnvironment? Environment { get; }

    internal List<Func<ISharedUserClientProvider, Func<TSession, ISessionFeature>>> Features { get; } = new();

    protected SymbolTrackingBuilder(SharedSymbol symbol, TradeEnvironment? environment = null)
    {
        Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        Environment = environment;
    }

    protected TBuilder RegisterFeature(string name, Func<TSession, ISessionFeature> create)
    {
        if (create == null)
            throw new ArgumentNullException(nameof(create));

        return AddFeature(name, _ => create);
    }

    internal TBuilder AddFeature(
        string name,
        Func<ISharedUserClientProvider, Func<TSession, ISessionFeature>> bind)
    {
        if (!_names.Add(name))
            throw new InvalidOperationException($"Feature '{name}' was already configured for this symbol.");

        Features.Add(bind);
        return (TBuilder)this;
    }
}

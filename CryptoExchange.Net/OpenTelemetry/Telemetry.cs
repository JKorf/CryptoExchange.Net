using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CryptoExchange.Net.OpenTelemetry;

public partial class Telemetry
{
    #region Scope Management
    /// <summary>
    /// Represents the current telemetry scope for asynchronous operations.
    /// </summary>
    private static readonly AsyncLocal<Telemetry?> CurrentScope = new();

    /// <summary>
    /// Gets the current telemetry instance associated with the active scope.
    /// </summary>
    public static Telemetry? Current => CurrentScope.Value;

    /// <summary>
    /// Represents a scope for managing asynchronous local state.
    /// </summary>
    private readonly struct AsyncLocalScope : IDisposable
    {
        private readonly Telemetry? _previous;

        public AsyncLocalScope(Telemetry? previous) => _previous = previous;

        public void Dispose() => CurrentScope.Value = _previous;
    }

    /// <summary>
    /// Starts a new telemetry scope and sets the specified telemetry as the current scope.
    /// </summary>
    public static IDisposable StartScope(Telemetry? telemetry)
    {
        var previous = CurrentScope.Value;
        CurrentScope.Value = telemetry;
        return new AsyncLocalScope(previous);
    }
    #endregion

    private readonly string _exchange;
    private readonly Version _exchangeLibVersion;
    private string _userIdentifier = null;
    
    private KeyValuePair<string, object?>[] _baseTags;

    /// <summary>
    /// Represents the activity source used for tracing in the CryptoExchange.Net library.
    /// </summary>
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Initializes a new instance of the <see cref="Telemetry"/> class.
    /// </summary>
    public Telemetry(string exchange, Version exchangeLibVersion)
    {
        _exchange = exchange;
        _exchangeLibVersion = exchangeLibVersion;
        _baseTags = CreateBaseTags();
    }

    private KeyValuePair<string, object?>[] CreateBaseTags()
    {
        var tagList = new TagList
        {
            { CryptoExchangeTelemetry.Tags.ExchangeName, _exchange },
            { CryptoExchangeTelemetry.Tags.ExchangeLibraryVersion, _exchangeLibVersion.ToString() }
        };
        
        if(!string.IsNullOrEmpty(_userIdentifier))
            tagList.Add(CryptoExchangeTelemetry.Tags.UserId, tagList);

        return tagList.ToArray();
    }

    public void SetUserIdentifier(string userId)
    {
        // TODO: Hash the userId for compliance by default, leave plain-text if allowed by the configuration.
        _userIdentifier = userId;
        _baseTags = CreateBaseTags();
    }

    /// <inheritdoc />
    public Activity? StartActivity(string name, ActivityKind kind, ActivityContext? parentContext = default, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, DateTimeOffset startOffset = default)
    {
        var tagList = tags != null ? new TagList([.._baseTags, ..tags]) : new TagList(_baseTags);
        var activity = CryptoExchangeTelemetry.ActivitySource.StartActivity(name, kind, parentContext ?? default, tagList, links, startOffset);
        return activity;
    }
}
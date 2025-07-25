using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CryptoExchange.Net.OpenTelemetry;

/// <summary>
/// Provides functionality for managing telemetry data and tracing activities within the application.
/// </summary>
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
    internal static Telemetry? Current => CurrentScope.Value;

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
    internal static IDisposable StartScope(Telemetry? telemetry)
    {
        var previous = CurrentScope.Value;
        CurrentScope.Value = telemetry;
        return new AsyncLocalScope(previous);
    }
    #endregion

    private readonly string _exchange;
    private readonly Version _exchangeLibVersion;
    private string? _userIdentifier;
    
    private KeyValuePair<string, object?>[] _baseTags;

    /// <summary>
    /// Initializes a new instance of the <see cref="Telemetry"/> class.
    /// </summary>
    internal Telemetry(string exchange, Version exchangeLibVersion)
    {
        _exchange = exchange;
        _exchangeLibVersion = exchangeLibVersion;
        _baseTags = CreateBaseTags();
    }

    /// <summary>
    /// Creates a collection of key-value pairs representing the base telemetry tags for the exchange.
    /// </summary>
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

    /// <summary>
    /// Sets the unique identifier for the user.
    /// </summary>
    /// <param name="userId">The unique identifier for the user. This value must not be null or empty.</param>
    internal void SetUserIdentifier(string userId)
    {
        // TODO: Hash the userId for compliance by default, leave plain-text if allowed by the configuration.
        _userIdentifier = userId;
        _baseTags = CreateBaseTags();
    }

    /// <summary>
    /// Starts a new activity with the specified name, kind, and optional parameters.
    /// </summary>
    /// <remarks>This method creates and starts a new activity using the provided parameters. The activity is
    /// created using the <see cref="ActivitySource"/> associated with the telemetry system. If <paramref name="tags"/>
    /// is provided, it is combined with any predefined base tags before being applied to the activity.</remarks>
    /// <param name="name">The name of the activity to start. This value cannot be null or empty.</param>
    /// <param name="kind">The kind of activity, such as internal, server, or client, represented by the <see cref="ActivityKind"/>
    /// enumeration.</param>
    /// <param name="parentContext">The optional parent context for the activity. If not provided, a default context is used.</param>
    /// <param name="tags">An optional collection of key-value pairs to associate with the activity as tags. These tags are merged with any
    /// base tags.</param>
    /// <param name="links">An optional collection of <see cref="ActivityLink"/> objects to associate with the activity.</param>
    /// <param name="startOffset">An optional timestamp indicating when the activity started. If not specified, the current time is used.</param>
    /// <returns>An <see cref="Activity"/> instance representing the started activity, or <see langword="null"/> if the activity
    /// could not be started.</returns>
    internal Activity? StartActivity(string name, ActivityKind kind, ActivityContext? parentContext = default, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, DateTimeOffset startOffset = default)
    {
        var tagList = tags != null ? new TagList([.._baseTags, ..tags]) : new TagList(_baseTags);
        var activity = CryptoExchangeTelemetry.ActivitySource.StartActivity(name, kind, parentContext ?? default, tagList, links, startOffset);
        return activity;
    }
}
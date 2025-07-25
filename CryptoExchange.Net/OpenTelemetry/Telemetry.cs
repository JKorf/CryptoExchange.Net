using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Interfaces;

namespace CryptoExchange.Net.OpenTelemetry;

public class Telemetry
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
    public Activity? StartActivity(string name, ActivityKind kind, ActivityContext? parentContext, IEnumerable<KeyValuePair<string, object?>>? tags, IEnumerable<ActivityLink>? links = null, DateTimeOffset startOffset = default)
    {
        var tagList = tags != null ? new TagList([.._baseTags, ..tags]) : new TagList(_baseTags);
        var activity = CryptoExchangeTelemetry.ActivitySource.StartActivity(name, kind, parentContext ?? default, tagList, links, startOffset);
        return activity;
    }

    public Activity? StartSendAsyncActivity(string baseAddress, RequestDefinition definition, int? weight,
        int? weightSingleLimiter, string? rateLimitKeySuffix)
    {
        var tags = new TagList(_baseTags)
        {
            { CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition.Method },
            { CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition.Path },
            { CryptoExchangeTelemetry.Tags.RequestAuthenticated, definition.Authenticated },
            { CryptoExchangeTelemetry.Tags.RequestWeight, weight ?? definition.Weight },
            { CryptoExchangeTelemetry.Tags.RequestRateLimitKeySuffix, rateLimitKeySuffix },
        };
        return StartActivity(CryptoExchangeTelemetry.Activities.SendRequest, ActivityKind.Client, Activity.Current?.Context, tags);
    }

    public void RecordCacheHit<T>(RequestDefinition definition, WebCallResult<T> cached)
    {
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.RequestCache, CryptoExchangeTelemetry.TagValues.RequestCacheHit);

        var tags = new TagList(_baseTags)
        {
            { CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition.Method },
            { CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition.Path }
        };

        CryptoExchangeTelemetry.CacheHitCounter.Add(1, tags);
        if (cached.ResponseLength.HasValue)
        {
            CryptoExchangeTelemetry.CacheHitBytesHistogram.Record(cached.ResponseLength.Value, tags);
        }
    }

    public void RecordCacheMiss(RequestDefinition? definition)
    {
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.RequestCache, CryptoExchangeTelemetry.TagValues.RequestCacheMiss);

        CryptoExchangeTelemetry.CacheMissCounter.Add(1, [
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition?.Method),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition?.Path)
        ]);
    }

    public void RecordTotalRequestsMade(RequestDefinition definition)
    {
        CryptoExchangeTelemetry.TotalRequestsMadeCounter.Add(1, [
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition.Method),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition.Path)
        ]);
    }

    public void RecordPrepareError(RequestDefinition definition, CallResult? prepareResult)
    {
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.ErrorType, CryptoExchangeTelemetry.TagValues.RequestFailTypePrepareFailure);
        Activity.Current?.SetStatus(ActivityStatusCode.Error, prepareResult?.Error!.ToString());

        CryptoExchangeTelemetry.RequestErrorsCounter.Add(1, [
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.ErrorType, CryptoExchangeTelemetry.TagValues.RequestFailTypePrepareFailure),
            new(CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition.Method),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition.Path)
        ]);
    }

    public void RecordRequestAttempt(int currentTry, int requestId)
    {
        Activity.Current?.AddEvent(new ActivityEvent(CryptoExchangeTelemetry.ActivityEvents.SendRequestIteration,
            tags: new ActivityTagsCollection
            {
                { CryptoExchangeTelemetry.Tags.RequestId, requestId },
            }));

        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.RequestId, requestId);
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.HttpRequestRetryCount, currentTry);
    }

    public void RecordRestApiErrorReceived<T>(RequestDefinition definition, WebCallResult<T> result)
    {
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.ErrorType, CryptoExchangeTelemetry.TagValues.RequestFailTypeRestApiError);
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.HttpResponseStatusCode, result.ResponseStatusCode);
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.HttpResponseBodySize, result.ResponseLength);
        
        if(result.Error?.Code != null)
            Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.ResultErrorCode, result.Error.Code);
        
        CryptoExchangeTelemetry.RequestErrorsCounter.Add(1, [
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.ErrorType, CryptoExchangeTelemetry.TagValues.RequestFailTypeRestApiError),
            new(CryptoExchangeTelemetry.Tags.HttpResponseStatusCode, result.ResponseStatusCode),
            new(CryptoExchangeTelemetry.Tags.ResultErrorCode, result.Error?.Code),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition.Path)
        ]);

        if (Activity.Current != null)
        {
            var elapsed = DateTime.UtcNow - Activity.Current.StartTimeUtc;
            // Histogram
        }
    }

    public void RecordRestApiResponseReceived<T>(RequestDefinition definition, WebCallResult<T> result)
    {
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.HttpResponseStatusCode, result.ResponseStatusCode);
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.HttpResponseBodySize, result.ResponseLength);

        var tags = new TagList([
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.HttpResponseStatusCode, result.ResponseStatusCode),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition.Path)
        ]);

        CryptoExchangeTelemetry.ApiResponseCounter.Add(1, tags);
        if (result.ResponseLength.HasValue)
        {
            CryptoExchangeTelemetry.ApiResponseBytesHistogram.Record(result.ResponseLength.Value, tags);
        }
        
        if (Activity.Current != null)
        {
            var elapsed = DateTime.UtcNow - Activity.Current.StartTimeUtc;
            // Histogram
        }
    }
    public void RecordRestApiCancellationRequested(RequestDefinition definition)
    {
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.ErrorType, CryptoExchangeTelemetry.TagValues.RequestFailTypeCancellationRequested);

        CryptoExchangeTelemetry.RequestErrorsCounter.Add(1, [
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.ErrorType, CryptoExchangeTelemetry.TagValues.RequestFailTypeCancellationRequested),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition.Path)
        ]);

        if (Activity.Current != null)
        {
            var elapsed = DateTime.UtcNow - Activity.Current.StartTimeUtc;
            // Histogram
        }
    }

    public void RecordCacheFill<T>(RequestDefinition definition, WebCallResult<T> result)
    {
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.RequestCache, CryptoExchangeTelemetry.TagValues.RequestCacheFill);

        CryptoExchangeTelemetry.CacheFillCounter.Add(1, [
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition?.Method),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition?.Path)
        ]);
    }
    
    public void RecordRateLimitConnectionFailed()
    {
    }

    public void RecordRateLimitRequestFailed(RequestDefinition definition, IRateLimitGuard guard, string host, RateLimitItemType itemType, RateLimitingBehaviour behavior)
    {
        CryptoExchangeTelemetry.RequestRateLimitCounter.Add(1, [
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition?.Method),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition?.Path),
            new(CryptoExchangeTelemetry.Tags.ServerAddress, definition),
            new(CryptoExchangeTelemetry.Tags.RateLimitGuardName, guard.Name),
            new(CryptoExchangeTelemetry.Tags.RateLimitItemType, itemType),
            new(CryptoExchangeTelemetry.Tags.RateLimitBehavior, behavior),
        ]);
    }
}
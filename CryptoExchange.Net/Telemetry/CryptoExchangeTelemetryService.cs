using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.Telemetry;

public interface ICryptoExchangeTelemetryService
{
    /// <summary>
    /// Gets a value indicating whether there are any active listeners for telemetry events.
    /// </summary>
    bool HasListeners { get; }
    
    void SetUserIdentifier(string userId);
    
    /// <summary>
    /// Starts a new telemetry activity with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the activity to start.</param>
    /// <param name="kind">The kind of activity, indicating its role in a distributed trace.</param>
    /// <param name="parentContext">The context of the parent activity, used to establish a relationship between activities in a trace.</param>
    /// <param name="tags">A collection of key-value pairs representing additional metadata to associate with the activity.</param>
    /// <param name="links">A collection of links to other activities, used to establish relationships between activities in a trace.</param>
    /// <param name="startOffset">The timestamp offset indicating when the activity started, relative to the current time.</param>
    /// <returns>
    /// An <see cref="Activity"/> instance representing the started activity, or <c>null</c> if the activity could not be started.
    /// </returns>
    Activity? StartActivity(string name, ActivityKind kind, ActivityContext? parentContext,
        IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null,
        DateTimeOffset startOffset = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseAddress"></param>
    /// <param name="requestDefinition"></param>
    /// <param name="weight"></param>
    /// <param name="weightSingleLimiter"></param>
    /// <param name="rateLimitKeySuffix"></param>
    /// <returns></returns>
    Activity? StartSendAsyncActivity(string baseAddress, RequestDefinition requestDefinition, int? weight, int? weightSingleLimiter, string? rateLimitKeySuffix);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="definition"></param>
    void RecordTotalRequestsMade(RequestDefinition definition);
    void RecordCacheHit<T>(RequestDefinition definition, WebCallResult<T> cached);
    void RecordCacheMiss(RequestDefinition definition);
    void RecordRequestAttempt(int currentTry, int requestId);
    void RecordPrepareError(RequestDefinition definition, CallResult? prepareResult);
    void RecordRestApiErrorReceived<T>(RequestDefinition definition, WebCallResult<T> result);
    void RecordRestApiResponseReceived<T>(RequestDefinition definition, WebCallResult<T> result);
    void RecordCacheFill<T>(RequestDefinition definition, WebCallResult<T> result);
    void RecordRestApiCancellationRequested(RequestDefinition definition);
}

public class CryptoExchangeTelemetryService : ICryptoExchangeTelemetryService
{
    private readonly string _exchange;
    private readonly Version _exchangeLibVersion;
    private string _userIdentifier = null;
    
    private KeyValuePair<string, object?>[] _baseTags;

    /// <inheritdoc />
    public bool HasListeners => CryptoExchangeTelemetry.ActivitySource.HasListeners();

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoExchangeTelemetryService"/> class.
    /// </summary>
    public CryptoExchangeTelemetryService(string exchange, Version exchangeLibVersion)
    {
        _exchange = exchange;
        _exchangeLibVersion = exchangeLibVersion;
        _baseTags = CreateBaseTags();
    }

    private KeyValuePair<string, object?>[] CreateBaseTags()
    {
        var tagList = new TagList
        {
            { CryptoExchangeTelemetry.Tags.ServiceName, _exchange },
            { CryptoExchangeTelemetry.Tags.ServiceVersion, _exchangeLibVersion.ToString() },
            { CryptoExchangeTelemetry.Tags.ServiceInstanceId, Environment.MachineName }
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
    public Activity? StartActivity(string name, ActivityKind kind, ActivityContext? parentContext, IEnumerable<KeyValuePair<string, object?>>? tags = null, IEnumerable<ActivityLink>? links = null, DateTimeOffset startOffset = default)
    {
        var activity = CryptoExchangeTelemetry.ActivitySource.StartActivity(name, kind, parentContext ?? default, tags, links, startOffset);
        if (activity != null)
        {
            foreach (var tag in _baseTags)
                activity.AddTag(tag.Key, tag.Value);
        }

        return activity;
    }

    public Activity? StartSendAsyncActivity(string baseAddress, RequestDefinition definition, int? weight,
        int? weightSingleLimiter, string? rateLimitKeySuffix)
    {
        if (!HasListeners)
            return null;
        
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
            CryptoExchangeTelemetry.CacheHitBytesCounter.Add(cached.ResponseLength.Value, tags);
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
            CryptoExchangeTelemetry.ApiResponseBytesCounter.Add(result.ResponseLength.Value, tags);
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
}
using System;
using System.Diagnostics;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.OpenTelemetry;

public partial class Telemetry
{
    /// <summary>
    /// Starts a telemetry activity for sending an asynchronous HTTP request.
    /// </summary>
    /// <param name="baseAddress">The base address of the HTTP request.</param>
    /// <param name="definition">The definition of the request, including method, path, and authentication details.</param>
    /// <param name="weight">The weight of the request, used for rate limiting. If null, the default weight from <paramref
    /// name="definition"/> is used.</param>
    /// <param name="weightSingleLimiter">The weight for a single rate limiter, if applicable. Can be null.</param>
    /// <param name="rateLimitKeySuffix">An optional suffix for the rate limit key, used to distinguish rate-limited requests.</param>
    /// <returns>An <see cref="Activity"/> instance representing the telemetry activity for the request, or <see
    /// langword="null"/> if no activity is started.</returns>
    internal Activity? StartSendAsyncActivity(string baseAddress, RequestDefinition definition, int? weight,
        int? weightSingleLimiter, string? rateLimitKeySuffix)
    {
        var tags = new TagList(_baseTags)
        {
            { CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition.Method },
            { CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition.Path },
            { CryptoExchangeTelemetry.Tags.RequestAuthenticated, definition.Authenticated },
            { CryptoExchangeTelemetry.Tags.RequestWeight, weight ?? definition.Weight },
        };
        
        if(!string.IsNullOrEmpty(rateLimitKeySuffix))
            tags.Add(CryptoExchangeTelemetry.Tags.RequestRateLimitKeySuffix, rateLimitKeySuffix);
        
        return StartActivity(CryptoExchangeTelemetry.Activities.SendRequest, ActivityKind.Client, Activity.Current?.Context, tags);
    }

    /// <summary>
    /// Records a cache hit for a request and updates relevant telemetry metrics.
    /// </summary>
    /// <remarks>This method updates telemetry counters and histograms to reflect a cache hit, including the 
    /// number of cache hits and the size of the cached response, if available. It also sets a telemetry  tag indicating
    /// that the request was served from the cache.</remarks>
    /// <typeparam name="T">The type of the cached response data.</typeparam>
    /// <param name="definition">The definition of the request, including method and path information.</param>
    /// <param name="cached">The cached result of the request, including response metadata such as length.</param>
    internal void RecordCacheHit<T>(RequestDefinition definition, WebCallResult<T> cached)
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

    /// <summary>
    /// Records a cache miss for the specified request definition.
    /// </summary>
    /// <param name="definition">The request definition associated with the cache miss. This may include details such as the HTTP method and URL
    /// path. If <paramref name="definition"/> is <see langword="null"/>, no additional request-specific tags will be
    /// recorded.</param>
    internal void RecordCacheMiss(RequestDefinition? definition)
    {
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.RequestCache, CryptoExchangeTelemetry.TagValues.RequestCacheMiss);

        CryptoExchangeTelemetry.CacheMissCounter.Add(1, [
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition?.Method),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition?.Path)
        ]);
    }

    /// <summary>
    /// Records the total number of requests made to the system.
    /// </summary>
    /// <remarks>This method increments a telemetry counter to track the total number of requests made..</remarks>
    /// <param name="definition">The definition of the request, including details such as the HTTP method and URL path.</param>
    internal void RecordTotalRequestsMade(RequestDefinition definition)
    {
        CryptoExchangeTelemetry.TotalRequestsMadeCounter.Add(1, [
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition.Method),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition.Path)
        ]);
    }

    /// <summary>
    /// Records an error that occurred during the preparation phase of a request.
    /// </summary>
    /// <remarks>This method logs telemetry data for errors encountered during the request preparation phase.</remarks>
    /// <param name="definition">The definition of the request, including its method and path.</param>
    /// <param name="prepareResult">The result of the preparation phase, which may include error details if the preparation failed.</param>
    internal void RecordPrepareError(RequestDefinition definition, CallResult? prepareResult)
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

    /// <summary>
    /// Records telemetry data for a request attempt, including the current retry count and request identifier.
    /// </summary>
    /// <remarks>This method adds an event to the current activity and sets tags for the request ID and retry
    /// count. It is intended for internal use to track request attempts in telemetry.</remarks>
    /// <param name="currentTry">The current retry attempt number for the request. Must be a non-negative integer.</param>
    /// <param name="requestId">The unique identifier for the request being tracked.</param>
    internal void RecordRequestAttempt(int currentTry, int requestId)
    {
        Activity.Current?.AddEvent(new ActivityEvent(CryptoExchangeTelemetry.ActivityEvents.SendRequestIteration,
            tags: new ActivityTagsCollection
            {
                { CryptoExchangeTelemetry.Tags.RequestId, requestId },
            }));

        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.RequestId, requestId);
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.HttpRequestRetryCount, currentTry);
    }

    /// <summary>
    /// Records telemetry data for a REST API error encountered during a request.
    /// </summary>
    /// <remarks>This method captures and logs telemetry information related to REST API errors, including
    /// error type,  HTTP response status code, response body size, and any error codes returned by the API.  It also
    /// increments a counter for request errors and associates relevant tags for monitoring purposes.</remarks>
    /// <typeparam name="T">The type of the result returned by the REST API call.</typeparam>
    /// <param name="definition">The definition of the request, including its path and other metadata.</param>
    /// <param name="result">The result of the REST API call, containing the response status, error details, and other information.</param>
    internal void RecordRestApiErrorReceived<T>(RequestDefinition definition, WebCallResult<T> result)
    {
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.ErrorType, CryptoExchangeTelemetry.TagValues.RequestFailTypeRestApiError);
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.HttpResponseStatusCode, result.ResponseStatusCode);
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.HttpResponseBodySize, result.ResponseLength);

        if (result.Error?.Code != null)
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

    /// <summary>
    /// Records telemetry data for a received REST API response, including response status, body size, and other
    /// relevant metrics.
    /// </summary>
    /// <remarks>This method captures telemetry information such as the HTTP response status code, response
    /// body size, and request path. It updates relevant metrics counters and histograms for monitoring purposes. If the
    /// response length is available, it is recorded in a histogram. Additionally, if an activity is currently active,
    /// the elapsed time since the activity's start is calculated and can be used for further telemetry
    /// purposes.</remarks>
    /// <typeparam name="T">The type of the response data returned by the API call.</typeparam>
    /// <param name="definition">The definition of the API request, including its path and other metadata.</param>
    /// <param name="result">The result of the API call, containing the response status code, response length, and other details.</param>
    internal void RecordRestApiResponseReceived<T>(RequestDefinition definition, WebCallResult<T> result)
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
    
    /// <summary>
    /// Records telemetry data for a REST API request that was canceled by the caller.
    /// </summary>
    /// <remarks>This method updates telemetry metrics and tags to reflect that a cancellation was requested
    /// for the specified API request. It is intended for internal use to track and analyze cancellation patterns in
    /// REST API operations.</remarks>
    /// <param name="definition">The definition of the request, including details such as the request path.</param>
    internal void RecordRestApiCancellationRequested(RequestDefinition definition)
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

    /// <summary>
    /// Records a cache fill event for a specific request and its result.
    /// </summary>
    /// <remarks>This method is used to log telemetry data related to cache fill operations, including request
    /// metadata such as the HTTP method and URL path. It increments a counter to track the number of cache fill events
    /// and associates relevant tags for monitoring and analysis.</remarks>
    /// <typeparam name="T">The type of the result associated with the request.</typeparam>
    /// <param name="definition">The definition of the request, including its method and URL path. This parameter cannot be null.</param>
    /// <param name="result">The result of the web call associated with the cache fill event. This parameter cannot be null.</param>
    internal void RecordCacheFill<T>(RequestDefinition definition, WebCallResult<T> result)
    {
        Activity.Current?.SetTag(CryptoExchangeTelemetry.Tags.RequestCache, CryptoExchangeTelemetry.TagValues.RequestCacheFill);

        CryptoExchangeTelemetry.CacheFillCounter.Add(1, [
            .._baseTags,
            new(CryptoExchangeTelemetry.Tags.HttpRequestMethod, definition?.Method),
            new(CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, definition?.Path)
        ]);
    }
}
using System;
using System.Diagnostics;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.OpenTelemetry;

public partial class Telemetry
{
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
}
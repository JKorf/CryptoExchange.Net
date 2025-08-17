using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;

namespace CryptoExchange.Net.OpenTelemetry;

public partial class Telemetry
{
    internal Activity? StartSocketConnectActivity(Uri uri, int connectionId)
    {
        var tags = new TagList(_baseTags)
        {
            { CryptoExchangeTelemetry.Tags.ServerAddress, uri.Host },
            { CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, uri.AbsolutePath },
            { CryptoExchangeTelemetry.Tags.WebSocketSocketId, connectionId }
        };
        return StartActivity(CryptoExchangeTelemetry.Activities.SocketQuery, ActivityKind.Client, Activity.Current?.Context, tags);
    }

    internal Activity? StartSocketAuthenticateActivity(Uri uri, int connectionId)
    {
        var tags = new TagList(_baseTags)
        {
            { CryptoExchangeTelemetry.Tags.ServerAddress, uri.Host },
            { CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, uri.AbsolutePath },
            { CryptoExchangeTelemetry.Tags.WebSocketSocketId, connectionId }
        };
        return StartActivity(CryptoExchangeTelemetry.Activities.SocketAuthenticate, ActivityKind.Client, Activity.Current?.Context, tags);
    }

    internal Activity? StartSocketSubscribeActivity(Subscription subscription)
    {
        var tags = new TagList(_baseTags)
        {
            { CryptoExchangeTelemetry.Tags.RequestAuthenticated, subscription.Authenticated },
            { CryptoExchangeTelemetry.Tags.WebSocketSubscriptionTopic, subscription.Topic },
            { CryptoExchangeTelemetry.Tags.WebSocketUser, subscription.UserSubscription },
            { CryptoExchangeTelemetry.Tags.WebSocketSubscriptionMatchers, subscription.MessageMatcher.ToString() }
        };
        return StartActivity(CryptoExchangeTelemetry.Activities.SocketSubscribe, ActivityKind.Client, Activity.Current?.Context, tags);
    }

    internal Activity? StartSocketQueryActivity(int connectionId, Query query, bool authenticated)
    {
        var tags = new TagList(_baseTags)
        {
            { CryptoExchangeTelemetry.Tags.WebSocketSocketId, connectionId },
            { CryptoExchangeTelemetry.Tags.WebSocketRequestId, query.Id },
            { CryptoExchangeTelemetry.Tags.RequestWeight, query.Weight },
            { CryptoExchangeTelemetry.Tags.RequestAuthenticated, authenticated },
            { CryptoExchangeTelemetry.Tags.WebSocketRequestMatchers, query.MessageMatcher.ToString() },
        };
        return StartActivity(CryptoExchangeTelemetry.Activities.SocketQuery, ActivityKind.Client, Activity.Current?.Context, tags);
    }
    
    private TagList SocketTags(Uri uri, bool? auth = null) => new(_baseTags)
    {
        { CryptoExchangeTelemetry.Tags.ServerAddress, uri.Host },
        { CryptoExchangeTelemetry.Tags.HttpRequestUrlPath, uri.AbsolutePath },
        { CryptoExchangeTelemetry.Tags.RequestAuthenticated, auth }
    };

    internal void RecordSocketOpened(Uri uri, bool authenticated)
        => CryptoExchangeTelemetry.SocketsOpenTracker.Add(1, SocketTags(uri, authenticated));

    internal void RecordSocketClosed(Uri uri)
        => CryptoExchangeTelemetry.SocketsOpenTracker.Add(-1, SocketTags(uri));

    internal void RecordSocketReconnect(Uri uri)
        => CryptoExchangeTelemetry.SocketsReconnectsCounter.Add(1, SocketTags(uri));

    internal void RecordSocketConnectFailure(Uri uri)
        => CryptoExchangeTelemetry.SocketsConnectFailuresCounter.Add(1, SocketTags(uri));

    internal void RecordSocketConnectionError(Uri uri)
        => CryptoExchangeTelemetry.SocketsErrorsCounter.Add(1, SocketTags(uri));

    internal void RecordSocketBytesReceived(Uri uri, int bytes, WebSocketMessageType messageType)
        => CryptoExchangeTelemetry.SocketsRecvBytesHistogram.Record(bytes, [
            ..SocketTags(uri),
            new(CryptoExchangeTelemetry.Tags.WebSocketMessageType, messageType)
        ]);

    internal void RecordSocketBytesSent(Uri uri, int bytes)
        => CryptoExchangeTelemetry.SocketsSendBytesHistogram.Record(bytes, SocketTags(uri));

    internal void RecordSocketMessageReceived(Uri uri, WebSocketMessageType messageType)
        => CryptoExchangeTelemetry.SocketsMessagesReceivedCounter.Add(1, [
            ..SocketTags(uri),
            new(CryptoExchangeTelemetry.Tags.WebSocketMessageType, messageType)
        ]);

    internal void RecordSocketMessageProcessed(Uri uri, long processingMs, long handlerUserMs)
    {
        CryptoExchangeTelemetry.SocketsMessagesProcessedCounter.Add(1, SocketTags(uri));
        CryptoExchangeTelemetry.SocketsMessageProcessingMsHistogram.Record(processingMs, SocketTags(uri));
        CryptoExchangeTelemetry.SocketsHandlerUserMsHistogram.Record(handlerUserMs, SocketTags(uri));
    }

    internal void RecordSocketParseFailure(Uri uri)
        => CryptoExchangeTelemetry.SocketsParseFailuresCounter.Add(1, SocketTags(uri));

    internal void RecordSocketDeserializeFailure(Uri uri)
        => CryptoExchangeTelemetry.SocketsDeserializeFailuresCounter.Add(1, SocketTags(uri));

    internal void RecordSocketUnmatched(Uri uri)
        => CryptoExchangeTelemetry.SocketsUnmatchedCounter.Add(1, SocketTags(uri));

    internal void RecordSubscribeAttempt(Uri uri, bool authenticated)
        => CryptoExchangeTelemetry.SocketsSubscribeAttemptsCounter.Add(1, SocketTags(uri, authenticated));

    internal void RecordSubscribeSuccess(Uri uri, bool authenticated)
    {
        CryptoExchangeTelemetry.SocketSubscriptionsOpenTracker.Add(1, SocketTags(uri));
        CryptoExchangeTelemetry.SocketsSubscribeSuccessCounter.Add(1, SocketTags(uri, authenticated));
    }

    internal void RecordSubscribeFailure(Uri uri, bool authenticated)
        => CryptoExchangeTelemetry.SocketsSubscribeFailuresCounter.Add(1, SocketTags(uri, authenticated));

    internal void RecordUnsubscribe(Uri uri)
    {
        CryptoExchangeTelemetry.SocketSubscriptionsOpenTracker.Add(-1, SocketTags(uri));
        CryptoExchangeTelemetry.SocketsUnsubscribeTotalCounter.Add(1, SocketTags(uri));
    }
        

    internal void RecordResubscribeAttempt(Uri uri, int batchSize)
    {
        CryptoExchangeTelemetry.SocketsResubscribeAttemptsCounter.Add(1, SocketTags(uri));
        CryptoExchangeTelemetry.SocketsResubscribeBatchSizeHistogram.Record(batchSize, SocketTags(uri));
    }

    internal void RecordResubscribeSuccess(Uri uri, long totalMs)
    {
        CryptoExchangeTelemetry.SocketsResubscribeSuccessCounter.Add(1, SocketTags(uri));
        CryptoExchangeTelemetry.SocketsResubscribeTotalMsHistogram.Record(totalMs, SocketTags(uri));
    }

    internal void RecordResubscribeFailure(Uri uri)
        => CryptoExchangeTelemetry.SocketsResubscribeFailuresCounter.Add(1, SocketTags(uri));

    internal void RecordWsQueryAttempt(Uri uri, bool authenticated, int weight)
    {
        CryptoExchangeTelemetry.WsQueryAttemptsCounter.Add(1, SocketTags(uri, authenticated));
        CryptoExchangeTelemetry.WsQueryWeightHistogram.Record(weight, SocketTags(uri, authenticated));
    }

    internal void RecordWsQuerySuccess(Uri uri, long latencyMs)
    {
        CryptoExchangeTelemetry.WsQuerySuccessCounter.Add(1, SocketTags(uri));
        CryptoExchangeTelemetry.WsQueryLatencyMsHistogram.Record(latencyMs, SocketTags(uri));
    }

    internal void RecordWsQueryFailure(Uri uri, long latencyMs)
    {
        CryptoExchangeTelemetry.WsQueryFailuresCounter.Add(1, SocketTags(uri));
        CryptoExchangeTelemetry.WsQueryLatencyMsHistogram.Record(latencyMs, SocketTags(uri));
    }

    internal void RecordWsQueryTimeout(Uri uri, long latencyMs)
    {
        CryptoExchangeTelemetry.WsQueryTimeoutsCounter.Add(1, SocketTags(uri));
        CryptoExchangeTelemetry.WsQueryLatencyMsHistogram.Record(latencyMs, SocketTags(uri));
    }

    internal void RecordNoDataTimeout(Uri uri)
        => CryptoExchangeTelemetry.SocketsNoDataTimeoutsCounter.Add(1, SocketTags(uri));
}

internal static class TelemetryEx
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CallResult RecordActivity(this CallResult result)
    {
        if (Activity.Current == null) return result;

        if (result.Success)
            Activity.Current.SetStatus(ActivityStatusCode.Ok);
        else
            Activity.Current.SetStatus(ActivityStatusCode.Error, result.Error?.ToString());

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CallResult<T> RecordActivity<T>(this CallResult<T> result)
    {
        if (Activity.Current == null) return result;
        
        if (result.Success)
            Activity.Current.SetStatus(ActivityStatusCode.Ok);
        else
            Activity.Current.SetStatus(ActivityStatusCode.Error, result.Error?.ToString());

        return result;
    }
}
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CryptoExchange.Net.OpenTelemetry;

/// <summary>
/// Provides constants for telemetry in the CryptoExchange.Net library.
/// </summary>
internal static class CryptoExchangeTelemetry
{
    /// <summary>
    /// Represents the name of the activity source used for tracing in the CryptoExchange.Net library.
    /// </summary>
    public const string ActivitySourceName = "CryptoExchange.Net";
    
    /// <summary>
    /// Represents the name of the meter used for tracking metrics in the CryptoExchange.Net library.
    /// </summary>
    public const string MeterName = "CryptoExchange.Net";
    
    /// <summary>
    /// Provides an <see cref="ActivitySource"/> for tracing and telemetry related to the CryptoExchange system.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new (ActivitySourceName, typeof(CryptoExchangeTelemetry).Assembly.GetName().Version?.ToString() ?? "unknown");

    /// <summary>
    /// Represents the meter used for metrics in the CryptoExchange.Net library.
    /// </summary>
    public static readonly Meter Meter = new (MeterName);
    
    public static readonly Counter<int> TotalRequestsMadeCounter
        = Meter.CreateCounter<int>("jkorf.requests.total", "requests", "Total number of requests made");

    public static readonly Counter<int> RequestErrorsCounter
        = Meter.CreateCounter<int>("jkorf.requests.errors", "requests", "Total number of request errors");

    public static readonly Counter<int> RequestRateLimitCounter
        = Meter.CreateCounter<int>("jkorf.requests.rate_limits", "requests", "Total number of request hitting rate limits");
    
    public static readonly Counter<long> RequestRateLimitDelayCounter
        = Meter.CreateCounter<long>("jkorf.requests.rate_limits.delay", "millis", "Total time delayed for rate limits in milliseconds");
   
    public static readonly Counter<long> ConnectionsRateLimitDelayCounter
        = Meter.CreateCounter<long>("jkorf.connections.rate_limits.delay", "millis", "Total time delayed for rate limits in milliseconds");

    public static readonly Counter<int> CacheHitCounter
        = Meter.CreateCounter<int>("jkorf.cache.hits", "requests", "Total number of request served from cache");
    
    public static readonly Histogram<long> CacheHitBytesHistogram
        = Meter.CreateHistogram<long>("jkorf.cache.hits_bytes_histogram", "bytes", "Total number of bytes served from cache");

    public static readonly Counter<int> CacheMissCounter
        = Meter.CreateCounter<int>("jkorf.cache.miss", "requests", "Total number of requests are cache misses");
    
    public static readonly Counter<int> CacheFillCounter
        = Meter.CreateCounter<int>("jkorf.cache.fills", "requests", "Total number of request placed into the cache");

    public static readonly Counter<int> ApiResponseCounter
        = Meter.CreateCounter<int>("jkorf.response.count", "requests", "Total number of request responses");
    
    public static readonly Histogram<long> ApiResponseBytesHistogram
        = Meter.CreateHistogram<long>("jkorf.response.bytes_histogram", "bytes", "Total number of content bytes from the server");

    public static readonly UpDownCounter<int> SocketsOpenTracker
        = Meter.CreateUpDownCounter<int>("jkorf.sockets.open", "connections", "Open websocket connections");

    public static readonly Counter<int> SocketsReconnectsCounter
        = Meter.CreateCounter<int>("jkorf.sockets.reconnects", "reconnects", "Websocket reconnect successes");

    public static readonly Counter<int> SocketsConnectFailuresCounter
        = Meter.CreateCounter<int>("jkorf.sockets.connect_failures", "failures", "Websocket connect failures");

    public static readonly Counter<int> SocketsErrorsCounter
        = Meter.CreateCounter<int>("jkorf.sockets.errors", "errors", "Websocket errors");

    public static readonly Histogram<long> SocketsRecvBytesHistogram
        = Meter.CreateHistogram<long>("jkorf.sockets.recv.bytes", "bytes", "Bytes received over websockets");

    public static readonly Histogram<long> SocketsSendBytesHistogram
        = Meter.CreateHistogram<long>("jkorf.sockets.send.bytes", "bytes", "Bytes sent over websockets");
    
    public static readonly Counter<int> SocketsMessagesReceivedCounter
        = Meter.CreateCounter<int>("jkorf.sockets.messages.received", "messages", "Messages received");

    public static readonly Counter<int> SocketsMessagesProcessedCounter
        = Meter.CreateCounter<int>("jkorf.sockets.messages.processed", "messages", "Messages processed by handlers");

    public static readonly Counter<int> SocketsParseFailuresCounter
        = Meter.CreateCounter<int>("jkorf.sockets.messages.parse_failures", "messages", "Messages failed to parse");

    public static readonly Counter<int> SocketsDeserializeFailuresCounter
        = Meter.CreateCounter<int>("jkorf.sockets.messages.deserialize_failures", "messages", "Messages failed to deserialize");

    public static readonly Counter<int> SocketsUnmatchedCounter
        = Meter.CreateCounter<int>("jkorf.sockets.messages.unmatched", "messages", "Messages not matched to any listener");

    public static readonly Histogram<long> SocketsMessageProcessingMsHistogram
        = Meter.CreateHistogram<long>("jkorf.sockets.message.processing_ms", "ms", "Message processing time");

    public static readonly Histogram<long> SocketsHandlerUserMsHistogram
        = Meter.CreateHistogram<long>("jkorf.sockets.message.handler_user_ms", "ms", "User handler time");

    public static readonly Counter<int> SocketsSubscribeAttemptsCounter
        = Meter.CreateCounter<int>("jkorf.sockets.subscribe.attempts", "ops", "Subscribe attempts");

    public static readonly Counter<int> SocketsSubscribeSuccessCounter
        = Meter.CreateCounter<int>("jkorf.sockets.subscribe.success", "ops", "Subscribe successes");

    public static readonly Counter<int> SocketsSubscribeFailuresCounter
        = Meter.CreateCounter<int>("jkorf.sockets.subscribe.failures", "ops", "Subscribe failures");

    public static readonly Counter<int> SocketsUnsubscribeTotalCounter
        = Meter.CreateCounter<int>("jkorf.sockets.unsubscribe.total", "ops", "Unsubscribe operations");

    public static readonly Counter<int> SocketsResubscribeAttemptsCounter
        = Meter.CreateCounter<int>("jkorf.sockets.resubscribe.attempts", "ops", "Resubscribe attempts");

    public static readonly Counter<int> SocketsResubscribeSuccessCounter
        = Meter.CreateCounter<int>("jkorf.sockets.resubscribe.success", "ops", "Resubscribe successes");

    public static readonly Counter<int> SocketsResubscribeFailuresCounter
        = Meter.CreateCounter<int>("jkorf.sockets.resubscribe.failures", "ops", "Resubscribe failures");

    public static readonly Histogram<int> SocketsResubscribeBatchSizeHistogram
        = Meter.CreateHistogram<int>("jkorf.sockets.resubscribe.batch_size", "subs", "Resubscribe batch sizes");

    public static readonly Histogram<long> SocketsResubscribeTotalMsHistogram
        = Meter.CreateHistogram<long>("jkorf.sockets.resubscribe.total_ms", "ms", "Total resubscribe duration");
    
    public static readonly Counter<int> WsQueryAttemptsCounter
        = Meter.CreateCounter<int>("jkorf.wsquery.attempts", "ops", "Websocket query attempts");

    public static readonly Counter<int> WsQuerySuccessCounter
        = Meter.CreateCounter<int>("jkorf.wsquery.success", "ops", "Websocket query successes");

    public static readonly Counter<int> WsQueryFailuresCounter
        = Meter.CreateCounter<int>("jkorf.wsquery.failures", "ops", "Websocket query failures");

    public static readonly Counter<int> WsQueryTimeoutsCounter
        = Meter.CreateCounter<int>("jkorf.wsquery.timeouts", "ops", "Websocket query timeouts");

    public static readonly Histogram<long> WsQueryLatencyMsHistogram
        = Meter.CreateHistogram<long>("jkorf.wsquery.latency_ms", "ms", "Websocket query latency");

    public static readonly Histogram<int> WsQueryWeightHistogram
        = Meter.CreateHistogram<int>("jkorf.wsquery.weight", "weight", "Websocket query weight");

    public static readonly Counter<int> SocketsNoDataTimeoutsCounter
        = Meter.CreateCounter<int>("jkorf.sockets.timeouts.no_data", "ops", "No-data timeouts triggering reconnect");

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class Tags
    {
        // Common tags and otel conventions
        public const string ErrorType = "error.type";
        public const string ServerAddress = "server.address";
        public const string HttpRequestMethod = "http.request.method";
        public const string HttpRequestUrlPath = "url.path";
        public const string HttpRequestRetryCount = "http.request.resend_count";
        public const string HttpResponseStatusCode = "http.response.status_code";
        public const string HttpResponseBodySize = "http.response.body.size";

        // Custom tags for CryptoExchange.Net telemetry
        public const string ExchangeName = "jkorf.exchange.name";
        public const string ExchangeLibraryVersion = "jkorf.exchange.version";

        public const string UserId = "jkorf.user.id";
        public const string RequestId = "jkorf.request.id";
        public const string RequestWeight = "jkorf.request.weight";
        public const string RequestRateLimitKeySuffix = "jkorf.request.ratelimit.key_suffix";
        public const string RequestAuthenticated = "jkorf.request.auth";
        public const string RequestCache = "jkorf.request.cache";
        public const string ResultErrorCode = "jkorf.result.error_code";
        public const string RateLimitGuardName = "jkorf.rate_limit.guard.name";
        public const string RateLimitItemType = "jkorf.rate_limit.item_type";
        public const string RateLimitBehavior = "jkorf.rate_limit.behavior";
        public const string RateLimitDelay = "jkorf.rate_limit.delay_millis";
        
        public const string WebSocketMessageType = "jkorf.ws.message.type";
        public const string WebSocketSocketId = "jkorf.ws.socket.id";
        public const string WebSocketSubscriptionTopic = "jkorf.ws.subscription.topic";
        public const string WebSocketSubscriptionMatchers = "jkorf.ws.subscription.matchers";
        public const string WebSocketUser = "jkorf.ws.user";
        public const string WebSocketRequestId = "jkorf.ws.request.id";
        public const string WebSocketRequestMatchers = "jkorf.ws.request.matchers";
    }

    public static class TagValues
    {
        public const string RequestFailTypePrepareFailure = "prepare_failure";
        public const string RequestFailTypeRestApiError = "rest_api_error";
        public const string RequestFailTypeCancellationRequested = "cancellation_requested";

        public const string RequestCacheHit = "hit";
        public const string RequestCacheMiss = "miss";
        public const string RequestCacheFill = "fill";
    }

    public static class Activities
    {
        public const string SendRequest = "CryptoExchange.SendAsync";
        public const string RateLimitDelayingRequest = "CryptoExchange.RateLimitDelayingRequest";
        public const string RateLimitDelayingConnection = "CryptoExchange.RateLimitDelayingConnection";

        public const string SocketConnect = "CryptoExchange.SocketConnect";
        public const string SocketAuthenticate = "CryptoExchange.SocketAuthenticate";
        public const string SocketSubscribe = "CryptoExchange.SocketSubscribe";
        public const string SocketQuery = "CryptoExchange.SocketQuery";
    }

    public static class ActivityEvents
    {
        public const string SendRequestIteration = "jkorf.request.iter";
        public const string RateLimitDelayRequest = "jkorf.request.rate_limit.delay";
        public const string RateLimitDelayConnection = "jkorf.connection.rate_limit.delay";
        public const string RateLimitFailRequest = "jkorf.request.rate_limit.fail";
        public const string RateLimitFailConnection = "jkorf.connection.rate_limit.fail";
    }
}

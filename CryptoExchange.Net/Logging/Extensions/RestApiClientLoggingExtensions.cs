using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;

namespace CryptoExchange.Net.Logging.Extensions
{
    internal static class RestApiClientLoggingExtensions
    {
        private static readonly Action<ILogger, int?, int?, long, string?, Exception?> _restApiErrorReceived;
        private static readonly Action<ILogger, int?, int?, long, string?, Exception?> _restApiResponseReceived;
        private static readonly Action<ILogger, int, string, Exception?> _restApiFailedToSyncTime;
        private static readonly Action<ILogger, int, string, Exception?> _restApiNoApiCredentials;
        private static readonly Action<ILogger, int, Uri, Exception?> _restApiCreatingRequest;
        private static readonly Action<ILogger, int, HttpMethod, string, Uri, string, Exception?> _restApiSendingRequest;
        private static readonly Action<ILogger, int, DateTime, Exception?> _restApiRateLimitRetry;
        private static readonly Action<ILogger, int, DateTime, Exception?> _restApiRateLimitPauseUntil;
        private static readonly Action<ILogger, int, RequestDefinition, string?, string, string, Exception?> _restApiSendRequest;


        static RestApiClientLoggingExtensions()
        {
            _restApiErrorReceived = LoggerMessage.Define<int?, int?, long, string?>(
                LogLevel.Warning,
                new EventId(4000, "RestApiErrorReceived"),
                "[Req {RequestId}] {ResponseStatusCode} - Error received in {ResponseTime}ms: {ErrorMessage}");

            _restApiResponseReceived = LoggerMessage.Define<int?, int?, long, string?>(
                LogLevel.Debug,
                new EventId(4001, "RestApiResponseReceived"),
                "[Req {RequestId}] {ResponseStatusCode} - Response received in {ResponseTime}ms: {OriginalData}");

            _restApiFailedToSyncTime = LoggerMessage.Define<int, string>(
                LogLevel.Debug,
                new EventId(4002, "RestApifailedToSyncTime"),
                "[Req {RequestId}] Failed to sync time, aborting request: {ErrorMessage}");

            _restApiNoApiCredentials = LoggerMessage.Define<int, string>(
                LogLevel.Warning,
                new EventId(4003, "RestApiNoApiCredentials"),
                "[Req {RequestId}] Request {RestApiUri} failed because no ApiCredentials were provided");

            _restApiCreatingRequest = LoggerMessage.Define<int, Uri>(
                LogLevel.Information,
                new EventId(4004, "RestApiCreatingRequest"),
                "[Req {RequestId}] Creating request for {RestApiUri}");

            _restApiSendingRequest = LoggerMessage.Define<int, HttpMethod, string, Uri, string>(
                LogLevel.Trace,
                new EventId(4005, "RestApiSendingRequest"),
                "[Req {RequestId}] Sending {Method} {Signed} request to {RestApiUri}{Query}");

            _restApiRateLimitRetry = LoggerMessage.Define<int, DateTime>(
                LogLevel.Warning,
                new EventId(4006, "RestApiRateLimitRetry"),
                "[Req {RequestId}] Received ratelimit error, retrying after {Timestamp}");

            _restApiRateLimitPauseUntil = LoggerMessage.Define<int, DateTime>(
                LogLevel.Warning,
                new EventId(4007, "RestApiRateLimitPauseUntil"),
                "[Req {RequestId}] Ratelimit error from server, pausing requests until {Until}");

            _restApiSendRequest = LoggerMessage.Define<int, RequestDefinition, string?, string, string>(
                LogLevel.Debug,
                new EventId(4008, "RestApiSendRequest"),
                "[Req {RequestId}] Sending {Definition} request with body {Body}, query parameters {Query} and headers {Headers}");
        }

        public static void RestApiErrorReceived(this ILogger logger, int? requestId, HttpStatusCode? responseStatusCode, long responseTime, string? error)
        {
            _restApiErrorReceived(logger, requestId, (int?)responseStatusCode, responseTime, error, null);
        }

        public static void RestApiResponseReceived(this ILogger logger, int? requestId, HttpStatusCode? responseStatusCode, long responseTime, string? originalData)
        {
            _restApiResponseReceived(logger, requestId, (int?)responseStatusCode, responseTime, originalData, null);
        }

        public static void RestApiFailedToSyncTime(this ILogger logger, int requestId, string error)
        {
            _restApiFailedToSyncTime(logger, requestId, error, null);
        }

        public static void RestApiNoApiCredentials(this ILogger logger, int requestId, string uri)
        {
            _restApiNoApiCredentials(logger, requestId, uri, null);
        }

        public static void RestApiCreatingRequest(this ILogger logger, int requestId, Uri uri)
        {
            _restApiCreatingRequest(logger, requestId, uri, null);
        }

        public static void RestApiSendingRequest(this ILogger logger, int requestId, HttpMethod method, string signed, Uri uri, string paramString)
        {
            _restApiSendingRequest(logger, requestId, method, signed, uri, paramString, null);
        }

        public static void RestApiRateLimitRetry(this ILogger logger, int requestId, DateTime retryAfter)
        {
            _restApiRateLimitRetry(logger, requestId, retryAfter, null);
        }

        public static void RestApiRateLimitPauseUntil(this ILogger logger, int requestId, DateTime retryAfter)
        {
            _restApiRateLimitPauseUntil(logger, requestId, retryAfter, null);
        }

        public static void RestApiSendRequest(this ILogger logger, int requestId, RequestDefinition definition, string? body, string query, string headers)
        {
            _restApiSendRequest(logger, requestId, definition, body, query, headers, null);
        }
    }
}

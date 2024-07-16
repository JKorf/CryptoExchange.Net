using Microsoft.Extensions.Logging;
using System;

namespace CryptoExchange.Net.Logging.Extensions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class RateLimitGateLoggingExtensions
    {
        private static readonly Action<ILogger, int, string, string, string, Exception?> _rateLimitRequestFailed;
        private static readonly Action<ILogger, int, string, string, Exception?> _rateLimitConnectionFailed;
        private static readonly Action<ILogger, int, string, TimeSpan, string, string, Exception?> _rateLimitDelayingRequest;
        private static readonly Action<ILogger, int, TimeSpan, string, string, Exception?> _rateLimitDelayingConnection;
        private static readonly Action<ILogger, int, string, string, string, int, Exception?> _rateLimitAppliedRequest;
        private static readonly Action<ILogger, int, string, string, int, Exception?> _rateLimitAppliedConnection;

        static RateLimitGateLoggingExtensions()
        {
            _rateLimitRequestFailed = LoggerMessage.Define<int, string, string, string>(
                LogLevel.Warning,
                new EventId(6000, "RateLimitRequestFailed"),
                "[Req {Id}] Call to {Path} failed because of ratelimit guard {Guard}; {Limit}");

            _rateLimitConnectionFailed = LoggerMessage.Define<int, string, string>(
                LogLevel.Warning,
                new EventId(6001, "RateLimitConnectionFailed"),
                "[Sckt {Id}] Connection failed because of ratelimit guard {Guard}; {Limit}");

            _rateLimitDelayingRequest = LoggerMessage.Define<int, string, TimeSpan, string, string>(
                LogLevel.Warning,
                new EventId(6002, "RateLimitDelayingRequest"),
                "[Req {Id}] Delaying call to {Path} by {Delay} because of ratelimit guard {Guard}; {Limit}");

            _rateLimitDelayingConnection = LoggerMessage.Define<int, TimeSpan, string, string>(
                LogLevel.Warning,
                new EventId(6003, "RateLimitDelayingConnection"),
                "[Sckt {Id}] Delaying connection by {Delay} because of ratelimit guard {Guard}; {Limit}");

            _rateLimitAppliedConnection = LoggerMessage.Define<int, string, string, int>(
                LogLevel.Trace,
                new EventId(6004, "RateLimitDelayingConnection"),
                "[Sckt {Id}] Connection passed ratelimit guard {Guard}; {Limit}, New count: {Current}");

            _rateLimitAppliedRequest = LoggerMessage.Define<int, string, string, string, int>(
                LogLevel.Trace,
                new EventId(6005, "RateLimitAppliedRequest"),
                "[Req {Id}] Call to {Path} passed ratelimit guard {Guard}; {Limit}, New count: {Current}");
        }

        public static void RateLimitRequestFailed(this ILogger logger, int requestId, string path, string guard, string limit)
        {
            _rateLimitRequestFailed(logger, requestId, path, guard, limit, null);
        }

        public static void RateLimitConnectionFailed(this ILogger logger, int connectionId, string guard, string limit)
        {
            _rateLimitConnectionFailed(logger, connectionId, guard, limit, null);
        }

        public static void RateLimitDelayingRequest(this ILogger logger, int requestId, string path, TimeSpan delay, string guard, string limit)
        {
            _rateLimitDelayingRequest(logger, requestId, path, delay, guard, limit, null);
        }

        public static void RateLimitDelayingConnection(this ILogger logger, int connectionId, TimeSpan delay, string guard, string limit)
        {
            _rateLimitDelayingConnection(logger, connectionId, delay, guard, limit, null);
        }

        public static void RateLimitAppliedConnection(this ILogger logger, int connectionId, string guard, string limit, int current)
        {
            _rateLimitAppliedConnection(logger, connectionId, guard, limit, current, null);
        }

        public static void RateLimitAppliedRequest(this ILogger logger, int requestIdId, string path, string guard, string limit, int current)
        {
            _rateLimitAppliedRequest(logger, requestIdId, path, guard, limit, current, null);
        }
    }
}

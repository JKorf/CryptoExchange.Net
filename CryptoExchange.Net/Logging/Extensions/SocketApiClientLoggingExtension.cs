using Microsoft.Extensions.Logging;
using System;

namespace CryptoExchange.Net.Logging.Extensions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class SocketApiClientLoggingExtension
    {
        private static readonly Action<ILogger, int, Exception?> _failedToAddSubscriptionRetryOnDifferentConnection;
        private static readonly Action<ILogger, int, Exception?> _hasBeenPausedCantSubscribeAtThisMoment;
        private static readonly Action<ILogger, int, string?, Exception?> _failedToSubscribe;
        private static readonly Action<ILogger, int, int, Exception?> _cancellationTokenSetClosingSubscription;
        private static readonly Action<ILogger, int, int, Exception?> _subscriptionCompletedSuccessfully;
        private static readonly Action<ILogger, int, Exception?> _hasBeenPausedCantSendQueryAtThisMoment;
        private static readonly Action<ILogger, int, Exception?> _attemptingToAuthenticate;
        private static readonly Action<ILogger, int, Exception?> _authenticationFailed;
        private static readonly Action<ILogger, int, Exception?> _authenticated;
        private static readonly Action<ILogger, string?, Exception?> _failedToDetermineConnectionUrl;
        private static readonly Action<ILogger, string, Exception?> _connectionAddressSetTo;
        private static readonly Action<ILogger, int, string, Exception?> _socketCreatedForAddress;
        private static readonly Action<ILogger, int, Exception?> _unsubscribingAll;
        private static readonly Action<ILogger, Exception?> _disposingSocketClient;
        private static readonly Action<ILogger, int, int, Exception?> _unsubscribingSubscription;
        private static readonly Action<ILogger, int, Exception?> _reconnectingAllConnections;
        private static readonly Action<ILogger, DateTime, Exception?> _addingRetryAfterGuard;

        static SocketApiClientLoggingExtension()
        {
            _failedToAddSubscriptionRetryOnDifferentConnection = LoggerMessage.Define<int>(
                LogLevel.Trace,
                new EventId(3000, "FailedToAddSubscriptionRetryOnDifferentConnection"),
                "[Sckt {SocketId}] failed to add subscription, retrying on different connection");

            _hasBeenPausedCantSubscribeAtThisMoment = LoggerMessage.Define<int>(
                LogLevel.Warning,
                new EventId(3001, "HasBeenPausedCantSubscribeAtThisMoment"),
                "[Sckt {SocketId}] has been paused, can't subscribe at this moment");

            _failedToSubscribe = LoggerMessage.Define<int, string?>(
                LogLevel.Warning,
                new EventId(3002, "FailedToSubscribe"),
                "[Sckt {SocketId}] failed to subscribe: {ErrorMessage}");

            _cancellationTokenSetClosingSubscription = LoggerMessage.Define<int, int>(
                LogLevel.Information,
                new EventId(3003, "CancellationTokenSetClosingSubscription"),
                "[Sckt {SocketId}] Cancellation token set, closing subscription {SubscriptionId}");

            _subscriptionCompletedSuccessfully = LoggerMessage.Define<int, int>(
                LogLevel.Information,
                new EventId(3004, "SubscriptionCompletedSuccessfully"),
                "[Sckt {SocketId}] subscription {SubscriptionId} completed successfully");

            _hasBeenPausedCantSendQueryAtThisMoment = LoggerMessage.Define<int>(
                LogLevel.Warning,
                new EventId(3005, "HasBeenPausedCantSendQueryAtThisMoment"),
                "[Sckt {SocketId}] has been paused, can't send query at this moment");

            _attemptingToAuthenticate = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(3006, "AttemptingToAuthenticate"),
                "[Sckt {SocketId}] Attempting to authenticate");

            _authenticationFailed = LoggerMessage.Define<int>(
                LogLevel.Warning,
                new EventId(3007, "AuthenticationFailed"),
                "[Sckt {SocketId}] authentication failed");

            _authenticated = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(3008, "Authenticated"),
                "[Sckt {SocketId}] authenticated");

            _failedToDetermineConnectionUrl = LoggerMessage.Define<string?>(
                LogLevel.Warning,
                new EventId(3009, "FailedToDetermineConnectionUrl"),
                "Failed to determine connection url: {ErrorMessage}");

            _connectionAddressSetTo = LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(3010, "ConnectionAddressSetTo"),
                "Connection address set to {ConnectionAddress}");

            _socketCreatedForAddress = LoggerMessage.Define<int, string>(
                LogLevel.Debug,
                new EventId(3011, "SocketCreatedForAddress"),
                "[Sckt {SocketId}] created for {Address}");

            _unsubscribingAll = LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(3013, "UnsubscribingAll"),
                "Unsubscribing all {SubscriptionCount} subscriptions");

            _disposingSocketClient = LoggerMessage.Define(
                LogLevel.Debug,
                new EventId(3015, "DisposingSocketClient"),
                "Disposing socket client, closing all subscriptions");

            _unsubscribingSubscription = LoggerMessage.Define<int, int>(
                LogLevel.Information,
                new EventId(3016, "UnsubscribingSubscription"),
                "[Sckt {SocketId}] Unsubscribing subscription {SubscriptionId}");

            _reconnectingAllConnections = LoggerMessage.Define<int>(
                LogLevel.Information,
                new EventId(3017, "ReconnectingAll"),
                "Reconnecting all {ConnectionCount} connections");

            _addingRetryAfterGuard = LoggerMessage.Define<DateTime>(
                LogLevel.Warning,
                new EventId(3018, "AddRetryAfterGuard"),
                "Adding RetryAfterGuard ({RetryAfter}) because the connection attempt was rate limited");
        }

        public static void FailedToAddSubscriptionRetryOnDifferentConnection(this ILogger logger, int socketId)
        {
            _failedToAddSubscriptionRetryOnDifferentConnection(logger, socketId, null);
        }

        public static void HasBeenPausedCantSubscribeAtThisMoment(this ILogger logger, int socketId)
        {
            _hasBeenPausedCantSubscribeAtThisMoment(logger, socketId, null);
        }

        public static void FailedToSubscribe(this ILogger logger, int socketId, string? error)
        {
            _failedToSubscribe(logger, socketId, error, null);
        }

        public static void CancellationTokenSetClosingSubscription(this ILogger logger, int socketId, int subscriptionId)
        {
            _cancellationTokenSetClosingSubscription(logger, socketId, subscriptionId, null);
        }

        public static void SubscriptionCompletedSuccessfully(this ILogger logger, int socketId, int subscriptionId)
        {
            _subscriptionCompletedSuccessfully(logger, socketId, subscriptionId, null);
        }

        public static void HasBeenPausedCantSendQueryAtThisMoment(this ILogger logger, int socketId)
        {
            _hasBeenPausedCantSendQueryAtThisMoment(logger, socketId, null);
        }

        public static void AttemptingToAuthenticate(this ILogger logger, int socketId)
        {
            _attemptingToAuthenticate(logger, socketId, null);
        }

        public static void AuthenticationFailed(this ILogger logger, int socketId)
        {
            _authenticationFailed(logger, socketId, null);
        }

        public static void Authenticated(this ILogger logger, int socketId)
        {
            _authenticated(logger, socketId, null);
        }

        public static void FailedToDetermineConnectionUrl(this ILogger logger, string? error)
        {
            _failedToDetermineConnectionUrl(logger, error, null);
        }

        public static void ConnectionAddressSetTo(this ILogger logger, string connectionAddress)
        {
            _connectionAddressSetTo(logger, connectionAddress, null);
        }

        public static void SocketCreatedForAddress(this ILogger logger, int socketId, string address)
        {
            _socketCreatedForAddress(logger, socketId, address, null);
        }

        public static void UnsubscribingAll(this ILogger logger, int subscriptionCount)
        {
            _unsubscribingAll(logger, subscriptionCount, null);
        }

        public static void DisposingSocketClient(this ILogger logger)
        {
            _disposingSocketClient(logger, null);
        }

        public static void UnsubscribingSubscription(this ILogger logger, int socketId, int subscriptionId)
        {
            _unsubscribingSubscription(logger, socketId, subscriptionId, null);
        }

        public static void ReconnectingAllConnections(this ILogger logger, int connectionCount)
        {
            _reconnectingAllConnections(logger, connectionCount, null);
        }

        public static void AddingRetryAfterGuard(this ILogger logger, DateTime retryAfter)
        {
            _addingRetryAfterGuard(logger, retryAfter, null);
        }
    }
}

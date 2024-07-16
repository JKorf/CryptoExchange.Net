using System;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Logging.Extensions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class SocketConnectionLoggingExtension
    {
        private static readonly Action<ILogger, int, bool, Exception?> _activityPaused;
        private static readonly Action<ILogger, int, Sockets.SocketConnection.SocketStatus, Sockets.SocketConnection.SocketStatus, Exception?> _socketStatusChanged;
        private static readonly Action<ILogger, int, string?, Exception?> _failedReconnectProcessing;
        private static readonly Action<ILogger, int, Exception?> _unkownExceptionWhileProcessingReconnection;
        private static readonly Action<ILogger, int, WebSocketError, string?, Exception?> _webSocketErrorCodeAndDetails;
        private static readonly Action<ILogger, int, string?, Exception?> _webSocketError;
        private static readonly Action<ILogger, int, int, Exception?> _messageSentNotPending;
        private static readonly Action<ILogger, int, string, Exception?> _receivedData;
        private static readonly Action<ILogger, int, string, Exception?> _failedToEvaluateMessage;
        private static readonly Action<ILogger, int, Exception?> _errorProcessingMessage;
        private static readonly Action<ILogger, int, int, string, Exception?> _processorMatched;
        private static readonly Action<ILogger, int, int, Exception?> _receivedMessageNotRecognized;
        private static readonly Action<ILogger, int, string?, Exception?> _failedToDeserializeMessage;
        private static readonly Action<ILogger, int, string, Exception?> _userMessageProcessingFailed;
        private static readonly Action<ILogger, int, long, long, Exception?> _messageProcessed;
        private static readonly Action<ILogger, int, int, Exception?> _closingSubscription;
        private static readonly Action<ILogger, int, Exception?> _notUnsubscribingSubscriptionBecauseDuplicateRunning;
        private static readonly Action<ILogger, int, Exception?> _alreadyClosing;
        private static readonly Action<ILogger, int, Exception?> _closingNoMoreSubscriptions;
        private static readonly Action<ILogger, int, int, int, Exception?> _addingNewSubscription;
        private static readonly Action<ILogger, int, Exception?> _nothingToResubscribeCloseConnection;
        private static readonly Action<ILogger, int, Exception?> _failedAuthenticationDisconnectAndRecoonect;
        private static readonly Action<ILogger, int, Exception?> _authenticationSucceeded;
        private static readonly Action<ILogger, int, string?, Exception?> _failedRequestRevitalization;
        private static readonly Action<ILogger, int, Exception?> _allSubscriptionResubscribed;
        private static readonly Action<ILogger, int, int, Exception?> _subscriptionUnsubscribed;
        private static readonly Action<ILogger, int, string, Exception?> _sendingPeriodic;
        private static readonly Action<ILogger, int, string, string, Exception?> _periodicSendFailed;
        private static readonly Action<ILogger, int, int, string, Exception?> _sendingData;
        private static readonly Action<ILogger, int, string, string, Exception?> _receivedMessageNotMatchedToAnyListener;

        static SocketConnectionLoggingExtension()
        {
            _activityPaused = LoggerMessage.Define<int, bool>(
                LogLevel.Information,
                new EventId(2000, "ActivityPaused"),
                "[Sckt {SocketId}] paused activity: {Paused}");

            _socketStatusChanged = LoggerMessage.Define<int, Sockets.SocketConnection.SocketStatus, Sockets.SocketConnection.SocketStatus>(
                LogLevel.Debug,
                new EventId(2001, "SocketStatusChanged"),
                "[Sckt {SocketId}] status changed from {OldStatus} to {NewStatus}");

            _failedReconnectProcessing = LoggerMessage.Define<int, string?>(
                LogLevel.Warning,
                new EventId(2002, "FailedReconnectProcessing"),
                "[Sckt {SocketId}] failed reconnect processing: {ErrorMessage}, reconnecting again");

            _unkownExceptionWhileProcessingReconnection = LoggerMessage.Define<int>(
                LogLevel.Warning,
                new EventId(2003, "UnkownExceptionWhileProcessingReconnection"),
                "[Sckt {SocketId}] Unknown exception while processing reconnection, reconnecting again");

            _webSocketErrorCodeAndDetails = LoggerMessage.Define<int, WebSocketError, string?>(
                LogLevel.Warning,
                new EventId(2004, "WebSocketErrorCode"),
                "[Sckt {SocketId}] error: Websocket error code {WebSocketErrorCdoe}, details: {Details}");

            _webSocketError = LoggerMessage.Define<int, string?>(
                LogLevel.Warning,
                new EventId(2005, "WebSocketError"),
                "[Sckt {SocketId}] error: {ErrorMessage}");

            _messageSentNotPending = LoggerMessage.Define<int, int>(
                LogLevel.Debug,
                new EventId(2006, "MessageSentNotPending"),
                "[Sckt {SocketId}] [Req {RequestId}] message sent, but not pending");

            _receivedData = LoggerMessage.Define<int, string>(
                LogLevel.Trace,
                new EventId(2007, "ReceivedData"),
                "[Sckt {SocketId}] received {OriginalData}");

            _failedToEvaluateMessage = LoggerMessage.Define<int, string>(
                LogLevel.Warning,
                new EventId(2008, "FailedToEvaluateMessage"),
                "[Sckt {SocketId}] failed to evaluate message. {OriginalData}");

            _errorProcessingMessage = LoggerMessage.Define<int>(
                LogLevel.Error,
                new EventId(2009, "ErrorProcessingMessage"),
                "[Sckt {SocketId}] error processing message");

            _processorMatched = LoggerMessage.Define<int, int, string>(
                LogLevel.Trace,
                new EventId(2010, "ProcessorMatched"),
                "[Sckt {SocketId}] {Count} processor(s) matched to message with listener identifier {ListenerId}");

            _receivedMessageNotRecognized = LoggerMessage.Define<int, int>(
                LogLevel.Warning,
                new EventId(2011, "ReceivedMessageNotRecognized"),
                "[Sckt {SocketId}] received message not recognized by handler {ProcessorId}");

            _failedToDeserializeMessage = LoggerMessage.Define<int, string?>(
                LogLevel.Warning,
                new EventId(2012, "FailedToDeserializeMessage"),
                "[Sckt {SocketId}] deserialization failed: {ErrorMessage}");

            _userMessageProcessingFailed = LoggerMessage.Define<int, string>(
                LogLevel.Warning,
                new EventId(2013, "UserMessageProcessingFailed"),
                "[Sckt {SocketId}] user message processing failed: {ErrorMessage}");

            _messageProcessed = LoggerMessage.Define<int, long, long>(
                LogLevel.Trace,
                new EventId(2014, "MessageProcessed"),
                "[Sckt {SocketId}] message processed in {ProcessingTime}ms, {ParsingTime}ms parsing");

            _closingSubscription = LoggerMessage.Define<int, int>(
                LogLevel.Debug,
                new EventId(2015, "ClosingSubscription"),
                "[Sckt {SocketId}] closing subscription {SubscriptionId}");

            _notUnsubscribingSubscriptionBecauseDuplicateRunning = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(2016, "NotUnsubscribingSubscription"),
                "[Sckt {SocketId}] not unsubscribing subscription as there is still a duplicate subscription running");

            _alreadyClosing = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(2017, "AlreadyClosing"),
                "[Sckt {SocketId}] already closing");

            _closingNoMoreSubscriptions = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(2018, "ClosingNoMoreSubscriptions"),
                "[Sckt {SocketId}] closing as there are no more subscriptions");

            _addingNewSubscription = LoggerMessage.Define<int, int, int>(
                LogLevel.Debug,
                new EventId(2019, "AddingNewSubscription"),
                "[Sckt {SocketId}] adding new subscription with id {SubscriptionId}, total subscriptions on connection: {UserSubscriptionCount}");

            _nothingToResubscribeCloseConnection = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(2020, "NothingToResubscribe"),
                "[Sckt {SocketId}] nothing to resubscribe, closing connection");

            _failedAuthenticationDisconnectAndRecoonect = LoggerMessage.Define<int>(
                LogLevel.Warning,
                new EventId(2021, "FailedAuthentication"),
                "[Sckt {SocketId}] authentication failed on reconnected socket. Disconnecting and reconnecting");

            _authenticationSucceeded = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(2022, "AuthenticationSucceeded"),
                "[Sckt {SocketId}] authentication succeeded on reconnected socket");

            _failedRequestRevitalization = LoggerMessage.Define<int, string?>(
                LogLevel.Warning,
                new EventId(2023, "FailedRequestRevitalization"),
                "[Sckt {SocketId}] failed request revitalization: {ErrorMessage}");

            _allSubscriptionResubscribed = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(2024, "AllSubscriptionResubscribed"),
                "[Sckt {SocketId}] all subscription successfully resubscribed on reconnected socket");

            _subscriptionUnsubscribed = LoggerMessage.Define<int, int>(
                LogLevel.Information,
                new EventId(2025, "SubscriptionUnsubscribed"),
                "[Sckt {SocketId}] subscription {SubscriptionId} unsubscribed");

            _sendingPeriodic = LoggerMessage.Define<int, string>(
                LogLevel.Trace,
                new EventId(2026, "SendingPeriodic"),
                "[Sckt {SocketId}] sending periodic {Identifier}");

            _periodicSendFailed = LoggerMessage.Define<int, string, string>(
                LogLevel.Warning,
                new EventId(2027, "PeriodicSendFailed"),
                "[Sckt {SocketId}] periodic send {Identifier} failed: {ErrorMessage}");

            _sendingData = LoggerMessage.Define<int, int, string>(
                LogLevel.Trace,
                new EventId(2028, "SendingData"),
                "[Sckt {SocketId}] [Req {RequestId}] sending messsage: {Data}");

            _receivedMessageNotMatchedToAnyListener = LoggerMessage.Define<int, string, string>(
                LogLevel.Warning,
                new EventId(2029, "ReceivedMessageNotMatchedToAnyListener"),
                "[Sckt {SocketId}] received message not matched to any listener. ListenId: {ListenId}, current listeners: {ListenIds}");
        }

        public static void ActivityPaused(this ILogger logger, int socketId, bool paused)
        {
            _activityPaused(logger, socketId, paused, null);
        }

        public static void SocketStatusChanged(this ILogger logger, int socketId, Sockets.SocketConnection.SocketStatus oldStatus, Sockets.SocketConnection.SocketStatus newStatus)
        {
            _socketStatusChanged(logger, socketId, oldStatus, newStatus, null);
        }

        public static void FailedReconnectProcessing(this ILogger logger, int socketId, string? error)
        {
            _failedReconnectProcessing(logger, socketId, error, null);
        }

        public static void UnkownExceptionWhileProcessingReconnection(this ILogger logger, int socketId, Exception e)
        {
            _unkownExceptionWhileProcessingReconnection(logger, socketId, e);
        }

        public static void WebSocketErrorCodeAndDetails(this ILogger logger, int socketId, WebSocketError error, string? details, Exception e)
        {
            _webSocketErrorCodeAndDetails(logger, socketId, error, details, e);
        }

        public static void WebSocketError(this ILogger logger, int socketId, string? errorMessage, Exception e)
        {
            _webSocketError(logger, socketId, errorMessage, e);
        }

        public static void MessageSentNotPending(this ILogger logger, int socketId, int requestId)
        {
            _messageSentNotPending(logger, socketId, requestId, null);
        }

        public static void ReceivedData(this ILogger logger, int socketId, string originalData)
        {
            _receivedData(logger, socketId, originalData, null);
        }
        public static void FailedToEvaluateMessage(this ILogger logger, int socketId, string originalData)
        {
            _failedToEvaluateMessage(logger, socketId, originalData, null);
        }
        public static void ErrorProcessingMessage(this ILogger logger, int socketId, Exception e)
        {
            _errorProcessingMessage(logger, socketId, e);
        }
        public static void ProcessorMatched(this ILogger logger, int socketId, int count, string listenerId)
        {
            _processorMatched(logger, socketId, count, listenerId, null);
        }
        public static void ReceivedMessageNotRecognized(this ILogger logger, int socketId, int id)
        {
            _receivedMessageNotRecognized(logger, socketId, id, null);
        }
        public static void FailedToDeserializeMessage(this ILogger logger, int socketId, string? errorMessage)
        {
            _failedToDeserializeMessage(logger, socketId, errorMessage, null);
        }
        public static void UserMessageProcessingFailed(this ILogger logger, int socketId, string errorMessage, Exception e)
        {
            _userMessageProcessingFailed(logger, socketId, errorMessage, e);
        }
        public static void MessageProcessed(this ILogger logger, int socketId, long processingTime, long parsingTime)
        {
            _messageProcessed(logger, socketId, processingTime, parsingTime, null);
        }
        public static void ClosingSubscription(this ILogger logger, int socketId, int subscriptionId)
        {
            _closingSubscription(logger, socketId, subscriptionId, null);
        }
        public static void NotUnsubscribingSubscriptionBecauseDuplicateRunning(this ILogger logger, int socketId)
        {
            _notUnsubscribingSubscriptionBecauseDuplicateRunning(logger, socketId, null);
        }
        public static void AlreadyClosing(this ILogger logger, int socketId)
        {
            _alreadyClosing(logger, socketId, null);
        }
        public static void ClosingNoMoreSubscriptions(this ILogger logger, int socketId)
        {
            _closingNoMoreSubscriptions(logger, socketId, null);
        }
        public static void AddingNewSubscription(this ILogger logger, int socketId, int subscriptionId, int userSubscriptionCount)
        {
            _addingNewSubscription(logger, socketId, subscriptionId, userSubscriptionCount, null);
        }

        public static void NothingToResubscribeCloseConnection(this ILogger logger, int socketId)
        {
            _nothingToResubscribeCloseConnection(logger, socketId, null);
        }
        public static void FailedAuthenticationDisconnectAndRecoonect(this ILogger logger, int socketId)
        {
            _failedAuthenticationDisconnectAndRecoonect(logger, socketId, null);
        }
        public static void AuthenticationSucceeded(this ILogger logger, int socketId)
        {
            _authenticationSucceeded(logger, socketId, null);
        }
        public static void FailedRequestRevitalization(this ILogger logger, int socketId, string? errorMessage)
        {
            _failedRequestRevitalization(logger, socketId, errorMessage, null);
        }
        public static void AllSubscriptionResubscribed(this ILogger logger, int socketId)
        {
            _allSubscriptionResubscribed(logger, socketId, null);
        }
        public static void SubscriptionUnsubscribed(this ILogger logger, int socketId, int subscriptionId)
        {
            _subscriptionUnsubscribed(logger, socketId, subscriptionId, null);
        }
        public static void SendingPeriodic(this ILogger logger, int socketId, string identifier)
        {
            _sendingPeriodic(logger, socketId, identifier, null);
        }
        public static void PeriodicSendFailed(this ILogger logger, int socketId, string identifier, string errorMessage, Exception e)
        {
            _periodicSendFailed(logger, socketId, identifier, errorMessage, e);
        }

        public static void SendingData(this ILogger logger, int socketId, int requestId, string data)
        {
            _sendingData(logger, socketId, requestId, data, null);
        }

        public static void ReceivedMessageNotMatchedToAnyListener(this ILogger logger, int socketId, string listenId, string listenIds)
        {
            _receivedMessageNotMatchedToAnyListener(logger, socketId, listenId, listenIds, null);
        }
    }
}
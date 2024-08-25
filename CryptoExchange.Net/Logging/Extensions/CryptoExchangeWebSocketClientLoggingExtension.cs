using Microsoft.Extensions.Logging;
using System;

namespace CryptoExchange.Net.Logging.Extensions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class CryptoExchangeWebSocketClientLoggingExtension
    {
        private static readonly Action<ILogger, int, Exception?> _connecting;
        private static readonly Action<ILogger, int, string, Exception?> _connectionFailed;
        private static readonly Action<ILogger, int, Uri, Exception?> _connected;
        private static readonly Action<ILogger, int, Exception?> _startingProcessing;
        private static readonly Action<ILogger, int, Exception?> _finishedProcessing;
        private static readonly Action<ILogger, int, Exception?> _attemptReconnect;
        private static readonly Action<ILogger, int, Uri, Exception?> _setReconnectUri;
        private static readonly Action<ILogger, int, int, int, Exception?> _addingBytesToSendBuffer;
        private static readonly Action<ILogger, int, Exception?> _reconnectRequested;
        private static readonly Action<ILogger, int, Exception?> _closeAsyncWaitingForExistingCloseTask;
        private static readonly Action<ILogger, int, Exception?> _closeAsyncSocketNotOpen;
        private static readonly Action<ILogger, int, Exception?> _closing;
        private static readonly Action<ILogger, int, Exception?> _closed;
        private static readonly Action<ILogger, int, Exception?> _disposing;
        private static readonly Action<ILogger, int, Exception?> _disposed;
        private static readonly Action<ILogger, int, int, int, Exception?> _sentBytes;
        private static readonly Action<ILogger, int, string, Exception?> _sendLoopStoppedWithException;
        private static readonly Action<ILogger, int, Exception?> _sendLoopFinished;
        private static readonly Action<ILogger, int, string, string ,Exception?> _receivedCloseMessage;
        private static readonly Action<ILogger, int, string, string ,Exception?> _receivedCloseConfirmation;
        private static readonly Action<ILogger, int, int, Exception?> _receivedPartialMessage;
        private static readonly Action<ILogger, int, int, Exception?> _receivedSingleMessage;
        private static readonly Action<ILogger, int, long, Exception?> _reassembledMessage;
        private static readonly Action<ILogger, int, long, Exception?> _discardIncompleteMessage;
        private static readonly Action<ILogger, int, Exception?> _receiveLoopStoppedWithException;
        private static readonly Action<ILogger, int, Exception?> _receiveLoopFinished;
        private static readonly Action<ILogger, int, TimeSpan?, Exception?> _startingTaskForNoDataReceivedCheck;
        private static readonly Action<ILogger, int, TimeSpan?, Exception?> _noDataReceiveTimoutReconnect;
        private static readonly Action<ILogger, int, string, string, Exception?> _socketProcessingStateChanged;

        static CryptoExchangeWebSocketClientLoggingExtension()
        {
            _connecting = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1000, "Connecting"),
                "[Sckt {SocketId}] connecting");

            _connectionFailed = LoggerMessage.Define<int, string>(
                LogLevel.Error,
                new EventId(1001, "ConnectionFailed"),
                "[Sckt {SocketId}] connection failed: {ErrorMessage}");

            _connected = LoggerMessage.Define<int, Uri?>(
                LogLevel.Debug,
                new EventId(1002, "Connected"),
                "[Sckt {SocketId}] connected to {Uri}");

            _startingProcessing = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1003, "StartingProcessing"),
                "[Sckt {SocketId}] starting processing tasks");

            _finishedProcessing = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1004, "FinishedProcessing"),
                "[Sckt {SocketId}] processing tasks finished");

            _attemptReconnect = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1005, "AttemptReconnect"),
                "[Sckt {SocketId}] attempting to reconnect");

            _setReconnectUri = LoggerMessage.Define<int, Uri>(
                LogLevel.Debug,
                new EventId(1006, "SetReconnectUri"),
                "[Sckt {SocketId}] reconnect URI set to {ReconnectUri}");

            _addingBytesToSendBuffer = LoggerMessage.Define<int, int, int>(
                LogLevel.Trace,
                new EventId(1007, "AddingBytesToSendBuffer"),
                "[Sckt {SocketId}] [Req {RequestId}] adding {NumBytes} bytes to send buffer");

            _reconnectRequested = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1008, "ReconnectRequested"),
                "[Sckt {SocketId}] reconnect requested");

            _closeAsyncWaitingForExistingCloseTask = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1009, "CloseAsyncWaitForExistingCloseTask"),
                "[Sckt {SocketId}] CloseAsync() waiting for existing close task");

            _closeAsyncSocketNotOpen = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1010, "CloseAsyncSocketNotOpen"),
                "[Sckt {SocketId}] CloseAsync() socket not open");

            _closing = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1011, "Closing"),
               "[Sckt {SocketId}] closing");

            _closed = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1012, "Closed"),
                "[Sckt {SocketId}] closed");

            _disposing = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1013, "Disposing"),
                "[Sckt {SocketId}] disposing");

            _disposed = LoggerMessage.Define<int>(
                LogLevel.Trace,
                new EventId(1014, "Disposed"),
                "[Sckt {SocketId}] disposed");

            _sentBytes = LoggerMessage.Define<int, int, int>(
                LogLevel.Trace,
                new EventId(1016, "SentBytes"),
                "[Sckt {SocketId}] [Req {RequestId}] sent {NumBytes} bytes");

            _sendLoopStoppedWithException = LoggerMessage.Define<int, string>(
                LogLevel.Warning,
                new EventId(1017, "SendLoopStoppedWithException"),
                "[Sckt {SocketId}] send loop stopped with exception: {ErrorMessage}");

            _sendLoopFinished = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1018, "SendLoopFinished"),
                "[Sckt {SocketId}] send loop finished");

            _receivedCloseMessage = LoggerMessage.Define<int, string, string>(
                LogLevel.Debug,
                new EventId(1019, "ReceivedCloseMessage"),
                "[Sckt {SocketId}] received `Close` message, CloseStatus: {CloseStatus}, CloseStatusDescription: {CloseStatusDescription}");

            _receivedPartialMessage = LoggerMessage.Define<int, int>(
                LogLevel.Trace,
                new EventId(1020, "ReceivedPartialMessage"),
                "[Sckt {SocketId}] received {NumBytes} bytes in partial message");

            _receivedSingleMessage = LoggerMessage.Define<int, int>(
                LogLevel.Trace,
                new EventId(1021, "ReceivedSingleMessage"),
                "[Sckt {SocketId}] received {NumBytes} bytes in single message");

            _reassembledMessage = LoggerMessage.Define<int, long>(
                LogLevel.Trace,
                new EventId(1022, "ReassembledMessage"),
                "[Sckt {SocketId}] reassembled message of {NumBytes} bytes");

            _discardIncompleteMessage = LoggerMessage.Define<int, long>(
                LogLevel.Trace,
                new EventId(1023, "DiscardIncompleteMessage"),
                "[Sckt {SocketId}] discarding incomplete message of {NumBytes} bytes");

            _receiveLoopStoppedWithException = LoggerMessage.Define<int>(
                LogLevel.Error,
                new EventId(1024, "ReceiveLoopStoppedWithException"),
                "[Sckt {SocketId}] receive loop stopped with exception");

            _receiveLoopFinished = LoggerMessage.Define<int>(
                LogLevel.Debug,
                new EventId(1025, "ReceiveLoopFinished"),
                "[Sckt {SocketId}] receive loop finished");

            _startingTaskForNoDataReceivedCheck = LoggerMessage.Define<int, TimeSpan?>(
                LogLevel.Debug,
                new EventId(1026, "StartingTaskForNoDataReceivedCheck"),
                "[Sckt {SocketId}] starting task checking for no data received for {Timeout}");

            _noDataReceiveTimoutReconnect = LoggerMessage.Define<int, TimeSpan?>(
                LogLevel.Debug,
                new EventId(1027, "NoDataReceiveTimeoutReconnect"),
                "[Sckt {SocketId}] no data received for {Timeout}, reconnecting socket");

            _receivedCloseConfirmation = LoggerMessage.Define<int, string, string>(
                LogLevel.Debug,
                new EventId(1028, "ReceivedCloseMessage"),
                "[Sckt {SocketId}] received `Close` message confirming our close request, CloseStatus: {CloseStatus}, CloseStatusDescription: {CloseStatusDescription}");

            _socketProcessingStateChanged = LoggerMessage.Define<int, string, string>(
                LogLevel.Trace,
                new EventId(1028, "SocketProcessingStateChanged"),
                "[Sckt {Id}] processing state change: {PreviousState} -> {NewState}");

        }

        public static void SocketConnecting(
            this ILogger logger, int socketId)
        {
            _connecting(logger, socketId, null);
        }

        public static void SocketConnectionFailed(
            this ILogger logger, int socketId, string message, Exception e)
        {
            _connectionFailed(logger, socketId, message, e);
        }

        public static void SocketConnected(
            this ILogger logger, int socketId, Uri uri)
        {
            _connected(logger, socketId, uri, null);
        }

        public static void SocketStartingProcessing(
            this ILogger logger, int socketId)
        {
            _startingProcessing(logger, socketId, null);
        }

        public static void SocketFinishedProcessing(
            this ILogger logger, int socketId)
        {
            _finishedProcessing(logger, socketId, null);
        }

        public static void SocketAttemptReconnect(
            this ILogger logger, int socketId)
        {
            _attemptReconnect(logger, socketId, null);
        }

        public static void SocketSetReconnectUri(
            this ILogger logger, int socketId, Uri uri)
        {
            _setReconnectUri(logger, socketId, uri, null);
        }

        public static void SocketAddingBytesToSendBuffer(
            this ILogger logger, int socketId, int requestId, byte[] bytes)
        {
            _addingBytesToSendBuffer(logger, socketId, requestId, bytes.Length, null);
        }

        public static void SocketReconnectRequested(
            this ILogger logger, int socketId)
        {
            _reconnectRequested(logger, socketId, null);
        }

        public static void SocketCloseAsyncWaitingForExistingCloseTask(
            this ILogger logger, int socketId)
        {
            _closeAsyncWaitingForExistingCloseTask(logger, socketId, null);
        }

        public static void SocketCloseAsyncSocketNotOpen(
            this ILogger logger, int socketId)
        {
            _closeAsyncSocketNotOpen(logger, socketId, null);
        }

        public static void SocketClosing(
            this ILogger logger, int socketId)
        {
            _closing(logger, socketId, null);
        }

        public static void SocketClosed(
            this ILogger logger, int socketId)
        {
            _closed(logger, socketId, null);
        }

        public static void SocketDisposing(
            this ILogger logger, int socketId)
        {
            _disposing(logger, socketId, null);
        }

        public static void SocketDisposed(
            this ILogger logger, int socketId)
        {
            _disposed(logger, socketId, null);
        }

        public static void SocketSentBytes(
            this ILogger logger, int socketId, int requestId, int numBytes)
        {
            _sentBytes(logger, socketId, requestId, numBytes, null);
        }

        public static void SocketSendLoopStoppedWithException(
            this ILogger logger, int socketId, string message, Exception e)
        {
            _sendLoopStoppedWithException(logger, socketId, message, e);
        }

        public static void SocketSendLoopFinished(
            this ILogger logger, int socketId)
        {
            _sendLoopFinished(logger, socketId, null);
        }

        public static void SocketReceivedCloseMessage(
            this ILogger logger, int socketId, string webSocketCloseStatus, string closeStatusDescription)
        {
            _receivedCloseMessage(logger, socketId, webSocketCloseStatus, closeStatusDescription, null);
        }

        public static void SocketReceivedCloseConfirmation(
            this ILogger logger, int socketId, string webSocketCloseStatus, string closeStatusDescription)
        {
            _receivedCloseConfirmation(logger, socketId, webSocketCloseStatus, closeStatusDescription, null);
        }

        public static void SocketReceivedPartialMessage(
            this ILogger logger, int socketId, int countBytes)
        {
            _receivedPartialMessage(logger, socketId, countBytes, null);
        }

        public static void SocketReceivedSingleMessage(
            this ILogger logger, int socketId, int countBytes)
        {
            _receivedSingleMessage(logger, socketId, countBytes, null);
        }

        public static void SocketReassembledMessage(
            this ILogger logger, int socketId, long countBytes)
        {
            _reassembledMessage(logger, socketId, countBytes, null);
        }

        public static void SocketDiscardIncompleteMessage(
            this ILogger logger, int socketId, long countBytes)
        {
            _discardIncompleteMessage(logger, socketId, countBytes, null);
        }

        public static void SocketReceiveLoopStoppedWithException(
            this ILogger logger, int socketId, Exception e)
        {
            _receiveLoopStoppedWithException(logger, socketId, e);
        }

        public static void SocketReceiveLoopFinished(
            this ILogger logger, int socketId)
        {
            _receiveLoopFinished(logger, socketId, null);
        }

        public static void SocketStartingTaskForNoDataReceivedCheck(
            this ILogger logger, int socketId, TimeSpan? timeSpan)
        {
            _startingTaskForNoDataReceivedCheck(logger, socketId, timeSpan, null);
        }

        public static void SocketNoDataReceiveTimoutReconnect(
            this ILogger logger, int socketId, TimeSpan? timeSpan)
        {
            _noDataReceiveTimoutReconnect(logger, socketId, timeSpan, null);
        }

        public static void SocketProcessingStateChanged(
            this ILogger logger, int socketId, string prevState, string newState)
        {
            _socketProcessingStateChanged(logger, socketId, prevState, newState, null);
        }
    }
}

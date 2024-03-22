using System;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Logging.Extensions
{
    internal static class SymbolOrderBookLoggingExtensions
    {
        private static readonly Action<ILogger, string, string, OrderBookStatus, OrderBookStatus, Exception?> _orderBookStatusChanged;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookStarting;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookStoppedStarting;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookStopping;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookStopped;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookConnectionLost;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookDisconnected;
        private static readonly Action<ILogger, string, int, Exception?> _orderBookProcessingBufferedUpdates;
        private static readonly Action<ILogger, string, string, long, long, Exception?> _orderBookUpdateSkipped;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookOutOfSyncChecksum;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookResyncFailed;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookResyncing;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookResynced;
        private static readonly Action<ILogger, string, Exception?> _orderBookMessageSkippedBecauseOfResubscribing;
        private static readonly Action<ILogger, string, string, long, long, long, Exception?> _orderBookDataSet;
        private static readonly Action<ILogger, string, string, long, long, long, long, Exception?> _orderBookUpdateBuffered;
        private static readonly Action<ILogger, string, string, decimal, decimal, Exception?> _orderBookOutOfSyncDetected;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookReconnectingSocket;
        private static readonly Action<ILogger, string, string, long, long, Exception?> _orderBookSkippedMessage;
        private static readonly Action<ILogger, string, string, long, long, Exception?> _orderBookProcessedMessage;
        private static readonly Action<ILogger, string, string, long, long, Exception?> _orderBookOutOfSync;

        static SymbolOrderBookLoggingExtensions()
        {
            _orderBookStatusChanged = LoggerMessage.Define<string, string, OrderBookStatus, OrderBookStatus>(
                LogLevel.Information,
                new EventId(5000, "OrderBookStatusChanged"),
                "{Id} order book {Symbol} status changed: {PreviousStatus} => {NewStatus}");

            _orderBookStarting = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(5001, "OrderBookStarting"),
                "{Id} order book {Symbol} starting");

            _orderBookStoppedStarting = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(5002, "OrderBookStoppedStarting"),
                "{Id} order book {Symbol} stopped while starting");

            _orderBookConnectionLost = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5003, "OrderBookConnectionLost"),
                "{Id} order book {Symbol} connection lost");

            _orderBookDisconnected = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5004, "OrderBookDisconnected"),
                "{Id} order book {Symbol} disconnected");

            _orderBookStopping = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(5005, "OrderBookStopping"),
                "{Id} order book {Symbol} stopping");


            _orderBookStopped = LoggerMessage.Define<string, string>(
                LogLevel.Trace,
                new EventId(5006, "OrderBookStopped"),
                "{Id} order book {Symbol} stopped");

            _orderBookProcessingBufferedUpdates = LoggerMessage.Define<string, int>(
                LogLevel.Debug,
                new EventId(5007, "OrderBookProcessingBufferedUpdates"),
                "{Id} Processing {NumberBufferedUpdated} buffered updates");

            _orderBookUpdateSkipped = LoggerMessage.Define<string, string, long, long>(
                LogLevel.Debug,
                new EventId(5008, "OrderBookUpdateSkipped"),
                "{Id} order book {Symbol} update skipped #{SequenceNumber}, currently at #{LastSequenceNumber}");

            _orderBookOutOfSync = LoggerMessage.Define<string, string, long, long>(
                LogLevel.Warning,
                new EventId(5009, "OrderBookOutOfSync"),
                "{Id} order book {Symbol} out of sync (expected {ExpectedSequenceNumber}, was {SequenceNumber}), reconnecting");

            _orderBookResynced = LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(5010, "OrderBookResynced"),
                "{Id} order book {Symbol} successfully resynchronized");

            _orderBookMessageSkippedBecauseOfResubscribing = LoggerMessage.Define<string>(
                LogLevel.Trace,
                new EventId(5011, "OrderBookMessageSkippedResubscribing"),
                "{Id} Skipping message because of resubscribing");

            _orderBookDataSet = LoggerMessage.Define<string, string, long, long, long>(
                LogLevel.Debug,
                new EventId(5012, "OrderBookDataSet"),
                "{Id} order book {Symbol} data set: {BidCount} bids, {AskCount} asks. #{EndUpdateId}");

            _orderBookUpdateBuffered = LoggerMessage.Define<string, string, long, long, long, long>(
                LogLevel.Trace,
                new EventId(5013, "OrderBookUpdateBuffered"),
                "{Id} order book {Symbol} update buffered #{StartUpdateId}-#{EndUpdateId} [{AsksCount} asks, {BidsCount} bids]");

            _orderBookOutOfSyncDetected = LoggerMessage.Define<string, string, decimal, decimal>(
                LogLevel.Warning,
                new EventId(5014, "OrderBookOutOfSyncDetected"),
                "{Id} order book {Symbol} detected out of sync order book. First ask: {FirstAsk}, first bid: {FirstBid}. Resyncing");

            _orderBookReconnectingSocket = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5015, "OrderBookReconnectingSocket"),
                "{Id} order book {Symbol} out of sync. Reconnecting socket");

            _orderBookResyncing = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5016, "OrderBookResyncing"),
                "{Id} order book {Symbol} out of sync. Resyncing");

            _orderBookResyncFailed = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5017, "OrderBookResyncFailed"),
                "{Id} order book {Symbol} resync failed, reconnecting socket");

            _orderBookSkippedMessage = LoggerMessage.Define<string, string, long, long>(
                LogLevel.Trace,
                new EventId(5018, "OrderBookSkippedMessage"),
                "{Id} order book {Symbol} update skipped #{FirstUpdateId}-{LastUpdateId}");

            _orderBookProcessedMessage = LoggerMessage.Define<string, string, long, long>(
                LogLevel.Trace,
                new EventId(5019, "OrderBookProcessedMessage"),
                "{Id} order book {Symbol} update processed #{FirstUpdateId}-{LastUpdateId}");

            _orderBookOutOfSyncChecksum = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5020, "OrderBookOutOfSyncChecksum"),
                "{Id} order book {Symbol} out of sync. Checksum mismatch, resyncing");
        }

        public static void OrderBookStatusChanged(this ILogger logger, string id, string symbol, OrderBookStatus previousStatus, OrderBookStatus newStatus)
        {
            _orderBookStatusChanged(logger, id, symbol, previousStatus, newStatus, null);
        }
        public static void OrderBookStarting(this ILogger logger, string id, string symbol)
        {
            _orderBookStarting(logger, id, symbol, null);
        }
        public static void OrderBookStoppedStarting(this ILogger logger, string id, string symbol)
        {
            _orderBookStoppedStarting(logger, id, symbol, null);
        }
        public static void OrderBookConnectionLost(this ILogger logger, string id, string symbol)
        {
            _orderBookConnectionLost(logger, id, symbol, null);
        }

        public static void OrderBookDisconnected(this ILogger logger, string id, string symbol)
        {
            _orderBookDisconnected(logger, id, symbol, null);
        }

        public static void OrderBookStopping(this ILogger logger, string id, string symbol)
        {
            _orderBookStopping(logger, id, symbol, null);
        }

        public static void OrderBookStopped(this ILogger logger, string id, string symbol)
        {
            _orderBookStopped(logger, id, symbol, null);
        }

        public static void OrderBookProcessingBufferedUpdates(this ILogger logger, string id, int numberBufferedUpdated)
        {
            _orderBookProcessingBufferedUpdates(logger, id, numberBufferedUpdated, null);
        }

        public static void OrderBookUpdateSkipped(this ILogger logger, string id, string symbol, long sequence, long lastSequenceNumber)
        {
            _orderBookUpdateSkipped(logger, id, symbol, sequence, lastSequenceNumber, null);
        }

        public static void OrderBookOutOfSync(this ILogger logger, string id, string symbol, long expectedSequenceNumber, long sequenceNumber)
        {
            _orderBookOutOfSync(logger, id, symbol, expectedSequenceNumber, sequenceNumber, null);
        }

        public static void OrderBookResynced(this ILogger logger, string id, string symbol)
        {
            _orderBookResynced(logger, id, symbol, null);
        }

        public static void OrderBookMessageSkippedResubscribing(this ILogger logger, string id)
        {
            _orderBookMessageSkippedBecauseOfResubscribing(logger, id, null);
        }
        public static void OrderBookDataSet(this ILogger logger, string id, string symbol, long bidCount, long askCount, long endUpdateId)
        {
            _orderBookDataSet(logger, id, symbol, bidCount, askCount, endUpdateId, null);
        }
        public static void OrderBookUpdateBuffered(this ILogger logger, string id, string symbol, long startUpdateId, long endUpdateId, long asksCount, long bidsCount)
        {
            _orderBookUpdateBuffered(logger, id, symbol, startUpdateId, endUpdateId, asksCount, bidsCount, null);
        }
        public static void OrderBookOutOfSyncDetected(this ILogger logger, string id, string symbol, decimal firstAsk, decimal firstBid)
        {
            _orderBookOutOfSyncDetected(logger, id, symbol, firstAsk, firstBid, null);
        }

        public static void OrderBookReconnectingSocket(this ILogger logger, string id, string symbol)
        {
            _orderBookReconnectingSocket(logger, id, symbol, null);
        }

        public static void OrderBookResyncing(this ILogger logger, string id, string symbol)
        {
            _orderBookResyncing(logger, id, symbol, null);
        }
        public static void OrderBookResyncFailed(this ILogger logger, string id, string symbol)
        {
            _orderBookResyncFailed(logger, id, symbol, null);
        }
        public static void OrderBookSkippedMessage(this ILogger logger, string id, string symbol, long firstUpdateId, long lastUpdateId)
        {
            _orderBookSkippedMessage(logger, id, symbol, firstUpdateId, lastUpdateId, null);
        }
        public static void OrderBookProcessedMessage(this ILogger logger, string id, string symbol, long firstUpdateId, long lastUpdateId)
        {
            _orderBookProcessedMessage(logger, id, symbol, firstUpdateId, lastUpdateId, null);
        }

        public static void OrderBookOutOfSyncChecksum(this ILogger logger, string id, string symbol)
        {
            _orderBookOutOfSyncChecksum(logger, id, symbol, null);
        }
    }
}

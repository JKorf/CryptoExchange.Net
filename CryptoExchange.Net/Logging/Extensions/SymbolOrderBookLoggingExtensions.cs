using System;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Logging.Extensions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public static class SymbolOrderBookLoggingExtensions
    {
        private static readonly Action<ILogger, string, string, OrderBookStatus, OrderBookStatus, Exception?> _orderBookStatusChanged;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookStarting;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookStoppedStarting;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookStopping;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookStopped;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookConnectionLost;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookDisconnected;
        private static readonly Action<ILogger, string, string, int, Exception?> _orderBookProcessingBufferedUpdates;
        private static readonly Action<ILogger, string, string, long, long, Exception?> _orderBookUpdateSkipped;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookOutOfSyncChecksum;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookResyncFailed;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookResyncing;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookResynced;
        private static readonly Action<ILogger, string, string, Exception?> _orderBookMessageSkippedBecauseOfResubscribing;
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
                "{Api} order book {Symbol} status changed: {PreviousStatus} => {NewStatus}");

            _orderBookStarting = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(5001, "OrderBookStarting"),
                "{Api} order book {Symbol} starting");

            _orderBookStoppedStarting = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(5002, "OrderBookStoppedStarting"),
                "{Api} order book {Symbol} stopped while starting");

            _orderBookConnectionLost = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5003, "OrderBookConnectionLost"),
                "{Api} order book {Symbol} connection lost");

            _orderBookDisconnected = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5004, "OrderBookDisconnected"),
                "{Api} order book {Symbol} disconnected");

            _orderBookStopping = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(5005, "OrderBookStopping"),
                "{Api} order book {Symbol} stopping");


            _orderBookStopped = LoggerMessage.Define<string, string>(
                LogLevel.Trace,
                new EventId(5006, "OrderBookStopped"),
                "{Api} order book {Symbol} stopped");

            _orderBookProcessingBufferedUpdates = LoggerMessage.Define<string, string, int>(
                LogLevel.Debug,
                new EventId(5007, "OrderBookProcessingBufferedUpdates"),
                "{Api} order book {Symbol} Processing {NumberBufferedUpdated} buffered updates");

            _orderBookUpdateSkipped = LoggerMessage.Define<string, string, long, long>(
                LogLevel.Debug,
                new EventId(5008, "OrderBookUpdateSkipped"),
                "{Api} order book {Symbol} update skipped #{SequenceNumber}, currently at #{LastSequenceNumber}");

            _orderBookOutOfSync = LoggerMessage.Define<string, string, long, long>(
                LogLevel.Warning,
                new EventId(5009, "OrderBookOutOfSync"),
                "{Api} order book {Symbol} out of sync (expected {ExpectedSequenceNumber}, was {SequenceNumber}), reconnecting");

            _orderBookResynced = LoggerMessage.Define<string, string>(
                LogLevel.Information,
                new EventId(5010, "OrderBookResynced"),
                "{Api} order book {Symbol} successfully resynchronized");

            _orderBookMessageSkippedBecauseOfResubscribing = LoggerMessage.Define<string, string>(
                LogLevel.Trace,
                new EventId(5011, "OrderBookMessageSkippedResubscribing"),
                "{Api} order book {Symbol} Skipping message because of resubscribing");

            _orderBookDataSet = LoggerMessage.Define<string, string, long, long, long>(
                LogLevel.Debug,
                new EventId(5012, "OrderBookDataSet"),
                "{Api} order book {Symbol} data set: {BidCount} bids, {AskCount} asks. #{EndUpdateId}");

            _orderBookUpdateBuffered = LoggerMessage.Define<string, string, long, long, long, long>(
                LogLevel.Trace,
                new EventId(5013, "OrderBookUpdateBuffered"),
                "{Api} order book {Symbol} update buffered #{StartUpdateId}-#{EndUpdateId} [{AsksCount} asks, {BidsCount} bids]");

            _orderBookOutOfSyncDetected = LoggerMessage.Define<string, string, decimal, decimal>(
                LogLevel.Warning,
                new EventId(5014, "OrderBookOutOfSyncDetected"),
                "{Api} order book {Symbol} detected out of sync order book. First ask: {FirstAsk}, first bid: {FirstBid}. Resyncing");

            _orderBookReconnectingSocket = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5015, "OrderBookReconnectingSocket"),
                "{Api} order book {Symbol} out of sync. Reconnecting socket");

            _orderBookResyncing = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5016, "OrderBookResyncing"),
                "{Api} order book {Symbol} out of sync. Resyncing");

            _orderBookResyncFailed = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5017, "OrderBookResyncFailed"),
                "{Api} order book {Symbol} resync failed, reconnecting socket");

            _orderBookSkippedMessage = LoggerMessage.Define<string, string, long, long>(
                LogLevel.Trace,
                new EventId(5018, "OrderBookSkippedMessage"),
                "{Api} order book {Symbol} update skipped #{FirstUpdateId}-{LastUpdateId}");

            _orderBookProcessedMessage = LoggerMessage.Define<string, string, long, long>(
                LogLevel.Trace,
                new EventId(5019, "OrderBookProcessedMessage"),
                "{Api} order book {Symbol} update processed #{FirstUpdateId}-{LastUpdateId}");

            _orderBookOutOfSyncChecksum = LoggerMessage.Define<string, string>(
                LogLevel.Warning,
                new EventId(5020, "OrderBookOutOfSyncChecksum"),
                "{Api} order book {Symbol} out of sync. Checksum mismatch, resyncing");
        }

        public static void OrderBookStatusChanged(this ILogger logger, string api, string symbol, OrderBookStatus previousStatus, OrderBookStatus newStatus)
        {
            _orderBookStatusChanged(logger, api, symbol, previousStatus, newStatus, null);
        }
        public static void OrderBookStarting(this ILogger logger, string api, string symbol)
        {
            _orderBookStarting(logger, api, symbol, null);
        }
        public static void OrderBookStoppedStarting(this ILogger logger, string api, string symbol)
        {
            _orderBookStoppedStarting(logger, api, symbol, null);
        }
        public static void OrderBookConnectionLost(this ILogger logger, string api, string symbol)
        {
            _orderBookConnectionLost(logger, api, symbol, null);
        }

        public static void OrderBookDisconnected(this ILogger logger, string api, string symbol)
        {
            _orderBookDisconnected(logger, api, symbol, null);
        }

        public static void OrderBookStopping(this ILogger logger, string api, string symbol)
        {
            _orderBookStopping(logger, api, symbol, null);
        }

        public static void OrderBookStopped(this ILogger logger, string api, string symbol)
        {
            _orderBookStopped(logger, api, symbol, null);
        }

        public static void OrderBookProcessingBufferedUpdates(this ILogger logger, string api, string symbol, int numberBufferedUpdated)
        {
            _orderBookProcessingBufferedUpdates(logger, api, symbol, numberBufferedUpdated, null);
        }

        public static void OrderBookUpdateSkipped(this ILogger logger, string api, string symbol, long sequence, long lastSequenceNumber)
        {
            _orderBookUpdateSkipped(logger, api, symbol, sequence, lastSequenceNumber, null);
        }

        public static void OrderBookOutOfSync(this ILogger logger, string api, string symbol, long expectedSequenceNumber, long sequenceNumber)
        {
            _orderBookOutOfSync(logger, api, symbol, expectedSequenceNumber, sequenceNumber, null);
        }

        public static void OrderBookResynced(this ILogger logger, string api, string symbol)
        {
            _orderBookResynced(logger, api, symbol, null);
        }

        public static void OrderBookMessageSkippedResubscribing(this ILogger logger, string api, string symbol)
        {
            _orderBookMessageSkippedBecauseOfResubscribing(logger, api, symbol, null);
        }
        public static void OrderBookDataSet(this ILogger logger, string api, string symbol, long bidCount, long askCount, long endUpdateId)
        {
            _orderBookDataSet(logger, api, symbol, bidCount, askCount, endUpdateId, null);
        }
        public static void OrderBookUpdateBuffered(this ILogger logger, string api, string symbol, long startUpdateId, long endUpdateId, long asksCount, long bidsCount)
        {
            _orderBookUpdateBuffered(logger, api, symbol, startUpdateId, endUpdateId, asksCount, bidsCount, null);
        }
        public static void OrderBookOutOfSyncDetected(this ILogger logger, string api, string symbol, decimal firstAsk, decimal firstBid)
        {
            _orderBookOutOfSyncDetected(logger, api, symbol, firstAsk, firstBid, null);
        }

        public static void OrderBookReconnectingSocket(this ILogger logger, string api, string symbol)
        {
            _orderBookReconnectingSocket(logger, api, symbol, null);
        }

        public static void OrderBookResyncing(this ILogger logger, string api, string symbol)
        {
            _orderBookResyncing(logger, api, symbol, null);
        }
        public static void OrderBookResyncFailed(this ILogger logger, string api, string symbol)
        {
            _orderBookResyncFailed(logger, api, symbol, null);
        }
        public static void OrderBookSkippedMessage(this ILogger logger, string api, string symbol, long firstUpdateId, long lastUpdateId)
        {
            _orderBookSkippedMessage(logger, api, symbol, firstUpdateId, lastUpdateId, null);
        }
        public static void OrderBookProcessedMessage(this ILogger logger, string api, string symbol, long firstUpdateId, long lastUpdateId)
        {
            _orderBookProcessedMessage(logger, api, symbol, firstUpdateId, lastUpdateId, null);
        }

        public static void OrderBookOutOfSyncChecksum(this ILogger logger, string api, string symbol)
        {
            _orderBookOutOfSyncChecksum(logger, api, symbol, null);
        }
    }
}

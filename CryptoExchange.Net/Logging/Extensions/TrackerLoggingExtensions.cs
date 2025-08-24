using System;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Logging.Extensions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public static class TrackerLoggingExtensions
{
    private static readonly Action<ILogger, string, SyncStatus, SyncStatus, Exception?> _klineTrackerStatusChanged;
    private static readonly Action<ILogger, string, Exception?> _klineTrackerStarting;
    private static readonly Action<ILogger, string, string, Exception?> _klineTrackerStartFailed;
    private static readonly Action<ILogger, string, Exception?> _klineTrackerStarted;
    private static readonly Action<ILogger, string, Exception?> _klineTrackerStopping;
    private static readonly Action<ILogger, string, Exception?> _klineTrackerStopped;
    private static readonly Action<ILogger, string, DateTime, Exception?> _klineTrackerInitialDataSet;
    private static readonly Action<ILogger, string, DateTime, Exception?> _klineTrackerKlineUpdated;
    private static readonly Action<ILogger, string, DateTime, Exception?> _klineTrackerKlineAdded;
    private static readonly Action<ILogger, string, Exception?> _klineTrackerConnectionLost;
    private static readonly Action<ILogger, string, Exception?> _klineTrackerConnectionClosed;
    private static readonly Action<ILogger, string, Exception?> _klineTrackerConnectionRestored;

    private static readonly Action<ILogger, string, SyncStatus, SyncStatus, Exception?> _tradeTrackerStatusChanged;
    private static readonly Action<ILogger, string, Exception?> _tradeTrackerStarting;
    private static readonly Action<ILogger, string, string, Exception?> _tradeTrackerStartFailed;
    private static readonly Action<ILogger, string, Exception?> _tradeTrackerStarted;
    private static readonly Action<ILogger, string, Exception?> _tradeTrackerStopping;
    private static readonly Action<ILogger, string, Exception?> _tradeTrackerStopped;
    private static readonly Action<ILogger, string, int, long, Exception?> _tradeTrackerInitialDataSet;
    private static readonly Action<ILogger, string, long, Exception?> _tradeTrackerPreSnapshotSkip;
    private static readonly Action<ILogger, string, long, Exception?> _tradeTrackerPreSnapshotApplied;
    private static readonly Action<ILogger, string, long, Exception?> _tradeTrackerTradeAdded;
    private static readonly Action<ILogger, string, Exception?> _tradeTrackerConnectionLost;
    private static readonly Action<ILogger, string, Exception?> _tradeTrackerConnectionClosed;
    private static readonly Action<ILogger, string, Exception?> _tradeTrackerConnectionRestored;

    static TrackerLoggingExtensions()
    {
        _klineTrackerStatusChanged = LoggerMessage.Define<string, SyncStatus, SyncStatus>(
            LogLevel.Debug,
            new EventId(6001, "KlineTrackerStatusChanged"),
            "Kline tracker for {Symbol} status changed: {OldStatus} => {NewStatus}");

        _klineTrackerStarting = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(6002, "KlineTrackerStarting"),
            "Kline tracker for {Symbol} starting");

        _klineTrackerStartFailed = LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(6003, "KlineTrackerStartFailed"),
            "Kline tracker for {Symbol} failed to start: {Error}");

        _klineTrackerStarted = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(6004, "KlineTrackerStarted"),
            "Kline tracker for {Symbol} started");

        _klineTrackerStopping = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(6005, "KlineTrackerStopping"),
            "Kline tracker for {Symbol} stopping");

        _klineTrackerStopped = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(6006, "KlineTrackerStopped"),
            "Kline tracker for {Symbol} stopped");

        _klineTrackerInitialDataSet = LoggerMessage.Define<string, DateTime>(
            LogLevel.Debug,
            new EventId(6007, "KlineTrackerInitialDataSet"),
            "Kline tracker for {Symbol} initial data set, last timestamp: {LastTime}");

        _klineTrackerKlineUpdated = LoggerMessage.Define<string, DateTime>(
            LogLevel.Trace,
            new EventId(6008, "KlineTrackerKlineUpdated"),
            "Kline tracker for {Symbol} kline updated for open time: {LastTime}");

        _klineTrackerKlineAdded = LoggerMessage.Define<string, DateTime>(
            LogLevel.Trace,
            new EventId(6009, "KlineTrackerKlineAdded"),
            "Kline tracker for {Symbol} new kline for open time: {LastTime}");

        _klineTrackerConnectionLost = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(6010, "KlineTrackerConnectionLost"),
            "Kline tracker for {Symbol} connection lost");

        _klineTrackerConnectionClosed = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(6011, "KlineTrackerConnectionClosed"),
            "Kline tracker for {Symbol} disconnected");

        _klineTrackerConnectionRestored = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(6012, "KlineTrackerConnectionRestored"),
            "Kline tracker for {Symbol} successfully resynchronized");

        _tradeTrackerStatusChanged = LoggerMessage.Define<string, SyncStatus, SyncStatus>(
            LogLevel.Debug,
            new EventId(6013, "KlineTrackerStatusChanged"),
            "Trade tracker for {Symbol} status changed: {OldStatus} => {NewStatus}");

        _tradeTrackerStarting = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(6014, "KlineTrackerStarting"),
            "Trade tracker for {Symbol} starting");

        _tradeTrackerStartFailed = LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(6015, "KlineTrackerStartFailed"),
            "Trade tracker for {Symbol} failed to start: {Error}");

        _tradeTrackerStarted = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(6016, "KlineTrackerStarted"),
            "Trade tracker for {Symbol} started");

        _tradeTrackerStopping = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(6017, "KlineTrackerStopping"),
            "Trade tracker for {Symbol} stopping");

        _tradeTrackerStopped = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(6018, "KlineTrackerStopped"),
            "Trade tracker for {Symbol} stopped");

        _tradeTrackerInitialDataSet = LoggerMessage.Define<string, int, long>(
            LogLevel.Debug,
            new EventId(6019, "TradeTrackerInitialDataSet"),
            "Trade tracker for {Symbol} snapshot set, Count: {Count}, Last id: {LastId}");

        _tradeTrackerPreSnapshotSkip = LoggerMessage.Define<string, long>(
            LogLevel.Trace,
            new EventId(6020, "TradeTrackerPreSnapshotSkip"),
            "Trade tracker for {Symbol} skipping {Id}, already in snapshot");

        _tradeTrackerPreSnapshotApplied = LoggerMessage.Define<string, long>(
            LogLevel.Trace,
            new EventId(6021, "TradeTrackerPreSnapshotApplied"),
            "Trade tracker for {Symbol} adding {Id} from pre-snapshot");

        _tradeTrackerTradeAdded = LoggerMessage.Define<string, long>(
            LogLevel.Trace,
            new EventId(6022, "TradeTrackerTradeAdded"),
            "Trade tracker for {Symbol} adding trade {Id}");

        _tradeTrackerConnectionLost = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(6023, "TradeTrackerConnectionLost"),
            "Trade tracker for {Symbol} connection lost");

        _tradeTrackerConnectionClosed = LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(6024, "TradeTrackerConnectionClosed"),
            "Trade tracker for {Symbol} disconnected");

        _tradeTrackerConnectionRestored = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(6025, "TradeTrackerConnectionRestored"),
            "Trade tracker for {Symbol} successfully resynchronized");
    }

    public static void KlineTrackerStatusChanged(this ILogger logger, string symbol, SyncStatus oldStatus, SyncStatus newStatus)
    {
        _klineTrackerStatusChanged(logger, symbol, oldStatus, newStatus, null);
    }

    public static void KlineTrackerStarting(this ILogger logger, string symbol)
    {
        _klineTrackerStarting(logger, symbol, null);
    }

    public static void KlineTrackerStartFailed(this ILogger logger, string symbol, string error, Exception? exception)
    {
        _klineTrackerStartFailed(logger, symbol, error, exception);
    }

    public static void KlineTrackerStarted(this ILogger logger, string symbol)
    {
        _klineTrackerStarted(logger, symbol, null);
    }

    public static void KlineTrackerStopping(this ILogger logger, string symbol)
    {
        _klineTrackerStopping(logger, symbol, null);
    }

    public static void KlineTrackerStopped(this ILogger logger, string symbol)
    {
        _klineTrackerStopped(logger, symbol, null);
    }

    public static void KlineTrackerInitialDataSet(this ILogger logger, string symbol, DateTime lastTime)
    {
        _klineTrackerInitialDataSet(logger, symbol, lastTime, null);
    }

    public static void KlineTrackerKlineUpdated(this ILogger logger, string symbol, DateTime lastTime)
    {
        _klineTrackerKlineUpdated(logger, symbol, lastTime, null);
    }

    public static void KlineTrackerKlineAdded(this ILogger logger, string symbol, DateTime lastTime)
    {
        _klineTrackerKlineAdded(logger, symbol, lastTime, null);
    }

    public static void KlineTrackerConnectionLost(this ILogger logger, string symbol)
    {
        _klineTrackerConnectionLost(logger, symbol, null);
    }

    public static void KlineTrackerConnectionClosed(this ILogger logger, string symbol)
    {
        _klineTrackerConnectionClosed(logger, symbol, null);
    }

    public static void KlineTrackerConnectionRestored(this ILogger logger, string symbol)
    {
        _klineTrackerConnectionRestored(logger, symbol, null);
    }

    public static void TradeTrackerStatusChanged(this ILogger logger, string symbol, SyncStatus oldStatus, SyncStatus newStatus)
    {
        _tradeTrackerStatusChanged(logger, symbol, oldStatus, newStatus, null);
    }

    public static void TradeTrackerStarting(this ILogger logger, string symbol)
    {
        _tradeTrackerStarting(logger, symbol, null);
    }

    public static void TradeTrackerStartFailed(this ILogger logger, string symbol, string error, Exception? ex)
    {
        _tradeTrackerStartFailed(logger, symbol, error, ex);
    }

    public static void TradeTrackerStarted(this ILogger logger, string symbol)
    {
        _tradeTrackerStarted(logger, symbol, null);
    }

    public static void TradeTrackerStopping(this ILogger logger, string symbol)
    {
        _tradeTrackerStopping(logger, symbol, null);
    }

    public static void TradeTrackerStopped(this ILogger logger, string symbol)
    {
        _tradeTrackerStopped(logger, symbol, null);
    }

    public static void TradeTrackerInitialDataSet(this ILogger logger, string symbol, int count, long lastId)
    {
        _tradeTrackerInitialDataSet(logger, symbol, count, lastId, null);
    }

    public static void TradeTrackerPreSnapshotSkip(this ILogger logger, string symbol, long lastId)
    {
        _tradeTrackerPreSnapshotSkip(logger, symbol, lastId, null);
    }

    public static void TradeTrackerPreSnapshotApplied(this ILogger logger, string symbol, long lastId)
    {
        _tradeTrackerPreSnapshotApplied(logger, symbol, lastId, null);
    }

    public static void TradeTrackerTradeAdded(this ILogger logger, string symbol, long lastId)
    {
        _tradeTrackerTradeAdded(logger, symbol, lastId, null);
    }

    public static void TradeTrackerConnectionLost(this ILogger logger, string symbol)
    {
        _tradeTrackerConnectionLost(logger, symbol, null);
    }

    public static void TradeTrackerConnectionClosed(this ILogger logger, string symbol)
    {
        _tradeTrackerConnectionClosed(logger, symbol, null);
    }

    public static void TradeTrackerConnectionRestored(this ILogger logger, string symbol)
    {
        _tradeTrackerConnectionRestored(logger, symbol, null);
    }
}

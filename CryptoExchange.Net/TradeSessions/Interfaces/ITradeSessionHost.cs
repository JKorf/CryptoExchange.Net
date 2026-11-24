using CryptoExchange.Net.Objects;
using CryptoExchange.Net.TradeSessions;
using CryptoExchange.Net.TradeSessions.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions.Interfaces
{
    /// <summary>
    /// Trade session host
    /// </summary>
    public interface ITradeSessionHost : IAsyncDisposable
    {
        /// <summary>
        /// The exchange the session is for
        /// </summary>
        string Exchange { get; }
        /// <summary>
        /// Whether the session has been started
        /// </summary>
        bool Started { get; }
        /// <summary>
        /// The interval of the update loop
        /// </summary>
        TimeSpan UpdateLoopInterval { get; }

        /// <summary>
        /// Start the registered data providers and optional update loop
        /// </summary>
        Task<CallResult> StartAsync(CancellationToken ct = default);

        /// <summary>
        /// Stop the data providers and optional update loop
        /// </summary>
        Task StopAsync();
    }
}

/// <summary>
/// Trade session host
/// </summary>
/// <typeparam name="TTradeSession"></typeparam>
public interface ITradeSessionHost<out TTradeSession> : ITradeSessionHost
    where TTradeSession : TradeSession
{
    /// <summary>
    /// The trade session instance
    /// </summary>
    TTradeSession TradeSession { get; }
}
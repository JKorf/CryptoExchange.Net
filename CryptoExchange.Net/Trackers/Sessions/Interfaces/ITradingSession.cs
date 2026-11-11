using CryptoExchange.Net.Objects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Trackers.Sessions.Interfaces
{
    /// <summary>
    /// Trading session
    /// </summary>
    public interface ITradingSession : IAsyncDisposable
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
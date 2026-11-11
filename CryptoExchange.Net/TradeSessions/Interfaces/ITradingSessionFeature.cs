using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions.Interfaces
{
    /// <summary>
    /// A trading session feature
    /// </summary>
    public interface ITradingSessionFeature : IAsyncDisposable
    {
        /// <summary>
        /// Start the feature
        /// </summary>
        Task<CallResult> StartAsync(CancellationToken ct);

        /// <summary>
        /// Stop the feature
        /// </summary>
        Task StopAsync();
    }
}

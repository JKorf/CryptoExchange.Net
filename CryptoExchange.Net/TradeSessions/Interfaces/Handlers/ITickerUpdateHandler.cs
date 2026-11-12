using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.TradeSessions.Interfaces.Handlers
{
    /// <summary>
    /// Ticker update handle capability
    /// </summary>
    public interface ITickerUpdateHandler
    {
        /// <summary>
        /// Handle a ticker update
        /// </summary>
        /// <param name="symbol">The symbol for which the ticker update is received</param>
        /// <param name="update">The ticker update data</param>
        void HandleTickerUpdate(SharedSymbol symbol, DataEvent<SharedTicker> update);
    }
}

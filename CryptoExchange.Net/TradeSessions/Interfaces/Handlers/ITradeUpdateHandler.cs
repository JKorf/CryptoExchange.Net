using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.TradeSessions.Interfaces.Handlers
{
    /// <summary>
    /// Trade update handle capability
    /// </summary>
    public interface ITradeUpdateHandler
    {
        /// <summary>
        /// Handle a trade update
        /// </summary>
        /// <param name="symbol">The symbol for which the trade update is received</param>
        /// <param name="update">The trade update data</param>
        void HandleTradeUpdate(SharedSymbol symbol, DataEvent<SharedTrade[]> update);
    }
}

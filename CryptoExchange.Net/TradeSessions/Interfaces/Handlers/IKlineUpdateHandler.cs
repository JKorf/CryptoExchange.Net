using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.TradeSessions.Interfaces
{
    /// <summary>
    /// Kline update handle capability
    /// </summary>
    public interface IKlineUpdateHandler
    {
        /// <summary>
        /// Handle a kline update
        /// </summary>
        /// <param name="symbol">The symbol for which the kline update is received</param>
        /// <param name="update">The kline update data</param>
        void HandleKlineUpdate(SharedSymbol symbol, DataEvent<SharedKline> update);

    }
}

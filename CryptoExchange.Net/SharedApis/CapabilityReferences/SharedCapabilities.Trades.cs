using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public static partial class SharedCapabilities
    {
        /// <summary>
        /// Public trade capabilities.
        /// </summary>
        public static class Trades
        {
            /// <summary>
            /// Get recent trades capability.
            /// </summary>
            public static SharedRestCapability<IGetRecentTrades, IGetRecentTradesRest> GetRecentTrades { get; } = new();

            /// <summary>
            /// Get trade history capability.
            /// </summary>
            public static SharedRestCapability<IGetTradeHistory, IGetTradeHistoryRest> GetTradeHistory { get; } = new();

            /// <summary>
            /// Subscribe to trade updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeTradesSocket> SubscribeTrades { get; } = new();
        }
    }
}

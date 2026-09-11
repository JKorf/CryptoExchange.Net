using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public static partial class SharedCapabilities
    {
        /// <summary>
        /// User trade capabilities.
        /// </summary>
        public static class UserTrades
        {
            /// <summary>
            /// Get trades for a spot order capability.
            /// </summary>
            public static SharedRestCapability<IGetSpotOrderTrades, IGetSpotOrderTradesRest> GetSpotOrderTrades { get; } = new();

            /// <summary>
            /// Get spot user trade history capability.
            /// </summary>
            public static SharedRestCapability<IGetSpotUserTradeHistory, IGetSpotUserTradeHistoryRest> GetSpotTradeHistory { get; } = new();

            /// <summary>
            /// Get trades for a futures order capability.
            /// </summary>
            public static SharedRestCapability<IGetFuturesOrderTrades, IGetFuturesOrderTradesRest> GetFuturesOrderTrades { get; } = new();

            /// <summary>
            /// Get futures user trade history capability.
            /// </summary>
            public static SharedRestCapability<IGetFuturesUserTradeHistory, IGetFuturesUserTradeHistoryRest> GetFuturesTradeHistory { get; } = new();

            /// <summary>
            /// Subscribe to user trade updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeUserTradesSocket> SubscribeUserTrades { get; } = new();
        }
    }
}

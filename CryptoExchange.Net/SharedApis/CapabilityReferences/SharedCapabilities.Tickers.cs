using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Ticker capabilities.
        /// </summary>
        public static class Tickers
        {
            /// <summary>
            /// Get a ticker capability.
            /// </summary>
            public static SharedRestCapability<IGetTicker, IGetTickerRest> GetTicker { get; } = new();

            /// <summary>
            /// Get all tickers capability.
            /// </summary>
            public static SharedRestCapability<IGetAllTickers, IGetAllTickersRest> GetAllTickers { get; } = new();

            /// <summary>
            /// Subscribe to ticker updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeTickerSocket> SubscribeTicker { get; } = new();

            /// <summary>
            /// Subscribe to all ticker updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeAllTickersSocket> SubscribeAllTickers { get; } = new();

            /// <summary>
            /// Subscribe to book ticker updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeBookTickerSocket> SubscribeBookTicker { get; } = new();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Order book capabilities.
        /// </summary>
        public static class OrderBooks
        {
            /// <summary>
            /// Get an order book capability.
            /// </summary>
            public static SharedRestCapability<IGetOrderBook, IGetOrderBookRest> GetOrderBook { get; } = new();

            /// <summary>
            /// Get a book ticker capability.
            /// </summary>
            public static SharedRestCapability<IGetBookTicker, IGetBookTickerRest> GetBookTicker { get; } = new();

            /// <summary>
            /// Subscribe to order book updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeOrderBookSocket> SubscribeOrderBook { get; } = new();

            /// <summary>
            /// Subscribe to incremental order book updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeIncrementalOrderBookSocket> SubscribeIncrementalOrderBook { get; } = new();
        }
    }
}

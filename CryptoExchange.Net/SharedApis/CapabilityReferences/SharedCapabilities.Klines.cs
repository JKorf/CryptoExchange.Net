using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Kline capabilities.
        /// </summary>
        public static class Klines
        {
            /// <summary>
            /// Get klines capability.
            /// </summary>
            public static SharedRestCapability<IGetKlines, IGetKlinesRest> GetKlines { get; } = new();

            /// <summary>
            /// Get mark price klines capability.
            /// </summary>
            public static SharedRestCapability<IGetMarkPriceKlines, IGetMarkPriceKlinesRest> GetMarkPriceKlines { get; } = new();

            /// <summary>
            /// Get index price klines capability.
            /// </summary>
            public static SharedRestCapability<IGetIndexPriceKlines, IGetIndexPriceKlinesRest> GetIndexPriceKlines { get; } = new();

            /// <summary>
            /// Subscribe to kline updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeKlinesSocket> SubscribeKlines { get; } = new();
        }

    }
}

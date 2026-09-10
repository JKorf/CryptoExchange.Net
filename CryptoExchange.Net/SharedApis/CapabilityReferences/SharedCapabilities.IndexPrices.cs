using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Index price capabilities.
        /// </summary>
        public static class IndexPrices
        {
            /// <summary>
            /// Get an index price capability.
            /// </summary>
            public static SharedRestCapability<IGetIndexPrice, IGetIndexPriceRest> GetIndexPrice { get; } = new();
            /// <summary>
            /// Get all index prices capability.
            /// </summary>
            public static SharedRestCapability<IGetAllIndexPrices, IGetAllIndexPricesRest> GetAllIndexPrices { get; } = new();
            /// <summary>
            /// Subscribe to index price updates capability.
            /// </summary>
            public static SharedCapabilityReference<ISubscribeIndexPriceSocket> SubscribeIndexPrice { get; } = new();
        }
    }
}

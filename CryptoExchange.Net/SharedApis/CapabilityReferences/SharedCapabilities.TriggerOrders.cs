using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Trigger order capabilities.
        /// </summary>
        public static class TriggerOrders
        {
            /// <summary>
            /// Spot trigger order capabilities.
            /// </summary>
            public static class Spot
            {
                /// <summary>
                /// Place a spot trigger order capability.
                /// </summary>
                public static SharedRestCapability<IPlaceSpotTriggerOrder, IPlaceSpotTriggerOrderRest> PlaceOrder { get; } = new();

                /// <summary>
                /// Get a spot trigger order capability.
                /// </summary>
                public static SharedRestCapability<IGetSpotTriggerOrder, IGetSpotTriggerOrderRest> GetOrder { get; } = new();

                /// <summary>
                /// Cancel a spot trigger order capability.
                /// </summary>
                public static SharedRestCapability<ICancelSpotTriggerOrder, ICancelSpotTriggerOrderRest> CancelOrder { get; } = new();
            }

            /// <summary>
            /// Futures trigger order capabilities.
            /// </summary>
            public static class Futures
            {
                /// <summary>
                /// Place a futures trigger order capability.
                /// </summary>
                public static SharedRestCapability<IPlaceFuturesTriggerOrder, IPlaceFuturesTriggerOrderRest> PlaceOrder { get; } = new();

                /// <summary>
                /// Get a futures trigger order capability.
                /// </summary>
                public static SharedRestCapability<IGetFuturesTriggerOrder, IGetFuturesTriggerOrderRest> GetOrder { get; } = new();

                /// <summary>
                /// Cancel a futures trigger order capability.
                /// </summary>
                public static SharedRestCapability<ICancelFuturesTriggerOrder, ICancelFuturesTriggerOrderRest> CancelOrder { get; } = new();
            }
        }
    }
}

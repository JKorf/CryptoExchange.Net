using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public static partial class SharedCapabilities
    {
        /// <summary>
        /// Order capabilities.
        /// </summary>
        public static class Orders
        {
            /// <summary>
            /// Spot order capabilities.
            /// </summary>
            public static class Spot
            {
                /// <summary>
                /// Place a spot order capability.
                /// </summary>
                public static SharedRestSocketCapability<IPlaceSpotOrder, IPlaceSpotOrderRest, IPlaceSpotOrderSocket> PlaceOrder { get; } = new();

                /// <summary>
                /// Place multiple spot orders capability.
                /// </summary>
                public static SharedRestSocketCapability<IPlaceMultipleSpotOrders, IPlaceMultipleSpotOrdersRest, IPlaceMultipleSpotOrdersSocket> PlaceMultipleOrders { get; } = new();

                /// <summary>
                /// Edit a spot order capability.
                /// </summary>
                public static SharedRestSocketCapability<IEditSpotOrder, IEditSpotOrderRest, IEditSpotOrderSocket> EditOrder { get; } = new();

                /// <summary>
                /// Edit a spot order by client order id capability.
                /// </summary>
                public static SharedRestSocketCapability<IEditSpotOrderByClientOrderId, IEditSpotOrderByClientOrderIdRest, IEditSpotOrderByClientOrderIdSocket> EditOrderByClientOrderId { get; } = new();

                /// <summary>
                /// Cancel a spot order capability.
                /// </summary>
                public static SharedRestSocketCapability<ICancelSpotOrder, ICancelSpotOrderRest, ICancelSpotOrderSocket> CancelOrder { get; } = new();

                /// <summary>
                /// Cancel a spot order by client order id capability.
                /// </summary>
                public static SharedRestSocketCapability<ICancelSpotOrderByClientOrderId, ICancelSpotOrderByClientOrderIdRest, ICancelSpotOrderByClientOrderIdSocket> CancelOrderByClientOrderId { get; } = new();

                /// <summary>
                /// Cancel all spot orders capability.
                /// </summary>
                public static SharedRestSocketCapability<ICancelAllSpotOrders, ICancelAllSpotOrdersRest, ICancelAllSpotOrdersSocket> CancelAllOrders { get; } = new();

                /// <summary>
                /// Cancel all spot orders for a symbol capability.
                /// </summary>
                public static SharedRestSocketCapability<ICancelAllSpotSymbolOrders, ICancelAllSpotSymbolOrdersRest, ICancelAllSpotSymbolOrdersSocket> CancelAllSymbolOrders { get; } = new();

                /// <summary>
                /// Get a spot order capability.
                /// </summary>
                public static SharedRestCapability<IGetSpotOrder, IGetSpotOrderRest> GetOrder { get; } = new();

                /// <summary>
                /// Get a spot order by client order id capability.
                /// </summary>
                public static SharedRestCapability<IGetSpotOrderByClientOrderId, IGetSpotOrderByClientOrderIdRest> GetOrderByClientOrderId { get; } = new();

                /// <summary>
                /// Get open spot orders capability.
                /// </summary>
                public static SharedRestCapability<IGetOpenSpotOrders, IGetOpenSpotOrdersRest> GetOpenOrders { get; } = new();

                /// <summary>
                /// Get closed spot orders capability.
                /// </summary>
                public static SharedRestCapability<IGetClosedSpotOrders, IGetClosedSpotOrdersRest> GetClosedOrders { get; } = new();

                /// <summary>
                /// Subscribe to spot order updates capability.
                /// </summary>
                public static SharedCapabilityReference<ISubscribeSpotOrdersSocket> SubscribeOrders { get; } = new();
            }

            /// <summary>
            /// Futures order capabilities.
            /// </summary>
            public static class Futures
            {
                /// <summary>
                /// Place a futures order capability.
                /// </summary>
                public static SharedRestSocketCapability<IPlaceFuturesOrder, IPlaceFuturesOrderRest, IPlaceFuturesOrderSocket> PlaceOrder { get; } = new();

                /// <summary>
                /// Place multiple futures orders capability.
                /// </summary>
                public static SharedRestSocketCapability<IPlaceMultipleFuturesOrders, IPlaceMultipleFuturesOrdersRest, IPlaceMultipleFuturesOrdersSocket> PlaceMultipleOrders { get; } = new();

                /// <summary>
                /// Edit a futures order capability.
                /// </summary>
                public static SharedRestSocketCapability<IEditFuturesOrder, IEditFuturesOrderRest, IEditFuturesOrderSocket> EditOrder { get; } = new();

                /// <summary>
                /// Edit a futures order by client order id capability.
                /// </summary>
                public static SharedRestSocketCapability<IEditFuturesOrderByClientOrderId, IEditFuturesOrderByClientOrderIdRest, IEditFuturesOrderByClientOrderIdSocket> EditOrderByClientOrderId { get; } = new();

                /// <summary>
                /// Cancel a futures order capability.
                /// </summary>
                public static SharedRestSocketCapability<ICancelFuturesOrder, ICancelFuturesOrderRest, ICancelFuturesOrderSocket> CancelOrder { get; } = new();

                /// <summary>
                /// Cancel a futures order by client order id capability.
                /// </summary>
                public static SharedRestSocketCapability<ICancelFuturesOrderByClientOrderId, ICancelFuturesOrderByClientOrderIdRest, ICancelFuturesOrderByClientOrderIdSocket> CancelOrderByClientOrderId { get; } = new();

                /// <summary>
                /// Cancel all futures orders capability.
                /// </summary>
                public static SharedRestSocketCapability<ICancelAllFuturesOrders, ICancelAllFuturesOrdersRest, ICancelAllFuturesOrdersSocket> CancelAllOrders { get; } = new();

                /// <summary>
                /// Cancel all futures orders for a symbol capability.
                /// </summary>
                public static SharedRestSocketCapability<ICancelAllFuturesSymbolOrders, ICancelAllFuturesSymbolOrdersRest, ICancelAllFuturesSymbolOrdersSocket> CancelAllSymbolOrders { get; } = new();

                /// <summary>
                /// Get a futures order capability.
                /// </summary>
                public static SharedRestCapability<IGetFuturesOrder, IGetFuturesOrderRest> GetOrder { get; } = new();

                /// <summary>
                /// Get a futures order by client order id capability.
                /// </summary>
                public static SharedRestCapability<IGetFuturesOrderByClientOrderId, IGetFuturesOrderByClientOrderIdRest> GetOrderByClientOrderId { get; } = new();

                /// <summary>
                /// Get open futures orders capability.
                /// </summary>
                public static SharedRestCapability<IGetOpenFuturesOrders, IGetOpenFuturesOrdersRest> GetOpenOrders { get; } = new();

                /// <summary>
                /// Get closed futures orders capability.
                /// </summary>
                public static SharedRestCapability<IGetClosedFuturesOrders, IGetClosedFuturesOrdersRest> GetClosedOrders { get; } = new();

                /// <summary>
                /// Subscribe to futures order updates capability.
                /// </summary>
                public static SharedCapabilityReference<ISubscribeFuturesOrdersSocket> SubscribeOrders { get; } = new();
            }
        }
    }
}

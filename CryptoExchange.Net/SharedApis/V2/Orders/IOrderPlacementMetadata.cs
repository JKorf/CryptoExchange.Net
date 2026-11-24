namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Exchange-compatible client order ID generation for an API surface
    /// </summary>
    public interface IClientOrderIdGenerator
    {
        /// <summary>
        /// Generate a new random client order id
        /// </summary>
        /// <returns></returns>
        string GenerateClientOrderId();
    }

    /// <summary>
    /// Spot order placement metadata for an API surface
    /// </summary>
    /// <see cref="IClientOrderIdGenerator"/>
    public interface ISpotOrderPlacementMetadata : IClientOrderIdGenerator
    {
        /// <summary>
        /// How the trading fee is deducted
        /// </summary>
        SharedFeeDeductionType SpotFeeDeductionType { get; }

        /// <summary>
        /// How the asset is determined in which the trading fee is paid
        /// </summary>
        SharedFeeAssetType SpotFeeAssetType { get; }

        /// <summary>
        /// Supported order types for spot orders
        /// </summary>
        SharedOrderType[] SpotSupportedOrderTypes { get; }

        /// <summary>
        /// Supported time in force types for placing spot orders
        /// </summary>
        SharedTimeInForce[] SpotSupportedTimeInForce { get; }

        /// <summary>
        /// Supported quantity types for placing spot orders
        /// </summary>
        SharedQuantitySupport SpotSupportedOrderQuantity { get; }
    }

    /// <summary>
    /// Futures order placement metadata for an API surface
    /// </summary>
    /// <see cref="IClientOrderIdGenerator"/>
    public interface IFuturesOrderPlacementMetadata : IClientOrderIdGenerator
    {
        /// <summary>
        /// How the trading fee is deducted
        /// </summary>
        SharedFeeDeductionType FuturesFeeDeductionType { get; }

        /// <summary>
        /// How the asset is determined in which the trading fee is paid
        /// </summary>
        SharedFeeAssetType FuturesFeeAssetType { get; }

        /// <summary>
        /// Supported order types for futures orders
        /// </summary>
        SharedOrderType[] FuturesSupportedOrderTypes { get; }

        /// <summary>
        /// Supported time in force types for futures orders
        /// </summary>
        SharedTimeInForce[] FuturesSupportedTimeInForce { get; }

        /// <summary>
        /// Supported quantity types for futures orders
        /// </summary>
        SharedQuantitySupport FuturesSupportedOrderQuantity { get; }
    }
}

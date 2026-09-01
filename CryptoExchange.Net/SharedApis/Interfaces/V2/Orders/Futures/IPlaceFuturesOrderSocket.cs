using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for placing and managing futures orders
    /// </summary>
    public interface IPlaceFuturesOrderSocket : ISharedSocket
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

        /// <summary>
        /// Generate a new random client order id
        /// </summary>
        /// <returns></returns>
        string GenerateClientOrderId();

        /// <summary>
        /// Futures place order request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        PlaceFuturesOrderSocketOptions PlaceFuturesOrderOptions { get; }

        /// <summary>
        /// Place a new futures order, see <see cref="PlaceFuturesOrderOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<QueryResult<SharedId>> PlaceFuturesOrderAsync(PlaceFuturesOrderRequest request, CancellationToken ct = default);

    }
}

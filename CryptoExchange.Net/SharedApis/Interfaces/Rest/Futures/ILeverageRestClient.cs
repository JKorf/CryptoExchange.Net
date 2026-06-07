using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for managing the leverage of a symbol
    /// </summary>
    public interface ILeverageRestClient : ISharedClient
    {
        /// <summary>
        /// How the leverage setting is configured on the exchange
        /// </summary>
        SharedLeverageSettingMode LeverageSettingType { get; }

        /// <summary>
        /// Leverage request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        EndpointOptions<GetLeverageRequest, ILeverageRestClient> GetLeverageOptions { get; }
        /// <summary>
        /// Get the current leverage setting for a symbol, see <see cref="GetLeverageOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedLeverage>> GetLeverageAsync(GetLeverageRequest request, CancellationToken ct = default);

        /// <summary>
        /// Leverage set request options.<br />
        /// Use <see cref="EndpointOptions.RequiredExchangeParameters"/> and <see cref="EndpointOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        SetLeverageOptions SetLeverageOptions { get; }
        /// <summary>
        /// Set the leverage for a symbol, see <see cref="SetLeverageOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<HttpResult<SharedLeverage>> SetLeverageAsync(SetLeverageRequest request, CancellationToken ct = default);

    }
}

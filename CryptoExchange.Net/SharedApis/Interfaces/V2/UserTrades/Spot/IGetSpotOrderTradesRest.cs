using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for getting trades for a specific spot order on an exchange.
    /// </summary>
    public interface IGetSpotOrderTrades : ISharedApiCapability
    {
        /// <summary>
        /// Spot get order trades request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotOrderTradesOptions GetSpotOrderTradesOptions { get; }
        /// <summary>
        /// Get trades for a specific spot order, see <see cref="GetSpotOrderTradesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// The result is paginated, if there are more results to be retrieved, the `NextPageRequest` property of the result will contain the pagination request to be used for the next request to continue pagination.
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedUserTrade[]>> GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);

    }

    /// <summary>
    /// Operation for getting trades for a specific spot order on an exchange via the REST API.
    /// </summary>
    public interface IGetSpotOrderTradesRest : IGetSpotOrderTrades, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedUserTrade[]>> GetSpotOrderTradesAsync(GetOrderTradesRequest request, CancellationToken ct = default);

    }
}

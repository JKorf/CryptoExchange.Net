using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving the index price for all symbols from an exchange.
    /// </summary>
    public interface IGetAllIndexPrices : ISharedApiCapability
    {
        /// <summary>
        /// Index price request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetAllIndexPricesOptions GetAllIndexPricesOptions { get; }
        /// <summary>
        /// Get the index price for a symbol, see <see cref="GetAllIndexPricesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedIndexPrice[]>> GetAllIndexPricesAsync(GetAllIndexPricesRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving the index price for all symbols from an exchange via the REST API.
    /// </summary>
    public interface IGetAllIndexPricesRest : IGetAllIndexPrices, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedIndexPrice[]>> GetAllIndexPricesAsync(GetAllIndexPricesRequest request, CancellationToken ct = default);
    }
}

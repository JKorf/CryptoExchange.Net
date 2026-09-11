using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving the mark price for all symbols from an exchange.
    /// </summary>
    public interface IGetAllMarkPrices : ISharedApiCapability
    {
        /// <summary>
        /// Mark price request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetAllMarkPricesOptions GetAllMarkPricesOptions { get; }
        /// <summary>
        /// Get the mark price for a symbol, see <see cref="GetAllMarkPricesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedMarkPrice[]>> GetAllMarkPricesAsync(GetAllMarkPricesRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving the mark price for all symbols from an exchange via the REST API.
    /// </summary>
    public interface IGetAllMarkPricesRest : IGetAllMarkPrices, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedMarkPrice[]>> GetAllMarkPricesAsync(GetAllMarkPricesRequest request, CancellationToken ct = default);
    }
}

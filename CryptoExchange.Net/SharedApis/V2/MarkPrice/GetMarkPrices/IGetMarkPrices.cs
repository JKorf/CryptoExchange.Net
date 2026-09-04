using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving the mark price for a symbol from an exchange.
    /// </summary>
    public interface IGetMarkPrices : ISharedApiCapability
    {
        /// <summary>
        /// Mark price request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetMarkPricesOptions GetMarkPricesOptions { get; }
        /// <summary>
        /// Get the mark price for a symbol, see <see cref="GetMarkPricesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedMarkPrice[]>> GetMarkPricesAsync(GetMarkPricesRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving the mark price for a symbol from an exchange via the REST API.
    /// </summary>
    public interface IGetMarkPricesRest : IGetMarkPrices, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedMarkPrice[]>> GetMarkPricesAsync(GetMarkPricesRequest request, CancellationToken ct = default);
    }
}

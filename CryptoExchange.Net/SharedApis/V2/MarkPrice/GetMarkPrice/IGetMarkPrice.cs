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
    public interface IGetMarkPrice : ISharedApiCapability
    {
        /// <summary>
        /// Mark price request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetMarkPriceOptions GetMarkPriceOptions { get; }
        /// <summary>
        /// Get the mark price for a symbol, see <see cref="GetMarkPriceOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedMarkPrice>> GetMarkPriceAsync(GetMarkPriceRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving the mark price for a symbol from an exchange via the REST API.
    /// </summary>
    public interface IGetMarkPriceRest : IGetMarkPrice, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedMarkPrice>> GetMarkPriceAsync(GetMarkPriceRequest request, CancellationToken ct = default);
    }
}

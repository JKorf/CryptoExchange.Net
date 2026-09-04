using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving a single spot ticker from an exchange.
    /// </summary>
    public interface IGetSpotTicker : ISharedApiCapability
    {
        /// <summary>
        /// Spot ticker request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetSpotTickerOptions GetSpotTickerOptions { get; }
        /// <summary>
        /// Get ticker for a specific spot symbol, see <see cref="GetSpotTickerOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedSpotTicker>> GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving a single spot ticker from an exchange via the REST API
    /// </summary>
    public interface IGetSpotTickerRest : IGetSpotTicker, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedSpotTicker>> GetSpotTickerAsync(GetTickerRequest request, CancellationToken ct = default);
    }
}

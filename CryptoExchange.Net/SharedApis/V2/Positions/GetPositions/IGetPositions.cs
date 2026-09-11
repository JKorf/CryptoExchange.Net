using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving open positions from an exchange.
    /// </summary>
    public interface IGetPositions : ISharedApiCapability
    {
        /// <summary>
        /// Positions request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetPositionsOptions GetPositionsOptions { get; }
        /// <summary>
        /// Get open position info, see <see cref="GetPositionsOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedPosition[]>> GetPositionsAsync(GetPositionsRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving open positions from an exchange via the REST API.
    /// </summary>
    public interface IGetPositionsRest : IGetPositions, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedPosition[]>> GetPositionsAsync(GetPositionsRequest request, CancellationToken ct = default);
    }
}

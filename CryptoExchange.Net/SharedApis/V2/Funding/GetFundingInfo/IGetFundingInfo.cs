using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving the current funding info for a symbol on an exchange.
    /// </summary>
    public interface IGetFundingInfo : ISharedApiCapability
    {
        /// <summary>
        /// Funding info request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFundingInfoOptions GetFundingInfoOptions { get; }
        /// <summary>
        /// Get current funding info for a symbol, see <see cref="GetFundingInfoOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedFundingInfo>> GetFundingInfoAsync(GetFundingInfoRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving the current funding info for a symbol on an exchange via the REST API.
    /// </summary>
    public interface IGetFundingInfoRest : IGetFundingInfo, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedFundingInfo>> GetFundingInfoAsync(GetFundingInfoRequest request, CancellationToken ct = default);
    }
}

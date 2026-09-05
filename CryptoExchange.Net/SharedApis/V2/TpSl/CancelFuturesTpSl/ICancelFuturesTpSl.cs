using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for canceling a take profit and/or stop loss for an open position on an exchange.
    /// </summary>
    public interface ICancelFuturesTpSl : ISharedApiCapability
    {
        /// <summary>
        /// Cancel a take profit and/or stop loss options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        CancelFuturesTpSlOptions CancelFuturesTpSlOptions { get; }
        /// <summary>
        /// Cancel an active take profit and/or stop loss for an open position, see <see cref="CancelFuturesTpSlOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<bool>> CancelFuturesTpSlAsync(CancelTpSlRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for canceling a take profit and/or stop loss for an open position on an exchange via the REST API.
    /// </summary>
    public interface ICancelFuturesTpSlRest : ICancelFuturesTpSl, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<bool>> CancelFuturesTpSlAsync(CancelTpSlRequest request, CancellationToken ct = default);
    }
}

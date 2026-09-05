using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for setting a take profit and/or stop loss for an open position on an exchange.
    /// </summary>
    public interface ISetFuturesTpSl : ISharedApiCapability
    {
        /// <summary>
        /// Set take profit and/or stop loss options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        SetFuturesTpSlOptions SetFuturesTpSlOptions { get; }
        /// <summary>
        /// Set a take profit and/or stop loss for an open position, see <see cref="SetFuturesTpSlOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedId>> SetFuturesTpSlAsync(SetTpSlRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for setting a take profit and/or stop loss for an open position on an exchange via the REST API.
    /// </summary>
    public interface ISetFuturesTpSlRest : ISetFuturesTpSl, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> SetFuturesTpSlAsync(SetTpSlRequest request, CancellationToken ct = default);
    }
}

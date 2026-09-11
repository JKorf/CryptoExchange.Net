using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for withdrawing a specific asset from an exchange.
    /// </summary>
    public interface IWithdraw : ISharedApiCapability
    {
        /// <summary>
        /// Withdraw request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        WithdrawOptions WithdrawOptions { get; }

        /// <summary>
        /// Request a withdrawal, see <see cref="WithdrawOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedId>> WithdrawAsync(WithdrawRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for withdrawing a specific asset from an exchange via the REST API.
    /// </summary>
    public interface IWithdrawRest : IWithdraw, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> WithdrawAsync(WithdrawRequest request, CancellationToken ct = default);
    }
}

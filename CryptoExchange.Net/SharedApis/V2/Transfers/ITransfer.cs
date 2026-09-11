using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for transferring funds between account types on an exchange.
    /// </summary>
    public interface ITransfer : ISharedApiCapability
    {
        /// <summary>
        /// Transfer request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        TransferOptions TransferOptions { get; }

        /// <summary>
        /// Transfer funds between account types, see <see cref="TransferOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedId>> TransferAsync(TransferRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for transferring funds between account types on an exchange via the REST API.
    /// </summary>
    public interface ITransferRest : ITransfer, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedId>> TransferAsync(TransferRequest request, CancellationToken ct = default);
    }
}

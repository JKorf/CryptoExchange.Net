using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving user asset balances from an exchange.
    /// </summary>
    public interface IGetBalances : ISharedApiCapability
    {
        /// <summary>
        /// Balances request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetBalancesOptions GetBalancesOptions { get; }

        /// <summary>
        /// Get balances for the user, see <see cref="GetBalancesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedBalance[]>> GetBalancesAsync(GetBalancesRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving user asset balances from an exchange via the REST API.
    /// </summary>
    public interface IGetBalancesRest : IGetBalances, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedBalance[]>> GetBalancesAsync(GetBalancesRequest request, CancellationToken ct = default);
    }
}

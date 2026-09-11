using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for retrieving user deposit addresses for an exchange.
    /// </summary>
    public interface IGetDepositAddresses : ISharedApiCapability
    {
        /// <summary>
        /// Deposit addresses request options.<br />
        /// Use <see cref="CapabilityOptions.RequestParameterRules"/> and <see cref="CapabilityOptions.ExchangeParameterRules"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetDepositAddressesOptions GetDepositAddressesOptions { get; }

        /// <summary>
        /// Get deposit addresses for an asset, see <see cref="GetDepositAddressesOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ICallResult<SharedDepositAddress[]>> GetDepositAddressesAsync(GetDepositAddressesRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for retrieving user deposit addresses for an exchange via the REST API.
    /// </summary>
    public interface IGetDepositAddressesRest : IGetDepositAddresses, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedDepositAddress[]>> GetDepositAddressesAsync(GetDepositAddressesRequest request, CancellationToken ct = default);
    }
}

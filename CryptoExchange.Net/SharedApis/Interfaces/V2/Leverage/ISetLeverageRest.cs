using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Operation for setting leverage on an exchange.
    /// </summary>
    public interface ISetLeverage : ISharedApiCapability
    {
        /// <summary>
        /// How the leverage setting is configured on the exchange
        /// </summary>
        SharedLeverageSettingMode LeverageSettingType { get; }

        /// <summary>
        /// Leverage set request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        SetLeverageOptions SetLeverageOptions { get; }
        /// <summary>
        /// Set the leverage for a symbol, see <see cref="SetLeverageOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedLeverage>> SetLeverageAsync(SetLeverageRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Operation for setting leverage on an exchange via the REST API.
    /// </summary>
    public interface ISetLeverageRest : ISetLeverage, ISharedRest
    {
        /// <inheritdoc />
        new Task<HttpResult<SharedLeverage>> SetLeverageAsync(SetLeverageRequest request, CancellationToken ct = default);
    }
}

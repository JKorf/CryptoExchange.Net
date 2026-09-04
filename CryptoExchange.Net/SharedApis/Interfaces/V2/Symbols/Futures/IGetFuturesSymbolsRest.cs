using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request definition for getting futures symbols from an exchange
    /// </summary>
    public interface IGetFuturesSymbols : ISharedApiCapability
    {
        /// <summary>
        /// Get the futures symbol catalog. Only available if <see cref="GetFuturesSymbolsAsync(GetSymbolsRequest, CancellationToken)"/> has been called previously.
        /// </summary>
        SharedSymbolCatalog? FuturesSymbolCatalog { get; }

        /// <summary>
        /// Futures symbol request options.<br />
        /// Use <see cref="CapabilityOptions.RequiredRequestParameters"/>, <see cref="CapabilityOptions.RequiredExchangeParameters"/> and <see cref="CapabilityOptions.OptionalExchangeParameters"/> to check for required and optional parameters for the request. <br />
        /// Exchange specific parameters can be added to the request via the `ExchangeParameters` property of the request object.
        /// </summary>
        GetFuturesSymbolsOptions GetFuturesSymbolsOptions { get; }

        /// <summary>
        /// Get all futures symbols for a specific base asset
        /// </summary>
        /// <param name="baseAsset">Asset, for example `ETH`</param>
        Task<ExchangeCallResult<SharedSymbol[]>> GetFuturesSymbolsForBaseAssetAsync(string baseAsset);

        /// <summary>
        /// Gets whether the client supports a futures symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        Task<ExchangeCallResult<bool>> SupportsFuturesSymbolAsync(SharedSymbol symbol);

        /// <summary>
        /// Gets whether the client supports a futures symbol
        /// </summary>
        /// <param name="symbolName">The symbol name</param>
        Task<ExchangeCallResult<bool>> SupportsFuturesSymbolAsync(string symbolName);

        /// <summary>
        /// Get info on all futures symbols supported on the exchange, see <see cref="GetFuturesSymbolsOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ICallResult<SharedFuturesSymbol[]>> GetFuturesSymbolsAsync(GetSymbolsRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Request definition for getting futures symbols from an exchange
    /// </summary>
    public interface IGetFuturesSymbolsRest : IGetFuturesSymbols, ISharedRest
    {
        /// <summary>
        /// Get info on all futures symbols supported on the exchange, see <see cref="GetFuturesSymbolsOptions"/> for request options and exchange specific required/optional parameters. <br />
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        new Task<HttpResult<SharedFuturesSymbol[]>> GetFuturesSymbolsAsync(GetSymbolsRequest request, CancellationToken ct = default);
    }
}

using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.Models;
using CryptoExchange.Net.SharedApis.Models.Options.Endpoints;
using CryptoExchange.Net.SharedApis.Models.Rest;
using CryptoExchange.Net.SharedApis.ResponseModels;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis.Interfaces.Rest.Futures
{
    /// <summary>
    /// Client for managing the leverage of a symbol
    /// </summary>
    public interface ILeverageRestClient : ISharedClient
    {
        /// <summary>
        /// How the leverage setting is configured on the exchange
        /// </summary>
        SharedLeverageSettingMode LeverageSettingType { get; }

        /// <summary>
        /// Leverage request options
        /// </summary>
        EndpointOptions<GetLeverageRequest> GetLeverageOptions { get; }
        /// <summary>
        /// Get the current leverage setting for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedLeverage>> GetLeverageAsync(GetLeverageRequest request, CancellationToken ct = default);

        /// <summary>
        /// Leverage set request options
        /// </summary>
        SetLeverageOptions SetLeverageOptions { get; }
        /// <summary>
        /// Set the leverage for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedLeverage>> SetLeverageAsync(SetLeverageRequest request, CancellationToken ct = default);

    }
}

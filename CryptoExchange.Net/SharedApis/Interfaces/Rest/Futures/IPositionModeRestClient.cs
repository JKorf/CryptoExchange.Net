using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for managing the position mode setting
    /// </summary>
    public interface IPositionModeRestClient : ISharedClient
    {
        /// <summary>
        /// How the exchange handles setting the position mode
        /// </summary>
        SharedPositionModeSelection PositionModeSettingType { get; }

        /// <summary>
        /// Position mode request options
        /// </summary>
        GetPositionModeOptions GetPositionModeOptions { get; }
        /// <summary>
        /// Get the current position mode setting
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedPositionModeResult>> GetPositionModeAsync(GetPositionModeRequest request, CancellationToken ct = default);

        /// <summary>
        /// Position mode set request options
        /// </summary>
        SetPositionModeOptions SetPositionModeOptions { get; }
        /// <summary>
        /// Set the position mode to a new value
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedPositionModeResult>> SetPositionModeAsync(SetPositionModeRequest request, CancellationToken ct = default);
    }
}

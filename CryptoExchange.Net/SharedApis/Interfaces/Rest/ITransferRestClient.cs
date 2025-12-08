using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for transferring funds between account types
    /// </summary>
    public interface ITransferRestClient : ISharedClient
    {
        /// <summary>
        /// Transfer request options
        /// </summary>
        TransferOptions TransferOptions { get; }

        /// <summary>
        /// Transfer funds between account types
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedId>> TransferAsync(TransferRequest request, CancellationToken ct = default);
    }
}

using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting to withdraw funds from the exchange
    /// </summary>
    public interface IWithdrawRestClient : ISharedClient
    {
        /// <summary>
        /// Withdraw request options
        /// </summary>
        WithdrawOptions WithdrawOptions { get; }

        /// <summary>
        /// Request a withdrawal
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns></returns>
        Task<ExchangeWebResult<SharedId>> WithdrawAsync(WithdrawRequest request, CancellationToken ct = default);
    }
}

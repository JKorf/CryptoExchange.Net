using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting user trading fees
    /// </summary>
    public interface IFeeRestClient : ISharedClient
    {
        /// <summary>
        /// Fee request options
        /// </summary>
        EndpointOptions<GetFeeRequest> GetFeeOptions { get; }

        /// <summary>
        /// Get trading fees for a symbol
        /// </summary>
        /// <param name="request">Request info</param>
        /// <param name="ct">Cancellation token</param>
        Task<ExchangeWebResult<SharedFee>> GetFeesAsync(GetFeeRequest request, CancellationToken ct = default);
    }
}

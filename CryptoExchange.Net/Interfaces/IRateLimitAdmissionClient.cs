using CryptoExchange.Net.RateLimiting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// A client supporting rate limit admission rules
    /// </summary>
    public interface IRateLimitAdmissionClient
    {
        /// <summary>
        /// Execute an operation with a specific rate limit admission rule
        /// </summary>
        Task<TResult> WithRateLimitAdmissionAsync<TResult>(
            RateLimitAdmission admission,
            Func<Task<TResult>> operation);
    }

}

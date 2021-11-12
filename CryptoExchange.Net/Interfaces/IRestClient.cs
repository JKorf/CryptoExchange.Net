using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiter;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Base class for rest API implementations
    /// </summary>
    public interface IRestClient: IDisposable
    {
        /// <summary>
        /// The factory for creating requests. Used for unit testing
        /// </summary>
        IRequestFactory RequestFactory { get; set; }

        /// <summary>
        /// The total amount of requests made with this client
        /// </summary>
        int TotalRequestsMade { get; }

        /// <summary>
        /// Adds a rate limiter to the client. There are 2 choices, the <see cref="RateLimiterTotal"/> and the <see cref="RateLimiterPerEndpoint"/>.
        /// </summary>
        /// <param name="limiter">The limiter to add</param>
        void AddRateLimiter(IRateLimiter limiter);

        /// <summary>
        /// Removes all rate limiters from this client
        /// </summary>
        void RemoveRateLimiters();

        /// <summary>
        /// The options provided for this client
        /// </summary>
        RestClientOptions ClientOptions { get; }
    }
}
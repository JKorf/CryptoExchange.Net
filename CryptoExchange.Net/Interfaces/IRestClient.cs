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
        /// What should happen when hitting a rate limit
        /// </summary>
        RateLimitingBehaviour RateLimitBehaviour { get; }

        /// <summary>
        /// List of active rate limiters
        /// </summary>
        IEnumerable<IRateLimiter> RateLimiters { get; }

        /// <summary>
        /// The total amount of requests made
        /// </summary>
        int TotalRequestsMade { get; }

        /// <summary>
        /// The base address of the API
        /// </summary>
        string BaseAddress { get; }

        /// <summary>
        /// Client name
        /// </summary>
        string ExchangeName { get; }

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
        /// Ping to see if the server is reachable
        /// </summary>
        /// <returns>The roundtrip time of the ping request</returns>
        CallResult<long> Ping(CancellationToken ct = default);

        /// <summary>
        /// Ping to see if the server is reachable
        /// </summary>
        /// <returns>The roundtrip time of the ping request</returns>
        Task<CallResult<long>> PingAsync(CancellationToken ct = default);
    }
}
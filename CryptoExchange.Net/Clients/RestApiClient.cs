using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Base rest API client for interacting with a REST API
    /// </summary>
    public abstract class RestApiClient: BaseApiClient
    {
        protected abstract TimeSyncModel GetTimeSyncParameters();
        protected abstract void UpdateTimeOffset(TimeSpan offset);
        public abstract TimeSpan GetTimeOffset();

        /// <summary>
        /// Total amount of requests made with this API client
        /// </summary>
        public int TotalRequestsMade { get; set; }

        /// <summary>
        /// Options for this client
        /// </summary>
        public RestApiClientOptions Options { get; }

        /// <summary>
        /// List of rate limiters
        /// </summary>
        internal IEnumerable<IRateLimiter> RateLimiters { get; }

        private Log _log;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options">The base client options</param>
        /// <param name="apiOptions">The Api client options</param>
        public RestApiClient(BaseRestClientOptions options, RestApiClientOptions apiOptions): base(options, apiOptions)
        {
            Options = apiOptions;

            var rateLimiters = new List<IRateLimiter>();
            foreach (var rateLimiter in apiOptions.RateLimiters)
                rateLimiters.Add(rateLimiter);
            RateLimiters = rateLimiters;
        }

        /// <summary>
        /// Retrieve the server time for the purpose of syncing time between client and server to prevent authentication issues
        /// </summary>
        /// <returns>Server time</returns>
        protected abstract Task<WebCallResult<DateTime>> GetServerTimestampAsync();

        internal async Task<WebCallResult<bool>> SyncTimeAsync()
        {
            var timeSyncParams = GetTimeSyncParameters();
            if (await timeSyncParams.Semaphore.WaitAsync(0).ConfigureAwait(false))
            {
                if (!timeSyncParams.SyncTime || (DateTime.UtcNow - timeSyncParams.LastSyncTime < TimeSpan.FromHours(1)))
                {
                    timeSyncParams.Semaphore.Release();
                    return new WebCallResult<bool>(null, null, true, null);
                }

                var localTime = DateTime.UtcNow;
                var result = await GetServerTimestampAsync().ConfigureAwait(false);
                if (!result)
                {
                    timeSyncParams.Semaphore.Release();
                    return result.As(false);
                }

                if (TotalRequestsMade == 1)
                {
                    // If this was the first request make another one to calculate the offset since the first one can be slower
                    localTime = DateTime.UtcNow;
                    result = await GetServerTimestampAsync().ConfigureAwait(false);
                    if (!result)
                    {
                        timeSyncParams.Semaphore.Release();
                        return result.As(false);
                    }
                }

                // Calculate time offset between local and server
                var offset = result.Data - localTime;
                if (offset.TotalMilliseconds >= 0 && offset.TotalMilliseconds < 500)
                {
                    // Small offset, probably mainly due to ping. Don't adjust time
                    UpdateTimeOffset(offset);
                    timeSyncParams.Semaphore.Release();
                }
                else
                {
                    UpdateTimeOffset(offset);
                    timeSyncParams.Semaphore.Release();
                }
            }

            return new WebCallResult<bool>(null, null, true, null);
        }
    }
}

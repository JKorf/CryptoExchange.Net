using CryptoExchange.Net.Objects;
using System;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Base rest API client
    /// </summary>
    public interface IRestApiClient : IBaseApiClient
    {
        /// <summary>
        /// The factory for creating requests. Used for unit testing
        /// </summary>
        IRequestFactory RequestFactory { get; set; }

        /// <summary>
        /// Total amount of requests made with this API client
        /// </summary>
        int TotalRequestsMade { get; set; }

        /// <summary>
        /// Get time offset for an API client. Return null if time syncing shouldnt/cant be done
        /// </summary>
        /// <returns></returns>
        TimeSpan? GetTimeOffset();

        /// <summary>
        /// Get time sync info for an API client. Return null if time syncing shouldnt/cant be done
        /// </summary>
        /// <returns></returns>
        TimeSyncInfo? GetTimeSyncInfo();
    }
}
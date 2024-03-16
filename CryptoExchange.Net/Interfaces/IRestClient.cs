using System;
using CryptoExchange.Net.Objects.Options;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Base class for rest API implementations
    /// </summary>
    public interface IRestClient: IDisposable
    {
        /// <summary>
        /// The options provided for this client
        /// </summary>
        ExchangeOptions ClientOptions { get; }

        /// <summary>
        /// The total amount of requests made with this client
        /// </summary>
        int TotalRequestsMade { get; }

        /// <summary>
        /// The exchange name
        /// </summary>
        string Exchange { get; }
    }
}
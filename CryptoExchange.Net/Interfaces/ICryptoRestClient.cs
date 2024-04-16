using CryptoExchange.Net.Interfaces.CommonClients;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Client for accessing REST API's for different exchanges
    /// </summary>
    public interface ICryptoRestClient
    {
        /// <summary>
        /// Get a list of all registered common ISpotClient types
        /// </summary>
        /// <returns></returns>
        IEnumerable<ISpotClient> GetSpotClients();

        /// <summary>
        /// Get an ISpotClient implementation by exchange name
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        ISpotClient? SpotClient(string exchangeName);

        /// <summary>
        /// Try get 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T TryGet<T>(Func<T> createFunc);
    }
}

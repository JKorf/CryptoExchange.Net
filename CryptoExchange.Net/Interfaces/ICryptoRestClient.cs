using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.CommonClients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Clients
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
        /// Try get 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T TryGet<T>(Func<T> createFunc);
    }
}

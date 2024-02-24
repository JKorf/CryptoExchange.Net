using CryptoExchange.Net.Interfaces.CommonClients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Client for accessing Websocket API's for different exchanges
    /// </summary>
    public interface ICryptoSocketClient
    {
        /// <summary>
        /// Try get a client by type for the service collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T TryGet<T>(Func<T> createFunc);
    }
}

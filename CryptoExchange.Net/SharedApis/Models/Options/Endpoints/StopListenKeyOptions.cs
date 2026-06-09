using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for stopping listenkey
    /// </summary>
    public class StopListenKeyOptions : EndpointOptions<StopListenKeyRequest, IListenKeyRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public StopListenKeyOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IListenKeyRestClient.StopListenKeyAsync))
        {
        }
    }
}

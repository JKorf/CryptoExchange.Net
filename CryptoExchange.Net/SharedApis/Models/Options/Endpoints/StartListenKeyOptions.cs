using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for starting listenkey
    /// </summary>
    public class StartListenKeyOptions : EndpointOptions<StartListenKeyRequest, IListenKeyRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public StartListenKeyOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IListenKeyRestClient.StartListenKeyAsync))
        {
        }
    }
}

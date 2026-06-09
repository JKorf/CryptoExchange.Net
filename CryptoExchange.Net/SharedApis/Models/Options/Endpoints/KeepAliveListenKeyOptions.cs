using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for starting listenkey
    /// </summary>
    public class KeepAliveListenKeyOptions : EndpointOptions<KeepAliveListenKeyRequest, IListenKeyRestClient>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public KeepAliveListenKeyOptions(string exchange, bool authenticated) : base(exchange, authenticated, nameof(IListenKeyRestClient.KeepAliveListenKeyAsync))
        {
        }
    }
}

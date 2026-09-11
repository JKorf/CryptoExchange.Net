using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Options for Shared API client instances
    /// </summary>
    public class SharedApiOptions
    {
        /// <summary>
        /// Whether to use XPerps as perpetual linear contracts when using the Shared API's
        /// </summary>
        public bool EuropeUseXPerps { get; set; }
        /// <summary>
        /// The preferred transport type when requesting transport agnostic capabilities
        /// </summary>
        public SharedTransport PreferredTransport { get; set; }
            = SharedTransport.Rest;
    }
}

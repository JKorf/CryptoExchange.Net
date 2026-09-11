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
        /// The preferred transport type when requesting transport agnostic capabilities
        /// </summary>
        public SharedTransport PreferredTransport { get; set; }
            = SharedTransport.Rest;
    }
}

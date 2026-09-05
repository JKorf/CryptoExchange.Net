using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// The type of transport used for the shared API
    /// </summary>
    public record SharedTransport(string Name)
    {
        /// <summary>
        /// REST transport
        /// </summary>
        public static SharedTransport Rest { get; } = new("REST");
        /// <summary>
        /// WebSocket transport
        /// </summary>
        public static SharedTransport Socket { get; } = new("WebSocket");
    }
}

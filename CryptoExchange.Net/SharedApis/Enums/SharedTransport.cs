using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// The type of transport used for the shared API
    /// </summary>
    public enum SharedTransport
    {
        /// <summary>
        /// REST transport
        /// </summary>
        Rest,
        /// <summary>
        /// WebSocket transport
        /// </summary>
        Socket
    }
}

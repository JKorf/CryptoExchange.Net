using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    public record SharedTransport(string Name)
    {
        public static SharedTransport Rest { get; } = new("REST");
        public static SharedTransport Socket { get; } = new("WebSocket");
    }
}

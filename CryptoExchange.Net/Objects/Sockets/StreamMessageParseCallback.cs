using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Sockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;

namespace CryptoExchange.Net.Objects.Sockets
{
    public class MessageInterpreterPipeline
    {
        public Func<WebSocketMessageType, Stream, Stream>? PreProcessCallback { get; set; }
        public Func<IMessageAccessor, string?> GetStreamIdentifier { get; set; }
        public Func<IMessageAccessor, string?> GetTypeIdentifier { get; set; }
    }
}

using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Objects.Sockets
{
    public class StreamMessageParseCallback
    {
        public List<string> TypeFields { get; set; } = new List<string>();
        public List<string> IdFields { get; set; } = new List<string>();
        public Func<Dictionary<string, string>, IEnumerable<BasePendingRequest>, IEnumerable<Subscription>, Type?> Callback { get; set; }
    }
}

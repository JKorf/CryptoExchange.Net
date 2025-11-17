using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Converters.MessageParsing
{
    public ref struct MessageInfo
    {
        public Type? Type { get; set; }
        public string? Identifier { get; set; }
    }

}

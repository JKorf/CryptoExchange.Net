using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record SharedSymbolRequest
    {
        public SharedSymbol Symbol { get; set; }

        public SharedSymbolRequest(SharedSymbol symbol)
        {
            Symbol = symbol;
        }
    }
}

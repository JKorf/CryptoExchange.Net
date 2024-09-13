using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record SharedRequest
    {
        public ExchangeParameters? ExchangeParameters { get; set; }

        public SharedRequest(ExchangeParameters? exchangeParameters = null)
        {
            ExchangeParameters = exchangeParameters;
        }
    }

    public record SharedSymbolRequest: SharedRequest
    {
        public SharedSymbol Symbol { get; set; }

        public SharedSymbolRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Symbol = symbol;
        }
    }
}

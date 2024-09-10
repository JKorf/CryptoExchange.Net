using System;
using System.Collections.Generic;
using System.Text;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetTickerRequest : SharedSymbolRequest
    {

        public GetTickerRequest(SharedSymbol symbol) : base(symbol)
        {
        }
    }
}

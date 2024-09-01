using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetLeverageRequest : SharedSymbolRequest
    {
        public GetLeverageRequest(SharedSymbol symbol) : base(symbol)
        {
        }
    }
}

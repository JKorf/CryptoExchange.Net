using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record SetLeverageRequest : SharedSymbolRequest
    {
        public decimal Leverage { get; set; }

        public SetLeverageRequest(SharedSymbol symbol, decimal leverage) : base(symbol)
        {
            Leverage = leverage;
        }
    }
}

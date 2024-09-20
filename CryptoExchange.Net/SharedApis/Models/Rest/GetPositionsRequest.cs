using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetPositionsRequest : SharedRequest
    {
        public TradingMode? ApiType { get; set; }
        public SharedSymbol? Symbol { get; set; }

        public GetPositionsRequest(TradingMode? apiType = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ApiType = apiType;
        }

        public GetPositionsRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters) 
        {
            Symbol = symbol;
        }
    }
}

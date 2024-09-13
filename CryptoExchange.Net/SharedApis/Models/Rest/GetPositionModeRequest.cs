using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record GetPositionModeRequest : SharedRequest
    {
        public ApiType? ApiType { get; set; }
        public SharedSymbol? Symbol { get; set; }

        public GetPositionModeRequest(SharedSymbol symbol, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Symbol = symbol;
        }

        public GetPositionModeRequest(ApiType apiType, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ApiType = apiType;
        }
    }
}

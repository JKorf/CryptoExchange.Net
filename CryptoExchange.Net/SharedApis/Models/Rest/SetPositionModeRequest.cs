using CryptoExchange.Net.Objects;
using CryptoExchange.Net.SharedApis.Enums;
using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record SetPositionModeRequest : SharedRequest
    {
        public SharedSymbol? Symbol { get; set; }
        public TradingMode? ApiType { get; set; }
        public SharedPositionMode Mode { get; set; }

        public SetPositionModeRequest(SharedPositionMode mode, TradingMode? apiType = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            ApiType = apiType;
            Mode = mode;
        }
        
        public SetPositionModeRequest(SharedSymbol symbol, SharedPositionMode mode, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Mode = mode;
            Symbol = symbol;
        }
    }
}

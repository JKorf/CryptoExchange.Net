using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record SharedFuturesSymbol : SharedSpotSymbol
    {
        public SharedSymbolType SymbolType { get; set; }
        public decimal? ContractSize { get; set; }
        public DateTime? DeliveryTime { get; set; }

        public SharedFuturesSymbol(SharedSymbolType symbolType, string baseAsset, string quoteAsset, string symbol, bool trading) :base(baseAsset, quoteAsset, symbol, trading)
        {
            SymbolType = symbolType;
        }
    }
}

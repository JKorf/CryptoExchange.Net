﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record SharedFuturesSymbol : SharedSpotSymbol
    {
        public decimal? ContractSize { get; set; }

        public DateTime? DeliveryTime { get; set; }

        public SharedFuturesSymbol(string baseAsset, string quoteAsset, string symbol) :base(baseAsset, quoteAsset, symbol)
        {

        }
    }
}
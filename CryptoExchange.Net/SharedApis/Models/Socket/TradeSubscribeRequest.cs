﻿using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.SubscribeModels
{
    public record TradeSubscribeRequest : SharedRequest
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }

        public TradeSubscribeRequest(string baseAsset, string quoteAsset)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record SharedKline
    {
        public DateTime OpenTime { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal BaseVolume { get; set; }

        public SharedKline(DateTime openTime, decimal closePrice, decimal highPrice, decimal lowPrice, decimal openPrice, decimal baseVolume)
        {
            OpenTime = openTime;
            ClosePrice = closePrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            OpenPrice = openPrice;
            BaseVolume = baseVolume;
        }
    }
}
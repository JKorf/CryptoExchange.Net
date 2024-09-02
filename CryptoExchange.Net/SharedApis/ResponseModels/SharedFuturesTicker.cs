using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record SharedFuturesTicker
    {
        public string Symbol { get; set; }
        public decimal LastPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal Volume { get; set; }

        public decimal? MarkPrice { get; set; }
        public decimal? IndexPrice { get; set; }
        public decimal? FundingRate { get; set; }
        public DateTime? NextFundingTime { get; set; }

        public SharedFuturesTicker(string symbol, decimal lastPrice, decimal highPrice, decimal lowPrice, decimal volume)
        {
            Symbol = symbol;
            LastPrice = lastPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            Volume = volume;
        }
    }
}

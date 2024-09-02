using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record SharedSpotTicker
    {
        public string Symbol { get; set; }
        public decimal LastPrice { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal Volume { get; set; }

        public SharedSpotTicker(string symbol, decimal lastPrice, decimal highPrice, decimal lowPrice, decimal volume)
        {
            Symbol = symbol;
            LastPrice = lastPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
            Volume = volume;
        }
    }
}

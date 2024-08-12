using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedBookTicker
    {
        public decimal BestAskPrice { get; set; }
        public decimal BestAskQuantity { get; set; }
        public decimal BestBidPrice { get; set; }
        public decimal BestBidQuantity { get; set; }

        public SharedBookTicker(decimal bestAskPrice, decimal bestAskQuantity, decimal bestBidPrice, decimal bestBidQuantity)
        {
            BestAskPrice = bestAskPrice;
            BestAskQuantity = bestAskQuantity;
            BestBidPrice = bestBidPrice;
            BestBidQuantity = bestBidQuantity;
        }
    }
}

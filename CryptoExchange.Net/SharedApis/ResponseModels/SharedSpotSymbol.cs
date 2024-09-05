using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.RequestModels
{
    public record SharedSpotSymbol
    {
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Minimal quantity of an order
        /// </summary>
        public decimal? MinTradeQuantity { get; set; }
        /// <summary>
        /// Max quantity of an order
        /// </summary>
        public decimal? MaxTradeQuantity { get; set; }
        /// <summary>
        /// Step with which the quantity should increase
        /// </summary>
        public decimal? QuantityStep { get; set; }
        /// <summary>
        /// step with which the price should increase
        /// </summary>
        public decimal? PriceStep { get; set; }
        /// <summary>
        /// The max amount of decimals for quantity
        /// </summary>
        public int? QuantityDecimals { get; set; }
        /// <summary>
        /// The max amount of decimal for price
        /// </summary>
        public int? PriceDecimals { get; set; }

        public bool Trading { get; set; }

        public SharedSpotSymbol(string baseAsset, string quoteAsset, string symbol, bool trading)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
            Name = symbol;
            Trading = trading;
        }
    }
}

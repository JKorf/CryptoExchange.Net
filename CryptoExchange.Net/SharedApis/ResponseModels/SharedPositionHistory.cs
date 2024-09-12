using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedPositionHistory
    {
        public string Symbol { get; set; }
        public SharedPositionSide PositionSide { get; set; }
        public decimal AverageOpenPrice { get; set; }
        public decimal AverageClosePrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal RealizedPnl { get; set; }
        public DateTime Timestamp { get; set; }

        public string? PositionId { get; set; }
        public decimal? Leverage { get; set; }
        public string? OrderId { get; set; }

        public SharedPositionHistory(
            string symbol,
            SharedPositionSide side,
            decimal openPrice,
            decimal closePrice,
            decimal quantity,
            decimal realizedPnl,
            DateTime timestamp)
        {
            Symbol = symbol;
            PositionSide = side;
            AverageOpenPrice = openPrice;
            AverageClosePrice = closePrice;
            Quantity = quantity;
            RealizedPnl = realizedPnl;
            Timestamp = timestamp;
        }
    }
}

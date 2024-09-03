using CryptoExchange.Net.SharedApis.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedPosition
    {
        public string Symbol { get; set; }
        public decimal PositionSize { get; set; }
        public SharedPositionSide PositionSide { get; set; }
        public decimal? AverageEntryPrice { get; set; }
        public decimal? UnrealizedPnl { get; set; }
        public decimal? LiquidationPrice { get; set; }
        public decimal? InitialMargin { get; set; }
        public decimal? MaintenanceMargin { get; set; }
        public decimal? Leverage { get; set; }
        public DateTime UpdateTime { get; set; }

        public SharedPosition(string symbol, decimal positionSize, DateTime updateTime)
        {
            Symbol = symbol;
            PositionSize = positionSize;
            UpdateTime = updateTime;
        }
    }
}

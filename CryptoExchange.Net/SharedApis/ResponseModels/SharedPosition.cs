using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedPosition
    {
        public string Symbol { get; set; }
        public decimal PositionSize { get; set; }
        public decimal? AverageEntryPrice { get; set; }
        public decimal? UnrealizedPnl { get; set; }
        public decimal? LiquidationPrice { get; set; }
        public decimal? InitialMargin { get; set; }
        public decimal? MaintenanceMargin { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}

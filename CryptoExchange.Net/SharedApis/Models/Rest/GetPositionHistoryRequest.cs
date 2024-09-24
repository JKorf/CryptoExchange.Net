using CryptoExchange.Net.Objects;
using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the position close history
    /// </summary>
    public record GetPositionHistoryRequest : SharedRequest
    {
        /// <summary>
        /// Trading mode
        /// </summary>
        public TradingMode? TradingMode { get; set; }
        /// <summary>
        /// Symbol
        /// </summary>
        public SharedSymbol? Symbol { get; set; }
        /// <summary>
        /// Filter by start time
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// Filter by end time
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// Max number of results
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol filter</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetPositionHistoryRequest(SharedSymbol symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Symbol = symbol;
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tradeMode">Trade mode</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetPositionHistoryRequest(TradingMode? tradeMode = null, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            TradingMode = tradeMode;
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }
    }
}

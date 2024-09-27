using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve kline/candlestick data
    /// </summary>
    public record GetKlinesRequest : SharedSymbolRequest
    {
        /// <summary>
        /// The kline interval
        /// </summary>
        public SharedKlineInterval Interval { get; set; }
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
        /// <param name="symbol">Symbol to request klines for</param>
        /// <param name="interval">Interval of the klines</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetKlinesRequest(SharedSymbol symbol, SharedKlineInterval interval, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            Interval = interval;
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }
    }
}

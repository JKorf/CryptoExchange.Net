using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the public trade history
    /// </summary>
    public record GetTradeHistoryRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Filter by start time
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Filter by end time
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// Max number of results
        /// </summary>
        public int? Limit { get; set; }

        public PageDirection? Direction { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to retrieve trades for</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetTradeHistoryRequest(SharedSymbol symbol, DateTime startTime, DateTime endTime, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }
    }
}

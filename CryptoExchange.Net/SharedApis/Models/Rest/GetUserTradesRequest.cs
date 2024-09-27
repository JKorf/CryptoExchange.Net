using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve the trades of the user
    /// </summary>
    public record GetUserTradesRequest : SharedSymbolRequest
    {
        /// <summary>
        /// Filter by start time
        /// </summary>
        public DateTime? StartTime { get; }
        /// <summary>
        /// Filter by end time
        /// </summary>
        public DateTime? EndTime { get; }
        /// <summary>
        /// Max number of results
        /// </summary>
        public int? Limit { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbol">Symbol to retrieve trades for</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetUserTradesRequest(SharedSymbol symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(symbol, exchangeParameters)
        {
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }
    }
}

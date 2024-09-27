using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Request to retrieve deposit history records
    /// </summary>
    public record GetDepositsRequest : SharedRequest
    {
        /// <summary>
        /// Filter by asset
        /// </summary>
        public string? Asset { get; set; }
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
        /// <param name="asset">Filter by asset</param>
        /// <param name="startTime">Filter by start time</param>
        /// <param name="endTime">Filter by end time</param>
        /// <param name="limit">Max number of results</param>
        /// <param name="exchangeParameters">Exchange specific parameters</param>
        public GetDepositsRequest(string? asset = null, DateTime? startTime = null, DateTime? endTime = null, int? limit = null, ExchangeParameters? exchangeParameters = null) : base(exchangeParameters)
        {
            Asset = asset;
            StartTime = startTime;
            EndTime = endTime;
            Limit = limit;
        }
    }
}

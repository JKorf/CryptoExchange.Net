using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Funding fee payment
    /// </summary>
    [DebuggerDisplay("[{Timestamp}] {Symbol,nq} {Fee} {Asset,nq}")]
    public record SharedFundingFee
    {
        /// <summary>
        /// Id
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// The symbol
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The asset the funding fee was paid in
        /// </summary>
        public string? Asset { get; set; }

        /// <summary>
        /// The funding fee paid
        /// </summary>
        public decimal Fee { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The side of the position
        /// </summary>
        public SharedPositionSide? Side { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedFundingFee(string symbol, decimal fee, DateTime timestamp)
        {
            Symbol = symbol;
            Fee = fee;
            Timestamp = timestamp;
        }
    }
}

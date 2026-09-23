using System;
using System.Diagnostics;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Transfer info
    /// </summary>
    [DebuggerDisplay("[{Timestamp}] {Quantity} {Asset,nq} {FromAccountType} -> {ToAccountType}")]
    public record SharedTransfer
    {
        /// <summary>
        /// The id of the Transfer
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// The asset of the Transfer
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// The quantity that was Transfered
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Timestamp of the Transfer
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Source account type
        /// </summary>
        public SharedAccountType FromAccountType { get; set; }
        /// <summary>
        /// Target account type
        /// </summary>
        public SharedAccountType ToAccountType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedTransfer(string asset, decimal quantity, SharedAccountType fromAccountType, SharedAccountType toAccountType, DateTime timestamp)
        {
            Asset = asset;
            Quantity = quantity;
            Timestamp = timestamp;
            FromAccountType = fromAccountType;
            ToAccountType = toAccountType;
        }
    }

}

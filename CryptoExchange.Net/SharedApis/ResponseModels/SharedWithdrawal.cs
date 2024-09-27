using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// A withdrawal record
    /// </summary>
    public record SharedWithdrawal
    {
        /// <summary>
        /// The id of the withdrawal
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// The asset that was withdrawn
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// The withdrawal address
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Quantity that was withdrawn
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// The timestamp of the withdrawal
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Whether the withdrawal was successfully completed
        /// </summary>
        public bool Completed { get; set; }
        /// <summary>
        /// Tag
        /// </summary>
        public string? Tag { get; set; }
        /// <summary>
        /// Used network
        /// </summary>
        public string? Network { get; set; }
        /// <summary>
        /// Transaction id
        /// </summary>
        public string? TransactionId { get; set; }
        /// <summary>
        /// Number of confirmations
        /// </summary>
        public int? Confirmations { get; set; }
        /// <summary>
        /// Fee paid
        /// </summary>
        public decimal? Fee { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedWithdrawal(string asset, string address, decimal quantity, bool completed, DateTime timestamp)
        {
            Asset = asset;
            Address = address;
            Quantity = quantity;
            Completed = completed;
            Timestamp = timestamp;
        }
    }

}

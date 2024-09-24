using System;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Deposit info
    /// </summary>
    public record SharedDeposit
    {
        /// <summary>
        /// The id of the deposit
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// The asset of the deposit
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// The quantity that was deposited
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Timestamp of the deposit
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Network used
        /// </summary>
        public string? Network { get; set; }
        /// <summary>
        /// Number of confirmations
        /// </summary>
        public int? Confirmations { get; set; }
        /// <summary>
        /// Transaction id
        /// </summary>
        public string? TransactionId { get; set; }
        /// <summary>
        /// Tag
        /// </summary>
        public string? Tag { get; set; }
        /// <summary>
        /// Whether the deposit was completed successfully
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedDeposit(string asset, decimal quantity, bool completed, DateTime timestamp)
        {
            Asset = asset;
            Quantity = quantity;
            Timestamp = timestamp;
            Completed = completed;
        }
    }

}

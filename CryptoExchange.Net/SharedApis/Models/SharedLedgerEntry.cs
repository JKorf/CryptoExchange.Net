using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Ledger entry
    /// </summary>
    [DebuggerDisplay("[{Timestamp}] {DeltaQuantity} {Asset,nq}")]
    public record SharedLedgerEntry
    {
        /// <summary>
        /// The id of the entry
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// The id of the relation that changed the balance, for example a trade id or transfer id
        /// </summary>
        public string? RelationId { get; set; }
        /// <summary>
        /// The asset 
        /// </summary>
        public string Asset { get; set; }
        /// <summary>
        /// The adjustment quantity, positive means the balance increased, negative means the balance decreased
        /// </summary>
        public decimal DeltaQuantity { get; set; }
        /// <summary>
        /// Timestamp of the action
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// The type of the entry
        /// </summary>
        public SharedLedgerEntryType Type { get; set; }
        /// <summary>
        /// The type of the entry as string, can be used for entry types not recognized by the Shared API
        /// </summary>
        public string TypeString { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedLedgerEntry(string asset, decimal deltaQuantity, SharedLedgerEntryType type, string typeString, DateTime timestamp)
        {
            Asset = asset;
            DeltaQuantity = deltaQuantity;
            Type = type;
            TypeString = typeString;
            Timestamp = timestamp;
        }
    }
}
using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedDeposit
    {
#warning set ids on existing mapping
        public string Id { get; set; }
        public string Asset { get; set; }
        public decimal Quantity { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Network { get; set; }
        public int? Confirmations { get; set; }
        public string? TransactionId { get; set; }
        public string? Tag { get; set; }
        public bool Completed { get; set; }

        public SharedDeposit(string asset, decimal quantity, bool completed, DateTime timestamp)
        {
            Asset = asset;
            Quantity = quantity;
            Timestamp = timestamp;
            Completed = completed;
        }
    }

}

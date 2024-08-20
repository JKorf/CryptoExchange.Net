using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedWithdrawal
    {
#warning set ids on existing mapping
        public string Id { get; set; }
        public string Asset { get; set; }
        public string Address { get; set; }
        public decimal Quantity { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Completed { get; set; }
        public string? Tag { get; set; }
        public string? Network { get; set; }
        public string? TransactionId { get; set; }
        public int? Confirmations { get; set; }
        public decimal? Fee { get; set; }

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

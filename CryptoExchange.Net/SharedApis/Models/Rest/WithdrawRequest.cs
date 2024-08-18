using CryptoExchange.Net.SharedApis.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models.Rest
{
    public record WithdrawRequest : SharedRequest
    {
        public string Asset { get; set; }
        public string Address { get; set; }
        public decimal Quantity { get; set; }
        public string? AddressTag { get; set; }
        public string? Network { get; set; }

        public WithdrawRequest(string asset, decimal quantity, string address, string network)
        {
            Asset = asset;
            Address = address;
            Quantity = quantity;
        }
    }
}

using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedDepositAddress
    {
        public string Address { get; set; }
        public string? Network { get; set; }
        public string? Tag { get; set; }

        public SharedDepositAddress(string address)
        {
            Address = address;
        }
    }

}

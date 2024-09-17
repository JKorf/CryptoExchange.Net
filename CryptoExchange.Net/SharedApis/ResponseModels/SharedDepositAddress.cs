using CryptoExchange.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedDepositAddress
    {
        public string Asset { get; set; }
        public string Address { get; set; }
        public string? Network { get; set; }
        public string? TagOrMemo { get; set; }

        public SharedDepositAddress(string asset, string address)
        {
            Asset = asset;
            Address = address;
        }
    }

}

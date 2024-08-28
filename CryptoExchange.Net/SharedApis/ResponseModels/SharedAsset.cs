using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.ResponseModels
{
    public record SharedAsset
    {
        public string Name { get; set; }
        public string? FullName { get; set; }

        public IEnumerable<SharedAssetNetwork>? Networks { get; set; } = Array.Empty<SharedAssetNetwork>();

        public SharedAsset(string name)
        {
            Name = name;
        }
    }

    public record SharedAssetNetwork
    {
        public string Name { get; set; }
        public string? FullName { get; set; }

        public decimal? WithdrawFee { get; set; }
        public decimal? MinWithdrawQuantity { get; set; }
        public decimal? MaxWithdrawQuantity { get; set; }
        public bool? WithdrawEnabled { get; set; }
        public bool? DepositEnabled { get; set; }
        public int? MinConfirmations { get; set; }

        public SharedAssetNetwork(string name)
        {
            Name = name;
        }
    }
}

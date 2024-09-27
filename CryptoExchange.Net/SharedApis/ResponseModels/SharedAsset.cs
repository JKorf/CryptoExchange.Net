using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Asset info
    /// </summary>
    public record SharedAsset
    {
        /// <summary>
        /// Name of the asset
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Full or alternative name of the asset
        /// </summary>
        public string? FullName { get; set; }
        /// <summary>
        /// Asset networks info
        /// </summary>
        public IEnumerable<SharedAssetNetwork>? Networks { get; set; } = Array.Empty<SharedAssetNetwork>();

        /// <summary>
        /// ctor
        /// </summary>
        public SharedAsset(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Asset network info
    /// </summary>
    public record SharedAssetNetwork
    {
        /// <summary>
        /// Network name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Network full/alternative name
        /// </summary>
        public string? FullName { get; set; }
        /// <summary>
        /// Withdrawal fee
        /// </summary>
        public decimal? WithdrawFee { get; set; }
        /// <summary>
        /// Minimal withdrawal quantity
        /// </summary>
        public decimal? MinWithdrawQuantity { get; set; }
        /// <summary>
        /// Max withdrawal quantity
        /// </summary>
        public decimal? MaxWithdrawQuantity { get; set; }
        /// <summary>
        /// Withdrawals are enabled
        /// </summary>
        public bool? WithdrawEnabled { get; set; }
        /// <summary>
        /// Deposits are enabled
        /// </summary>
        public bool? DepositEnabled { get; set; }
        /// <summary>
        /// Min number of confirmations
        /// </summary>
        public int? MinConfirmations { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedAssetNetwork(string name)
        {
            Name = name;
        }
    }
}

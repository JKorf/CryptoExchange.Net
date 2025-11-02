using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// An alias used by the exchange for an asset commonly known by another name
    /// </summary>
    public class AssetAlias
    {
        /// <summary>
        /// Alias type
        /// </summary>
        public AliasType Type { get; set; }
        /// <summary>
        /// The name of the asset on the exchange
        /// </summary>
        public string ExchangeAssetName { get; set; }
        /// <summary>
        /// The name of the asset as it's commonly known
        /// </summary>
        public string CommonAssetName { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public AssetAlias(string exchangeName, string commonName, AliasType type = AliasType.BothWays)
        {
            ExchangeAssetName = exchangeName;
            CommonAssetName = commonName;
            Type = type;
        }
    }

    /// <summary>
    /// Alias type
    /// </summary>
    public enum AliasType
    {
        /// <summary>
        /// Translate both from and to exchange
        /// </summary>
        BothWays,
        /// <summary>
        /// Only translate when converting to exchange
        /// </summary>
        OnlyToExchange
    }
}

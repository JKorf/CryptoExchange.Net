using System;
using System.Linq;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Exchange configuration for asset aliases
    /// </summary>
    public class AssetAliasConfiguration
    {
        /// <summary>
        /// Defined aliases
        /// </summary>
        public AssetAlias[] Aliases { get; set; } = [];

        /// <summary>
        /// Auto convert asset names when using the Shared interfaces. Defaults to true
        /// </summary>
        public bool AutoConvertEnabled { get; set; } = true;

        /// <summary>
        /// Map the common name to an exchange name for an asset. If there is no alias the input name is returned
        /// </summary>
        public string CommonToExchangeName(string commonName) => 
            !AutoConvertEnabled ? commonName : Aliases.FirstOrDefault(x => x.CommonAssetName.Equals(commonName, StringComparison.InvariantCulture))?.ExchangeAssetName ?? commonName;

        /// <summary>
        /// Map the exchange name to a common name for an asset. If there is no alias the input name is returned
        /// </summary>
        public string ExchangeToCommonName(string exchangeName)
        {
            if (!AutoConvertEnabled)
                return exchangeName;

            var alias = Aliases.FirstOrDefault(x => x.ExchangeAssetName.Equals(exchangeName, StringComparison.InvariantCulture));
            if (alias == null || alias.Type == AliasType.OnlyToExchange)
                return exchangeName;

            return alias.CommonAssetName;
        }

    }
}

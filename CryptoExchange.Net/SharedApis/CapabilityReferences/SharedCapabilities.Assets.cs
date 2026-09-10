using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Catalog of Shared API capability references.
    /// </summary>
    public partial class SharedCapabilities
    {
        /// <summary>
        /// Asset capabilities.
        /// </summary>
        public static class Assets
        {
            /// <summary>
            /// Get asset capability.
            /// </summary>
            public static SharedRestCapability<IGetAsset, IGetAssetRest> GetAsset { get; } = new();
            /// <summary>
            /// Get all assets capability.
            /// </summary>
            public static SharedRestCapability<IGetAllAssets, IGetAllAssetsRest> GetAllAssets { get; } = new();
        }
    }
}

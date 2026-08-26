using CryptoExchange.Net.Objects;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Shared interfaces utilities
    /// </summary>
    public static class SharedUtils
    {
        /// <summary>
        /// Get client information including supported features
        /// </summary>
        public static SharedClientInfo GetClientInfo(PlatformInfo platformInfo, ISharedApi client)
        {
            return new SharedClientInfo
            {
                Exchange = client.Exchange,
                TypeName = client.GetType().Name,
                SupportedEnvironments = platformInfo.SupportedEnvironments,
                SupportedTradingModes = client.SupportedTradingModes,
                CentralizationType = platformInfo.CentralizationType,
                Capabilities = client.Capabilities.Where(x => x.Supported).ToArray()
            };
        }

        /// <summary>
        /// Apply symbols request filter for asset type and trading mode
        /// </summary>
        public static T[] ApplySymbolFilter<T>(T[] symbols, GetSymbolsRequest request) where T : SharedSpotSymbol
        {
            IEnumerable<T> resultData = symbols;
            if (request.TradingMode != null)
                resultData = resultData.Where(x => x.TradingMode == request.TradingMode);
            if (request.BaseAssetType != null)
                resultData = resultData.Where(x => x.BaseAssetType == request.BaseAssetType);
            if (request.QuoteAssetType != null)
                resultData = resultData.Where(x => x.QuoteAssetType == request.QuoteAssetType);
            if (request.BaseAssetSubType != null)
                resultData = resultData.Where(x => x.BaseAssetSubType == request.BaseAssetSubType);
            if (request.QuoteAssetSubType != null)
                resultData = resultData.Where(x => x.QuoteAssetSubType == request.QuoteAssetSubType);
            return resultData.ToArray();
        }
    }
}

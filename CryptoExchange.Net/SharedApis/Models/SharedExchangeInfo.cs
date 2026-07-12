using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.SharedApis.Models
{
    public class SharedExchangeInfo
    {
        public Dictionary<string, SharedAssetInfo> Assets { get; set; } = new Dictionary<string, SharedAssetInfo>();
        public Dictionary<string, SharedSymbolInfo> Symbols { get; set; } = new Dictionary<string, SharedSymbolInfo>();
    }

    public class SharedAssetInfo
    {
        public string Name { get; set; }
        public SharedAssetType Type { get; set; }
        public SharedAssetSubType? SubType { get; set; }

        public SharedAssetInfo(string name, SharedAssetType type, SharedAssetSubType? subType)
        {
            Name = name;
            Type = type;
            SubType = subType;
        }
    }

    public class SharedSymbolInfo
    {
        public string Name { get; set; }
        public SharedAssetInfo QuoteAsset { get; set; }
        public SharedAssetInfo BaseAsset { get; set; }

        public SharedSymbolInfo(string name, SharedAssetInfo baseAsset, SharedAssetInfo quoteAsset)
        {
            Name = name;
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }
    }
}

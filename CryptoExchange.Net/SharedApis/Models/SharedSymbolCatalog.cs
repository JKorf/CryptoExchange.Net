using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Symbol and asset catalog for a shared client
    /// </summary>
    public class SharedSymbolCatalog
    {
        /// <summary>
        /// Assets supported
        /// </summary>
        public IReadOnlyDictionary<string, SharedAssetInfo> Assets { get; set; } = new Dictionary<string, SharedAssetInfo>();
        /// <summary>
        /// Symbols supported
        /// </summary>
        public IReadOnlyDictionary<string, SharedSymbolInfo> Symbols { get; set; } = new Dictionary<string, SharedSymbolInfo>();
    }

    /// <summary>
    /// Asset info
    /// </summary>
    [DebuggerDisplay("{DebugView,nq}")]
    public class SharedAssetInfo
    {
        private string DebugView => $"{Name} - {Type}{(SubType == null ? "": $" {SubType}")}";

        /// <summary>
        /// Asset name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Asset type
        /// </summary>
        public SharedAssetType Type { get; set; }
        /// <summary>
        /// Asset sub type
        /// </summary>
        public SharedAssetSubType? SubType { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedAssetInfo(string name, SharedAssetType type, SharedAssetSubType? subType)
        {
            Name = name;
            Type = type;
            SubType = subType;
        }
    }

    /// <summary>
    /// Symbol info
    /// </summary>
    [DebuggerDisplay("{Name,nq} BaseAsset: {BaseAsset}, QuoteAsset: {QuoteAsset}")]
    public class SharedSymbolInfo
    {
        /// <summary>
        /// Symbol name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Base asset info
        /// </summary>
        public SharedAssetInfo BaseAsset { get; set; }
        /// <summary>
        /// Quote asset info
        /// </summary>
        public SharedAssetInfo QuoteAsset { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public SharedSymbolInfo(string name, SharedAssetInfo baseAsset, SharedAssetInfo quoteAsset)
        {
            Name = name;
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }
    }
}

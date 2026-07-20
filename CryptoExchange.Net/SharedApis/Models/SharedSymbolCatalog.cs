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
        /// Exchange name
        /// </summary>
        public string Exchange { get; set; } = string.Empty;
        /// <summary>
        /// Assets supported
        /// </summary>
        public IReadOnlyDictionary<string, SharedAssetInfo> Assets { get; set; } = new Dictionary<string, SharedAssetInfo>();
        /// <summary>
        /// Symbols supported
        /// </summary>
        public IReadOnlyDictionary<string, SharedSpotSymbol> Symbols { get; set; } = new Dictionary<string, SharedSpotSymbol>();
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
}

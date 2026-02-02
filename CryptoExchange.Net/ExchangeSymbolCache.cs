using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Cache for symbol parsing
    /// </summary>
    public static class ExchangeSymbolCache
    {
        private static ConcurrentDictionary<string, ExchangeInfo> _symbolInfos = new ConcurrentDictionary<string, ExchangeInfo>();

        /// <summary>
        /// Update the cached symbol data for an exchange
        /// </summary>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="updateData">Symbol data</param>
        public static void UpdateSymbolInfo(string topicId, SharedSpotSymbol[] updateData)
        {
            if(!_symbolInfos.TryGetValue(topicId, out var exchangeInfo))
            {
                exchangeInfo = new ExchangeInfo(DateTime.UtcNow, updateData.ToDictionary(x => x.Name, x => x.SharedSymbol));                
                _symbolInfos.TryAdd(topicId, exchangeInfo);
            }

            if (DateTime.UtcNow - exchangeInfo.UpdateTime < TimeSpan.FromMinutes(60))
                return;

            _symbolInfos[topicId] = new ExchangeInfo(DateTime.UtcNow, updateData.ToDictionary(x => x.Name, x => x.SharedSymbol));
        }

        /// <summary>
        /// Whether the specific topic has been cached
        /// </summary>
        /// <param name="topicId">Id</param>
        public static bool HasCached(string topicId)
        {
            if (!_symbolInfos.TryGetValue(topicId, out var exchangeInfo))
                return false;

            return exchangeInfo.Symbols.Count > 0;
        }

        /// <summary>
        /// Whether a specific exchange(topic) support the provided symbol
        /// </summary>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="symbolName">The symbol name</param>
        public static bool SupportsSymbol(string topicId, string symbolName)
        {
            if (!_symbolInfos.TryGetValue(topicId, out var exchangeInfo))
                return false;

            if (!exchangeInfo.Symbols.TryGetValue(symbolName, out var symbolInfo))
                return false;

            return true;
        }

        /// <summary>
        /// Whether a specific exchange(topic) support the provided symbol
        /// </summary>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="symbol">The symbol info</param>
        public static bool SupportsSymbol(string topicId, SharedSymbol symbol)
        {
            if (!_symbolInfos.TryGetValue(topicId, out var exchangeInfo))
                return false;

            return exchangeInfo.Symbols.Any(x => 
                x.Value.TradingMode == symbol.TradingMode 
                && x.Value.BaseAsset == symbol.BaseAsset 
                && x.Value.QuoteAsset == symbol.QuoteAsset);
        }

        /// <summary>
        /// Get all symbols for a specific base asset
        /// </summary>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="baseAsset">Base asset name</param>
        public static SharedSymbol[] GetSymbolsForBaseAsset(string topicId, string baseAsset)
        {
            if (!_symbolInfos.TryGetValue(topicId, out var exchangeInfo))
                return [];

            return exchangeInfo.Symbols
                .Where(x => x.Value.BaseAsset.Equals(baseAsset, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Value)
                .ToArray();
        }

        /// <summary>
        /// Parse a symbol name to a SharedSymbol
        /// </summary>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="symbolName">Symbol name</param>
        public static SharedSymbol? ParseSymbol(string topicId, string? symbolName)
        {
            if (symbolName == null)
                return null;

            if (!_symbolInfos.TryGetValue(topicId, out var exchangeInfo))
                return null;

            if (!exchangeInfo.Symbols.TryGetValue(symbolName, out var symbolInfo))
                return null;

            return new SharedSymbol(symbolInfo.TradingMode, symbolInfo.BaseAsset, symbolInfo.QuoteAsset, symbolName)
            {
                DeliverTime = symbolInfo.DeliverTime
            };
        }

        class ExchangeInfo
        {
            public DateTime UpdateTime { get; set; }
            public Dictionary<string, SharedSymbol> Symbols { get; set; }

            public ExchangeInfo(DateTime updateTime, Dictionary<string, SharedSymbol> symbols)
            {
                UpdateTime = updateTime;
                Symbols = symbols;
            }
        }
    }
}

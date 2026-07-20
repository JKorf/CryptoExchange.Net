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
        private static ConcurrentDictionary<string, ExchangeKeyedCache> _symbolInfos = new ConcurrentDictionary<string, ExchangeKeyedCache>();

        /// <summary>
        /// Update the cached symbol data for an exchange
        /// </summary>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="environment">Trading environment</param>
        /// <param name="key">Optional data set key</param>
        /// <param name="updateData">Symbol data</param>
        public static void UpdateSymbolInfo(string topicId, string environment, string? key, SharedSpotSymbol[] updateData)
        {
            var id = topicId + environment;
            if(!_symbolInfos.TryGetValue(id, out var exchangeInfo))
            {
                exchangeInfo = new ExchangeKeyedCache();                
                _symbolInfos.TryAdd(id, exchangeInfo);
            }

            var keyedCache = exchangeInfo.Get(key);
            if (keyedCache != null && DateTime.UtcNow - keyedCache.UpdateTime < TimeSpan.FromMinutes(60))
                return;

            exchangeInfo.Set(key, new ExchangeInfo(DateTime.UtcNow, updateData.ToDictionary(x => x.Name, x => x)));
        }

        /// <summary>
        /// Whether the specific topic has been cached
        /// </summary>
        /// <param name="topicId">Id</param>
        /// <param name="environment">Trading environment</param>
        /// <param name="key">Optional data set key</param>
        public static bool HasCached(string topicId, string environment, string? key)
        {
            var id = topicId + environment;
            if (!_symbolInfos.TryGetValue(id, out var exchangeInfo))
                return false;

            return exchangeInfo.HasCached(key);
        }

        /// <summary>
        /// Whether a specific exchange(topic) support the provided symbol
        /// </summary>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="environment">Trading environment</param>
        /// <param name="key">Optional data set key</param>
        /// <param name="symbolName">The symbol name</param>
        public static bool SupportsSymbol(string topicId, string environment, string? key, string symbolName)
        {
            var id = topicId + environment;
            if (!_symbolInfos.TryGetValue(id, out var exchangeInfo))
                return false;

            return exchangeInfo.SupportsSymbol(key, symbolName);
        }

        /// <summary>
        /// Whether a specific exchange(topic) support the provided symbol
        /// </summary>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="environment">Trading environment</param>
        /// <param name="key">Optional data set key</param>
        /// <param name="symbol">The symbol info</param>
        public static bool SupportsSymbol(string topicId, string environment, string? key,  SharedSymbol symbol)
        {
            var id = topicId + environment;
            if (!_symbolInfos.TryGetValue(id, out var exchangeInfo))
                return false;

            return exchangeInfo.SupportsSymbol(key, symbol);
        }

        /// <summary>
        /// Get all symbols for a specific base asset
        /// </summary>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="environment">Trading environment</param>
        /// <param name="key">Optional data set key</param>
        /// <param name="baseAsset">Base asset name</param>
        public static SharedSymbol[] GetSymbolsForBaseAsset(string topicId, string environment, string? key, string baseAsset)
        {
            var id = topicId + environment;
            if (!_symbolInfos.TryGetValue(id, out var exchangeInfo))
                return [];

            return exchangeInfo.GetSymbolsForBaseAsset(key, baseAsset);
        }

        /// <summary>
        /// Parse a symbol name to a SharedSymbol
        /// </summary>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="symbolName">Symbol name</param>
        /// <param name="environment">Trade environment</param>
        /// <param name="key">Additional data set identification key</param>
        public static SharedSymbol? ParseSymbol(string topicId, string environment, string? key, string? symbolName)
        {
            if (symbolName == null)
                return null;

            var id = topicId + environment;
            if (!_symbolInfos.TryGetValue(id, out var exchangeInfo))
                return null;

            return exchangeInfo.ParseSymbol(key, symbolName);
        }

        /// <summary>
        /// Get a symbol catalog for a specific exchange(topic) and environment. Only available if <see cref="UpdateSymbolInfo(string, string, string?, SharedSpotSymbol[])"/> has been called previously.
        /// </summary>
        /// <param name="exchange">Exchange name</param>
        /// <param name="topicId">Id for the provided data</param>
        /// <param name="environmentName">Trade environment</param>
        /// <param name="key">Additional data set identification key</param>
        public static SharedSymbolCatalog? GetSymbolCatalog(string exchange, string topicId, string environmentName, string? key)
        {
            var id = topicId + environmentName;
            if (!_symbolInfos.TryGetValue(id, out var exchangeInfo))
                return null;

            return exchangeInfo.GetSymbolCatalog(exchange, key);
        }

        class ExchangeKeyedCache
        {
            private ExchangeInfo? _noKeyCache;
            private ConcurrentDictionary<string, ExchangeInfo> _keyedCache = new ConcurrentDictionary<string, ExchangeInfo>();

            public ExchangeInfo? Get(string? key)
            {
                if (key == null)
                    return _noKeyCache;

                if (_keyedCache.TryGetValue(key, out var exchangeInfo))
                    return exchangeInfo;

                return null;
            }

            public void Set(string? key, ExchangeInfo exchangeInfo)
            {
                if (key == null)
                    _noKeyCache = exchangeInfo;
                else
                    _keyedCache[key] = exchangeInfo;
            }

            public bool HasCached(string? key)
            {
                if (key == null)
                {
                    if (_noKeyCache?.Symbols.Count > 0)
                        return true;

                    foreach (var cache in _keyedCache.Values)
                    {
                        if (cache.Symbols.Count > 0)
                            return true;
                    }

                    return false;
                }

                return _keyedCache.TryGetValue(key, out var exchangeInfo) && exchangeInfo.Symbols.Count > 0;
            }

            public SharedSymbol? ParseSymbol(string? key, string symbolName)
            {
                SharedSpotSymbol? symbolInfo = null;
                if (key == null)
                {
                    if (_noKeyCache != null)
                    {
                        if (!_noKeyCache.Symbols.TryGetValue(symbolName, out symbolInfo))
                            return null;

                        return new SharedSymbol(symbolInfo.TradingMode, symbolInfo.BaseAsset, symbolInfo.QuoteAsset, symbolName)
                        {
                            DeliverTime = (symbolInfo as SharedFuturesSymbol)?.DeliveryTime
                        };
                    }

                    foreach(var cache in _keyedCache.Values)
                    {
                        if (cache.Symbols.TryGetValue(symbolName, out symbolInfo))
                        {
                            return new SharedSymbol(symbolInfo.TradingMode, symbolInfo.BaseAsset, symbolInfo.QuoteAsset, symbolName)
                            {
                                DeliverTime = (symbolInfo as SharedFuturesSymbol)?.DeliveryTime
                            };
                        }
                    }

                    return null;
                }

                var hasKeyedSet = _keyedCache.TryGetValue(key, out var exchangeInfo);
                if (!hasKeyedSet || exchangeInfo == null)
                    return null;

                if (exchangeInfo.Symbols.TryGetValue(symbolName, out symbolInfo))
                {
                    return new SharedSymbol(symbolInfo.TradingMode, symbolInfo.BaseAsset, symbolInfo.QuoteAsset, symbolName)
                    {
                        DeliverTime = (symbolInfo as SharedFuturesSymbol)?.DeliveryTime
                    };
                }

                return null;
            }

            public bool SupportsSymbol(string? key, string symbolName)
            {
                if (key == null)
                {
                    if (_noKeyCache?.Symbols.ContainsKey(symbolName) == true)
                        return true;

                    foreach(var cache in _keyedCache.Values)
                    {
                        if (cache.Symbols.ContainsKey(symbolName))
                            return true;
                    }

                    return false;
                }

                return _keyedCache.TryGetValue(key, out var exchangeInfo) && exchangeInfo.Symbols.ContainsKey(symbolName);
            }

            public bool SupportsSymbol(string? key, SharedSymbol symbol)
            {
                if (key == null)
                {
                    if (_noKeyCache?.Symbols.Any(x =>
                        x.Value.TradingMode == symbol.TradingMode
                        && x.Value.BaseAsset == symbol.BaseAsset
                        && x.Value.QuoteAsset == symbol.QuoteAsset) == true)
                    {
                        return true;
                    }

                    foreach (var cache in _keyedCache.Values)
                    {
                        if (cache.Symbols.Any(x =>
                            x.Value.TradingMode == symbol.TradingMode
                            && x.Value.BaseAsset == symbol.BaseAsset
                            && x.Value.QuoteAsset == symbol.QuoteAsset))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                    
                return _keyedCache.TryGetValue(key, out var exchangeInfo) && exchangeInfo.Symbols.Any(x =>
                    x.Value.TradingMode == symbol.TradingMode
                    && x.Value.BaseAsset == symbol.BaseAsset
                    && x.Value.QuoteAsset == symbol.QuoteAsset);
            }

            public SharedSymbol[] GetSymbolsForBaseAsset(string? key, string baseAsset)
            {
                if (key == null)
                {
                    if (_noKeyCache != null)
                    {
                        return _noKeyCache.Symbols
                            .Where(x => x.Value.BaseAsset.Equals(baseAsset, StringComparison.InvariantCultureIgnoreCase))
                            .Select(x => x.Value.SharedSymbol)
                            .ToArray();
                    }

                    var result = new List<SharedSymbol>();
                    foreach(var cache in _keyedCache.Values)
                    {
                        result.AddRange(cache.Symbols
                            .Where(x => x.Value.BaseAsset.Equals(baseAsset, StringComparison.InvariantCultureIgnoreCase))
                            .Select(x => x.Value.SharedSymbol));
                    }

                    return result.ToArray();
                }

                var hasKeyedSet = _keyedCache.TryGetValue(key, out var exchangeInfo);
                if (!hasKeyedSet || exchangeInfo == null)
                    return [];

                return exchangeInfo.Symbols
                    .Where(x => x.Value.BaseAsset.Equals(baseAsset, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Value.SharedSymbol)
                    .ToArray();
            }

            internal SharedSymbolCatalog? GetSymbolCatalog(string exchange, string? key)
            {
                IEnumerable<SharedSpotSymbol> cachedSymbols;
                if (key == null)
                {
                    if (_noKeyCache != null)
                        cachedSymbols = _noKeyCache.Symbols.Values;
                    else
                        cachedSymbols = _keyedCache.Values.SelectMany(x => x.Symbols.Values);
                }
                else
                {
                    if (!_keyedCache.TryGetValue(key, out var exchangeInfo) || exchangeInfo == null)
                        return null;

                    cachedSymbols = exchangeInfo.Symbols.Values;
                }

                var assets = new Dictionary<string, SharedAssetInfo>();
                var symbols = new Dictionary<string, SharedSpotSymbol>();
                foreach (var symbol in cachedSymbols)
                {
                    if (!assets.TryGetValue(symbol.BaseAsset, out var baseAssetInfo))
                    {
                        baseAssetInfo = new SharedAssetInfo(symbol.BaseAsset, symbol.BaseAssetType, symbol.BaseAssetSubType);
                        assets.Add(symbol.BaseAsset, baseAssetInfo);
                    }

                    if (!assets.TryGetValue(symbol.QuoteAsset, out var quoteAssetInfo))
                    {
                        quoteAssetInfo = new SharedAssetInfo(symbol.QuoteAsset, symbol.QuoteAssetType, symbol.QuoteAssetSubType);
                        assets.Add(symbol.QuoteAsset, quoteAssetInfo);
                    }

                    symbols.Add(symbol.Name, symbol);
                }

                return new SharedSymbolCatalog
                {
                    Exchange = exchange,
                    Assets = assets,
                    Symbols = symbols
                };
            }
        }


        class ExchangeInfo
        {
            public DateTime UpdateTime { get; set; }
            public Dictionary<string, SharedSpotSymbol> Symbols { get; set; }

            public ExchangeInfo(DateTime updateTime, Dictionary<string, SharedSpotSymbol> symbols)
            {
                UpdateTime = updateTime;
                Symbols = symbols;
            }
        }
    }
}

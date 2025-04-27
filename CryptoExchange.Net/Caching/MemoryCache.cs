﻿using System;
using System.Collections.Concurrent;
using System.Linq;

namespace CryptoExchange.Net.Caching
{
    internal class MemoryCache
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();
        private readonly object _lock = new object();

        /// <summary>
        /// Add a new cache entry. Will override an existing entry if it already exists
        /// </summary>
        /// <param name="key">The key identifier</param>
        /// <param name="value">Cache value</param>
        public void Add(string key, object value)
        {
            var cacheItem = new CacheItem(DateTime.UtcNow, value);
            _cache.AddOrUpdate(key, cacheItem, (key, val1) => cacheItem);
        }

        /// <summary>
        /// Get a cached value
        /// </summary>
        /// <param name="key">The key identifier</param>
        /// <param name="maxAge">The max age of the cached entry</param>
        /// <returns>Cached value if it was in cache</returns>
        public object? Get(string key, TimeSpan maxAge)
        {
            foreach (var item in _cache.Where(x => DateTime.UtcNow - x.Value.CacheTime > maxAge).ToList())
                _cache.TryRemove(item.Key, out _);
            
            _cache.TryGetValue(key, out CacheItem? value);
            if (value == null)
                return null;

            return value.Value;
        }

        private class CacheItem
        {
            public DateTime CacheTime { get; }
            public object Value { get; }

            public CacheItem(DateTime cacheTime, object value)
            {
                CacheTime = cacheTime;
                Value = value;
            }
        }
    }
}

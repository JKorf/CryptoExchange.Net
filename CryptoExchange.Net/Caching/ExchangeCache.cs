using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Caching
{
    public class ExchangeCache
    {
        private Dictionary<string, CacheItem> _cache = new Dictionary<string, CacheItem>();
        private Dictionary<string, CacheItemDefinition> _valueDefinitions = new Dictionary<string, CacheItemDefinition>();

        public ExchangeCache(params CacheItemDefinition[] cacheItemDefinitions)
        {
            foreach (var definition in cacheItemDefinitions)
            {
                _valueDefinitions[definition.Key] = definition;
            }
        }

        public void Define<T>(string key, TimeSpan ttl, Func<Task<T>> valueFactory)
        {
            _valueDefinitions ??= new Dictionary<string, CacheItemDefinition>();
            _valueDefinitions.Add(key, new CacheItemDefinition<T>
            {
                Ttl = ttl,
                ValueFactory = valueFactory
            });
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                if (value.ExpireTime > DateTime.UtcNow)
                {
                    if (value is CacheItem<T> typedValue)
                        return typedValue.Value;

                    throw new InvalidCastException($"{key} value can't be cast to {typeof(T)}");
                }

                _cache.Remove(key);
            }

            var definition = _valueDefinitions[key];
            if (definition is not CacheItemDefinition<T> typedDefinition)
                throw new InvalidCastException($"{key} definition can't be cast to {typeof(CacheItemDefinition<T>)}");

            var factValue = await typedDefinition.ValueFactory().ConfigureAwait(false);
            _cache.Add(key, new CacheItem<T> { EntryTime = DateTime.UtcNow, ExpireTime = DateTime.UtcNow + definition.Ttl, Value = factValue });
            return factValue;
        }
    }

    public abstract class CacheItem
    {
        public DateTime EntryTime { get; set; }
        public DateTime? ExpireTime { get; set; }
    }

    public class CacheItem<T> : CacheItem
    {
        public T Value { get; set; }
    }

    public abstract class CacheItemDefinition
    {
        public string Key { get; set; }
        public TimeSpan Ttl { get; set; }
    }

    public class CacheItemDefinition<T> : CacheItemDefinition
    {
        public Func<Task<T>> ValueFactory { get; set; }
    }
}

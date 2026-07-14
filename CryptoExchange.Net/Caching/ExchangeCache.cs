using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Caching
{
    public class ExchangeCache
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();
        private readonly ConcurrentDictionary<string, CacheItemDefinition> _valueDefinitions = new ConcurrentDictionary<string, CacheItemDefinition>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _retrievalLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        public ExchangeCache(params CacheItemDefinition[] cacheItemDefinitions)
        {
            foreach (var definition in cacheItemDefinitions)
            {
                _valueDefinitions[definition.Key] = definition;
            }
        }

        public T? Get<T>(string key, bool ignoreExpireTime = false)
        {
            return TryGetValue<T>(key, ignoreExpireTime, out var value) ? value : default;
        }

        public async Task<T?> GetOrRetrieveAsync<T>(string key)
        {
            if (TryGetValue<T>(key, false, out var cachedValue))
                return cachedValue;

            var retrievalLock = _retrievalLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await retrievalLock.WaitAsync().ConfigureAwait(false);
            try
            {
                // Another caller might've already retrieved the value
                if (TryGetValue<T>(key, false, out cachedValue))
                    return cachedValue;

                _cache.TryRemove(key, out _);

                var definition = _valueDefinitions[key];
                if (definition is not CacheItemDefinition<T> typedDefinition)
                    throw new InvalidCastException($"{key} definition can't be cast to {typeof(CacheItemDefinition<T>)}");

                var factoryValue = await typedDefinition.ValueFactory().ConfigureAwait(false);
                var entryTime = DateTime.UtcNow;
                _cache[key] = new CacheItem<T>
                {
                    EntryTime = entryTime,
                    ExpireTime = entryTime + definition.Ttl,
                    Value = factoryValue
                };
                return factoryValue;
            }
            finally
            {
                retrievalLock.Release();
            }
        }

        private bool TryGetValue<T>(string key, bool ignoreExpireTime, out T? result)
        {
            if (_cache.TryGetValue(key, out var value)
                && (value.ExpireTime > DateTime.UtcNow || ignoreExpireTime))
            {
                if (value is CacheItem<T> typedValue)
                {
                    result = typedValue.Value;
                    return true;
                }

                throw new InvalidCastException($"{key} value can't be cast to {typeof(T)}");
            }

            result = default;
            return false;
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

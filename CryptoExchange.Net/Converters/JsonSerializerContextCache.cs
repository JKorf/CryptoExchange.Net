using System;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// Caching for JsonSerializerContext instances
    /// </summary>
    public static class JsonSerializerContextCache
    {
        private static ConcurrentDictionary<Type, JsonSerializerContext> _cache = new ConcurrentDictionary<Type, JsonSerializerContext>();

        /// <summary>
        /// Get the instance of the provided type T. It will be created if it doesn't exist yet.
        /// </summary>
        /// <typeparam name="T">Implementation type of the JsonSerializerContext</typeparam>
        public static JsonSerializerContext GetOrCreate<T>() where T: JsonSerializerContext, new()
        {
            var contextType = typeof(T);
            if (_cache.TryGetValue(contextType, out var context))
                return context;

            var instance = new T();
            _cache[contextType] = instance;
            return instance;
        }
    }
}

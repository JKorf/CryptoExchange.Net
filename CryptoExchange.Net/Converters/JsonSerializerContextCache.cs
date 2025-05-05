using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters
{
    public static class JsonSerializerContextCache
    {
        private static ConcurrentDictionary<Type, JsonSerializerContext> _cache = new ConcurrentDictionary<Type, JsonSerializerContext>();

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

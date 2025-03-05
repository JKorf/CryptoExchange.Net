using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Serializer options
    /// </summary>
    public static class SerializerOptions
    {
        private static readonly ConcurrentDictionary<JsonSerializerContext, JsonSerializerOptions> _cache = new ConcurrentDictionary<JsonSerializerContext, JsonSerializerOptions>();

        /// <summary>
        /// Get Json serializer settings which includes standard converters for DateTime, bool, enum and number types
        /// </summary>
        public static JsonSerializerOptions WithConverters(JsonSerializerContext typeResolver)
        {
            if (!_cache.TryGetValue(typeResolver, out var options))
            {
                options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = false,
                    Converters =
                    {
                        new DateTimeConverter(),
                        new BoolConverter(),
                        new DecimalConverter(),
                        new IntConverter(),
                        new LongConverter(),
                        new NullableEnumConverterFactory(typeResolver)
                    },
                    TypeInfoResolver = typeResolver,
                };
                _cache.TryAdd(typeResolver, options);
            }

            return options;
        }
    }
}

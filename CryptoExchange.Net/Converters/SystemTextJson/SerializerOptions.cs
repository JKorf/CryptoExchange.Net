using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Serializer options
    /// </summary>
    public static class SerializerOptions
    {
        /// <summary>
        /// Json serializer settings which includes the EnumConverter, DateTimeConverter, BoolConverter and DecimalConverter
        /// </summary>
        public static JsonSerializerOptions WithConverters { get; } = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
            PropertyNameCaseInsensitive = false,
            Converters =
                    {
                        new DateTimeConverter(),
                        new EnumConverter(),
                        new BoolConverter(),
                        new DecimalConverter(),
                        new IntConverter(),
                        new LongConverter()
                    }
        };
    }
}

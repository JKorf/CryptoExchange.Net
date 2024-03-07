using CryptoExchange.Net.Converters.SystemTextJson;
using System.Globalization;
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
        public static JsonSerializerOptions WithConverters {
            get {
                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = false
                };
                options.Converters.Add(new DateTimeConverter());
                options.Converters.Add(new EnumConverter());
                options.Converters.Add(new BoolConverter());
                options.Converters.Add(new DecimalConverter());

                return options;
            }
        }
    }
}

using Newtonsoft.Json;
using System.Globalization;

namespace CryptoExchange.Net.Converters.JsonNet
{
    /// <summary>
    /// Serializer options
    /// </summary>
    public static class SerializerOptions
    {
        /// <summary>
        /// Json serializer settings which includes the EnumConverter, DateTimeConverter and BoolConverter
        /// </summary>
        public static JsonSerializerSettings WithConverters => new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Culture = CultureInfo.InvariantCulture,
            Converters =
            {
                new EnumConverter(),
                new DateTimeConverter(),
                new BoolConverter()
            }
        };

        /// <summary>
        /// Default json serializer settings
        /// </summary>
        public static JsonSerializerSettings Default => new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Culture = CultureInfo.InvariantCulture
        };
    }
}

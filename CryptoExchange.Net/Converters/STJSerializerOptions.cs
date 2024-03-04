using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// Serializer options
    /// </summary>
    public static class STJSerializerOptions
    {
        /// <summary>
        /// Json serializer settings which includes the EnumConverter, DateTimeConverter and BoolConverter
        /// </summary>
        public static JsonSerializerOptions WithConverters {
            get {
                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = false
    
                    //DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    //Culture = CultureInfo.InvariantCulture,
                    //Converters =
                    //{
                    //    new EnumConverter(),
                    //    new DateTimeConverter(),
                    //    new BoolConverter()
                    //}
                };
                options.Converters.Add(new STJDateTimeConverter());
                options.Converters.Add(new STJNullableDateTimeConverter());
                options.Converters.Add(new STJEnumConverter());
                options.Converters.Add(new STJBoolConverter());

                return options;
            }
        }
    }
}

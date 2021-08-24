using System;
using System.Globalization;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// Converter for seconds to datetime
    /// </summary>
    public class TimestampSecondsConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            if (reader.Value is double d)
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(d);

            var t = double.Parse(reader.Value.ToString(), CultureInfo.InvariantCulture);
            // Set ticks instead of seconds or milliseconds, because AddSeconds/AddMilliseconds rounds to nearest millisecond
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)(t * TimeSpan.TicksPerSecond)); 
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteValue((DateTime?)null);
            else
                writer.WriteValue((long)Math.Round(((DateTime)value! - new DateTime(1970, 1, 1)).TotalSeconds));
        }
    }
}

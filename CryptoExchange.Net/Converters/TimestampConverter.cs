using System;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// converter for milliseconds to datetime
    /// </summary>
    public class TimestampConverter : JsonConverter
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

            var t = long.Parse(reader.Value.ToString());
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(t);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if(value == null)
                writer.WriteValue((DateTime?)null);
            else
                writer.WriteValue((long)Math.Round(((DateTime)value - new DateTime(1970, 1, 1)).TotalMilliseconds));
        }
    }
}

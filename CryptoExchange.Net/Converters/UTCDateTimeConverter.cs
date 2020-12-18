using System;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// Converter for utc datetime
    /// </summary>
    public class UTCDateTimeConverter: JsonConverter
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(JsonConvert.SerializeObject(value));
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            DateTime value;
            if (reader.Value is string s)
                value = (DateTime)JsonConvert.DeserializeObject(s)!;
            else
                value = (DateTime) reader.Value;

            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }
    }
}

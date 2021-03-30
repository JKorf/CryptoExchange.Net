using System;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// converter for datetime string (yyyymmdd) to datetime
    /// </summary>
    public class TimestampStringConverter : JsonConverter
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

            var value = reader.Value.ToString();
            if (value.Length == 8)
                return new DateTime(int.Parse(value.Substring(0, 4)), int.Parse(value.Substring(4, 2)), int.Parse(value.Substring(6, 2)), 0, 0, 0, DateTimeKind.Utc);
            else if(value.Length == 6)
                return new DateTime(int.Parse(value.Substring(0, 2)), int.Parse(value.Substring(2, 2)), int.Parse(value.Substring(4, 2)), 0, 0, 0, DateTimeKind.Utc);

            throw new Exception("Unknown datetime value: " + value);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteValue((DateTime?)null);
            else
            {
                var dateTimeValue = (DateTime)value;
                writer.WriteValue(int.Parse($"{dateTimeValue.Year}{dateTimeValue.Month}{dateTimeValue.Day}"));
            }
        }
    }
}

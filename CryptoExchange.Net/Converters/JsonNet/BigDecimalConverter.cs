using System;
using System.Globalization;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters.JsonNet
{
    /// <summary>
    /// Decimal converter that handles overflowing decimal values (by setting it to decimal.MaxValue)
    /// </summary>
    public class BigDecimalConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            if (Nullable.GetUnderlyingType(objectType) != null)
                return Nullable.GetUnderlyingType(objectType) == typeof(decimal);
            return objectType == typeof(decimal);
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                try
                {
                    return decimal.Parse(reader.Value!.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
                }
                catch (OverflowException) 
                {
                    // Value doesn't fit decimal; set it to max value
                    return decimal.MaxValue;
                }
            }

            if (reader.TokenType == JsonToken.String)
            {
                try
                {
                    var value = reader.Value!.ToString();
                    return decimal.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
                }
                catch (OverflowException)
                {
                    // Value doesn't fit decimal; set it to max value
                    return decimal.MaxValue;
                }
            }

            return null;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
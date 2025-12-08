using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Decimal converter
    /// </summary>
    public class DecimalConverter : JsonConverter<decimal?>
    {
        /// <inheritdoc />
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                return ExchangeHelpers.ParseDecimal(value);
            }

            try
            {
                return reader.GetDecimal();
            }
            catch(FormatException)
            {
                // Format issue, assume value is too large
                return decimal.MaxValue;
            }
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteNumberValue(value.Value);
        }
    }
}

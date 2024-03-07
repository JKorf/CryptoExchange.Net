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
                if (string.IsNullOrEmpty(value))
                    return null;
            }

            return reader.GetDecimal();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            writer.WriteNullValue();
        }
    }
}

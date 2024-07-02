using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Int converter
    /// </summary>
    public class LongConverter : JsonConverter<long?>
    {
        /// <inheritdoc />
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrEmpty(value))
                    return null;

                return long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }

            return reader.GetInt64();
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteNumberValue(value.Value);
        }
    }
}

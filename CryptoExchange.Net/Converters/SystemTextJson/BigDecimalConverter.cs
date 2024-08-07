using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Decimal converter that handles overflowing decimal values (by setting it to decimal.MaxValue)
    /// </summary>
    public class BigDecimalConverter : JsonConverter<decimal>
    {
        /// <inheritdoc />
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                try
                {
                    return decimal.Parse(reader.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture);
                }
                catch(OverflowException)
                {
                    // Value doesn't fit decimal, default to max value
                    return decimal.MaxValue;
                }
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
        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}

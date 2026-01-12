using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Bool converter
    /// </summary>
    public class IntBoolConverter : JsonConverter<bool>
    {
        private readonly int _trueValue;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="trueValue">The int value representing the true value</param>
        public IntBoolConverter(int trueValue)
        {
            _trueValue = trueValue;
        }

        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.Number)
                return false;

            return reader.GetDecimal() == _trueValue;              
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(_trueValue);
        }
    }
}

using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    internal class SharedQuantityConverter : SharedQuantityReferenceConverter<SharedQuantity> { }
    internal class SharedOrderQuantityConverter : SharedQuantityReferenceConverter<SharedOrderQuantity> { }

    internal class SharedQuantityReferenceConverter<T> : JsonConverter<T> where T: SharedQuantityReference, new()
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new Exception("");

            reader.Read(); // Start array
            var baseQuantity = reader.TokenType == JsonTokenType.Null ? (decimal?)null : reader.GetDecimal();
            reader.Read();
            var quoteQuantity = reader.TokenType == JsonTokenType.Null ? (decimal?)null : reader.GetDecimal();
            reader.Read();
            var contractQuantity = reader.TokenType == JsonTokenType.Null ? (decimal?)null : reader.GetDecimal();
            reader.Read();

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new Exception("");

            reader.Read(); // End array

            var result = new T();
            result.QuantityInBaseAsset = baseQuantity;
            result.QuantityInQuoteAsset = quoteQuantity;
            result.QuantityInContracts = contractQuantity;
            return result;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            if (value.QuantityInBaseAsset == null)
                writer.WriteNullValue();
            else
                writer.WriteNumberValue(value.QuantityInBaseAsset.Value);

            if (value.QuantityInQuoteAsset == null)
                writer.WriteNullValue();
            else
                writer.WriteNumberValue(value.QuantityInQuoteAsset.Value);

            if (value.QuantityInContracts == null)
                writer.WriteNullValue();
            else
                writer.WriteNumberValue(value.QuantityInContracts.Value);
            writer.WriteEndArray();
        }
    }
}

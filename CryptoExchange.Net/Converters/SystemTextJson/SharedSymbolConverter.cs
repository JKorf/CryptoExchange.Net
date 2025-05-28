using CryptoExchange.Net.SharedApis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    internal class SharedSymbolConverter : JsonConverter<SharedSymbol>
    {
        public override SharedSymbol? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new Exception("");

            reader.Read(); // Start array
            var tradingMode = (TradingMode)Enum.Parse(typeof(TradingMode), reader.GetString()!);
            reader.Read();
            var baseAsset = reader.GetString()!;
            reader.Read();
            var quoteAsset = reader.GetString()!;
            reader.Read();
            var timeStr = reader.GetString()!;
            var deliverTime = string.IsNullOrEmpty(timeStr) ? (DateTime?)null : DateTime.Parse(timeStr);
            reader.Read();

            if (reader.TokenType != JsonTokenType.EndArray)
                throw new Exception("");

            reader.Read(); // End array

            return new SharedSymbol(tradingMode, baseAsset, quoteAsset, deliverTime);
        }

        public override void Write(Utf8JsonWriter writer, SharedSymbol value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.TradingMode.ToString());
            writer.WriteStringValue(value.BaseAsset);
            writer.WriteStringValue(value.QuoteAsset);
            writer.WriteStringValue(value.DeliverTime?.ToString());
            writer.WriteEndArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    public class BoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True)
                return true;

            if (reader.TokenType == JsonTokenType.False)
                return false;

            var value = reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.GetInt16().ToString(),
                _ => null
            };

            value = value?.ToLowerInvariant().Trim();
            if (string.IsNullOrEmpty(value))
            {
                Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Received null bool value, but property type is not a nullable bool");
                return false;
            }

            switch (value)
            {
                case "true":
                case "yes":
                case "y":
                case "1":
                case "on":
                    return true;
                case "false":
                case "no":
                case "n":
                case "0":
                case "off":
                case "-1":
                    return false;
            }

            throw new SerializationException($"Can't convert bool value {value}");
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteNullValue();
        }
    }

    public class STJNullableBoolConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString()?.ToLowerInvariant().Trim();
            if (string.IsNullOrEmpty(value))
                return null;

            switch (value)
            {
                case "true":
                case "yes":
                case "y":
                case "1":
                case "on":
                    return true;
                case "false":
                case "no":
                case "n":
                case "0":
                case "off":
                case "-1":
                    return false;
            }

            throw new SerializationException($"Can't convert bool value {value}");
        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            writer.WriteNullValue();
        }
    }
}

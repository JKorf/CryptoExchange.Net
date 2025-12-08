using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Bool converter
    /// </summary>
    public class BoolConverter : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(bool) || typeToConvert == typeof(bool?);
        }

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return typeToConvert == typeof(bool) ? new BoolConverterInner() : new BoolConverterInnerNullable();
        }

        private class BoolConverterInnerNullable : JsonConverter<bool?>
        {
            public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => ReadBool(ref reader, typeToConvert, options);
            
            public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
            {
                if (value is bool boolVal)
                    writer.WriteBooleanValue(boolVal);
                else
                    writer.WriteNullValue();
            }
        }

        private class BoolConverterInner : JsonConverter<bool>
        {
            public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => ReadBool(ref reader, typeToConvert, options) ?? false;

            public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
            {
                writer.WriteBooleanValue(value);
            }
        }

        private static bool? ReadBool(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                if (typeToConvert == typeof(bool))
                    LibraryHelpers.StaticLogger?.LogWarning("Received null or empty bool value, but property type is not a nullable bool. Resolver: {Resolver}", options.TypeInfoResolver?.GetType()?.Name);
                return default;
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

    }
}

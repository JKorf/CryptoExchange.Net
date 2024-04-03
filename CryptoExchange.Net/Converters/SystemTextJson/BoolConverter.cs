using System;
using System.Diagnostics;
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
            Type converterType = typeof(BoolConverterInner<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }

        private class BoolConverterInner<T> : JsonConverter<T>
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => (T)((object?)ReadBool(ref reader, typeToConvert, options) ?? default(T))!;
            
            public bool? ReadBool(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                        Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Received null bool value, but property type is not a nullable bool");
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

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value is bool boolVal)
                    writer.WriteBooleanValue(boolVal);
                else
                    writer.WriteNullValue();
            }
        }

    }
}

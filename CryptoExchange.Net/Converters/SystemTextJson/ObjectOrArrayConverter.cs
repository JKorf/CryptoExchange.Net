using System;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Converter for parsing object or array responses
    /// </summary>
    public class ObjectOrArrayConverter : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert) => true;
        /// <inheritdoc />
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var type = typeof(InternalObjectOrArrayConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(type)!;
        }

        private class InternalObjectOrArrayConverter<T> : JsonConverter<T>
        {
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.StartObject && !typeToConvert.IsArray)
                {
                    // Object to object
                    return JsonDocument.ParseValue(ref reader).Deserialize<T>(options);
                }
                else if (reader.TokenType == JsonTokenType.StartArray && typeToConvert.IsArray)
                {
                    // Array to array
                    return JsonDocument.ParseValue(ref reader).Deserialize<T>(options);
                }
                else if (reader.TokenType == JsonTokenType.StartArray)
                {
                    // Array to object
                    JsonDocument.ParseValue(ref reader).Deserialize<T[]>(options);
                    return default;
                }
                else
                {
                    // Object to array
                    JsonDocument.ParseValue(ref reader);
                    return default;
                }
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }
}

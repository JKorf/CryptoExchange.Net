using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectStringConverter<T> : JsonConverter<T>
    {
        /// <inheritdoc />
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return default;

            return (T?)JsonDocument.Parse(value!).Deserialize(typeof(T));
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value is null)
                writer.WriteStringValue("");

            writer.WriteStringValue(JsonSerializer.Serialize(value, options));
        }
    }
}

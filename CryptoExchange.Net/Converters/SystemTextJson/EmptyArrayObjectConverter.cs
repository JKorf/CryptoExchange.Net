using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Converter mapping to an object but also handles when an empty array is send
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EmptyArrayObjectConverter<T> : JsonConverter<T>
    {
        private static JsonSerializerOptions _defaultConverter = SerializerOptions.WithConverters;

        /// <inheritdoc />
        public override T? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartArray:
                    _ = JsonSerializer.Deserialize<object[]>(ref reader, options);
                    return default;
                case JsonTokenType.StartObject:
                    return JsonSerializer.Deserialize<T>(ref reader, _defaultConverter);
            };

            return default;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, (object?)value, options);
    }
}

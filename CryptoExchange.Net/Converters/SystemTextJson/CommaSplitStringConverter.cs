using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Converter for comma separated string values
    /// </summary>
    public class CommaSplitStringConverter : JsonConverter<string[]>
    {
        /// <inheritdoc />
        public override string[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (string.IsNullOrEmpty(str))
                return [];

            return str!.Split(',').ToArray() ?? [];
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, string[] value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(string.Join(",", value));
        }
    }
}

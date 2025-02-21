using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Converter for comma seperated enum values
    /// </summary>
    public class CommaSplitEnumConverter<T> : JsonConverter<IEnumerable<T>> where T : Enum
    {
        /// <inheritdoc />
        public override IEnumerable<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (reader.GetString()?.Split(',').Select(x => EnumConverter<T>.ParseString(x)).ToArray() ?? new T[0])!;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(string.Join(",", value.Select(x => EnumConverter<T>.GetString(x))));
        }
    }
}

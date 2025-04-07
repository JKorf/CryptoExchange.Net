using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Converter for comma separated enum values
    /// </summary>
#if NET5_0_OR_GREATER
    public class CommaSplitEnumConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)] T> : JsonConverter<T[]> where T : struct, Enum
#else
    public class CommaSplitEnumConverter<T> : JsonConverter<T[]> where T : struct, Enum
#endif

    {
        /// <inheritdoc />
        public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (string.IsNullOrEmpty(str))
                return [];

            return str!.Split(',').Select(x => (T)EnumConverter.ParseString<T>(x)!).ToArray() ?? [];
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, T[] value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(string.Join(",", value.Select(x => EnumConverter.GetString(x))));
        }
    }
}

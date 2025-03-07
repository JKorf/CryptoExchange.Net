using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Replace a value on a string property
    /// </summary>
    public abstract class ReplaceConverter : JsonConverter<string>
    {
        private readonly (string ValueToReplace, string ValueToReplaceWith)[] _replacementSets;

        /// <summary>
        /// ctor
        /// </summary>
        public ReplaceConverter(params string[] replaceSets)
        {
            _replacementSets = replaceSets.Select(x =>
            {
                var split = x.Split(new string[] { "->" }, StringSplitOptions.None);
                if (split.Length != 2)
                    throw new ArgumentException("Invalid replacement config");
                return (split[0], split[1]);
            }).ToArray();
        }

        /// <inheritdoc />
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            foreach (var set in _replacementSets)
                value = value?.Replace(set.ValueToReplace, set.ValueToReplaceWith);
            return value;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue(value);
    }
}

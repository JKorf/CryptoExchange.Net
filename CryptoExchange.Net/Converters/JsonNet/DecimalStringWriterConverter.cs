using Newtonsoft.Json;
using System;
using System.Globalization;

namespace CryptoExchange.Net.Converters.JsonNet
{
    /// <summary>
    /// Converter for serializing decimal values as string
    /// </summary>
    public class DecimalStringWriterConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanRead => false;

        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => objectType == typeof(decimal) || objectType == typeof(decimal?);

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => writer.WriteValue(((decimal?)value)?.ToString(CultureInfo.InvariantCulture) ?? null);
    }
}

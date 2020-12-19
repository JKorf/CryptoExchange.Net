using System;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// Converter for nanoseconds to datetime
    /// </summary>
    public class TimestampNanoSecondsConverter : JsonConverter
    {
        private const decimal ticksPerNanosecond = TimeSpan.TicksPerMillisecond / 1000m / 1000;

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            var nanoSeconds = long.Parse(reader.Value.ToString());
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)Math.Round(nanoSeconds * ticksPerNanosecond));
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue((long)Math.Round(((DateTime)value! - new DateTime(1970, 1, 1)).Ticks / ticksPerNanosecond));
        }
    }
}

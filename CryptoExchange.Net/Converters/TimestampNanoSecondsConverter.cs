using System;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters
{
    public class TimestampNanoSecondsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            var ticksPerNanosecond = (TimeSpan.TicksPerMillisecond / 1000m / 1000);
            var nanoSeconds = long.Parse(reader.Value.ToString());
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long)Math.Round(nanoSeconds * ticksPerNanosecond));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ticksPerNanosecond = (TimeSpan.TicksPerMillisecond / 1000m / 1000);
            writer.WriteValue((long)Math.Round(((DateTime)value - new DateTime(1970, 1, 1)).Ticks / ticksPerNanosecond));
        }
    }
}

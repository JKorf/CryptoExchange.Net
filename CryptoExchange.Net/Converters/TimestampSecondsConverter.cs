using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters
{
    public class TimestampSecondsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var t = long.Parse(reader.Value.ToString());
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(t);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Math.Round((((DateTime)value) - new DateTime(1970, 1, 1)).TotalSeconds));
        }
    }
}

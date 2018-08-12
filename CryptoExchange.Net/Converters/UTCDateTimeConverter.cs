using System;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters
{
    public class UTCDateTimeConverter: JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(JsonConvert.SerializeObject(value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            DateTime value;
            if (reader.Value is string)
                value = (DateTime)JsonConvert.DeserializeObject((string)reader.Value);
            else
                value = (DateTime) reader.Value;

            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }
    }
}

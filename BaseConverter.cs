using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CryptoExchange.Net
{
    public abstract class BaseConverter<T>: JsonConverter
    {
        protected abstract Dictionary<T, string> Mapping { get; }
        private readonly bool quotes;
        
        protected BaseConverter(bool useQuotes)
        {
            quotes = useQuotes;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (quotes)
                writer.WriteValue(Mapping[(T)value]);
            else
                writer.WriteRawValue(Mapping[(T)value]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Mapping.Single(v => v.Value.ToLower() == reader.Value.ToString().ToLower()).Key;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }
    }
}

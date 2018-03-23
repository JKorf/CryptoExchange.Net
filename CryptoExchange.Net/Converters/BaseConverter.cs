using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters
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
            var val = Mapping.SingleOrDefault(v => v.Value == reader.Value.ToString()).Key;
            if (val != null)
                return val;
            return Mapping.Single(v => v.Value.ToLower() == reader.Value.ToString().ToLower()).Key;
        }

        public T ReadString(string data)
        {
            return Mapping.Single(v => v.Value == data).Key;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }
    }
}

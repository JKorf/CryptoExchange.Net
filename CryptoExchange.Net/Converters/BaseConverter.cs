using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters
{
    public abstract class BaseConverter<T>: JsonConverter
    {
        protected abstract List<KeyValuePair<T, string>> Mapping { get; }
        private readonly bool quotes;
        
        protected BaseConverter(bool useQuotes)
        {
            quotes = useQuotes;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var stringValue = GetValue((T) value);
            if (quotes)
                writer.WriteValue(stringValue);
            else
                writer.WriteRawValue(stringValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            if (!GetValue(reader.Value.ToString(), out var result))
            {
                Debug.WriteLine($"Cannot map enum. Type: {typeof(T)}, Value: {reader.Value}");
                return null;
            }

            return result;
        }

        public T ReadString(string data)
        {
            return Mapping.FirstOrDefault(v => v.Value == data).Key;
        }

        public override bool CanConvert(Type objectType)
        {
            // Check if it is type, or nullable of type
            return objectType == typeof(T) || Nullable.GetUnderlyingType(objectType) == typeof(T);
        }

        private bool GetValue(string value, out T result)
        {
            var mapping = Mapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase));
            if (!mapping.Equals(default(KeyValuePair<T, string>)))
            {
                result = mapping.Key;
                return true;
            }

            result = default(T);
            return false;
        }

        private string GetValue(T value)
        {
            return Mapping.FirstOrDefault(v => v.Key.Equals(value)).Value;
        }
    }
}

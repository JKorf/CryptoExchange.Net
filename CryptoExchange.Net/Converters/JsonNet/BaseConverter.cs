using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters.JsonNet
{
    /// <summary>
    /// Base class for enum converters
    /// </summary>
    /// <typeparam name="T">Type of enum to convert</typeparam>
    public abstract class BaseConverter<T>: JsonConverter where T: struct
    {
        /// <summary>
        /// The enum->string mapping
        /// </summary>
        protected abstract List<KeyValuePair<T, string>> Mapping { get; }
        private readonly bool _quotes;
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="useQuotes"></param>
        protected BaseConverter(bool useQuotes)
        {
            _quotes = useQuotes;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var stringValue = value == null? null: GetValue((T) value);
            if (_quotes)
                writer.WriteValue(stringValue);
            else
                writer.WriteRawValue(stringValue);
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            var stringValue = reader.Value.ToString();
            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            if (!GetValue(stringValue, out var result))
            {
                Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Cannot map enum value. EnumType: {typeof(T)}, Value: {reader.Value}, Known values: {string.Join(", ", Mapping.Select(m => m.Value))}. If you think {reader.Value} should added please open an issue on the Github repo");
                return null;
            }

            return result;
        }

        /// <summary>
        /// Convert a string value
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public T ReadString(string data)
        {
            return Mapping.FirstOrDefault(v => v.Value == data).Key;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            // Check if it is type, or nullable of type
            return objectType == typeof(T) || Nullable.GetUnderlyingType(objectType) == typeof(T);
        }

        private bool GetValue(string value, out T result)
        {
            // Check for exact match first, then if not found fallback to a case insensitive match 
            var mapping = Mapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCulture));
            if(mapping.Equals(default(KeyValuePair<T, string>)))
                mapping = Mapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase));

            if (!mapping.Equals(default(KeyValuePair<T, string>)))
            {
                result = mapping.Key;
                return true;
            }

            result = default;
            return false;
        }

        private string GetValue(T value)
        {
            return Mapping.FirstOrDefault(v => v.Key.Equals(value)).Value;
        }
    }
}

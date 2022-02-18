using CryptoExchange.Net.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CryptoExchange.Net.Converters
{
    /// <summary>
    /// Converter for enum values. Enums entries should be noted with a MapAttribute to map the enum value to a string value
    /// </summary>
    public class EnumConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<Type, List<KeyValuePair<object, string>>> _mapping = new();

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            objectType = Nullable.GetUnderlyingType(objectType) ?? objectType;
            if (!_mapping.TryGetValue(objectType, out var mapping))
                mapping = AddMapping(objectType);

            if (reader.Value == null)
                return null;

            var stringValue = reader.Value.ToString();
            if (string.IsNullOrWhiteSpace(stringValue))
                return null;

            if (!GetValue(objectType, mapping, stringValue, out var result))
            {
                Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Cannot map enum value. EnumType: {objectType.Name}, Value: {reader.Value}, Known values: {string.Join(", ", mapping.Select(m => m.Value))}. If you think {reader.Value} should added please open an issue on the Github repo");
                return null;
            }

            return result;
        }

        private static List<KeyValuePair<object, string>> AddMapping(Type objectType) 
        {
            var mapping = new List<KeyValuePair<object, string>>();
            var enumMembers = objectType.GetMembers();
            foreach (var member in enumMembers)
            {
                var maps = member.GetCustomAttributes(typeof(MapAttribute), false);
                foreach (MapAttribute attribute in maps)
                {
                    foreach (var value in attribute.Values)
                        mapping.Add(new KeyValuePair<object, string>(Enum.Parse(objectType, member.Name), value));
                }
            }
            _mapping.TryAdd(objectType, mapping);
            return mapping;
        }

        private static bool GetValue(Type objectType, List<KeyValuePair<object, string>> enumMapping, string value, out object? result)
        {
            // Check for exact match first, then if not found fallback to a case insensitive match 
            var mapping = enumMapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCulture));
            if (mapping.Equals(default(KeyValuePair<object, string>)))
                mapping = enumMapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase));

            if (!mapping.Equals(default(KeyValuePair<object, string>)))
            {
                result = mapping.Key;
                return true;
            }

            try
            {
                result = Enum.Parse(objectType, value, true);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Get the string value for an enum value using the MapAttribute mapping. When multiple values are mapped for a enum entry the first value will be returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string? GetString<T>(T enumValue)
        {
            var objectType = typeof(T);
            objectType = Nullable.GetUnderlyingType(objectType) ?? objectType;

            if (!_mapping.TryGetValue(objectType, out var mapping))
                mapping = AddMapping(objectType);

            return enumValue == null ? null : (mapping.FirstOrDefault(v => v.Key.Equals(enumValue)).Value ?? enumValue.ToString());            
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var stringValue = GetString(value);
            writer.WriteRawValue(stringValue);
        }
    }
}

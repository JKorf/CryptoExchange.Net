using CryptoExchange.Net.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    public class EnumConverter : JsonConverterFactory
    {
        private static readonly ConcurrentDictionary<Type, List<KeyValuePair<object, string>>> _mapping = new();

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum || Nullable.GetUnderlyingType(typeToConvert)?.IsEnum == true;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(EnumConverterInner<>).MakeGenericType(
                    new Type[] { typeToConvert }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { },
                culture: null)!;

            return converter;
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

        private class EnumConverterInner<T> : JsonConverter<T> where T : Enum
        {
            // TODO
            private bool _warnOnMissingEntry = true;
            private bool _writeAsInt;

            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var enumType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
                if (!_mapping.TryGetValue(enumType, out var mapping))
                    mapping = AddMapping(enumType);

                var stringValue = reader.TokenType switch
                {
                    JsonTokenType.String => reader.GetString(),
                    JsonTokenType.Number => reader.GetInt16().ToString(),
                    _ => null
                };

                if (string.IsNullOrEmpty(stringValue))
                {
                    // Received null value
                    var emptyResult = GetDefaultValue(typeToConvert, enumType);
                    if (emptyResult != null)
                        // If the property we're parsing to isn't nullable there isn't a correct way to return this as null will either throw an exception (.net framework) or the default enum value (dotnet core).
                        Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Received null enum value, but property type is not a nullable enum. EnumType: {enumType.Name}. If you think {enumType.Name} should be nullable please open an issue on the Github repo");

                    return (T?)emptyResult;
                }

                if (!GetValue(enumType, mapping, stringValue!, out var result))
                {
                    var defaultValue = GetDefaultValue(typeToConvert, enumType);
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        if (defaultValue != null)
                            // We received an empty string and have no mapping for it, and the property isn't nullable
                            Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Received empty string as enum value, but property type is not a nullable enum. EnumType: {enumType.Name}. If you think {enumType.Name} should be nullable please open an issue on the Github repo");
                    }
                    else
                    {
                        // We received an enum value but weren't able to parse it.
                        if (_warnOnMissingEntry)
                            Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Cannot map enum value. EnumType: {enumType.Name}, Value: {stringValue}, Known values: {string.Join(", ", mapping.Select(m => m.Value))}. If you think {stringValue} should added please open an issue on the Github repo");
                    }

                    return (T?)defaultValue;
                }

                return (T?)result;
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {

            }

            private static object? GetDefaultValue(Type objectType, Type enumType)
            {
                if (Nullable.GetUnderlyingType(objectType) != null)
                    return null;

                return Activator.CreateInstance(enumType); // return default value
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
                    // If no explicit mapping is found try to parse string
                    result = Enum.Parse(objectType, value, true);
                    return true;
                }
                catch (Exception)
                {
                    result = default;
                    return false;
                }
            }
        }

        /// <summary>
        /// Get the string value for an enum value using the MapAttribute mapping. When multiple values are mapped for a enum entry the first value will be returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("enumValue")]
        public static string? GetString<T>(T enumValue) => GetString(typeof(T), enumValue);


        [return: NotNullIfNotNull("enumValue")]
        private static string? GetString(Type objectType, object? enumValue)
        {
            objectType = Nullable.GetUnderlyingType(objectType) ?? objectType;

            if (!_mapping.TryGetValue(objectType, out var mapping))
                mapping = AddMapping(objectType);

            return enumValue == null ? null : (mapping.FirstOrDefault(v => v.Key.Equals(enumValue)).Value ?? enumValue.ToString());
        }
    }
}

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
    /// <summary>
    /// Converter for enum values. Enums entries should be noted with a MapAttribute to map the enum value to a string value
    /// </summary>
#if NET5_0_OR_GREATER
    public class EnumConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)] T> : JsonConverter<T>
# else
    public class EnumConverter<T> : JsonConverter<T>
#endif
    {
        private static readonly ConcurrentDictionary<Type, List<KeyValuePair<object, string>>> _mapping = new();
        private bool _warnOnMissingEntry = true;
        private bool _writeAsInt;

        /// <summary>
        /// ctor
        /// </summary>
        public EnumConverter() : this(false, true)
        { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="writeAsInt"></param>
        /// <param name="warnOnMissingEntry"></param>
        public EnumConverter(bool writeAsInt, bool warnOnMissingEntry)
        {
            _warnOnMissingEntry = warnOnMissingEntry;
            _writeAsInt = writeAsInt;
        }

        /// <inheritdoc />
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var t = typeof(T);
            var enumType = Nullable.GetUnderlyingType(t) ?? t;
            if (!_mapping.TryGetValue(enumType, out var mapping))
                mapping = AddMapping();

            var stringValue = reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.GetInt16().ToString(),
                JsonTokenType.True => reader.GetBoolean().ToString(),
                JsonTokenType.False => reader.GetBoolean().ToString(),
                JsonTokenType.Null => null,
                _ => throw new Exception("Invalid token type for enum deserialization: " + reader.TokenType)
            };

            if (string.IsNullOrEmpty(stringValue))
            {
                // Received null value
                var emptyResult = GetDefaultValue();
                if (emptyResult != null)
                    // If the property we're parsing to isn't nullable there isn't a correct way to return this as null will either throw an exception (.net framework) or the default enum value (dotnet core).
                    Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Received null enum value, but property type is not a nullable enum. EnumType: {enumType.Name}. If you think {enumType.Name} should be nullable please open an issue on the Github repo");

                return (T?)emptyResult;
            }

            if (!GetValue(enumType, mapping, stringValue!, out var result))
            {
                var defaultValue = GetDefaultValue();
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

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                if (!_writeAsInt)
                {
                    var stringValue = GetString(value);
                    writer.WriteStringValue(stringValue);
                }
                else
                {
                    writer.WriteNumberValue((int)Convert.ChangeType(value, typeof(int)));
                }
            }
        }

        private static object? GetDefaultValue()
        {
            if (Nullable.GetUnderlyingType(typeof(T)) != null)
                return null;

            return Activator.CreateInstance(typeof(T)); // return default value
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

            if (objectType.IsDefined(typeof(FlagsAttribute)))
            {
                var intValue = int.Parse(value);
                result = Enum.ToObject(objectType, intValue);
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

        private static List<KeyValuePair<object, string>> AddMapping()
        {
            var mapping = new List<KeyValuePair<object, string>>();
            var enumMembers = typeof(T).GetFields();
            foreach (var member in enumMembers)
            {
                var maps = member.GetCustomAttributes(typeof(MapAttribute), false);
                foreach (MapAttribute attribute in maps)
                {
                    foreach (var value in attribute.Values)
                        mapping.Add(new KeyValuePair<object, string>(Enum.Parse(typeof(T), member.Name), value));
                }
            }
            _mapping.TryAdd(typeof(T), mapping);
            return mapping;
        }

        /// <summary>
        /// Get the string value for an enum value using the MapAttribute mapping. When multiple values are mapped for a enum entry the first value will be returned
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("enumValue")]
        public static string? GetString(T? enumValue)
        {
            var type = typeof(T);
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (!_mapping.TryGetValue(type, out var mapping))
                mapping = AddMapping();

            return enumValue == null ? null : (mapping.FirstOrDefault(v => v.Key.Equals(enumValue)).Value ?? enumValue.ToString());
        }

        /// <summary>
        /// Get the enum value from a string
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns></returns>
        public static T? ParseString(string value)
        {
            var type = typeof(T);
            if (!_mapping.TryGetValue(type, out var enumMapping))
                enumMapping = AddMapping();

            var mapping = enumMapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCulture));
            if (mapping.Equals(default(KeyValuePair<object, string>)))
                mapping = enumMapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase));

            if (!mapping.Equals(default(KeyValuePair<object, string>)))
            {
                return (T)mapping.Key;
            }

            try
            {
                // If no explicit mapping is found try to parse string
                return (T)Enum.Parse(type, value, true);
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}

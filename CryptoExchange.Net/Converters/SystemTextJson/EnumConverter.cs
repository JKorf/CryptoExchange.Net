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
    public class EnumConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)] T>
#else
    public class EnumConverter<T>
#endif
         : JsonConverter<T>, INullableConverter where T : struct, Enum
    {
        private static List<KeyValuePair<T, string>>? _mapping = null;
        private bool _warnOnMissingEntry = true;
        private bool _writeAsInt;
        private NullableEnumConverter? nullableEnumConverter = null;

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

        internal class NullableEnumConverter : JsonConverter<T?>
        {
            private readonly EnumConverter<T> enumConverter;
            public NullableEnumConverter(EnumConverter<T> enumConverter)
            {
                this.enumConverter = enumConverter;
            }
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return enumConverter.ReadNullable(ref reader, typeToConvert, options, out var isEmptyString);
            }

            public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    enumConverter.Write(writer, value.Value, options);
                }
            }
        }

        /// <inheritdoc />
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var t = ReadNullable(ref reader, typeToConvert, options, out var isEmptyString);
            if (t == null)
            {
                if (isEmptyString)
                {
                    // We received an empty string and have no mapping for it, and the property isn't nullable
                    Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Received empty string as enum value, but property type is not a nullable enum. EnumType: {typeof(T).Name}. If you think {typeof(T).Name} should be nullable please open an issue on the Github repo");
                }
                else
                {
                    Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Received null enum value, but property type is not a nullable enum. EnumType: {typeof(T).Name}. If you think {typeof(T).Name} should be nullable please open an issue on the Github repo");
                }
                return new T(); // return default value
            }
            else
            {
                return t.Value;
            }
        }
        private T? ReadNullable(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, out bool isEmptyString)
        {
            isEmptyString = false;
            var enumType = typeof(T);
            if (_mapping == null)
                _mapping = AddMapping();

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
                return null;
            }

            if (!GetValue(enumType, stringValue!, out var result))
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    isEmptyString = true;
                }
                else
                {
                    // We received an enum value but weren't able to parse it.
                    if (_warnOnMissingEntry)
                        Trace.WriteLine($"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} | Warning | Cannot map enum value. EnumType: {enumType.Name}, Value: {stringValue}, Known values: {string.Join(", ", _mapping.Select(m => m.Value))}. If you think {stringValue} should added please open an issue on the Github repo");
                }

                return null;
            }

            return result;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
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

        //private static T? GetDefaultValue()
        //{
        //    if (Nullable.GetUnderlyingType(typeof(T)) != null)
        //        return null;

        //    return Activator.CreateInstance(typeof(T)); // return default value
        //}

        private static bool GetValue(Type objectType, string value, out T? result)
        {
            if (_mapping != null)
            {
                // Check for exact match first, then if not found fallback to a case insensitive match 
                var mapping = _mapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCulture));
                if (mapping.Equals(default(KeyValuePair<object, string>)))
                    mapping = _mapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase));

                if (!mapping.Equals(default(KeyValuePair<object, string>)))
                {
                    result = mapping.Key;
                    return true;
                }
            }

            if (objectType.IsDefined(typeof(FlagsAttribute)))
            {
                var intValue = int.Parse(value);
                result = (T)Enum.ToObject(objectType, intValue);
                return true;
            }

            try
            {
                // If no explicit mapping is found try to parse string
                result = (T)Enum.Parse(objectType, value, true);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        private static List<KeyValuePair<T, string>> AddMapping()
        {
            var mapping = new List<KeyValuePair<T, string>>();
            var enumType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            var enumMembers = enumType.GetFields();
            foreach (var member in enumMembers)
            {
                var maps = member.GetCustomAttributes(typeof(MapAttribute), false);
                foreach (MapAttribute attribute in maps)
                {
                    foreach (var value in attribute.Values)
                        mapping.Add(new KeyValuePair<T, string>((T)Enum.Parse(enumType, member.Name), value));
                }
            }
            _mapping = mapping;
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
            if (_mapping == null)
                _mapping = AddMapping();

            return enumValue == null ? null : (_mapping.FirstOrDefault(v => v.Key.Equals(enumValue)).Value ?? enumValue.ToString());
        }

        /// <summary>
        /// Get the enum value from a string
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns></returns>
        public static T? ParseString(string value)
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (_mapping == null)
                _mapping = AddMapping();

            var mapping = _mapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCulture));
            if (mapping.Equals(default(KeyValuePair<object, string>)))
                mapping = _mapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase));

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

        public JsonConverter CreateNullableConverter()
        {
            nullableEnumConverter ??= new NullableEnumConverter(this);
            return nullableEnumConverter;
        }
    }
}

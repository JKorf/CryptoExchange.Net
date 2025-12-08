using CryptoExchange.Net.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Static EnumConverter methods
    /// </summary>
    public static class EnumConverter
    {
        /// <summary>
        /// Get the enum value from a string
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns></returns>
#if NET5_0_OR_GREATER
        public static T? ParseString<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)] T>(string value) where T : struct, Enum
#else
        public static T? ParseString<T>(string value) where T : struct, Enum
#endif
            => EnumConverter<T>.ParseString(value);

        /// <summary>
        /// Get the string value for an enum value using the MapAttribute mapping. When multiple values are mapped for a enum entry the first value will be returned
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
#if NET5_0_OR_GREATER
        public static string GetString<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)] T>(T enumValue) where T : struct, Enum
#else
        public static string GetString<T>(T enumValue) where T : struct, Enum
#endif
            => EnumConverter<T>.GetString(enumValue);

        /// <summary>
        /// Get the string value for an enum value using the MapAttribute mapping. When multiple values are mapped for a enum entry the first value will be returned
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("enumValue")]
#if NET5_0_OR_GREATER
        public static string? GetString<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)] T>(T? enumValue) where T : struct, Enum
#else
        public static string? GetString<T>(T? enumValue) where T : struct, Enum
#endif
            => EnumConverter<T>.GetString(enumValue);
    }

    /// <summary>
    /// Converter for enum values. Enums entries should be noted with a MapAttribute to map the enum value to a string value
    /// </summary>
#if NET5_0_OR_GREATER
    public class EnumConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields)] T>
#else
    public class EnumConverter<T>
#endif
         : JsonConverter<T>, INullableConverterFactory where T : struct, Enum
    {
        private static List<KeyValuePair<T, string>>? _mapping = null;
        private NullableEnumConverter? _nullableEnumConverter = null;

        private static ConcurrentBag<string> _unknownValuesWarned = new ConcurrentBag<string>();

        internal class NullableEnumConverter : JsonConverter<T?>
        {
            private readonly EnumConverter<T> _enumConverter;

            public NullableEnumConverter(EnumConverter<T> enumConverter)
            {
                _enumConverter = enumConverter;
            }
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return _enumConverter.ReadNullable(ref reader, typeToConvert, options, out var isEmptyString);
            }

            public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                }
                else
                {
                    _enumConverter.Write(writer, value.Value, options);
                }
            }
        }

        /// <inheritdoc />
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var t = ReadNullable(ref reader, typeToConvert, options, out var isEmptyString);
            if (t == null)
            {
                if (isEmptyString && !_unknownValuesWarned.Contains(null))
                {
                    // We received an empty string and have no mapping for it, and the property isn't nullable
                    LibraryHelpers.StaticLogger?.LogWarning($"Received null or empty enum value, but property type is not a nullable enum. EnumType: {typeof(T).FullName}. If you think {typeof(T).FullName} should be nullable please open an issue on the Github repo");
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
                JsonTokenType.Number => reader.GetInt32().ToString(),
                JsonTokenType.True => reader.GetBoolean().ToString(),
                JsonTokenType.False => reader.GetBoolean().ToString(),
                JsonTokenType.Null => null,
                _ => throw new Exception("Invalid token type for enum deserialization: " + reader.TokenType)
            };

            if (stringValue is null)
                return null;

            if (!GetValue(enumType, stringValue, out var result))
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    isEmptyString = true;
                }
                else
                {
                    // We received an enum value but weren't able to parse it.
                    if (!_unknownValuesWarned.Contains(stringValue))
                    {
                        _unknownValuesWarned.Add(stringValue!);
                        LibraryHelpers.StaticLogger?.LogWarning($"Cannot map enum value. EnumType: {enumType.FullName}, Value: {stringValue}, Known values: {string.Join(", ", _mapping.Select(m => m.Value))}. If you think {stringValue} should added please open an issue on the Github repo");
                    }
                }

                return null;
            }

            return result;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var stringValue = GetString(value);
            writer.WriteStringValue(stringValue);
        }

        private static bool GetValue(Type objectType, string value, out T? result)
        {
            if (_mapping != null)
            {
                // Check for exact match first, then if not found fallback to a case insensitive match 
                var mapping = _mapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCulture));
                if (mapping.Equals(default(KeyValuePair<T, string>)))
                    mapping = _mapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase));

                if (!mapping.Equals(default(KeyValuePair<T, string>)))
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

            if (_unknownValuesWarned.Contains(value))
            {
                // Check if it is an known unknown value
                // Done here to prevent lookup overhead for normal conversions, but prevent expensive exception throwing
                result = default;
                return false;
            }

            if (String.IsNullOrEmpty(value))
            {
                // An empty/null value will always fail when parsing, so just return here
                result = default;
                return false;
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
            if (mapping.Equals(default(KeyValuePair<T, string>)))
                mapping = _mapping.FirstOrDefault(kv => kv.Value.Equals(value, StringComparison.InvariantCultureIgnoreCase));

            if (!mapping.Equals(default(KeyValuePair<T, string>)))
                return mapping.Key;

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

        /// <inheritdoc />
        public JsonConverter CreateNullableConverter()
        {
            _nullableEnumConverter ??= new NullableEnumConverter(this);
            return _nullableEnumConverter;
        }
    }
}

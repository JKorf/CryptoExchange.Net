using CryptoExchange.Net.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif
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
        class EnumMapping
        {
            public T Value { get; set; }
            public string StringValue { get; set; }

            public EnumMapping(T value, string stringValue)
            {
                Value = value;
                StringValue = stringValue;
            }
        }

#if NET8_0_OR_GREATER
        private static FrozenSet<EnumMapping>? _mappingToEnum = null;
        private static FrozenDictionary<T, string>? _mappingToString = null;

        private static bool RunOptimistic => true;
#else
        private static List<EnumMapping>? _mappingToEnum = null;
        private static Dictionary<T, string>? _mappingToString = null;
        
        // In NetStandard the `ValueTextEquals` method used is slower than just string comparing
        // so only bother in newer frameworks
        private static bool RunOptimistic => false;
#endif
        private NullableEnumConverter? _nullableEnumConverter = null;

        private static Type _enumType = typeof(T);
        private static T? _undefinedEnumValue;
        private static bool _hasFlagsAttribute = _enumType.IsDefined(typeof(FlagsAttribute));
        private static ConcurrentBag<string> _unknownValuesWarned = new ConcurrentBag<string>();
        private static ConcurrentBag<string> _notOptimalValuesWarned = new ConcurrentBag<string>();

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
            var t = ReadNullable(ref reader, typeToConvert, options, out var isEmptyStringOrNull);
            if (t != null)
                return t.Value;
            
            if (isEmptyStringOrNull && !_unknownValuesWarned.Contains(null))
            {
                // We received an empty string and have no mapping for it, and the property isn't nullable
                _unknownValuesWarned.Add(null!);
                LibraryHelpers.StaticLogger?.LogWarning($"Received null or empty enum value, but property type is not a nullable enum. EnumType: {typeof(T).FullName}. If you think {typeof(T).FullName} should be nullable please open an issue on the Github repo");
            }

            return GetUndefinedEnumValue();
        }

        private T GetUndefinedEnumValue()
        {
            if (_undefinedEnumValue != null)
                return _undefinedEnumValue.Value;

            var type = typeof(T);
            if (!Enum.IsDefined(type, -9))
                _undefinedEnumValue = (T)Enum.ToObject(type, -9);
            else if (!Enum.IsDefined(type, -99))
                _undefinedEnumValue = (T)Enum.ToObject(type, -99);
            else
                _undefinedEnumValue = (T)Enum.ToObject(type, -999);

            return (T)_undefinedEnumValue;
        }

        private T? ReadNullable(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, out bool isEmptyStringOrNull)
        {
            isEmptyStringOrNull = false;
            if (_mappingToEnum == null)
                CreateMapping();

            if (RunOptimistic)
            {
                var resultOptimistic = GetValueOptimistic(ref reader);
                if (resultOptimistic != null)
                    return resultOptimistic.Value;
            }

            var isNumber = reader.TokenType == JsonTokenType.Number;
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
            {
                isEmptyStringOrNull = true;
                return null;
            }

            if (!GetValue(stringValue, out var result))
            {
                // Note: checking this here and before the GetValue seems redundant but it allows enum mapping for empty strings
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    isEmptyStringOrNull = true;
                }
                else
                {
                    // We received an enum value but weren't able to parse it.
                    if (!_unknownValuesWarned.Contains(stringValue))
                    {
                        _unknownValuesWarned.Add(stringValue!);
                        LibraryHelpers.StaticLogger?.LogWarning($"Cannot map enum value. EnumType: {_enumType.FullName}, Value: {stringValue}, Known values: [{string.Join(", ", _mappingToEnum!.Select(m => $"{m.StringValue}: {m.Value}"))}]. If you think {stringValue} should be added please open an issue on the Github repo");
                    }
                }

                return null;
            }

            if (RunOptimistic && !isNumber)
            {
                if (!_notOptimalValuesWarned.Contains(stringValue))
                {
                    _notOptimalValuesWarned.Add(stringValue!);
                    LibraryHelpers.StaticLogger?.LogTrace($"Enum mapping sub-optimal. EnumType: {_enumType.FullName}, Value: {stringValue}, Known values: [{string.Join(", ", _mappingToEnum!.Select(m => $"{m.StringValue}: {m.Value}"))}]");
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var stringValue = GetString(value);
            writer.WriteStringValue(stringValue);
        }

        /// <summary>
        /// Try to get the enum value based on the string value using the Utf8JsonReader's ValueTextEquals method. 
        /// This is an optimization to avoid string allocations when possible, but can only match case insensitively
        /// </summary>
        private static T? GetValueOptimistic(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.String)
                return null;

            foreach (var item in _mappingToEnum!)
            {
                if (reader.ValueTextEquals(item.StringValue))
                    return item.Value;
            }

            return null;
        }

        private static bool GetValue(string value, out T? result)
        {
            if (_mappingToEnum != null)
            {
                EnumMapping? mapping = null;
                // If we tried the optimistic path first we already know its not case match
                if (!RunOptimistic) 
                {
                    // Try match on full equals
                    foreach (var item in _mappingToEnum)
                    {
                        if (item.StringValue.Equals(value, StringComparison.Ordinal))
                        {
                            mapping = item;
                            break;
                        }
                    }
                }

                // If not found, try matching ignoring case
                if (mapping == null)
                {
                    foreach (var item in _mappingToEnum)
                    {
                        if (item.StringValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                        {
                            mapping = item;
                            break;
                        }
                    }
                }

                if (mapping != null)
                {
                    result = mapping.Value;
                    return true;
                }
            }

            if (_hasFlagsAttribute)
            {
                var intValue = int.Parse(value);
                result = (T)Enum.ToObject(_enumType, intValue);
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
#if NET8_0_OR_GREATER
                result = Enum.Parse<T>(value, true);
#else
                result = (T)Enum.Parse(_enumType, value, true);
#endif
                if (!Enum.IsDefined(_enumType, result))
                {
                    result = default;
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        private static void CreateMapping()
        {
            var mappingStringToEnum = new List<EnumMapping>();
            var mappingEnumToString = new Dictionary<T, string>();

#pragma warning disable IL2080
            var enumMembers = _enumType.GetFields();
#pragma warning restore IL2080
            foreach (var member in enumMembers)
            {
                var maps = member.GetCustomAttributes(typeof(MapAttribute), false);
                foreach (MapAttribute attribute in maps)
                {
                    foreach (var value in attribute.Values)
                    {
#if NET8_0_OR_GREATER
                        var enumVal = Enum.Parse<T>(member.Name);
#else
                        var enumVal = (T)Enum.Parse(_enumType, member.Name);
#endif

                        mappingStringToEnum.Add(new EnumMapping(enumVal, value));
                        if (!mappingEnumToString.ContainsKey(enumVal))
                            mappingEnumToString.Add(enumVal, value);
                    }
                }
            }

#if NET8_0_OR_GREATER
            _mappingToEnum = mappingStringToEnum.ToFrozenSet();
            _mappingToString = mappingEnumToString.ToFrozenDictionary();
#else
            _mappingToEnum = mappingStringToEnum;
            _mappingToString = mappingEnumToString;
#endif
        }

        // For testing purposes only, allows resetting the static mapping and warnings
        internal static void Reset()
        {
            _undefinedEnumValue = null;
            _unknownValuesWarned = new ConcurrentBag<string>();
        }

        /// <summary>
        /// Get the string value for an enum value using the MapAttribute mapping. When multiple values are mapped for a enum entry the first value will be returned
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("enumValue")]
        public static string? GetString(T? enumValue)
        {
            if (_mappingToString == null)
                CreateMapping();

            return enumValue == null ? null : (_mappingToString!.TryGetValue(enumValue.Value, out var str) ? str : enumValue.ToString());
        }

        /// <summary>
        /// Get the enum value from a string
        /// </summary>
        /// <param name="value">String value</param>
        /// <returns></returns>
        public static T? ParseString(string value)
        {
            if (_mappingToEnum == null)
                CreateMapping();

            EnumMapping? mapping = null;
            // Try match on full equals
            foreach(var item in _mappingToEnum!)
            {
                if (item.StringValue.Equals(value, StringComparison.Ordinal))
                {
                    mapping = item;
                    break;
                }
            }

            // If not found, try matching ignoring case
            if (mapping == null)
            {
                foreach (var item in _mappingToEnum)
                {
                    if (item.StringValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                    {
                        mapping = item;
                        break;
                    }
                }
            }

            if (mapping != null)
                return mapping.Value;

            try
            {
#if NET8_0_OR_GREATER
                return Enum.Parse<T>(value, true);
#else
                return (T)Enum.Parse(_enumType, value, true);
#endif
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

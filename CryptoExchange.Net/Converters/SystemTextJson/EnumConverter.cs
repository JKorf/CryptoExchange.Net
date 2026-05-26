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

#if NET8_0_OR_GREATER
        private static FrozenDictionary<string, T>? _mappingToEnum = null;
        private static FrozenDictionary<T, string>? _mappingToString = null;

        private static bool RunOptimistic => true;
#else
        private static Dictionary<string, T>? _mappingToEnum = null;
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

        private const int _optimisticValueCountThreshold = 6;

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

            bool optimisticCheckDone = false;
            if (RunOptimistic)
            {
                var resultOptimistic = GetValueOptimistic(ref reader, ref optimisticCheckDone);
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

            if (!GetValue(stringValue, optimisticCheckDone, out var result))
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
                        LibraryHelpers.StaticLogger?.LogWarning($"Cannot map enum value. EnumType: {_enumType.FullName}, Value: {stringValue}, Known values: [{string.Join(", ", _mappingToEnum!.Select(m => $"{m.Key}: {m.Value}"))}]. If you think {stringValue} should be added please open an issue on the Github repo");
                    }
                }

                return null;
            }

            if (optimisticCheckDone)
            {
                if (!_notOptimalValuesWarned.Contains(stringValue))
                {
                    _notOptimalValuesWarned.Add(stringValue!);
                    LibraryHelpers.StaticLogger?.LogTrace($"Enum mapping sub-optimal. EnumType: {_enumType.FullName}, Value: {stringValue}, Known values: [{string.Join(", ", _mappingToEnum!.Select(m => $"{m.Key}: {m.Value}"))}]");
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
        /// This is an optimization to avoid string allocations when possible, but can only match case sensitively
        /// </summary>
        private static T? GetValueOptimistic(ref Utf8JsonReader reader, ref bool optimisticCheckDone)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                optimisticCheckDone = false;
                return null;
            }

            if (_mappingToEnum!.Count >= _optimisticValueCountThreshold)
            {
                optimisticCheckDone = false;
                return null;
            }

            optimisticCheckDone = true;
            foreach (var item in _mappingToEnum!)
            {
                if (reader.ValueTextEquals(item.Key))
                    return item.Value;
            }

            return null;
        }

        private static bool GetValue(string value, bool optimisticCheckDone, out T? result)
        {
            if (_mappingToEnum == null)
                throw new InvalidOperationException("Enum mapping not initialized");

            T? mapping = null;
            // If we tried the optimistic path first we already know its not case match
            if (!optimisticCheckDone) 
            {
                // Try match on full equals
                foreach (var item in _mappingToEnum)
                {
                    if (item.Key.Equals(value, StringComparison.Ordinal))
                    {
                        mapping = item.Value;
                        break;
                    }
                }
            }

            // If not found, try matching ignoring case
            if (mapping == null)
            {
                foreach (var item in _mappingToEnum)
                {
                    if (item.Key.Equals(value, StringComparison.OrdinalIgnoreCase))
                    {
                        mapping = item.Value;
                        break;
                    }
                }
            }

            if (mapping != null)
            {
                result = mapping;
                return true;
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
            var mappingStringToEnum = new Dictionary<string, T>();
            var mappingEnumToString = new Dictionary<T, string>();

#pragma warning disable IL2080
            var enumMembers = _enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
#pragma warning restore IL2080
            foreach (var member in enumMembers)
            {
                var enumVal = (T)member.GetValue(null)!;
                var maps = member.GetCustomAttributes(typeof(MapAttribute), false);
                foreach (MapAttribute attribute in maps)
                {
                    foreach (var value in attribute.Values)
                    {
                        mappingStringToEnum.Add(value, enumVal);
                        if (!mappingEnumToString.ContainsKey(enumVal))
                            mappingEnumToString.Add(enumVal, value);
                    }
                }
            }

#if NET8_0_OR_GREATER
            _mappingToEnum = mappingStringToEnum.ToFrozenDictionary();
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

            // Try match on full equals
            foreach(var item in _mappingToEnum!)
            {
                if (item.Key.Equals(value, StringComparison.Ordinal))
                    return item.Value;
            }

            // If not found, try matching ignoring case
            foreach (var item in _mappingToEnum)
            {
                if (item.Key.Equals(value, StringComparison.OrdinalIgnoreCase))
                    return item.Value;
            }

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

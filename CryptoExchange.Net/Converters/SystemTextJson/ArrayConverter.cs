using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using CryptoExchange.Net.Attributes;
using System.Collections.Generic;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <summary>
    /// Converter for arrays to objects. Can deserialize data like [0.1, 0.2, "test"] to an object. Mapping is done by marking the class with [JsonConverter(typeof(ArrayConverter))] and the properties
    /// with [ArrayProperty(x)] where x is the index of the property in the array
    /// </summary>
    public class ArrayConverter : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert) => true;

        /// <inheritdoc />
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type converterType = typeof(ArrayConverterInner<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }

        private class ArrayPropertyInfo
        {
            public PropertyInfo PropertyInfo { get; set; } = null!;
            public ArrayPropertyAttribute ArrayProperty { get; set; } = null!;
            public Type? JsonConverterType { get; set; }
            public bool DefaultDeserialization { get; set; }
            public Type TargetType { get; set; } = null!;
        }

        private class ArrayConverterInner<T> : JsonConverter<T>
        {
            private static readonly ConcurrentDictionary<Type, List<ArrayPropertyInfo>> _typeAttributesCache = new ConcurrentDictionary<Type, List<ArrayPropertyInfo>>();
            private static readonly ConcurrentDictionary<Type, JsonSerializerOptions> _converterOptionsCache = new ConcurrentDictionary<Type, JsonSerializerOptions>();

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                    return;
                }

                writer.WriteStartArray();

                var valueType = value.GetType();
                if (!_typeAttributesCache.TryGetValue(valueType, out var typeAttributes))
                    typeAttributes = CacheTypeAttributes(valueType);

                var ordered = typeAttributes.Where(x => x.ArrayProperty != null).OrderBy(p => p.ArrayProperty.Index);
                var last = -1;
                foreach (var prop in ordered)
                {
                    if (prop.ArrayProperty.Index == last)
                        continue;

                    while (prop.ArrayProperty.Index != last + 1)
                    {
                        writer.WriteNullValue();
                        last += 1;
                    }

                    last = prop.ArrayProperty.Index;

                    var objValue = prop.PropertyInfo.GetValue(value);
                    if (objValue == null)
                    {
                        writer.WriteNullValue();
                        continue;
                    }

                    JsonSerializerOptions? typeOptions = null;
                    if (prop.JsonConverterType != null)
                    {
                        var converter = (JsonConverter)Activator.CreateInstance(prop.JsonConverterType);
                        typeOptions = new JsonSerializerOptions();
                        typeOptions.Converters.Clear();
                        typeOptions.Converters.Add(converter);
                    }

                    if (prop.JsonConverterType == null && IsSimple(prop.PropertyInfo.PropertyType))
                    {
                        if (prop.PropertyInfo.PropertyType == typeof(string))
                            writer.WriteStringValue(Convert.ToString(objValue, CultureInfo.InvariantCulture));
                        else
                            writer.WriteRawValue(Convert.ToString(objValue, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, objValue, typeOptions ?? options);
                    }
                }

                writer.WriteEndArray();
            }

            /// <inheritdoc />
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return default;

                var result = Activator.CreateInstance(typeToConvert);
                return (T)ParseObject(ref reader, result, typeToConvert, options);
            }

            private static bool IsSimple(Type type)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // nullable type, check if the nested type is simple.
                    return IsSimple(type.GetGenericArguments()[0]);
                }
                return type.IsPrimitive
                  || type.IsEnum
                  || type == typeof(string)
                  || type == typeof(decimal);
            }

            private static List<ArrayPropertyInfo> CacheTypeAttributes(Type type)
            {
                var attributes = new List<ArrayPropertyInfo>();
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var att = property.GetCustomAttribute<ArrayPropertyAttribute>();
                    if (att == null)
                        continue;

                    attributes.Add(new ArrayPropertyInfo
                    {
                        ArrayProperty = att,
                        PropertyInfo = property,
                        DefaultDeserialization = property.GetCustomAttribute<JsonConversionAttribute>() != null,
                        JsonConverterType = property.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType ?? property.PropertyType.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType,
                        TargetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType
                    });
                }

                _typeAttributesCache.TryAdd(type, attributes);
                return attributes;
            }

            private static object ParseObject(ref Utf8JsonReader reader, object result, Type objectType, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new Exception("Not an array");

                if (!_typeAttributesCache.TryGetValue(objectType, out var attributes))
                    attributes = CacheTypeAttributes(objectType);

                int index = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    var indexAttributes = attributes.Where(a => a.ArrayProperty.Index == index);
                    if (!indexAttributes.Any())
                    {
                        index++;
                        continue;
                    }

                    foreach (var attribute in indexAttributes)
                    {
                        var targetType = attribute.TargetType;
                        object? value = null;
                        if (attribute.JsonConverterType != null)
                        {
                            if (!_converterOptionsCache.TryGetValue(attribute.JsonConverterType, out var newOptions))
                            {
                                var converter = (JsonConverter)Activator.CreateInstance(attribute.JsonConverterType);
                                newOptions = new JsonSerializerOptions
                                {
                                    NumberHandling = SerializerOptions.WithConverters.NumberHandling,
                                    PropertyNameCaseInsensitive = SerializerOptions.WithConverters.PropertyNameCaseInsensitive,
                                    Converters = { converter },
                                };
                                _converterOptionsCache.TryAdd(attribute.JsonConverterType, newOptions);
                            }

                            value = JsonDocument.ParseValue(ref reader).Deserialize(targetType, newOptions);
                        }
                        else if (attribute.DefaultDeserialization)
                        {
                            // Use default deserialization
                            value = JsonDocument.ParseValue(ref reader).Deserialize(targetType, options);
                        }
                        else
                        {
                            value = reader.TokenType switch
                            {
                                JsonTokenType.Null => null,
                                JsonTokenType.False => false,
                                JsonTokenType.True => true,
                                JsonTokenType.String => reader.GetString(),
                                JsonTokenType.Number => reader.GetDecimal(),
                                JsonTokenType.StartObject => JsonSerializer.Deserialize(ref reader, attribute.TargetType, options),
                                _ => throw new NotImplementedException($"Array deserialization of type {reader.TokenType} not supported"),
                            };
                        }

                        if (targetType.IsAssignableFrom(value?.GetType()))
                            attribute.PropertyInfo.SetValue(result, value == null ? null : value);
                        else
                            attribute.PropertyInfo.SetValue(result, value == null ? null : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture));
                    }

                    index++;
                }

                return result;
            }
        }
    }
}

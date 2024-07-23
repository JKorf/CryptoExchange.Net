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


            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                // TODO
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return default;

                var result = Activator.CreateInstance(typeToConvert);
                return (T)ParseObject(ref reader, result, typeToConvert);
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
                        JsonConverterType = property.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType,
                        TargetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType
                    });
                }

                _typeAttributesCache.TryAdd(type, attributes);
                return attributes;
            }

            private static object ParseObject(ref Utf8JsonReader reader, object result, Type objectType)
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

                    var attribute = attributes.SingleOrDefault(a => a.ArrayProperty.Index == index);
                    if (attribute == null)
                    {
                        index++;
                        continue;
                    }

                    var targetType = attribute.TargetType;
                    object? value = null;
                    if (attribute.JsonConverterType != null)
                    {
                        // Has JsonConverter attribute
                        var options = new JsonSerializerOptions();
                        options.Converters.Add((JsonConverter)Activator.CreateInstance(attribute.JsonConverterType));
                        value = JsonDocument.ParseValue(ref reader).Deserialize(targetType, options);
                    }
                    else if (attribute.DefaultDeserialization)
                    {
                        // Use default deserialization
                        value = JsonDocument.ParseValue(ref reader).Deserialize(targetType);
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
                            _ => throw new NotImplementedException($"Array deserialization of type {reader.TokenType} not supported"),
                        };
                    }

                    attribute.PropertyInfo.SetValue(result, value == null ? null : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture));
                    
                    index++;
                }

                return result;
            }
        }
    }
}

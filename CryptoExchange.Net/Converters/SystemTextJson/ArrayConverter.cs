using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.JsonNet;
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
            public PropertyInfo PropertyInfo { get; set; }
            public ArrayPropertyAttribute ArrayProperty { get; set; }
            public Type? JsonConverterType { get; set; }
            public bool DefaultDeserialization { get; set; }
        }

        private class ArrayConverterInner<T> : JsonConverter<T>
        {
            private static readonly ConcurrentDictionary<Type, List<ArrayPropertyInfo>> _typeAttributesCache = new ConcurrentDictionary<Type, List<ArrayPropertyInfo>>();


            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return default;

                //if (objectType == typeof(JToken))
                //    return JToken.Load(reader);

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
                        JsonConverterType = property.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType
                    });
                }

                _typeAttributesCache.TryAdd(type, attributes);
                return attributes;
            }

            private static object ParseObject(ref Utf8JsonReader reader, object result, Type objectType)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new Exception("1");

                if (!_typeAttributesCache.TryGetValue(objectType, out var attributes))
                    attributes = CacheTypeAttributes(objectType);

                int index = 0;
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    var attribute = attributes.SingleOrDefault(a => a.ArrayProperty.Index == index);
                    var targetType = attribute.PropertyInfo.PropertyType;

                    try
                    {
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
                                JsonTokenType.Number => reader.GetDecimal()
                            };
                        }

                        attribute.PropertyInfo.SetValue(result, value == null ? null : Convert.ChangeType(value, attribute.PropertyInfo.PropertyType, CultureInfo.InvariantCulture));
                    }
                    catch (Exception ex)
                    {

                    }

                    index++;
                }

                return result;

                //    foreach (var property in objectType.GetProperties())
                //    {
                //        var attribute = GetCustomAttribute<ArrayPropertyAttribute>(property);

                //        if (attribute == null)
                //            continue;

                //        if (attribute.Index >= arr.Count)
                //            continue;

                //        if (property.PropertyType.BaseType == typeof(Array))
                //        {
                //            var objType = property.PropertyType.GetElementType();
                //            var innerArray = (JArray)arr[attribute.Index];
                //            var count = 0;
                //            if (innerArray.Count == 0)
                //            {
                //                var arrayResult = (IList)Activator.CreateInstance(property.PropertyType, new[] { 0 });
                //                property.SetValue(result, arrayResult);
                //            }
                //            else if (innerArray[0].Type == JTokenType.Array)
                //            {
                //                var arrayResult = (IList)Activator.CreateInstance(property.PropertyType, new[] { innerArray.Count });
                //                foreach (var obj in innerArray)
                //                {
                //                    var innerObj = Activator.CreateInstance(objType!);
                //                    arrayResult[count] = ParseObject((JArray)obj, innerObj, objType!);
                //                    count++;
                //                }
                //                property.SetValue(result, arrayResult);
                //            }
                //            else
                //            {
                //                var arrayResult = (IList)Activator.CreateInstance(property.PropertyType, new[] { 1 });
                //                var innerObj = Activator.CreateInstance(objType!);
                //                arrayResult[0] = ParseObject(innerArray, innerObj, objType!);
                //                property.SetValue(result, arrayResult);
                //            }
                //            continue;
                //        }

                //        var converterAttribute = GetCustomAttribute<JsonConverterAttribute>(property) ?? GetCustomAttribute<JsonConverterAttribute>(property.PropertyType);
                //        var conversionAttribute = GetCustomAttribute<JsonConversionAttribute>(property) ?? GetCustomAttribute<JsonConversionAttribute>(property.PropertyType);

                //        object? value;
                //        if (converterAttribute != null)
                //        {
                //            value = arr[attribute.Index].ToObject(property.PropertyType, new JsonSerializer { Converters = { (JsonConverter)Activator.CreateInstance(converterAttribute.ConverterType) } });
                //        }
                //        else if (conversionAttribute != null)
                //        {
                //            value = arr[attribute.Index].ToObject(property.PropertyType);
                //        }
                //        else
                //        {
                //            value = arr[attribute.Index];
                //        }

                //        if (value != null && property.PropertyType.IsInstanceOfType(value))
                //        {
                //            property.SetValue(result, value);
                //        }
                //        else
                //        {
                //            if (value is JToken token)
                //            {
                //                if (token.Type == JTokenType.Null)
                //                    value = null;

                //                if (token.Type == JTokenType.Float)
                //                    value = token.Value<decimal>();
                //            }

                //            if (value is decimal)
                //            {
                //                property.SetValue(result, value);
                //            }
                //            else if ((property.PropertyType == typeof(decimal)
                //             || property.PropertyType == typeof(decimal?))
                //             && (value != null && value.ToString().IndexOf("e", StringComparison.OrdinalIgnoreCase) >= 0))
                //            {
                //                var v = value.ToString();
                //                if (decimal.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var dec))
                //                    property.SetValue(result, dec);
                //            }
                //            else
                //            {
                //                property.SetValue(result, value == null ? null : Convert.ChangeType(value, property.PropertyType));
                //            }
                //        }
                //    }
                //    return result;
                //}

                ///// <inheritdoc />
                //public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
                //{
                //    if (value == null)
                //        return;

                //    writer.WriteStartArray();
                //    var props = value.GetType().GetProperties();
                //    var ordered = props.OrderBy(p => GetCustomAttribute<ArrayPropertyAttribute>(p)?.Index);

                //    var last = -1;
                //    foreach (var prop in ordered)
                //    {
                //        var arrayProp = GetCustomAttribute<ArrayPropertyAttribute>(prop);
                //        if (arrayProp == null)
                //            continue;

                //        if (arrayProp.Index == last)
                //            continue;

                //        while (arrayProp.Index != last + 1)
                //        {
                //            writer.WriteValue((string?)null);
                //            last += 1;
                //        }

                //        last = arrayProp.Index;
                //        var converterAttribute = GetCustomAttribute<JsonConverterAttribute>(prop);
                //        if (converterAttribute != null)
                //            writer.WriteRawValue(JsonConvert.SerializeObject(prop.GetValue(value), (JsonConverter)Activator.CreateInstance(converterAttribute.ConverterType)));
                //        else if (!IsSimple(prop.PropertyType))
                //            serializer.Serialize(writer, prop.GetValue(value));
                //        else
                //            writer.WriteValue(prop.GetValue(value));
                //    }
                //    writer.WriteEndArray();
                //}

                //private static bool IsSimple(Type type)
                //{
                //    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                //    {
                //        // nullable type, check if the nested type is simple.
                //        return IsSimple(type.GetGenericArguments()[0]);
                //    }
                //    return type.IsPrimitive
                //      || type.IsEnum
                //      || type == typeof(string)
                //      || type == typeof(decimal);
                //}

                //private static T? GetCustomAttribute<T>(MemberInfo memberInfo) where T : Attribute =>
                //    (T?)_attributeByMemberInfoAndTypeCache.GetOrAdd((memberInfo, typeof(T)), tuple => memberInfo.GetCustomAttribute(typeof(T)));

                //private static T? GetCustomAttribute<T>(Type type) where T : Attribute =>
                //    (T?)_attributeByTypeAndTypeCache.GetOrAdd((type, typeof(T)), tuple => type.GetCustomAttribute(typeof(T)));
            }
        }
    }
}

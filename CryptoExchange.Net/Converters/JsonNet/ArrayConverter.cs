using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CryptoExchange.Net.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net.Converters.JsonNet
{
    /// <summary>
    /// Converter for arrays to objects. Can deserialize data like [0.1, 0.2, "test"] to an object. Mapping is done by marking the class with [JsonConverter(typeof(ArrayConverter))] and the properties
    /// with [ArrayProperty(x)] where x is the index of the property in the array
    /// </summary>
    public class ArrayConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<(MemberInfo, Type), Attribute> _attributeByMemberInfoAndTypeCache = new ConcurrentDictionary<(MemberInfo, Type), Attribute>();
        private static readonly ConcurrentDictionary<(Type, Type), Attribute> _attributeByTypeAndTypeCache = new ConcurrentDictionary<(Type, Type), Attribute>();

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return true;
        }
        
        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (objectType == typeof(JToken))
                return JToken.Load(reader);

            var result = Activator.CreateInstance(objectType);
            var arr = JArray.Load(reader);
            return ParseObject(arr, result, objectType);
        }

        private static object ParseObject(JArray arr, object result, Type objectType)
        {
            foreach (var property in objectType.GetProperties())
            {
                var attribute = GetCustomAttribute<ArrayPropertyAttribute>(property);

                if (attribute == null)
                    continue;

                if (attribute.Index >= arr.Count)
                    continue;

                if (property.PropertyType.BaseType == typeof(Array))
                {
                    var objType = property.PropertyType.GetElementType();
                    var innerArray = (JArray)arr[attribute.Index];
                    var count = 0;
                    if (innerArray.Count == 0)
                    {
                        var arrayResult = (IList)Activator.CreateInstance(property.PropertyType, new [] { 0 });
                        property.SetValue(result, arrayResult);
                    }
                    else if (innerArray[0].Type == JTokenType.Array)
                    {
                        var arrayResult = (IList)Activator.CreateInstance(property.PropertyType, new [] { innerArray.Count });
                        foreach (var obj in innerArray)
                        {
                            var innerObj = Activator.CreateInstance(objType!);
                            arrayResult[count] = ParseObject((JArray)obj, innerObj, objType!);
                            count++;
                        }
                        property.SetValue(result, arrayResult);
                    }
                    else
                    {
                        var arrayResult = (IList)Activator.CreateInstance(property.PropertyType, new [] { 1 });
                        var innerObj = Activator.CreateInstance(objType!);
                        arrayResult[0] = ParseObject(innerArray, innerObj, objType!);
                        property.SetValue(result, arrayResult);
                    }
                    continue;
                }

                var converterAttribute = GetCustomAttribute<JsonConverterAttribute>(property) ?? GetCustomAttribute<JsonConverterAttribute>(property.PropertyType);
                var conversionAttribute = GetCustomAttribute<JsonConversionAttribute>(property) ?? GetCustomAttribute<JsonConversionAttribute>(property.PropertyType);

                object? value;
                if (converterAttribute != null)
                {
                    value = arr[attribute.Index].ToObject(property.PropertyType, new JsonSerializer {Converters = {(JsonConverter) Activator.CreateInstance(converterAttribute.ConverterType)}});
                }
                else if (conversionAttribute != null)
                {
                    value = arr[attribute.Index].ToObject(property.PropertyType);
                }
                else
                {
                    value = arr[attribute.Index];
                }

                if (value != null && property.PropertyType.IsInstanceOfType(value))
                {
                    property.SetValue(result, value);
                }
                else
                {
                    if (value is JToken token)
                    {
                        if (token.Type == JTokenType.Null)
                            value = null;

                        if (token.Type == JTokenType.Float)
                            value = token.Value<decimal>();
                    }

                    if (value is decimal)
                    {
                        property.SetValue(result, value);
                    }
                    else if ((property.PropertyType == typeof(decimal)
                     || property.PropertyType == typeof(decimal?))
                     && (value != null && value.ToString().IndexOf("e", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        var v = value.ToString();
                        if (decimal.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var dec))
                            property.SetValue(result, dec);
                    }
                    else
                    {
                        property.SetValue(result, value == null ? null : Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }
            return result;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
                return;

            writer.WriteStartArray();
            var props = value.GetType().GetProperties();
            var ordered = props.OrderBy(p => GetCustomAttribute<ArrayPropertyAttribute>(p)?.Index);

            var last = -1;
            foreach (var prop in ordered)
            {
                var arrayProp = GetCustomAttribute<ArrayPropertyAttribute>(prop);
                if (arrayProp == null)
                    continue;

                if (arrayProp.Index == last)
                    continue;

                while (arrayProp.Index != last + 1)
                {
                    writer.WriteValue((string?)null);
                    last += 1;
                }

                last = arrayProp.Index;
                var converterAttribute = GetCustomAttribute<JsonConverterAttribute>(prop);
                if (converterAttribute != null)
                    writer.WriteRawValue(JsonConvert.SerializeObject(prop.GetValue(value), (JsonConverter)Activator.CreateInstance(converterAttribute.ConverterType)));
                else if (!IsSimple(prop.PropertyType))
                    serializer.Serialize(writer, prop.GetValue(value));
                else
                    writer.WriteValue(prop.GetValue(value));
            }
            writer.WriteEndArray();
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

        private static T? GetCustomAttribute<T>(MemberInfo memberInfo) where T : Attribute =>
            (T?)_attributeByMemberInfoAndTypeCache.GetOrAdd((memberInfo, typeof(T)), tuple => memberInfo.GetCustomAttribute(typeof(T)));

        private static T? GetCustomAttribute<T>(Type type) where T : Attribute =>
            (T?)_attributeByTypeAndTypeCache.GetOrAdd((type, typeof(T)), tuple => type.GetCustomAttribute(typeof(T)));
    }
}

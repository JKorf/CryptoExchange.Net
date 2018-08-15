using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net.Converters
{
    public class ArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = Activator.CreateInstance(objectType);
            var arr = JArray.Load(reader);
            foreach (var property in objectType.GetProperties())
            {
                var attribute =
                    (ArrayPropertyAttribute)property.GetCustomAttribute(typeof(ArrayPropertyAttribute));
                if (attribute == null)
                    continue;

                if (attribute.Index >= arr.Count)
                    continue;

                object value;
                var converterAttribute = (JsonConverterAttribute)property.GetCustomAttribute(typeof(JsonConverterAttribute));
                if (converterAttribute != null)
                    value = arr[attribute.Index].ToObject(property.PropertyType, new JsonSerializer() { Converters = { (JsonConverter)Activator.CreateInstance(converterAttribute.ConverterType) } });
                else
                    value = arr[attribute.Index];

                if (value != null && property.PropertyType.IsInstanceOfType(value))
                    property.SetValue(result, value);
                else
                {
                    if (value is JToken)
                        if (((JToken)value).Type == JTokenType.Null)
                            value = null;

                    if ((property.PropertyType == typeof(decimal) 
                     || property.PropertyType == typeof(decimal?))
                     && (value != null && value.ToString().Contains("e")))
                    {
                        if (decimal.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var dec))
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

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            var props = value.GetType().GetProperties();
            var ordered = props.OrderBy(p => p.GetCustomAttribute<ArrayPropertyAttribute>()?.Index);

            int last = -1;
            foreach (var prop in ordered)
            {
                var arrayProp = prop.GetCustomAttribute<ArrayPropertyAttribute>();
                if (arrayProp == null)
                    continue;

                if (arrayProp.Index == last)
                    continue;

                while (arrayProp.Index != last + 1)
                {
                    writer.WriteValue((string)null);
                    last += 1;
                }

                last = arrayProp.Index;
                var converterAttribute = (JsonConverterAttribute)prop.GetCustomAttribute(typeof(JsonConverterAttribute));
                if(converterAttribute != null)
                    writer.WriteRawValue(JsonConvert.SerializeObject(prop.GetValue(value), (JsonConverter)Activator.CreateInstance(converterAttribute.ConverterType)));
                else if(!IsSimple(prop.PropertyType))
                    writer.WriteValue(JsonConvert.SerializeObject(prop.GetValue(value)));
                else
                    writer.WriteValue(prop.GetValue(value));
            }
            writer.WriteEndArray();
        }

        private bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal));
        }
    }

    public class ArrayPropertyAttribute: Attribute
    {
        public int Index { get; }

        public ArrayPropertyAttribute(int index)
        {
            Index = index;
        }
    }
}

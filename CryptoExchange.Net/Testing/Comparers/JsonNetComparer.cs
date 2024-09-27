using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.JsonNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net.Testing.Comparers
{
    internal class JsonNetComparer
    {
        internal static void CompareData(
            string method,
            object resultData,
            string json,
            string? nestedJsonProperty,
            List<string>? ignoreProperties = null,
            bool userSingleArrayItem = false)
        {
            var resultProperties = resultData.GetType().GetProperties().Select(p => (p, (JsonPropertyAttribute?)p.GetCustomAttributes(typeof(JsonPropertyAttribute), true).SingleOrDefault()));
            var jsonObject = JToken.Parse(json);
            if (nestedJsonProperty != null)
            {
                var nested = nestedJsonProperty.Split('.');
                foreach (var nest in nested)
                {
                    if (int.TryParse(nest, out var index))
                        jsonObject = jsonObject![index];
                    else
                        jsonObject = jsonObject![nest];
                }
            }

            if (userSingleArrayItem)
                jsonObject = ((JArray)jsonObject!)[0];

            if (resultData.GetType().GetInterfaces().Contains(typeof(IDictionary)))
            {
                var dict = (IDictionary)resultData;
                var jObj = (JObject)jsonObject!;
                var properties = jObj.Properties();
                foreach (var dictProp in properties)
                {
                    if (!dict.Contains(dictProp.Name))
                        throw new Exception($"{method}: Dictionary has no value for {dictProp.Name} while input json `{dictProp.Name}` has value {dictProp.Value}");

                    if (dictProp.Value.Type == JTokenType.Object)
                    {
                        // TODO Some additional checking for objects
                        foreach (var prop in ((JObject)dictProp.Value).Properties())
                            CheckObject(method, prop, dict[dictProp.Name]!, ignoreProperties!);
                    }
                    else
                    {
                        if (dict[dictProp.Name] == default && dictProp.Value.Type != JTokenType.Null)
                            // Property value not correct
                            throw new Exception($"{method}: Dictionary entry `{dictProp.Name}` has no value while input json has value {dictProp.Value}");
                    }
                }
            }
            else if (jsonObject!.Type == JTokenType.Array)
            {
                var jObjs = (JArray)jsonObject;
                if (resultData is IEnumerable list)
                {
                    var enumerator = list.GetEnumerator();
                    foreach (var jObj in jObjs)
                    {
                        enumerator.MoveNext();
                        if (jObj.Type == JTokenType.Object)
                        {
                            foreach (var subProp in ((JObject)jObj).Properties())
                            {
                                if (ignoreProperties?.Contains(subProp.Name) == true)
                                    continue;
                                CheckObject(method, subProp, enumerator.Current, ignoreProperties!);
                            }
                        }
                        else if (jObj.Type == JTokenType.Array)
                        {
                            var resultObj = enumerator.Current;
                            if (resultObj is string)
                                // string list
                                continue;

                            var resultProps = resultObj.GetType().GetProperties().Select(p => (p, p.GetCustomAttributes(typeof(ArrayPropertyAttribute), true).Cast<ArrayPropertyAttribute>().SingleOrDefault()));
                            var arrayConverterProperty = resultObj.GetType().GetCustomAttributes(typeof(JsonConverterAttribute), true).FirstOrDefault();
                            var jsonConverter = ((JsonConverterAttribute)arrayConverterProperty!).ConverterType;
                            if (jsonConverter != typeof(ArrayConverter))
                                // Not array converter?
                                continue;

                            int i = 0;
                            foreach (var item in jObj.Children())
                            {
                                var arrayProp = resultProps.Where(p => p.Item2 != null).SingleOrDefault(p => p.Item2!.Index == i).p;
                                if (arrayProp != null)
                                    CheckPropertyValue(method, item, arrayProp.GetValue(resultObj), arrayProp.PropertyType, arrayProp.Name, "Array index " + i, ignoreProperties!);
                                i++;
                            }
                        }
                        else
                        {
                            var value = enumerator.Current;
                            if (value == default && ((JValue)jObj).Type != JTokenType.Null)
                                throw new Exception($"{method}: Array has no value while input json array has value {jObj}");
                        }
                    }
                }
                else
                {
                    var resultProps = resultData.GetType().GetProperties().Select(p => (p, p.GetCustomAttributes(typeof(ArrayPropertyAttribute), true).Cast<ArrayPropertyAttribute>().SingleOrDefault()));
                    int i = 0;
                    foreach (var item in jObjs.Children())
                    {
                        var arrayProp = resultProps.Where(p => p.Item2 != null).SingleOrDefault(p => p.Item2!.Index == i).p;
                        if (arrayProp != null)
                            CheckPropertyValue(method, item, arrayProp.GetValue(resultData), arrayProp.PropertyType, arrayProp.Name, "Array index " + i, ignoreProperties!);
                        i++;
                    }
                }
            }
            else
            {
                foreach (var item in jsonObject)
                {
                    if (item is JProperty prop)
                    {
                        if (ignoreProperties?.Contains(prop.Name) == true)
                            continue;

                        CheckObject(method, prop, resultData, ignoreProperties);
                    }
                }
            }

            Debug.WriteLine($"Successfully validated {method}");
        }

        private static void CheckObject(string method, JProperty prop, object obj, List<string>? ignoreProperties)
        {
            var resultProperties = obj.GetType().GetProperties().Select(p => (p, ((JsonPropertyAttribute?)p.GetCustomAttributes(typeof(JsonPropertyAttribute), true).SingleOrDefault())?.PropertyName));

            // Property has a value
            var property = resultProperties.SingleOrDefault(p => p.PropertyName == prop.Name).p;
            property ??= resultProperties.SingleOrDefault(p => p.p.Name == prop.Name).p;
            property ??= resultProperties.SingleOrDefault(p => p.p.Name.Equals(prop.Name, StringComparison.InvariantCultureIgnoreCase)).p;

            if (property is null)
                // Property not found
                throw new Exception($"{method}: Missing property `{prop.Name}` on `{obj.GetType().Name}`");

            var propertyValue = property.GetValue(obj);
            if (property.GetCustomAttribute<JsonPropertyAttribute>(true)?.ItemConverterType == null)
                CheckPropertyValue(method, prop.Value, propertyValue, property.PropertyType, property.Name, prop.Name, ignoreProperties);
        }

        private static void CheckPropertyValue(string method, JToken propValue, object? propertyValue, Type propertyType, string? propertyName = null, string? propName = null, List<string>? ignoreProperties = null)
        {
            if (propertyValue == default && propValue.Type != JTokenType.Null && !string.IsNullOrEmpty(propValue.ToString()))
            {
                if (propertyType == typeof(DateTime?) && (propValue.ToString() == "" || propValue.ToString() == "0" || propValue.ToString() == "-1"))
                    return;

                // Property value not correct
                if (propValue.ToString() != "0")
                    throw new Exception($"{method}: Property `{propertyName}` has no value while input json `{propName}` has value {propValue}");
            }

            if (propertyValue == default && (propValue.Type == JTokenType.Null || string.IsNullOrEmpty(propValue.ToString())) || propValue.ToString() == "0")
                return;

            if (propertyValue!.GetType().GetInterfaces().Contains(typeof(IDictionary)))
            {
                var dict = (IDictionary)propertyValue;
                var jObj = (JObject)propValue;
                var properties = jObj.Properties();
                foreach (var dictProp in properties)
                {
                    if (!dict.Contains(dictProp.Name))
                        throw new Exception($"{method}: Property `{propertyName}` has no value while input json `{propName}` has value {propValue}");

                    if (dictProp.Value.Type == JTokenType.Object)
                    {
                        CheckObject(method, dictProp, dict[dictProp.Name]!, ignoreProperties);
                    }
                    else
                    {
                        if (dict[dictProp.Name] == default && dictProp.Value.Type != JTokenType.Null)
                            // Property value not correct
                            throw new Exception($"{method}: Dictionary entry `{dictProp.Name}` has no value while input json has value {propValue} for");
                    }
                }
            }
            else if (propertyValue.GetType().GetInterfaces().Contains(typeof(IEnumerable))
                && propertyValue.GetType() != typeof(string))
            {
                var jObjs = (JArray)propValue;
                var list = (IEnumerable)propertyValue;
                var enumerator = list.GetEnumerator();
                foreach (JToken jtoken in jObjs)
                {
                    enumerator.MoveNext();
                    var typeConverter = enumerator.Current.GetType().GetCustomAttributes(typeof(JsonConverterAttribute), true);
                    if (typeConverter.Length != 0 && ((JsonConverterAttribute)typeConverter.First()).ConverterType != typeof(ArrayConverter))
                        // Custom converter for the type, skip
                        continue;

                    if (jtoken.Type == JTokenType.Object)
                    {
                        foreach (var subProp in ((JObject)jtoken).Properties())
                        {
                            if (ignoreProperties?.Contains(subProp.Name) == true)
                                continue;

                            CheckObject(method, subProp, enumerator.Current, ignoreProperties);
                        }
                    }
                    else if (jtoken.Type == JTokenType.Array)
                    {
                        var resultObj = enumerator.Current;
                        var resultProps = resultObj.GetType().GetProperties().Select(p => (p, p.GetCustomAttributes(typeof(ArrayPropertyAttribute), true).Cast<ArrayPropertyAttribute>().SingleOrDefault()));
                        var arrayConverterProperty = resultObj.GetType().GetCustomAttributes(typeof(JsonConverterAttribute), true).FirstOrDefault();
                        var jsonConverter = ((JsonConverterAttribute)arrayConverterProperty!).ConverterType;
                        if (jsonConverter != typeof(ArrayConverter))
                            // Not array converter?
                            continue;

                        int i = 0;
                        foreach (var item in jtoken.Children())
                        {
                            var arrayProp = resultProps.Where(p => p.Item2 != null).SingleOrDefault(p => p.Item2!.Index == i).p;
                            if (arrayProp != null)
                                CheckPropertyValue(method, item, arrayProp.GetValue(resultObj), arrayProp.PropertyType, arrayProp.Name, "Array index " + i, ignoreProperties!);

                            i++;
                        }
                    }
                    else
                    {
                        var value = enumerator.Current;
                        if (value == default && ((JValue)jtoken).Type != JTokenType.Null)
                            throw new Exception($"{method}: Property `{propertyName}` has no value while input json `{propName}` has value {jtoken}");

                        CheckValues(method, propertyName!, propertyType, (JValue)jtoken, value!);
                    }
                }
            }
            else
            {
                if (propValue.Type == JTokenType.Object)
                {
                    foreach (var item in propValue)
                    {
                        if (item is JProperty prop)
                        {
                            if (ignoreProperties?.Contains(prop.Name) == true)
                                continue;

                            CheckObject(method, prop, propertyValue, ignoreProperties);
                        }
                    }
                }
                else if(propValue.Type == JTokenType.Array)
                {
                    var jObjs = (JArray)propValue;
                    if (propertyValue is IEnumerable list)
                    {
                        var enumerator = list.GetEnumerator();
                        foreach (var jObj in jObjs)
                        {
                            if (!enumerator.MoveNext())
                            {
                            }

                            if (jObj.Type == JTokenType.Object)
                            {
                                foreach (var subProp in ((JObject)jObj).Properties())
                                {
                                    if (ignoreProperties?.Contains(subProp.Name) == true)
                                        continue;
                                    CheckObject(method, subProp, enumerator.Current, ignoreProperties!);
                                }
                            }
                            else if (jObj.Type == JTokenType.Array)
                            {
                                var resultObj = enumerator.Current;
                                var resultProps = resultObj.GetType().GetProperties().Select(p => (p, p.GetCustomAttributes(typeof(ArrayPropertyAttribute), true).Cast<ArrayPropertyAttribute>().SingleOrDefault()));
                                var arrayConverterProperty = resultObj.GetType().GetCustomAttributes(typeof(JsonConverterAttribute), true).FirstOrDefault();
                                var jsonConverter = ((JsonConverterAttribute)arrayConverterProperty!).ConverterType;
                                if (jsonConverter != typeof(ArrayConverter))
                                    // Not array converter?
                                    continue;

                                int i = 0;
                                foreach (var item in jObj.Values())
                                {
                                    var arrayProp = resultProps.SingleOrDefault(p => p.Item2!.Index == i).p;
                                    if (arrayProp != null)
                                        CheckPropertyValue(method, item, arrayProp.GetValue(resultObj), arrayProp.PropertyType, arrayProp.Name, "Array index " + i, ignoreProperties!);
                                    i++;
                                }
                            }
                            else
                            {
                                var value = enumerator.Current;
                                if (value == default && ((JValue)jObj).Type != JTokenType.Null)
                                    throw new Exception($"{method}: Array has no value while input json array has value {jObj}");
                            }
                        }
                    }
                    else
                    {
                        var resultProps = propertyValue.GetType().GetProperties().Select(p => (p, p.GetCustomAttributes(typeof(ArrayPropertyAttribute), true).Cast<ArrayPropertyAttribute>().SingleOrDefault()));
                        int i = 0;
                        foreach (var item in jObjs.Children())
                        {
                            var arrayProp = resultProps.Where(p => p.Item2 != null).SingleOrDefault(p => p.Item2!.Index == i).p;
                            if (arrayProp != null)
                                CheckPropertyValue(method, item, arrayProp.GetValue(propertyValue), arrayProp.PropertyType, arrayProp.Name, "Array index " + i, ignoreProperties!);
                            i++;
                        }
                    }
                }
                else
                {
                    CheckValues(method, propertyName!, propertyType, (JValue)propValue, propertyValue);
                }
            }
        }

        private static void CheckValues(string method, string property, Type propertyType, JValue jsonValue, object objectValue)
        {
            if (jsonValue.Type == JTokenType.String)
            {
                if (objectValue is decimal dec)
                {
                    if (jsonValue.Value<decimal>() != dec)
                        throw new Exception($"{method}: {property} not equal: {jsonValue.Value<decimal>()} vs {dec}");
                }
                else if (objectValue is DateTime time)
                {
                    if (time != DateTimeConverter.ParseFromString(jsonValue.Value<string>()!))
                        throw new Exception($"{method}: {property} not equal: {jsonValue.Value<decimal>()} vs {time}");
                }
                else if (propertyType.IsEnum || Nullable.GetUnderlyingType(propertyType)?.IsEnum == true)
                {
                    // TODO enum comparing
                }
                else if (!jsonValue.Value<string>()!.Equals(Convert.ToString(objectValue, CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception($"{method}: {property} not equal: {jsonValue.Value<string>()} vs {objectValue}");
                }
            }
            else if (jsonValue.Type == JTokenType.Integer)
            {
                if (objectValue is DateTime time)
                {
                    if (time != DateTimeConverter.ParseFromLong(jsonValue.Value<long>()!))
                        throw new Exception($"{method}: {property} not equal: {jsonValue.Value<decimal>()} vs {time}");
                }
                else if (propertyType.IsEnum)
                {
                    // TODO enum comparing
                }
                else if (jsonValue.Value<long>() != Convert.ToInt64(objectValue))
                {
                    throw new Exception($"{method}: {property} not equal: {jsonValue.Value<long>()} vs {Convert.ToInt64(objectValue)}");
                }
            }
            else if (jsonValue.Type == JTokenType.Boolean)
            {
                if (objectValue is bool boolVal && jsonValue.Value<bool>() != boolVal)
                    throw new Exception($"{method}: {property} not equal: {jsonValue.Value<bool>()} vs {(bool)objectValue}");

                if (jsonValue.Value<bool>() != bool.Parse(objectValue.ToString()))
                    throw new Exception($"{method}: {property} not equal: {jsonValue.Value<bool>()} vs {(bool)objectValue}");
            }
        }
    }
}

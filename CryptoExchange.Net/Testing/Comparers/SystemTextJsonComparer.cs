using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;

namespace CryptoExchange.Net.Testing.Comparers
{
    internal class SystemTextJsonComparer
    {
        internal static void CompareData(
            string method,
            object? resultData,
            string json,
            string? nestedJsonProperty,
            List<string>? ignoreProperties = null,
            bool userSingleArrayItem = false)
        {
            var jsonObject = JsonDocument.Parse(json).RootElement;
            if (nestedJsonProperty != null)
            {
                var nested = nestedJsonProperty.Split('.');
                foreach (var nest in nested)
                {
                    if (int.TryParse(nest, out var index))
                        jsonObject = jsonObject![index];
                    else
                        jsonObject = jsonObject!.GetProperty(nest);
                }
            }

            if (userSingleArrayItem)
                jsonObject = jsonObject[0];


            if (resultData == null)
            {
                if (jsonObject.ValueKind == JsonValueKind.Null)
                    return;

                if (jsonObject.ValueKind == JsonValueKind.Object && jsonObject.GetPropertyCount() == 0)
                    return;

                throw new Exception("ResultData null");
            }

            if (resultData.GetType().GetInterfaces().Contains(typeof(IDictionary)))
            {
                var dict = (IDictionary)resultData;
                var jObj = jsonObject!;
                foreach (var dictProp in jObj.EnumerateObject())
                {
                    if (!dict.Contains(dictProp.Name))
                        throw new Exception($"{method}: Dictionary has no value for {dictProp.Name} while input json `{dictProp.Name}` has value {dictProp.Value}");

                    if (dictProp.Value.ValueKind == JsonValueKind.Object)
                    {
                        // TODO Some additional checking for objects
                        foreach (var prop in dictProp.Value.EnumerateObject())
                            CheckObject(method, prop, dict[dictProp.Name]!, ignoreProperties!);
                    }
                    else
                    {
                        if (dict[dictProp.Name] == default && dictProp.Value.ValueKind != JsonValueKind.Null)
                        {
                            if (dictProp.Value.ToString() == "")
                                continue;

                            // Property value not correct
                            throw new Exception($"{method}: Dictionary entry `{dictProp.Name}` has no value while input json has value {dictProp.Value}");
                        }
                    }
                }
            }
            else if (jsonObject!.ValueKind == JsonValueKind.Array)
            {
                if (resultData is IEnumerable list)
                {
                    var enumerator = list.GetEnumerator();
                    foreach (var jObj in jsonObject.EnumerateArray())
                    {
                        if (!enumerator.MoveNext())
                        {
                        }

                        if (jObj.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var subProp in jObj.EnumerateObject())
                            {
                                if (ignoreProperties?.Contains(subProp.Name) == true)
                                    continue;
                                CheckObject(method, subProp, enumerator.Current, ignoreProperties!);
                            }
                        }
                        else if (jObj.ValueKind == JsonValueKind.Array)
                        {
                            var resultObj = enumerator.Current;
                            if (resultObj is string)
                                // string list
                                continue;

                            var resultProps = resultObj.GetType().GetProperties().Select(p => (p, p.GetCustomAttributes(typeof(ArrayPropertyAttribute), true).Cast<ArrayPropertyAttribute>().SingleOrDefault()));
                            var arrayConverterProperty = resultObj.GetType().GetCustomAttributes(typeof(JsonConverterAttribute), true).FirstOrDefault();
                            var jsonConverter = ((JsonConverterAttribute)arrayConverterProperty!).ConverterType;
                            if (jsonConverter != typeof(ArrayConverter<>))
                                // Not array converter?
                                continue;

                            int i = 0;
                            foreach (var item in jObj.EnumerateObject())
                            {
                                var arrayProp = resultProps.Where(p => p.Item2 != null).FirstOrDefault(p => p.Item2!.Index == i).p;
                                if (arrayProp != null)
                                    CheckPropertyValue(method, item.Value, arrayProp.GetValue(resultObj), arrayProp.PropertyType, arrayProp.Name, "Array index " + i, ignoreProperties!);
                                i++;
                            }
                        }
                        else
                        {
                            var value = enumerator.Current;
                            if (value == default && jObj.ValueKind != JsonValueKind.Null)
                                throw new Exception($"{method}: Array has no value while input json array has value {jObj}");
                        }
                    }
                }
                else
                {
                    var resultProps = resultData.GetType().GetProperties().Select(p => (p, p.GetCustomAttributes(typeof(ArrayPropertyAttribute), true).Cast<ArrayPropertyAttribute>().SingleOrDefault()));
                    int i = 0;
                    foreach (var item in jsonObject.EnumerateArray())
                    {
                        var arrayProp = resultProps.Where(p => p.Item2 != null).FirstOrDefault(p => p.Item2!.Index == i).p;
                        if (arrayProp != null)
                            CheckPropertyValue(method, item, arrayProp.GetValue(resultData), arrayProp.PropertyType, arrayProp.Name, "Array index " + i, ignoreProperties!);
                        i++;
                    }
                }
            }
            else if (jsonObject.ValueKind == JsonValueKind.Object)
            {
                foreach (var item in jsonObject.EnumerateObject())
                {
                    //if (item is JProperty prop)
                    //{
                        if (ignoreProperties?.Contains(item.Name) == true)
                            continue;

                        CheckObject(method, item, resultData, ignoreProperties);
                    //}
                }
            }
            else
            {
                //?
            }

            Debug.WriteLine($"Successfully validated {method}");
        }

        private static void CheckObject(string method, JsonProperty prop, object obj, List<string>? ignoreProperties)
        {
            var publicProperties = obj.GetType().GetProperties(
                System.Reflection.BindingFlags.Public 
                | System.Reflection.BindingFlags.GetProperty 
                | System.Reflection.BindingFlags.SetProperty
                | System.Reflection.BindingFlags.Instance).Select(p => (p, ((JsonPropertyNameAttribute?)p.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true).SingleOrDefault())?.Name));

            var internalProperties = obj.GetType().GetProperties(
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.GetProperty
                | System.Reflection.BindingFlags.SetProperty
                | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CustomAttributes.Any(x => x.AttributeType == typeof(JsonIncludeAttribute)))
                .Select(p => (p, ((JsonPropertyNameAttribute?)p.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true).SingleOrDefault())?.Name));

            var resultProperties = publicProperties.Concat(internalProperties);

            // Property has a value
            var property = resultProperties.SingleOrDefault(p => p.Name == prop.Name).p;
            property ??= resultProperties.SingleOrDefault(p => p.p.Name == prop.Name).p;

            if (property is null)
                // Property not found
                throw new Exception($"{method}: Missing property `{prop.Name}` on `{obj.GetType().Name}`");

            var getMethod = property.GetGetMethod();
            if (getMethod is null)
                // There is no getter, so probably just a set for an alternative json name
                return;

            var propertyValue = property.GetValue(obj);
            CheckPropertyValue(method, prop.Value, propertyValue, property.PropertyType, property.Name, prop.Name, ignoreProperties);
        }

        private static void CheckPropertyValue(string method, JsonElement propValue, object? propertyValue, Type propertyType, string? propertyName = null, string? propName = null, List<string>? ignoreProperties = null)
        {
            if (propertyValue == default && propValue.ValueKind != JsonValueKind.Null && !string.IsNullOrEmpty(propValue.ToString()))
            {
                if (propertyType == typeof(DateTime?) && (propValue.ToString() == "" || propValue.ToString() == "0" || propValue.ToString() == "-1" || propValue.ToString() == "01/01/0001 00:00:00"))
                    return;

                // Property value not correct
                if (propValue.ToString() != "0")
                    throw new Exception($"{method}: Property `{propertyName}` has no value while input json `{propName}` has value {propValue}");
            }

            if ((propertyValue == default && (propValue.ValueKind == JsonValueKind.Null || string.IsNullOrEmpty(propValue.ToString()))) || propValue.ToString() == "0")
                return;

            if (propertyValue!.GetType().GetInterfaces().Contains(typeof(IDictionary)))
            {
                var dict = (IDictionary)propertyValue;
                foreach (var dictProp in propValue.EnumerateObject())
                {
                    if (!dict.Contains(dictProp.Name))
                        throw new Exception($"{method}: Property `{propertyName}` has no value while input json `{propName}` has value {propValue}");

                    if (dictProp.Value.ValueKind == JsonValueKind.Object)
                    {
                        CheckPropertyValue(method, dictProp.Value, dict[dictProp.Name]!, dict[dictProp.Name]!.GetType(), null, null, ignoreProperties);
                    }
                    else
                    {
                        if (dict[dictProp.Name] == default && dictProp.Value.ValueKind != JsonValueKind.Null)
                            // Property value not correct
                            throw new Exception($"{method}: Dictionary entry `{dictProp.Name}` has no value while input json has value {propValue} for");
                    }
                }
            }
            else if (propertyValue.GetType().GetInterfaces().Contains(typeof(IEnumerable))
                && propertyValue.GetType() != typeof(string))
            {
                if (propValue.ValueKind != JsonValueKind.Array)
                    return;

                var list = (IEnumerable)propertyValue;
                var enumerator = list.GetEnumerator();
                foreach (var jToken in propValue.EnumerateArray())
                {
                    var moved = enumerator.MoveNext();
                    if (!moved)
                        throw new Exception("Enumeration not moved; incorrect amount of results?");

                    var typeConverter = enumerator.Current.GetType().GetCustomAttributes(typeof(JsonConverterAttribute), true);
                    if (typeConverter.Length != 0 && ((JsonConverterAttribute)typeConverter.First()).ConverterType != typeof(ArrayConverter<>))
                        // Custom converter for the type, skip
                        continue;

                    if (jToken.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var subProp in jToken.EnumerateObject())
                        {
                            if (ignoreProperties?.Contains(subProp.Name) == true)
                                continue;

                            CheckObject(method, subProp, enumerator.Current, ignoreProperties);
                        }
                    }
                    else if (jToken.ValueKind == JsonValueKind.Array)
                    {
                        var resultObj = enumerator.Current;
                        var resultProps = resultObj.GetType().GetProperties().Select(p => (p, p.GetCustomAttributes(typeof(ArrayPropertyAttribute), true).Cast<ArrayPropertyAttribute>().SingleOrDefault()));
                        var arrayConverterProperty = resultObj.GetType().GetCustomAttributes(typeof(JsonConverterAttribute), true).FirstOrDefault();
                        var jsonConverter = ((JsonConverterAttribute)arrayConverterProperty!).ConverterType;
                        if (jsonConverter != typeof(ArrayConverter<>))
                            // Not array converter?
                            continue;

                        int i = 0;
                        foreach (var item in jToken.EnumerateArray())
                        {
                            var arrayProp = resultProps.Where(p => p.Item2 != null).FirstOrDefault(p => p.Item2!.Index == i).p;
                            if (arrayProp != null)
                                CheckPropertyValue(method, item, arrayProp.GetValue(resultObj), arrayProp.PropertyType, arrayProp.Name, "Array index " + i, ignoreProperties);

                            i++;
                        }
                    }
                    else
                    {
                        var value = enumerator.Current;
                        if (value == default && jToken.ValueKind != JsonValueKind.Null)
                            throw new Exception($"{method}: Property `{propertyName}` has no value while input json `{propName}` has value {jToken}");

                        CheckValues(method, propertyName!, propertyType, jToken, value!);
                    }
                }
            }
            else
            {
                if (propValue.ValueKind == JsonValueKind.Object)
                {
                    foreach (var item in propValue.EnumerateObject())
                    {
                        //if (item is JProperty prop)
                        //{
                            if (ignoreProperties?.Contains(item.Name) == true)
                                continue;

                            CheckObject(method, item, propertyValue, ignoreProperties);
                        //}
                    }
                }
                else if (propValue.ValueKind == JsonValueKind.Array)
                {
                    if (propertyValue is IEnumerable list)
                    {
                        var enumerator = list.GetEnumerator();
                        foreach (var jObj in propValue.EnumerateArray())
                        {
                            if (!enumerator.MoveNext())
                            {
                            }

                            if (jObj.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var subProp in jObj.EnumerateObject())
                                {
                                    if (ignoreProperties?.Contains(subProp.Name) == true)
                                        continue;
                                    CheckObject(method, subProp, enumerator.Current, ignoreProperties!);
                                }
                            }
                            else if (jObj.ValueKind == JsonValueKind.Array)
                            {
                                var resultObj = enumerator.Current;
                                var resultProps = resultObj.GetType().GetProperties().Select(p => (p, p.GetCustomAttributes(typeof(ArrayPropertyAttribute), true).Cast<ArrayPropertyAttribute>().SingleOrDefault()));
                                var arrayConverterProperty = resultObj.GetType().GetCustomAttributes(typeof(JsonConverterAttribute), true).FirstOrDefault();
                                var jsonConverter = ((JsonConverterAttribute)arrayConverterProperty!).ConverterType;
                                if (jsonConverter != typeof(ArrayConverter<>))
                                    // Not array converter?
                                    continue;

                                int i = 0;
                                foreach (var item in jObj.EnumerateArray())
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
                                if (value == default && jObj.ValueKind != JsonValueKind.Null)
                                    throw new Exception($"{method}: Array has no value while input json array has value {jObj}");
                            }
                        }
                    }
                    else
                    {
                        var resultProps = propertyValue.GetType().GetProperties().Select(p => (p, p.GetCustomAttributes(typeof(ArrayPropertyAttribute), true).Cast<ArrayPropertyAttribute>().SingleOrDefault()));
                        int i = 0;
                        foreach (var item in propValue.EnumerateArray())
                        {
                            var arrayProp = resultProps.Where(p => p.Item2 != null).FirstOrDefault(p => p.Item2!.Index == i).p;
                            if (arrayProp != null)
                                CheckPropertyValue(method, item, arrayProp.GetValue(propertyValue), arrayProp.PropertyType, arrayProp.Name, "Array index " + i, ignoreProperties!);
                            i++;
                        }
                    }
                }
                else
                {
                    CheckValues(method, propertyName!, propertyType, propValue, propertyValue);
                }
            }
        }

        private static void CheckValues(string method, string property, Type propertyType, JsonElement jsonValue, object objectValue)
        {
            if (jsonValue.ValueKind == JsonValueKind.String)
            {
                var stringValue = jsonValue.GetString();
                if (objectValue is decimal dec)
                {
                    if (decimal.Parse(stringValue!, CultureInfo.InvariantCulture) != dec)
                        throw new Exception($"{method}: {property} not equal: {stringValue} vs {dec}");
                }
                else if (objectValue is DateTime time)
                {
                    if (!string.IsNullOrEmpty(stringValue) && time != DateTimeConverter.ParseFromString(stringValue!))
                        throw new Exception($"{method}: {property} not equal: {stringValue} vs {time}");
                }
                else if (objectValue is bool bl)
                {
                    if (bl && (stringValue != "1" && stringValue != "true" && stringValue != "True"))
                        throw new Exception($"{method}: {property} not equal: {stringValue} vs {bl}");
                    if (!bl && (stringValue != "0" && stringValue != "-1" && stringValue != "false" && stringValue != "False"))
                        throw new Exception($"{method}: {property} not equal: {stringValue} vs {bl}");
                }
                else if (propertyType.IsEnum || Nullable.GetUnderlyingType(propertyType)?.IsEnum == true)
                {
                    // TODO enum comparing
                }
                else if (!stringValue!.Equals(Convert.ToString(objectValue, CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new Exception($"{method}: {property} not equal: {stringValue} vs {objectValue}");
                }
            }
            else if (jsonValue.ValueKind == JsonValueKind.Number)
            {
                var value = jsonValue.GetDecimal();
                if (objectValue is DateTime time)
                {
                    if (time != DateTimeConverter.ParseFromDouble((double)value))
                        throw new Exception($"{method}: {property} not equal: {DateTimeConverter.ParseFromDouble((double)value!)} vs {time}");
                }
                else if (propertyType.IsEnum || Nullable.GetUnderlyingType(propertyType)?.IsEnum == true)
                {
                    // TODO enum comparing
                }
                else if(objectValue is decimal dec)
                {
                    if (dec != value)
                        throw new Exception($"{method}: {property} not equal: {dec} vs {value}");
                }
                else if (objectValue is double dbl)
                {
                    if ((decimal)dbl != value)
                        throw new Exception($"{method}: {property} not equal: {dbl} vs {value}");
                }
                else if(objectValue is string objStr)
                {
                    if (objStr != value.ToString())
                        throw new Exception($"{method}: {property} not equal: {value} vs {objStr}");
                }
                else if (value != Convert.ToInt64(objectValue, CultureInfo.InvariantCulture))
                {
                    throw new Exception($"{method}: {property} not equal: {value} vs {Convert.ToInt64(objectValue)}");
                }
            }
            else if (jsonValue.ValueKind == JsonValueKind.True || jsonValue.ValueKind == JsonValueKind.False)
            {
                if (jsonValue.GetBoolean() != (bool)objectValue)
                    throw new Exception($"{method}: {property} not equal: {jsonValue.GetBoolean()} vs {(bool)objectValue}");
            }
        }
    }
}

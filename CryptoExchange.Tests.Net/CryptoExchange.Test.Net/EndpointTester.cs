using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace CryptoExchange.Test.Net
{
    public class EndpointTester<TClient> where TClient : BaseRestClient
    {
        private readonly Func<WebCallResult, bool> _isAuthenticated;
        private readonly string _folder;
        private readonly string _baseAddress;
        private readonly string? _nestedPropertyForCompare;

        public EndpointTester(string folder, string baseAddress, Func<WebCallResult, bool> isAuthenticated, string? nestedPropertyForCompare = null)
        {
            _folder = folder;
            _baseAddress = baseAddress;
            _nestedPropertyForCompare = nestedPropertyForCompare;
            _isAuthenticated = isAuthenticated;
        }

        public async Task ValidateAsync<TResponse>(
            Func<TClient, Task<WebCallResult<TResponse>>> methodInvoke,
            string name,
            string? nestedJsonProperty = null,
            List<string>? ignoreProperties = null)
        {
            var listener = new EnumValueTraceListener();
            Trace.Listeners.Add(listener);
            
            var path = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
            FileStream? file = null;
            try
            {
                file = File.OpenRead(Path.Combine(path, _folder, $"{name}.txt"));
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Response file not found");
            }

            var buffer = new byte[file.Length];
            await file.ReadAsync(buffer, 0, buffer.Length);
            file.Close();

            var data = Encoding.UTF8.GetString(buffer);
            using var reader = new StringReader(data);
            var expectedMethod = reader.ReadLine();
            var expectedPath = reader.ReadLine();
            var expectedAuth = bool.Parse(reader.ReadLine()!);
            var response = reader.ReadToEnd();

            var client = TestHelpers.CreateRestClient<TClient>(response, System.Net.HttpStatusCode.OK);
            var result = await methodInvoke(client);

            // asset
            Assert.That(result.Error, Is.Null, name + " returned error");
            Assert.That(_isAuthenticated(result.AsDataless()), Is.EqualTo(expectedAuth));
            Assert.That(result.RequestMethod, Is.EqualTo(new HttpMethod(expectedMethod!)));
            Assert.That(expectedPath, Is.EqualTo(result.RequestUrl!.Replace(_baseAddress, "").Split(new char[] { '?' })[0]));

            object responseData = result.Data!;
            ProcessData(name, result.Data!, response, nestedJsonProperty, ignoreProperties);
           
            Trace.Listeners.Remove(listener);
        }

        internal void ProcessData(
            string method,
            object resultData,
            string json,
            string nestedJsonProperty,
            List<string>? ignoreProperties = null)
        {
            var resultProperties = resultData.GetType().GetProperties().Select(p => (p, (JsonPropertyAttribute?)p.GetCustomAttributes(typeof(JsonPropertyAttribute), true).SingleOrDefault()));
            var jsonObject = JToken.Parse(json);
            if (_nestedPropertyForCompare != null)
                jsonObject = jsonObject[_nestedPropertyForCompare]!;

            if (nestedJsonProperty != null)
                jsonObject = jsonObject[nestedJsonProperty];

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
                var list = (IEnumerable)resultData;
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
                                CheckPropertyValue(method, item, arrayProp.GetValue(resultObj), arrayProp.Name, "Array index " + i, ignoreProperties!);
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
            var property = resultProperties.SingleOrDefault(p => p.Item2 == prop.Name).p;
            property ??= resultProperties.SingleOrDefault(p => p.p.Name == prop.Name).p;
            property ??= resultProperties.SingleOrDefault(p => p.p.Name.ToUpperInvariant() == prop.Name.ToUpperInvariant()).p;

            if (property is null)
                // Property not found
                throw new Exception($"{method}: Missing property `{prop.Name}` on `{obj.GetType().Name}`");

            var propertyValue = property.GetValue(obj);
            if (property.GetCustomAttribute<JsonPropertyAttribute>(true)?.ItemConverterType == null)
                CheckPropertyValue(method, prop.Value, propertyValue, property.Name, prop.Name, ignoreProperties);
        }

        private static void CheckPropertyValue(string method, JToken propValue, object? propertyValue, string? propertyName = null, string? propName = null, List<string>? ignoreProperties = null)
        {
            if (propertyValue == default && propValue.Type != JTokenType.Null && !string.IsNullOrEmpty(propValue.ToString()))
            {
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
                    if (typeConverter.Any() && ((JsonConverterAttribute)typeConverter.First()).ConverterType != typeof(ArrayConverter))
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
                        foreach (var item in jtoken.Values())
                        {
                            var arrayProp = resultProps.SingleOrDefault(p => p.Item2!.Index == i).p;
                            if (arrayProp != null)
                                CheckPropertyValue(method, item, arrayProp.GetValue(resultObj), arrayProp.Name, "Array index " + i, ignoreProperties);

                            i++;
                        }
                    }
                    else
                    {
                        var value = enumerator.Current;
                        if (value == default && ((JValue)jtoken).Type != JTokenType.Null)
                            throw new Exception($"{method}: Property `{propertyName}` has no value while input json `{propName}` has value {jtoken}");

                        CheckValues(method, propertyName!, (JValue)jtoken, value!);
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
                else
                {
                    CheckValues(method, propertyName!, (JValue)propValue, propertyValue);
                }
            }
        }

        private static void CheckValues(string method, string property, JValue jsonValue, object objectValue)
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
                else if (jsonValue.Value<string>()!.ToLowerInvariant() != objectValue.ToString()!.ToLowerInvariant())
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
                else if (jsonValue.Value<long>() != Convert.ToInt64(objectValue))
                {
                    throw new Exception($"{method}: {property} not equal: {jsonValue.Value<long>()} vs {Convert.ToInt64(objectValue)}");
                }
            }
            else if (jsonValue.Type == JTokenType.Boolean)
            {
                if (jsonValue.Value<bool>() != (bool)objectValue)
                    throw new Exception($"{method}: {property} not equal: {jsonValue.Value<bool>()} vs {(bool)objectValue}");
            }
        }

    }
}

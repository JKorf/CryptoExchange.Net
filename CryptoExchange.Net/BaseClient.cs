using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CryptoExchange.Net
{
    public abstract class BaseClient
    {
        protected string baseAddress;
        protected Log log;
        protected ApiProxy apiProxy;
        protected AuthenticationProvider authProvider;

        protected static int lastId;
        protected static object idLock = new object();

        private static readonly JsonSerializer defaultSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        });

        public BaseClient(ExchangeOptions options, AuthenticationProvider authenticationProvider)
        {
            log = new Log();
            authProvider = authenticationProvider;
            Configure(options);
        }

        /// <summary>
        /// Configure the client using the provided options
        /// </summary>
        /// <param name="exchangeOptions">Options</param>
        protected virtual void Configure(ExchangeOptions exchangeOptions)
        {
            log.UpdateWriters(exchangeOptions.LogWriters);
            log.Level = exchangeOptions.LogVerbosity;

            baseAddress = exchangeOptions.BaseAddress;
            apiProxy = exchangeOptions.Proxy;
            if (apiProxy != null)
                log.Write(LogVerbosity.Info, $"Setting api proxy to {exchangeOptions.Proxy.Host}:{exchangeOptions.Proxy.Port}");
        }

        /// <summary>
        /// Set the authentication provider
        /// </summary>
        /// <param name="authentictationProvider"></param>
        protected void SetAuthenticationProvider(AuthenticationProvider authentictationProvider)
        {
            log.Write(LogVerbosity.Debug, "Setting api credentials");
            authProvider = authentictationProvider;
        }

        protected CallResult<T> Deserialize<T>(string data, bool checkObject = true, JsonSerializer serializer = null) where T : class
        {
            if (serializer == null)
                serializer = defaultSerializer;

            try
            {
                var obj = JToken.Parse(data);
                if (checkObject && log.Level == LogVerbosity.Debug)
                {
                    try
                    {
                        if (obj is JObject o)
                        {
                            CheckObject(typeof(T), o);
                        }
                        else
                        {
                            var ary = (JArray)obj;
                            if (ary.HasValues && ary[0] is JObject jObject)
                                CheckObject(typeof(T).GetElementType(), jObject);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Write(LogVerbosity.Debug, "Failed to check response data: " + e.Message);
                    }
                }

                return new CallResult<T>(obj.ToObject<T>(serializer), null);
            }
            catch (JsonReaderException jre)
            {
                var info = $"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}. Received data: {data}";
                log.Write(LogVerbosity.Error, info);
                return new CallResult<T>(null, new DeserializeError(info));
            }
            catch (JsonSerializationException jse)
            {
                var info = $"Deserialize JsonSerializationException: {jse.Message}. Received data: {data}";
                log.Write(LogVerbosity.Error, info);
                return new CallResult<T>(null, new DeserializeError(info));
            }
            catch (Exception ex)
            {
                var info = $"Deserialize Unknown Exception: {ex.Message}. Received data: {data}";
                log.Write(LogVerbosity.Error, info);
                return new CallResult<T>(null, new DeserializeError(info));
            }
        }

        private void CheckObject(Type type, JObject obj)
        {
            if (type.GetCustomAttribute<JsonConverterAttribute>(true) != null)
                // If type has a custom JsonConverter we assume this will handle property mapping
                return;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return;

            if (!obj.HasValues && type != typeof(object))
            {
                log.Write(LogVerbosity.Warning, $"Expected `{type.Name}`, but received object was empty");
                return;
            }

            bool isDif = false;
            var properties = new List<string>();
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault();
                var ignore = prop.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).FirstOrDefault();
                if (ignore != null)
                    continue;

                properties.Add(attr == null ? prop.Name : ((JsonPropertyAttribute)attr).PropertyName);
            }
            foreach (var token in obj)
            {
                var d = properties.SingleOrDefault(p => p == token.Key);
                if (d == null)
                {
                    d = properties.SingleOrDefault(p => p.ToLower() == token.Key.ToLower());
                    if (d == null && !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                    {
                        log.Write(LogVerbosity.Warning, $"Local object doesn't have property `{token.Key}` expected in type `{type.Name}`");
                        isDif = true;
                        continue;
                    }
                }
                properties.Remove(d);

                var propType = GetProperty(d, props)?.PropertyType;
                if (propType == null)
                    continue;
                if (!IsSimple(propType) && propType != typeof(DateTime))
                {
                    if (propType.IsArray && token.Value.HasValues && ((JArray)token.Value).Any() && ((JArray)token.Value)[0] is JObject)
                        CheckObject(propType.GetElementType(), (JObject)token.Value[0]);
                    else if (token.Value is JObject)
                        CheckObject(propType, (JObject)token.Value);
                }
            }

            foreach (var prop in properties)
            {
                var propInfo = props.First(p => p.Name == prop ||
                    ((JsonPropertyAttribute)p.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault())?.PropertyName == prop);
                var optional = propInfo.GetCustomAttributes(typeof(JsonOptionalPropertyAttribute), false).FirstOrDefault();
                if (optional != null)
                    continue;

                isDif = true;
                log.Write(LogVerbosity.Warning, $"Local object has property `{prop}` but was not found in received object of type `{type.Name}`");
            }

            if (isDif)
                log.Write(LogVerbosity.Debug, "Returned data: " + obj);
        }

        private PropertyInfo GetProperty(string name, PropertyInfo[] props)
        {
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault();
                if (attr == null)
                {
                    if (prop.Name.ToLower() == name.ToLower())
                        return prop;
                }
                else
                {
                    if (((JsonPropertyAttribute)attr).PropertyName == name)
                        return prop;
                }
            }
            return null;
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
                   || type == typeof(string)
                   || type == typeof(decimal);
        }

        protected int NextId()
        {
            lock (idLock)
            {
                lastId += 1;
                return lastId;
            }
        }

        protected static string FillPathParameter(string endpoint, params string[] values)
        {
            foreach (var value in values)
            {
                int index = endpoint.IndexOf("{}", StringComparison.Ordinal);
                if (index >= 0)
                {
                    endpoint = endpoint.Remove(index, 2);
                    endpoint = endpoint.Insert(index, value);
                }
            }
            return endpoint;
        }

        public virtual void Dispose()
        {
            authProvider?.Credentials?.Dispose();
            log.Write(LogVerbosity.Debug, "Disposing exchange client");
        }
    }
}

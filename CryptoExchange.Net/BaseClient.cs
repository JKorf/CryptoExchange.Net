using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net
{
    /// <summary>
    /// The base for all clients
    /// </summary>
    public abstract class BaseClient : IDisposable
    {
        /// <summary>
        /// The address of the client
        /// </summary>
        public string BaseAddress { get; }
        /// <summary>
        /// The name of the client
        /// </summary>
        public string ClientName { get; }
        /// <summary>
        /// The log object
        /// </summary>
        protected internal Log log;
        /// <summary>
        /// The api proxy
        /// </summary>
        protected ApiProxy? apiProxy;
        /// <summary>
        /// The auth provider
        /// </summary>
        protected internal AuthenticationProvider? authProvider;
        /// <summary>
        /// Should check objects for missing properties based on the model and the received JSON
        /// </summary>
        public bool ShouldCheckObjects { get; set; }

        /// <summary>
        /// The last used id
        /// </summary>
        protected static int lastId;
        /// <summary>
        /// Lock for id generating
        /// </summary>
        protected static object idLock = new object();

        private static readonly JsonSerializer defaultSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Culture = CultureInfo.InvariantCulture
        });

        /// <summary>
        /// Last is used
        /// </summary>
        public static int LastId => lastId;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="options"></param>
        /// <param name="authenticationProvider"></param>
        protected BaseClient(string clientName, ClientOptions options, AuthenticationProvider? authenticationProvider)
        {
            log = new Log(clientName);
            authProvider = authenticationProvider;
            log.UpdateWriters(options.LogWriters);
            log.Level = options.LogVerbosity;

            ClientName = clientName;
            BaseAddress = options.BaseAddress;
            apiProxy = options.Proxy;

            log.Write(LogVerbosity.Debug, $"Client configuration: {options}");
            ShouldCheckObjects = options.ShouldCheckObjects;
        }

        /// <summary>
        /// Set the authentication provider
        /// </summary>
        /// <param name="authenticationProvider"></param>
        protected void SetAuthenticationProvider(AuthenticationProvider authenticationProvider)
        {
            log.Write(LogVerbosity.Debug, "Setting api credentials");
            authProvider = authenticationProvider;
        }

        /// <summary>
        /// Tries to parse the json data and returns a token
        /// </summary>
        /// <param name="data">The data to parse</param>
        /// <returns></returns>
        protected CallResult<JToken> ValidateJson(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                var info = "Empty data object received";
                log.Write(LogVerbosity.Error, info);
                return new CallResult<JToken>(null, new DeserializeError(info, data));
            }

            try
            {
                return new CallResult<JToken>(JToken.Parse(data), null);
            }
            catch (JsonReaderException jre)
            {
                var info = $"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}";
                return new CallResult<JToken>(null, new DeserializeError(info, data));
            }
            catch (JsonSerializationException jse)
            {
                var info = $"Deserialize JsonSerializationException: {jse.Message}";
                return new CallResult<JToken>(null, new DeserializeError(info, data));
            }
            catch (Exception ex)
            {
                var info = $"Deserialize Unknown Exception: {(ex.InnerException?.Message ?? ex.Message)}";
                return new CallResult<JToken>(null, new DeserializeError(info, data));
            }
        }

        /// <summary>
        /// Deserialize a string into an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="data">The data to deserialize</param>
        /// <param name="checkObject">Whether or not the parsing should be checked for missing properties (will output data to the logging if log verbosity is Debug)</param>
        /// <param name="serializer">A specific serializer to use</param>
        /// <param name="requestId">Id of the request</param>
        /// <returns></returns>
        protected CallResult<T> Deserialize<T>(string data, bool? checkObject = null, JsonSerializer? serializer = null, int? requestId = null)
        {
            var tokenResult = ValidateJson(data);
            if (!tokenResult)
            {
                log.Write(LogVerbosity.Error, tokenResult.Error!.Message);
                return new CallResult<T>(default, tokenResult.Error);
            }

            return Deserialize<T>(tokenResult.Data, checkObject, serializer, requestId);
        }

        /// <summary>
        /// Deserialize a JToken into an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="obj">The data to deserialize</param>
        /// <param name="checkObject">Whether or not the parsing should be checked for missing properties (will output data to the logging if log verbosity is Debug)</param>
        /// <param name="serializer">A specific serializer to use</param>
        /// <param name="requestId">A request identifier</param>
        /// <returns></returns>
        protected CallResult<T> Deserialize<T>(JToken obj, bool? checkObject = null, JsonSerializer? serializer = null, int? requestId = null)
        {
            if (serializer == null)
                serializer = defaultSerializer;

            try
            {
                if ((checkObject ?? ShouldCheckObjects)&& log.Level == LogVerbosity.Debug)
                {
                    try
                    {
                        if (obj is JObject o)
                        {
                            CheckObject(typeof(T), o, requestId);
                        }
                        else if (obj is JArray j)
                        {
                            if (j.HasValues && j[0] is JObject jObject)
                                CheckObject(typeof(T).GetElementType(), jObject, requestId);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Write(LogVerbosity.Debug, $"{(requestId != null ? $"[{ requestId}] " : "")}Failed to check response data: " + (e.InnerException?.Message ?? e.Message));
                    }
                }

                return new CallResult<T>(obj.ToObject<T>(serializer), null);
            }
            catch (JsonReaderException jre)
            {
                var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}";
                log.Write(LogVerbosity.Error, info);
                return new CallResult<T>(default, new DeserializeError(info, obj));
            }
            catch (JsonSerializationException jse)
            {
                var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonSerializationException: {jse.Message}";
                log.Write(LogVerbosity.Error, info);
                return new CallResult<T>(default, new DeserializeError(info, obj));
            }
            catch (Exception ex)
            {
                var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize Unknown Exception: {(ex.InnerException?.Message ?? ex.Message)}";
                log.Write(LogVerbosity.Error, info);
                return new CallResult<T>(default, new DeserializeError(info, obj));
            }
        }

        /// <summary>
        /// Deserialize a stream into an object
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="stream">The stream to deserialize</param>
        /// <param name="serializer">A specific serializer to use</param>
        /// <param name="requestId">Id of the request</param>
        /// <param name="elapsedMilliseconds">Milliseconds response time</param>
        /// <returns></returns>
        protected async Task<CallResult<T>> Deserialize<T>(Stream stream, JsonSerializer? serializer = null, int? requestId = null, long? elapsedMilliseconds = null)
        {
            if (serializer == null)
                serializer = defaultSerializer;

            try
            {
                using var reader = new StreamReader(stream, Encoding.UTF8, false, 512, true);
                if (log.Level == LogVerbosity.Debug)
                {
                    var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                    log.Write(LogVerbosity.Debug, $"{(requestId != null ? $"[{requestId}] ": "")}Response received{(elapsedMilliseconds != null ? $" in {elapsedMilliseconds}" : " ")}ms: {data}");
                    return Deserialize<T>(data, null, serializer, requestId);
                }
                
                using var jsonReader = new JsonTextReader(reader);
                return new CallResult<T>(serializer.Deserialize<T>(jsonReader), null);
            }
            catch (JsonReaderException jre)
            {
                if(stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);
                var data = await ReadStream(stream).ConfigureAwait(false);
                log.Write(LogVerbosity.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}, data: {data}");
                return new CallResult<T>(default, new DeserializeError($"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}", data));
            }
            catch (JsonSerializationException jse)
            {
                if (stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);
                var data = await ReadStream(stream).ConfigureAwait(false);
                log.Write(LogVerbosity.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonSerializationException: {jse.Message}, data: {data}");
                return new CallResult<T>(default, new DeserializeError($"Deserialize JsonSerializationException: {jse.Message}", data));
            }
            catch (Exception ex)
            {
                if (stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);
                var data = await ReadStream(stream).ConfigureAwait(false);
                log.Write(LogVerbosity.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize Unknown Exception: {(ex.InnerException?.Message ?? ex.Message)}, data: {data}");
                return new CallResult<T>(default, new DeserializeError($"Deserialize Unknown Exception: {(ex.InnerException?.Message ?? ex.Message)}", data));
            }
        }

        private async Task<string> ReadStream(Stream stream)
        { 
            using var reader = new StreamReader(stream, Encoding.UTF8, false, 512, true);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        private void CheckObject(Type type, JObject obj, int? requestId = null)
        {
            if (type == null)
                return;

            if (type.GetCustomAttribute<JsonConverterAttribute>(true) != null)
                // If type has a custom JsonConverter we assume this will handle property mapping
                return;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return;

            if (!obj.HasValues && type != typeof(object))
            {
                log.Write(LogVerbosity.Warning, $"{(requestId != null ? $"[{requestId}] " : "")}Expected `{type.Name}`, but received object was empty");
                return;
            }

            var isDif = false;
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
                var d = properties.FirstOrDefault(p => p == token.Key);
                if (d == null)
                {
                    d = properties.SingleOrDefault(p => string.Equals(p, token.Key, StringComparison.CurrentCultureIgnoreCase));
                    if (d == null)
                    {
                        if (!(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                        {
                            log.Write(LogVerbosity.Warning, $"{(requestId != null ? $"[{requestId}] " : "")}Local object doesn't have property `{token.Key}` expected in type `{type.Name}`");
                            isDif = true;
                        }
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
                        CheckObject(propType.GetElementType(), (JObject)token.Value[0], requestId);
                    else if (token.Value is JObject o)
                        CheckObject(propType, o, requestId);
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
                log.Write(LogVerbosity.Warning, $"{(requestId != null ? $"[{requestId}] " : "")}Local object has property `{prop}` but was not found in received object of type `{type.Name}`");
            }

            if (isDif)
                log.Write(LogVerbosity.Debug, $"{(requestId != null ? $"[{ requestId}] " : "")}Returned data: " + obj);
        }

        private static PropertyInfo? GetProperty(string name, IEnumerable<PropertyInfo> props)
        {
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault();
                if (attr == null)
                {
                    if (string.Equals(prop.Name, name, StringComparison.CurrentCultureIgnoreCase))
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

        /// <summary>
        /// Generate a unique id
        /// </summary>
        /// <returns></returns>
        protected int NextId()
        {
            lock (idLock)
            {
                lastId += 1;
                return lastId;
            }
        }

        /// <summary>
        /// Fill parameters in a path. Parameters are specified by '{}' and should be specified in occuring sequence
        /// </summary>
        /// <param name="path">The total path string</param>
        /// <param name="values">The values to fill</param>
        /// <returns></returns>
        protected static string FillPathParameter(string path, params string[] values)
        {
            foreach (var value in values)
            {
                var index = path.IndexOf("{}", StringComparison.Ordinal);
                if (index >= 0)
                {
                    path = path.Remove(index, 2);
                    path = path.Insert(index, value);
                }
            }
            return path;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            authProvider?.Credentials?.Dispose();
            log.Write(LogVerbosity.Debug, "Disposing exchange client");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiter;
using CryptoExchange.Net.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net
{
    public abstract class ExchangeClient : IDisposable
    {
        public IRequestFactory RequestFactory { get; set; } = new RequestFactory();

        protected Log log;
        protected ApiProxy apiProxy;
        protected RateLimitingBehaviour rateLimitBehaviour;

        protected AuthenticationProvider authProvider;
        private List<IRateLimiter> rateLimiters;

        private static JsonSerializer defaultSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc            
        });

        protected ExchangeClient(ExchangeOptions exchangeOptions, AuthenticationProvider authenticationProvider)
        {
            log = new Log();
            authProvider = authenticationProvider;
            Configure(exchangeOptions);
        }

        /// <summary>
        /// Configure the client using the provided options
        /// </summary>
        /// <param name="exchangeOptions">Options</param>
        protected void Configure(ExchangeOptions exchangeOptions)
        {
            log.UpdateWriters(exchangeOptions.LogWriters);
            log.Level = exchangeOptions.LogVerbosity;

            apiProxy = exchangeOptions.Proxy;
            if (apiProxy != null)
                log.Write(LogVerbosity.Info, $"Setting api proxy to {exchangeOptions.Proxy.Host}:{exchangeOptions.Proxy.Port}");

            rateLimitBehaviour = exchangeOptions.RateLimitingBehaviour;
            rateLimiters = new List<IRateLimiter>();
            foreach (var rateLimiter in exchangeOptions.RateLimiters)
                rateLimiters.Add(rateLimiter);
        }

        /// <summary>
        /// Adds a rate limiter to the client. There are 2 choices, the <see cref="RateLimiterTotal"/> and the <see cref="RateLimiterPerEndpoint"/>.
        /// </summary>
        /// <param name="limiter">The limiter to add</param>
        public void AddRateLimiter(IRateLimiter limiter)
        {
            rateLimiters.Add(limiter);
        }

        /// <summary>
        /// Removes all rate limiters from this client
        /// </summary>
        public void RemoveRateLimiters()
        {
            rateLimiters.Clear();
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

        protected virtual async Task<CallResult<T>> ExecuteRequest<T>(Uri uri, string method = "GET", Dictionary<string, object> parameters = null, bool signed = false) where T : class
        {
            log.Write(LogVerbosity.Debug, $"Creating request for " + uri);
            if (signed && authProvider == null)
            { 
                log.Write(LogVerbosity.Warning, $"Request {uri.AbsolutePath} failed because no ApiCredentials were provided");
                return new CallResult<T>(null, new NoApiCredentialsError());
            }

            var request = ConstructRequest(uri, method, parameters, signed);

            if (apiProxy != null)
            {
                log.Write(LogVerbosity.Debug, "Setting proxy");
                request.SetProxy(apiProxy.Host, apiProxy.Port);
            }

            foreach (var limiter in rateLimiters)
            {
                var limitResult = limiter.LimitRequest(uri.AbsolutePath, rateLimitBehaviour);
                if (!limitResult.Success)
                {
                    log.Write(LogVerbosity.Debug, $"Request {uri.AbsolutePath} failed because of rate limit");
                    return new CallResult<T>(null, limitResult.Error);
                }
                else if (limitResult.Data > 0)
                    log.Write(LogVerbosity.Debug, $"Request {uri.AbsolutePath} was limited by {limitResult.Data}ms by {limiter.GetType().Name}");                
            }

            string paramString = null;
            if (parameters != null)
            {
                paramString = "with parameters";
                
                foreach (var param in parameters)
                    paramString += $" {param.Key}={(param.Value.GetType().IsArray ? $"[{string.Join(", ", ((object[])param.Value).Select(p => p.ToString()))}]": param.Value )},";

                paramString = paramString.Trim(',');
            }

            log.Write(LogVerbosity.Debug, $"Sending {(signed ? "signed" : "")} request to {request.Uri} {(paramString ?? "")}");
            var result = await ExecuteRequest(request).ConfigureAwait(false);
            return result.Error != null ? new CallResult<T>(null, result.Error) : Deserialize<T>(result.Data);
        }

        protected virtual IRequest ConstructRequest(Uri uri, string method, Dictionary<string, object> parameters, bool signed)
        {
            var uriString = uri.ToString();

            if (parameters != null)
            {
                if (!uriString.EndsWith("?"))
                    uriString += "?";

                var arraysParameters = parameters.Where(p => p.Value.GetType().IsArray).ToList();
                foreach(var arrayEntry in arraysParameters)
                {
                    uriString += $"{string.Join("&", ((object[])arrayEntry.Value).Select(v => $"{arrayEntry.Key}[]={v}"))}&";
                }

                uriString += $"{string.Join("&", parameters.Where(p => !p.Value.GetType().IsArray).Select(s => $"{s.Key}={s.Value}"))}";
                uriString = uriString.TrimEnd('&');
            }

            if (authProvider != null)
                uriString = authProvider.AddAuthenticationToUriString(uriString, signed);

            var request = RequestFactory.Create(uriString);
            request.Method = method;

            if (authProvider != null)
                request = authProvider.AddAuthenticationToRequest(request, signed);

            return request;
        }

        private async Task<CallResult<string>> ExecuteRequest(IRequest request)
        {
            var returnedData = "";
            try
            {
                var response = await request.GetResponse().ConfigureAwait(false);
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    returnedData = await reader.ReadToEndAsync().ConfigureAwait(false);
                    log.Write(LogVerbosity.Debug, "Data returned: " + returnedData);
                    return new CallResult<string>(returnedData, null);
                }
            }
            catch (WebException we)
            {
                var response = (HttpWebResponse)we.Response;
                try
                {
                    var reader = new StreamReader(response.GetResponseStream());
                    var responseData = await reader.ReadToEndAsync().ConfigureAwait(false);
                    log.Write(LogVerbosity.Warning, "Server returned an error: " + responseData);
                    return new CallResult<string>(null, ParseErrorResponse(responseData));
                }
                catch (Exception)
                {
                }

                var infoMessage = "No response from server";
                if (response == null)
                {
                    infoMessage += $" | {we.Status} - {we.Message}";
                    log.Write(LogVerbosity.Warning, infoMessage);
                    return new CallResult<string>(null, new WebError(infoMessage));
                }

                infoMessage = $"Status: {response.StatusCode}-{response.StatusDescription}, Message: {we.Message}";
                log.Write(LogVerbosity.Warning, infoMessage);
                return new CallResult<string>(null, new ServerError(infoMessage));
            }
            catch (Exception e)
            {
                log.Write(LogVerbosity.Error, $"Unkown error occured: {e.GetType()}, {e.Message}, {e.StackTrace}");
                return new CallResult<string>(null, new UnknownError(e.Message + ", data: " + returnedData));
            }
        }

        protected virtual Error ParseErrorResponse(string error)
        {
            return new ServerError(error);
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
            var props = type.GetProperties();
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
                var propInfo = props.FirstOrDefault(p => p.Name == prop ||
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

        public virtual void Dispose()
        {
            authProvider?.Credentials?.Dispose();
            log.Write(LogVerbosity.Debug, "Disposing exchange client");
        }
    }
}

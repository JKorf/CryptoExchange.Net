using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.RateLimiter;
using CryptoExchange.Net.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net
{
    public abstract class ExchangeClient: IDisposable
    {
        public IRequestFactory RequestFactory { get; set; } = new RequestFactory();

        protected Log log;
        protected ApiProxy apiProxy;

        private AuthenticationProvider authProvider;
        private List<IRateLimiter> rateLimiters;
        
        protected ExchangeClient(ExchangeOptions exchangeOptions, AuthenticationProvider authentictationProvider)
        {
            log = new Log();
            authProvider = authentictationProvider;
            Configure(exchangeOptions);
        }

        /// <summary>
        /// Configure the client using the provided options
        /// </summary>
        /// <param name="exchangeOptions">Options</param>
        protected void Configure(ExchangeOptions exchangeOptions)
        {
            if (exchangeOptions.LogWriter != null)
                log.TextWriter = exchangeOptions.LogWriter;

            log.Level = exchangeOptions.LogVerbosity;
            apiProxy = exchangeOptions.Proxy;

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
            authProvider = authentictationProvider;
        }

        protected async Task<CallResult<T>> ExecuteRequest<T>(Uri uri, string method = "GET", Dictionary<string, object> parameters = null, bool signed = false) where T : class
        {
            if(signed && authProvider == null)
                return new CallResult<T>(null, new NoApiCredentialsError());

            var uriString = uri.ToString();

            if (parameters != null)
            {
                if (!uriString.EndsWith("?"))
                    uriString += "?";

                uriString += $"{string.Join("&", parameters.Select(s => $"{s.Key}={s.Value}"))}";
            }
            
            uriString = authProvider.AddAuthenticationToUriString(uriString, signed);

            var request = RequestFactory.Create(uriString);
            request.Method = method;

            if (apiProxy != null)
                request.SetProxy(apiProxy.Host, apiProxy.Port);
            
            request = authProvider.AddAuthenticationToRequest(request, signed);

            foreach (var limiter in rateLimiters)
            {
                var limitedBy = limiter.LimitRequest(uri.AbsolutePath);
                if (limitedBy > 0)
                    log.Write(LogVerbosity.Debug, $"Request {uri.AbsolutePath} was limited by {limitedBy}ms by {limiter.GetType().Name}");
            }

            log.Write(LogVerbosity.Debug, $"Sending request to {uriString}");
            var result = await ExecuteRequest(request);
            return result.Error != null ? new CallResult<T>(null, result.Error) : Deserialize<T>(result.Data);
        }

        private async Task<CallResult<string>> ExecuteRequest(IRequest request)
        {
            var returnedData = "";
            try
            {
                var response = request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    returnedData = await reader.ReadToEndAsync().ConfigureAwait(false);
                    return new CallResult<string>(returnedData, null);
                }
            }
            catch (WebException we)
            {
                var response = (HttpWebResponse)we.Response;
                string responseData = null;
                try
                {
                    var reader = new StreamReader(response.GetResponseStream());
                    responseData = reader.ReadToEnd();
                }
                catch (Exception)
                {
                }

                var infoMessage = "No response from server";
                if (response == null)
                    return new CallResult<string>(null, new WebError(infoMessage));

                if (responseData != null)
                    infoMessage = "Server returned error: " + responseData;
                else
                    infoMessage = $"Status: {response.StatusCode}-{response.StatusDescription}, Message: {we.Message}";
                return new CallResult<string>(null, new ServerError(infoMessage));
            }
            catch (Exception e)
            {
                return new CallResult<string>(null, new UnknownError(e.Message + ", data: " + returnedData));
            }
        }

        private CallResult<T> Deserialize<T>(string data) where T: class
        {
            try
            {
                var obj = JToken.Parse(data);
                if (log.Level == LogVerbosity.Debug)
                {
                    if (obj is JObject o)
                        CheckObject(typeof(T), o);
                    else
                    {
                        var ary = (JArray) obj;
                        if (ary.HasValues && ary[0] is JObject jObject)
                            CheckObject(typeof(T).GetElementType(), jObject);
                    }
                }

                return new CallResult<T>(obj.ToObject<T>(), null);
            }
            catch (JsonReaderException jre)
            {
                return new CallResult<T>(null, new DeserializeError($"Error occured at Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}. Received data: {data}"));
            }
            catch (JsonSerializationException jse)
            {
                return new CallResult<T>(null, new DeserializeError($"Message: {jse.Message}. Received data: {data}"));
            }
        }

        private void CheckObject(Type type, JObject obj)
        {
            var properties = new List<string>();
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false).FirstOrDefault();
                var ignore = prop.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).FirstOrDefault();
                if (ignore != null)
                    continue;

                properties.Add(attr == null ? prop.Name : ((JsonPropertyAttribute) attr).PropertyName);
            }
            foreach (var token in obj)
            {
                var d = properties.SingleOrDefault(p => p == token.Key);
                if (d == null)
                {
                    d = properties.SingleOrDefault(p => p.ToLower() == token.Key.ToLower());
                    if (d == null)
                    {
                        log.Write(LogVerbosity.Warning, $"Didn't find property `{token.Key}` in object of type `{type.Name}`");
                        continue;
                    }
                }
                properties.Remove(d);

                var propType = GetProperty(d, props)?.PropertyType;
                if (propType == null)
                    continue;
                if (!IsSimple(propType) && propType != typeof(DateTime))
                {
                    if(propType.IsArray && token.Value.HasValues && ((JArray)token.Value).Any() && ((JArray)token.Value)[0] is JObject)
                        CheckObject(propType.GetElementType(), (JObject)token.Value[0]);
                    else if(token.Value is JObject)
                        CheckObject(propType, (JObject)token.Value);
                }
            }

            foreach(var prop in properties) 
                log.Write(LogVerbosity.Warning, $"Didn't find key `{prop}` in returned data object of type `{type.Name}`");
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
                    if (((JsonPropertyAttribute) attr).PropertyName == name)
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
        }
    }
}

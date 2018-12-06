using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net
{
    public abstract class RestClient: BaseClient
    {
        /// <summary>
        /// The factory for creating requests. Used for unit testing
        /// </summary>
        public IRequestFactory RequestFactory { get; set; } = new RequestFactory();
        

        protected PostParameters postParametersPosition = PostParameters.InBody;
        protected RequestBodyFormat requestBodyFormat = RequestBodyFormat.Json;

        protected TimeSpan RequestTimeout { get; private set; }
        public RateLimitingBehaviour RateLimitBehaviour { get; private set; }
        public IEnumerable<IRateLimiter> RateLimiters { get; private set; }

        protected RestClient(ClientOptions exchangeOptions, AuthenticationProvider authenticationProvider): base(exchangeOptions, authenticationProvider)
        {
            Configure(exchangeOptions);
        }

        /// <summary>
        /// Configure the client using the provided options
        /// </summary>
        /// <param name="exchangeOptions">Options</param>
        protected void Configure(ClientOptions exchangeOptions)
        {
            RequestTimeout = exchangeOptions.RequestTimeout;
            RateLimitBehaviour = exchangeOptions.RateLimitingBehaviour;
            var rateLimiters = new List<IRateLimiter>();
            foreach (var rateLimiter in exchangeOptions.RateLimiters)
                rateLimiters.Add(rateLimiter);
            RateLimiters = rateLimiters;
        }

        /// <summary>
        /// Adds a rate limiter to the client. There are 2 choices, the <see cref="RateLimiterTotal"/> and the <see cref="RateLimiterPerEndpoint"/>.
        /// </summary>
        /// <param name="limiter">The limiter to add</param>
        public void AddRateLimiter(IRateLimiter limiter)
        {
            var rateLimiters = RateLimiters.ToList();
            rateLimiters.Add(limiter);
            RateLimiters = rateLimiters;
        }

        /// <summary>
        /// Removes all rate limiters from this client
        /// </summary>
        public void RemoveRateLimiters()
        {
            RateLimiters = new List<IRateLimiter>();
        }

        /// <summary>
        /// Ping to see if the server is reachable
        /// </summary>
        /// <returns>The roundtrip time of the ping request</returns>
        public virtual CallResult<long> Ping() => PingAsync().Result;

        /// <summary>
        /// Ping to see if the server is reachable
        /// </summary>
        /// <returns>The roundtrip time of the ping request</returns>
        public virtual async Task<CallResult<long>> PingAsync()
        {
            var ping = new Ping();
            var uri = new Uri(BaseAddress);
            PingReply reply;
            try
            {
                reply = await ping.SendPingAsync(uri.Host).ConfigureAwait(false);
            }
            catch(PingException e)
            {
                if(e.InnerException != null)
                {
                    if (e.InnerException is SocketException exception)
                        return new CallResult<long>(0, new CantConnectError() { Message = "Ping failed: " + exception.SocketErrorCode });
                    return new CallResult<long>(0, new CantConnectError() { Message = "Ping failed: " + e.InnerException.Message });
                }
                return new CallResult<long>(0, new CantConnectError() { Message = "Ping failed: " + e.Message });
            }
            if (reply.Status == IPStatus.Success)
                return new CallResult<long>(reply.RoundtripTime, null);
            return new CallResult<long>(0, new CantConnectError() { Message = "Ping failed: " + reply.Status });
        }

        /// <summary>
        /// Execute a request
        /// </summary>
        /// <typeparam name="T">The expected result type</typeparam>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <param name="signed">Whether or not the request should be authenticated</param>
        /// <param name="checkResult">Whether or not the resulting object should be checked for missing properties in the mapping (only outputs if log verbosity is Debug)</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<T>> ExecuteRequest<T>(Uri uri, string method = Constants.GetMethod, Dictionary<string, object> parameters = null, bool signed = false, bool checkResult = true) where T : class
        {
            log.Write(LogVerbosity.Debug, "Creating request for " + uri);
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

            foreach (var limiter in RateLimiters)
            {
                var limitResult = limiter.LimitRequest(uri.AbsolutePath, RateLimitBehaviour);
                if (!limitResult.Success)
                {
                    log.Write(LogVerbosity.Debug, $"Request {uri.AbsolutePath} failed because of rate limit");
                    return new CallResult<T>(null, limitResult.Error);
                }

                if (limitResult.Data > 0)
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

            log.Write(LogVerbosity.Debug, $"Sending {method}{(signed ? " signed" : "")} request to {request.Uri} {(paramString ?? "")}");
            var result = await ExecuteRequest(request).ConfigureAwait(false);
            if(!result.Success)
                return new CallResult<T>(null, result.Error);

            var jsonResult = ValidateJson(result.Data);
            if(!jsonResult.Success)
                return new CallResult<T>(null, jsonResult.Error);

            if (IsErrorResponse(jsonResult.Data))
                return new CallResult<T>(null, ParseErrorResponse(jsonResult.Data));
            
            return Deserialize<T>(jsonResult.Data, checkResult);
        }

        /// <summary>
        /// Can be overridden to indicate if a response is an error response
        /// </summary>
        /// <param name="data">The received data</param>
        /// <returns>True if error response</returns>
        protected virtual bool IsErrorResponse(JToken data)
        {
            return false;
        }

        /// <summary>
        /// Creates a request object
        /// </summary>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <param name="signed">Whether or not the request should be authenticated</param>
        /// <returns></returns>
        protected virtual IRequest ConstructRequest(Uri uri, string method, Dictionary<string, object> parameters, bool signed)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            var uriString = uri.ToString();
            if(authProvider != null)
                parameters = authProvider.AddAuthenticationToParameters(uriString, method, parameters, signed);

            if((method == Constants.GetMethod || method == Constants.DeleteMethod || (postParametersPosition == PostParameters.InUri)) && parameters?.Any() == true)            
                uriString += "?" + parameters.CreateParamString(true);
            
            var request = RequestFactory.Create(uriString);
            request.ContentType = requestBodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
            request.Accept = Constants.JsonContentHeader;
            request.Method = method;

            var headers = new Dictionary<string, string>();
            if (authProvider != null)
                headers = authProvider.AddAuthenticationToHeaders(uriString, method, parameters, signed);

            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);

            if ((method == Constants.PostMethod || method == Constants.PutMethod) && postParametersPosition != PostParameters.InUri)
            {
                if(parameters?.Any() == true)
                    WriteParamBody(request, parameters);
                else                
                    WriteParamBody(request, "{}");
            }

            return request;
        }

        /// <summary>
        /// Writes the string data of the parameters to the request body stream
        /// </summary>
        /// <param name="request"></param>
        /// <param name="stringData"></param>
        protected virtual void WriteParamBody(IRequest request, string stringData)
        {
            var data = Encoding.UTF8.GetBytes(stringData);
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream().Result)
                stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes the parameters of the request to the request object, either in the query string or the request body
        /// </summary>
        /// <param name="request"></param>
        /// <param name="parameters"></param>
        protected virtual void WriteParamBody(IRequest request, Dictionary<string, object> parameters)
        {
            if (requestBodyFormat == RequestBodyFormat.Json)
            {
                var stringData = JsonConvert.SerializeObject(parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value));
                WriteParamBody(request, stringData);
            }
            else if(requestBodyFormat == RequestBodyFormat.FormData)
            {
                NameValueCollection formData = HttpUtility.ParseQueryString(String.Empty);
                foreach (var kvp in parameters.OrderBy(p => p.Key))
                    formData.Add(kvp.Key, kvp.Value.ToString());
                string stringData = formData.ToString();
                WriteParamBody(request, stringData);
            }
        }

        /// <summary>
        /// Executes the request and returns the string result
        /// </summary>
        /// <param name="request">The request object to execute</param>
        /// <returns></returns>
        private async Task<CallResult<string>> ExecuteRequest(IRequest request)
        {
            var returnedData = "";
            try
            {
                request.Timeout = RequestTimeout;
                var response = await request.GetResponse().ConfigureAwait(false);
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    returnedData = await reader.ReadToEndAsync().ConfigureAwait(false);
                    log.Write(LogVerbosity.Debug, "Data returned: " + returnedData);
                }

                response.Close();
                return new CallResult<string>(returnedData, null);                
            }
            catch (WebException we)
            {
                var response = (HttpWebResponse)we.Response;
                try
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        returnedData = await reader.ReadToEndAsync().ConfigureAwait(false);
                        log.Write(LogVerbosity.Warning, "Server returned an error: " + returnedData);
                    }

                    response.Close();

                    var jsonResult = ValidateJson(returnedData);
                    if (!jsonResult.Success)
                        return new CallResult<string>(null, jsonResult.Error);

                    return new CallResult<string>(null, ParseErrorResponse(jsonResult.Data));                    
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
                response.Close();
                return new CallResult<string>(null, new ServerError(infoMessage));
            }
            catch (Exception e)
            {
                log.Write(LogVerbosity.Error, $"Unknown error occured: {e.GetType()}, {e.Message}, {e.StackTrace}");
                return new CallResult<string>(null, new UnknownError(e.Message + ", data: " + returnedData));
            }
        }

        /// <summary>
        /// Parse an error response from the server. Only used when server returns a status other than Success(200)
        /// </summary>
        /// <param name="error">The string the request returned</param>
        /// <returns></returns>
        protected virtual Error ParseErrorResponse(JToken error)
        {
            return new ServerError(error.ToString());
        }
    }
}

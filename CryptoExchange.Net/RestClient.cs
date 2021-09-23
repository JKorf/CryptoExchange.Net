using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiter;
using CryptoExchange.Net.Requests;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Base rest client
    /// </summary>
    public abstract class RestClient : BaseClient, IRestClient
    {
        /// <summary>
        /// The factory for creating requests. Used for unit testing
        /// </summary>
        public IRequestFactory RequestFactory { get; set; } = new RequestFactory();

        /// <summary>
        /// Where to put the parameters for requests with different Http methods
        /// </summary>
        protected Dictionary<HttpMethod, HttpMethodParameterPosition> ParameterPositions { get; set; } = new Dictionary<HttpMethod, HttpMethodParameterPosition>
        { 
            { HttpMethod.Get, HttpMethodParameterPosition.InUri },
            { HttpMethod.Post, HttpMethodParameterPosition.InBody },
            { HttpMethod.Delete, HttpMethodParameterPosition.InBody },
            { HttpMethod.Put, HttpMethodParameterPosition.InBody }
        };

        /// <summary>
        /// Request body content type
        /// </summary>
        protected RequestBodyFormat requestBodyFormat = RequestBodyFormat.Json;

        /// <summary>
        /// Whether or not we need to manually parse an error instead of relying on the http status code
        /// </summary>
        protected bool manualParseError = false;

        /// <summary>
        /// How to serialize array parameters when making requests
        /// </summary>
        protected ArrayParametersSerialization arraySerialization = ArrayParametersSerialization.Array;

        /// <summary>
        /// What request body should be set when no data is send (only used in combination with postParametersPosition.InBody)
        /// </summary>
        protected string requestBodyEmptyContent = "{}";

        /// <summary>
        /// Timeout for requests. This setting is ignored when injecting a HttpClient in the options, requests timeouts should be set on the client then.
        /// </summary>
        public TimeSpan RequestTimeout { get; }
        /// <summary>
        /// What should happen when running into a rate limit
        /// </summary>
        public RateLimitingBehaviour RateLimitBehaviour { get; }
        /// <summary>
        /// List of rate limiters
        /// </summary>
        public IEnumerable<IRateLimiter> RateLimiters { get; private set; }
        /// <summary>
        /// Total requests made by this client
        /// </summary>
        public int TotalRequestsMade { get; private set; }

        /// <summary>
        /// Request headers to be sent with each request
        /// </summary>
        protected Dictionary<string, string>? StandardRequestHeaders { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="exchangeName">The name of the exchange this client is for</param>
        /// <param name="exchangeOptions">The options for this client</param>
        /// <param name="authenticationProvider">The authentication provider for this client (can be null if no credentials are provided)</param>
        protected RestClient(string exchangeName, RestClientOptions exchangeOptions, AuthenticationProvider? authenticationProvider) : base(exchangeName, exchangeOptions, authenticationProvider)
        {
            if (exchangeOptions == null)
                throw new ArgumentNullException(nameof(exchangeOptions));

            RequestTimeout = exchangeOptions.RequestTimeout;
            RequestFactory.Configure(exchangeOptions.RequestTimeout, exchangeOptions.Proxy, exchangeOptions.HttpClient);
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
            if (limiter == null)
                throw new ArgumentNullException(nameof(limiter));

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
        public virtual CallResult<long> Ping(CancellationToken ct = default) => PingAsync(ct).Result;

        /// <summary>
        /// Ping to see if the server is reachable
        /// </summary>
        /// <returns>The roundtrip time of the ping request</returns>
        public virtual async Task<CallResult<long>> PingAsync(CancellationToken ct = default)
        {
            var ping = new Ping();
            var uri = new Uri(BaseAddress);
            PingReply reply;

            var ctRegistration = ct.Register(() => ping.SendAsyncCancel());
            try
            {
                reply = await ping.SendPingAsync(uri.Host).ConfigureAwait(false);
            }
            catch (PingException e)
            {
                if (e.InnerException == null)
                    return new CallResult<long>(0, new CantConnectError { Message = "Ping failed: " + e.Message });

                if (e.InnerException is SocketException exception)
                    return new CallResult<long>(0, new CantConnectError { Message = "Ping failed: " + exception.SocketErrorCode });
                return new CallResult<long>(0, new CantConnectError { Message = "Ping failed: " + e.InnerException.Message });
            }
            finally
            {
                ctRegistration.Dispose();
                ping.Dispose();
            }

            if (ct.IsCancellationRequested)
                return new CallResult<long>(0, new CancellationRequestedError());

            return reply.Status == IPStatus.Success ? new CallResult<long>(reply.RoundtripTime, null) : new CallResult<long>(0, new CantConnectError { Message = "Ping failed: " + reply.Status });
        }

        /// <summary>
        /// Execute a request to the uri and deserialize the response into the provided type parameter
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <param name="signed">Whether or not the request should be authenticated</param>
        /// <param name="checkResult">Whether or not the resulting object should be checked for missing properties in the mapping (only outputs if log verbosity is Debug)</param> 
        /// <param name="parameterPosition">Where the parameters should be placed, overwrites the value set in the client</param>
        /// <param name="arraySerialization">How array parameters should be serialized, overwrites the value set in the client</param>
        /// <param name="credits">Credits used for the request</param>
        /// <param name="deserializer">The JsonSerializer to use for deserialization</param>
        /// <param name="additionalHeaders">Additional headers to send with the request</param>
        /// <returns></returns>
        [return: NotNull]
        protected virtual async Task<WebCallResult<T>> SendRequestAsync<T>(
            Uri uri, 
            HttpMethod method, 
            CancellationToken cancellationToken,
            Dictionary<string, object>? parameters = null, 
            bool signed = false, 
            bool checkResult = true,
            HttpMethodParameterPosition? parameterPosition = null,
            ArrayParametersSerialization? arraySerialization = null, 
            int credits = 1,
            JsonSerializer? deserializer = null,
            Dictionary<string, string>? additionalHeaders = null) where T : class
        {
            var requestId = NextId();
            log.Write(LogLevel.Debug, $"[{requestId}] Creating request for " + uri);
            if (signed && authProvider == null)
            {
                log.Write(LogLevel.Warning, $"[{requestId}] Request {uri.AbsolutePath} failed because no ApiCredentials were provided");
                return new WebCallResult<T>(null, null, null, new NoApiCredentialsError());
            }

            var paramsPosition = parameterPosition ?? ParameterPositions[method];
            var request = ConstructRequest(uri, method, parameters, signed, paramsPosition, arraySerialization ?? this.arraySerialization, requestId, additionalHeaders);
            foreach (var limiter in RateLimiters)
            {
                var limitResult = limiter.LimitRequest(this, uri.AbsolutePath, RateLimitBehaviour, credits);
                if (!limitResult.Success)
                {
                    log.Write(LogLevel.Information, $"[{requestId}] Request {uri.AbsolutePath} failed because of rate limit");
                    return new WebCallResult<T>(null, null, null, limitResult.Error);
                }

                if (limitResult.Data > 0)
                    log.Write(LogLevel.Information, $"[{requestId}] Request {uri.AbsolutePath} was limited by {limitResult.Data}ms by {limiter.GetType().Name}");
            }

            string? paramString = "";
            if (paramsPosition == HttpMethodParameterPosition.InBody)
                paramString = " with request body " + request.Content;

            if (log.Level == LogLevel.Trace)
            {
                var headers = request.GetHeaders();
                if (headers.Any())
                    paramString += " with headers " + string.Join(", ", headers.Select(h => h.Key + $"=[{string.Join(",", h.Value)}]"));
            }

            log.Write(LogLevel.Debug, $"[{requestId}] Sending {method}{(signed ? " signed" : "")} request to {request.Uri}{paramString ?? " "}{(apiProxy == null ? "" : $" via proxy {apiProxy.Host}")}");
            return await GetResponseAsync<T>(request, deserializer, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the request and returns the result deserialized into the type parameter class
        /// </summary>
        /// <param name="request">The request object to execute</param>
        /// <param name="deserializer">The JsonSerializer to use for deserialization</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        protected virtual async Task<WebCallResult<T>> GetResponseAsync<T>(IRequest request, JsonSerializer? deserializer, CancellationToken cancellationToken)
        {
            try
            {
                TotalRequestsMade++;
                var sw = Stopwatch.StartNew();
                var response = await request.GetResponseAsync(cancellationToken).ConfigureAwait(false);
                sw.Stop();
                var statusCode = response.StatusCode;
                var headers = response.ResponseHeaders;
                var responseStream = await response.GetResponseStreamAsync().ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    // If we have to manually parse error responses (can't rely on HttpStatusCode) we'll need to read the full
                    // response before being able to deserialize it into the resulting type since we don't know if it an error response or data
                    if (manualParseError)
                    {
                        using var reader = new StreamReader(responseStream);
                        var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                        responseStream.Close();
                        response.Close();
                        log.Write(LogLevel.Debug, $"[{request.RequestId}] Response received in {sw.ElapsedMilliseconds}ms: {data}");

                        // Validate if it is valid json. Sometimes other data will be returned, 502 error html pages for example
                        var parseResult = ValidateJson(data);
                        if (!parseResult.Success)
                            return WebCallResult<T>.CreateErrorResult(response.StatusCode, response.ResponseHeaders, parseResult.Error!);

                        // Let the library implementation see if it is an error response, and if so parse the error
                        var error = await TryParseErrorAsync(parseResult.Data).ConfigureAwait(false);
                        if (error != null)
                            return WebCallResult<T>.CreateErrorResult(response.StatusCode, response.ResponseHeaders, error);

                        // Not an error, so continue deserializing
                        var deserializeResult = Deserialize<T>(parseResult.Data, null, deserializer, request.RequestId);
                        return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, OutputOriginalData ? data: null, deserializeResult.Data, deserializeResult.Error);
                    }
                    else
                    {
                        // Success status code, and we don't have to check for errors. Continue deserializing directly from the stream
                        var desResult = await DeserializeAsync<T>(responseStream, deserializer, request.RequestId, sw.ElapsedMilliseconds).ConfigureAwait(false);
                        responseStream.Close();
                        response.Close();

                        return new WebCallResult<T>(statusCode, headers, OutputOriginalData ? desResult.OriginalData : null, desResult.Data, desResult.Error);
                    }
                }
                else
                {
                    // Http status code indicates error
                    using var reader = new StreamReader(responseStream);
                    var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                    log.Write(LogLevel.Debug, $"[{request.RequestId}] Error received: {data}");
                    responseStream.Close();
                    response.Close();
                    var parseResult = ValidateJson(data);
                    var error = parseResult.Success ? ParseErrorResponse(parseResult.Data) : parseResult.Error!;
                    if(error.Code == null || error.Code == 0)
                        error.Code = (int)response.StatusCode;
                    return new WebCallResult<T>(statusCode, headers, default, error);
                }
            }
            catch (HttpRequestException requestException)
            {
                // Request exception, can't reach server for instance
                var exceptionInfo = requestException.ToLogString();
                log.Write(LogLevel.Warning, $"[{request.RequestId}] Request exception: " + exceptionInfo);
                return new WebCallResult<T>(null, null, default, new WebError(exceptionInfo));
            }
            catch (OperationCanceledException canceledException)
            {
                if (canceledException.CancellationToken == cancellationToken)
                {
                    // Cancellation token cancelled by caller
                    log.Write(LogLevel.Warning, $"[{request.RequestId}] Request cancel requested");
                    return new WebCallResult<T>(null, null, default, new CancellationRequestedError());
                }
                else
                {
                    // Request timed out
                    log.Write(LogLevel.Warning, $"[{request.RequestId}] Request timed out");
                    return new WebCallResult<T>(null, null, default, new WebError($"[{request.RequestId}] Request timed out"));
                }
            }
        }

        /// <summary>
        /// Can be used to parse an error even though response status indicates success. Some apis always return 200 OK, even though there is an error.
        /// When setting manualParseError to true this method will be called for each response to be able to check if the response is an error or not.
        /// If the response is an error this method should return the parsed error, else it should return null
        /// </summary>
        /// <param name="data">Received data</param>
        /// <returns>Null if not an error, Error otherwise</returns>
        protected virtual Task<ServerError?> TryParseErrorAsync(JToken data)
        {
            return Task.FromResult<ServerError?>(null);
        }

        /// <summary>
        /// Creates a request object
        /// </summary>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <param name="signed">Whether or not the request should be authenticated</param>
        /// <param name="parameterPosition">Where the parameters should be placed</param>
        /// <param name="arraySerialization">How array parameters should be serialized</param>
        /// <param name="requestId">Unique id of a request</param>
        /// <param name="additionalHeaders">Additional headers to send with the request</param>
        /// <returns></returns>
        protected virtual IRequest ConstructRequest(
            Uri uri,
            HttpMethod method,
            Dictionary<string, object>? parameters,
            bool signed,
            HttpMethodParameterPosition parameterPosition,
            ArrayParametersSerialization arraySerialization,
            int requestId,
            Dictionary<string, string>? additionalHeaders)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            var uriString = uri.ToString();
            if (authProvider != null)
                parameters = authProvider.AddAuthenticationToParameters(uriString, method, parameters, signed, parameterPosition, arraySerialization);

            if (parameterPosition == HttpMethodParameterPosition.InUri && parameters?.Any() == true)
                uriString += "?" + parameters.CreateParamString(true, arraySerialization);

            var contentType = requestBodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
            var request = RequestFactory.Create(method, uriString, requestId);
            request.Accept = Constants.JsonContentHeader;

            var headers = new Dictionary<string, string>();
            if (authProvider != null)
                headers = authProvider.AddAuthenticationToHeaders(uriString, method, parameters!, signed, parameterPosition, arraySerialization);

            foreach (var header in headers)
                request.AddHeader(header.Key, header.Value);

            if (additionalHeaders != null) 
            { 
                foreach (var header in additionalHeaders)
                    request.AddHeader(header.Key, header.Value);
            }

            if(StandardRequestHeaders != null)
            {
                foreach (var header in StandardRequestHeaders)
                    // Only add it if it isn't overwritten
                    if(additionalHeaders?.ContainsKey(header.Key) != true)
                        request.AddHeader(header.Key, header.Value);
            }

            if (parameterPosition == HttpMethodParameterPosition.InBody)
            {
                if (parameters?.Any() == true)
                    WriteParamBody(request, parameters, contentType);
                else
                    request.SetContent(requestBodyEmptyContent, contentType);
            }

            return request;
        }

        /// <summary>
        /// Writes the parameters of the request to the request object body
        /// </summary>
        /// <param name="request">The request to set the parameters on</param>
        /// <param name="parameters">The parameters to set</param>
        /// <param name="contentType">The content type of the data</param>
        protected virtual void WriteParamBody(IRequest request, Dictionary<string, object> parameters, string contentType)
        {
            if (requestBodyFormat == RequestBodyFormat.Json)
            {
                // Write the parameters as json in the body
                var stringData = JsonConvert.SerializeObject(parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value));
                request.SetContent(stringData, contentType);
            }
            else if (requestBodyFormat == RequestBodyFormat.FormData)
            {
                // Write the parameters as form data in the body
                var formData = HttpUtility.ParseQueryString(string.Empty);
                foreach (var kvp in parameters.OrderBy(p => p.Key))
                {
                    if (kvp.Value.GetType().IsArray)
                    {
                        var array = (Array)kvp.Value;
                        foreach (var value in array)
                            formData.Add(kvp.Key, value.ToString());
                    }
                    else
                        formData.Add(kvp.Key, kvp.Value.ToString());
                }
                var stringData = formData.ToString();
                request.SetContent(stringData, contentType);
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

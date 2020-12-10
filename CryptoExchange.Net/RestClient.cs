using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
        /// Where to place post parameters
        /// </summary>
        protected PostParameters postParametersPosition = PostParameters.InBody;
        /// <summary>
        /// Request body content type
        /// </summary>
        protected RequestBodyFormat requestBodyFormat = RequestBodyFormat.Json;

        /// <summary>
        /// Whether or not we need to manually parse an error instead of relying on the http status code
        /// </summary>
        protected bool manualParseError = false;

        /// <summary>
        /// How to serialize array parameters
        /// </summary>
        protected ArrayParametersSerialization arraySerialization = ArrayParametersSerialization.Array;

        /// <summary>
        /// What request body should be when no data is send
        /// </summary>
        protected string requestBodyEmptyContent = "{}";

        /// <summary>
        /// Timeout for requests
        /// </summary>
        public TimeSpan RequestTimeout { get; }
        /// <summary>
        /// Rate limiting behaviour
        /// </summary>
        public RateLimitingBehaviour RateLimitBehaviour { get; }
        /// <summary>
        /// List of rate limiters
        /// </summary>
        public IEnumerable<IRateLimiter> RateLimiters { get; private set; }
        /// <summary>
        /// Total requests made
        /// </summary>
        public int TotalRequestsMade { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="exchangeOptions"></param>
        /// <param name="authenticationProvider"></param>
        protected RestClient(RestClientOptions exchangeOptions, AuthenticationProvider? authenticationProvider) : base(exchangeOptions, authenticationProvider)
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
        /// Execute a request
        /// </summary>
        /// <typeparam name="T">The expected result type</typeparam>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <param name="signed">Whether or not the request should be authenticated</param>
        /// <param name="checkResult">Whether or not the resulting object should be checked for missing properties in the mapping (only outputs if log verbosity is Debug)</param> 
        /// <param name="postPosition">Where the post parameters should be placed</param>
        /// <param name="arraySerialization">How array parameters should be serialized</param>
        /// <returns></returns>
        [return: NotNull]
        protected virtual async Task<WebCallResult<T>> SendRequest<T>(Uri uri, HttpMethod method, CancellationToken cancellationToken,
            Dictionary<string, object>? parameters = null, bool signed = false, bool checkResult = true, PostParameters? postPosition = null, ArrayParametersSerialization? arraySerialization = null) where T : class
        {
            var requestId = NextId();
            log.Write(LogVerbosity.Debug, $"[{requestId}] Creating request for " + uri);
            if (signed && authProvider == null)
            {
                log.Write(LogVerbosity.Warning, $"[{requestId}] Request {uri.AbsolutePath} failed because no ApiCredentials were provided");
                return new WebCallResult<T>(null, null, null, new NoApiCredentialsError());
            }

            var request = ConstructRequest(uri, method, parameters, signed, postPosition ?? postParametersPosition, arraySerialization ?? this.arraySerialization, requestId);
            foreach (var limiter in RateLimiters)
            {
                var limitResult = limiter.LimitRequest(this, uri.AbsolutePath, RateLimitBehaviour);
                if (!limitResult.Success)
                {
                    log.Write(LogVerbosity.Debug, $"[{requestId}] Request {uri.AbsolutePath} failed because of rate limit");
                    return new WebCallResult<T>(null, null, null, limitResult.Error);
                }

                if (limitResult.Data > 0)
                    log.Write(LogVerbosity.Debug, $"[{requestId}] Request {uri.AbsolutePath} was limited by {limitResult.Data}ms by {limiter.GetType().Name}");
            }

            string? paramString = null;
            if (method == HttpMethod.Post)
                paramString = " with request body " + request.Content;

            log.Write(LogVerbosity.Debug, $"[{requestId}] Sending {method}{(signed ? " signed" : "")} request to {request.Uri}{paramString ?? " "}{(apiProxy == null ? "" : $" via proxy {apiProxy.Host}")}");
            return await GetResponse<T>(request, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the request and returns the string result
        /// </summary>
        /// <param name="request">The request object to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        private async Task<WebCallResult<T>> GetResponse<T>(IRequest request, CancellationToken cancellationToken)
        {
            try
            {
                TotalRequestsMade++;
                var sw = Stopwatch.StartNew();
                var response = await request.GetResponse(cancellationToken).ConfigureAwait(false);
                sw.Stop();
                var statusCode = response.StatusCode;
                var headers = response.ResponseHeaders;
                var responseStream = await response.GetResponseStream().ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    if (manualParseError)
                    {
                        using var reader = new StreamReader(responseStream);
                        var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                        responseStream.Close();
                        response.Close();
                        log.Write(LogVerbosity.Debug, $"[{request.RequestId}] Response received in {sw.ElapsedMilliseconds}ms: {data}");

                        var parseResult = ValidateJson(data);
                        if (!parseResult.Success)
                            return WebCallResult<T>.CreateErrorResult(response.StatusCode, response.ResponseHeaders, parseResult.Error!);
                        var error = await TryParseError(parseResult.Data);
                        if (error != null)
                            return WebCallResult<T>.CreateErrorResult(response.StatusCode, response.ResponseHeaders, error);

                        var deserializeResult = Deserialize<T>(parseResult.Data, null, null, request.RequestId);
                        return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, deserializeResult.Data, deserializeResult.Error);
                    }
                    else
                    {
                        var desResult = await Deserialize<T>(responseStream, null, request.RequestId, sw.ElapsedMilliseconds).ConfigureAwait(false);
                        responseStream.Close();
                        response.Close();

                        return new WebCallResult<T>(statusCode, headers, desResult.Data, desResult.Error);
                    }
                }
                else
                {
                    using var reader = new StreamReader(responseStream);
                    var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                    log.Write(LogVerbosity.Debug, $"[{request.RequestId}] Error received in {sw.ElapsedMilliseconds}ms: {data}");
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
                log.Write(LogVerbosity.Warning, $"[{request.RequestId}] Request exception: " + (requestException.InnerException?.Message ?? requestException.Message));
                return new WebCallResult<T>(null, null, default, new ServerError(requestException.Message));
            }
            catch (TaskCanceledException canceledException)
            {
                if (canceledException.CancellationToken == cancellationToken)
                {
                    // Cancellation token cancelled
                    log.Write(LogVerbosity.Warning, $"[{request.RequestId}] Request cancel requested");
                    return new WebCallResult<T>(null, null, default, new CancellationRequestedError());
                }
                else
                {
                    // Request timed out
                    log.Write(LogVerbosity.Warning, $"[{request.RequestId}] Request timed out");
                    return new WebCallResult<T>(null, null, default, new WebError($"[{request.RequestId}] Request timed out"));
                }
            }
        }

        /// <summary>
        /// Can be used to parse an error even though response status indicates success. Some apis always return 200 OK, even though there is an error.
        /// This can be used together with ManualParseError to check if it is an error before deserializing to an object
        /// </summary>
        /// <param name="data">Received data</param>
        /// <returns>Null if not an error, Error otherwise</returns>
        protected virtual Task<ServerError?> TryParseError(JToken data)
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
        /// <param name="postPosition">Where the post parameters should be placed</param>
        /// <param name="arraySerialization">How array parameters should be serialized</param>
        /// <param name="requestId">Unique id of a request</param>
        /// <returns></returns>
        protected virtual IRequest ConstructRequest(Uri uri, HttpMethod method, Dictionary<string, object>? parameters, bool signed, PostParameters postPosition, ArrayParametersSerialization arraySerialization, int requestId)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            var uriString = uri.ToString();
            if (authProvider != null)
                parameters = authProvider.AddAuthenticationToParameters(uriString, method, parameters, signed, postPosition, arraySerialization);

            if ((method == HttpMethod.Get || method == HttpMethod.Delete || postPosition == PostParameters.InUri) && parameters?.Any() == true)
                uriString += "?" + parameters.CreateParamString(true, arraySerialization);

            var contentType = requestBodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
            var request = RequestFactory.Create(method, uriString, requestId);
            request.Accept = Constants.JsonContentHeader;

            var headers = new Dictionary<string, string>();
            if (authProvider != null)
                headers = authProvider.AddAuthenticationToHeaders(uriString, method, parameters!, signed, postPosition, arraySerialization);

            foreach (var header in headers)
                request.AddHeader(header.Key, header.Value);

            if ((method == HttpMethod.Post || method == HttpMethod.Put) && postPosition != PostParameters.InUri)
            {
                if (parameters?.Any() == true)
                    WriteParamBody(request, parameters, contentType);
                else
                    request.SetContent(requestBodyEmptyContent, contentType);
            }

            return request;
        }

        /// <summary>
        /// Writes the parameters of the request to the request object, either in the query string or the request body
        /// </summary>
        /// <param name="request"></param>
        /// <param name="parameters"></param>
        /// <param name="contentType"></param>
        protected virtual void WriteParamBody(IRequest request, Dictionary<string, object> parameters, string contentType)
        {
            if (requestBodyFormat == RequestBodyFormat.Json)
            {
                var stringData = JsonConvert.SerializeObject(parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value));
                request.SetContent(stringData, contentType);
            }
            else if (requestBodyFormat == RequestBodyFormat.FormData)
            {
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

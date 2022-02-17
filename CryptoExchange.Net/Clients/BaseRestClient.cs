using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Requests;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Base rest client
    /// </summary>
    public abstract class BaseRestClient : BaseClient, IRestClient
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

        /// <inheritdoc />
        public int TotalRequestsMade => ApiClients.OfType<RestApiClient>().Sum(s => s.TotalRequestsMade);

        /// <summary>
        /// Request headers to be sent with each request
        /// </summary>
        protected Dictionary<string, string>? StandardRequestHeaders { get; set; }

        /// <summary>
        /// Client options
        /// </summary>
        public new BaseRestClientOptions ClientOptions { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name">The name of the API this client is for</param>
        /// <param name="options">The options for this client</param>
        protected BaseRestClient(string name, BaseRestClientOptions options) : base(name, options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            ClientOptions = options;
            RequestFactory.Configure(options.RequestTimeout, options.Proxy, options.HttpClient);
        }

        /// <inheritdoc />
        public void SetApiCredentials(ApiCredentials credentials)
        {
            foreach (var apiClient in ApiClients)
                apiClient.SetApiCredentials(credentials);
        }

        /// <summary>
        /// Execute a request to the uri and deserialize the response into the provided type parameter
        /// </summary>
        /// <typeparam name="T">The type to deserialize into</typeparam>
        /// <param name="apiClient">The API client the request is for</param>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <param name="signed">Whether or not the request should be authenticated</param>
        /// <param name="parameterPosition">Where the parameters should be placed, overwrites the value set in the client</param>
        /// <param name="arraySerialization">How array parameters should be serialized, overwrites the value set in the client</param>
        /// <param name="requestWeight">Credits used for the request</param>
        /// <param name="deserializer">The JsonSerializer to use for deserialization</param>
        /// <param name="additionalHeaders">Additional headers to send with the request</param>
        /// <returns></returns>
        [return: NotNull]
        protected virtual async Task<WebCallResult<T>> SendRequestAsync<T>(
            RestApiClient apiClient,
            Uri uri, 
            HttpMethod method, 
            CancellationToken cancellationToken,            
            Dictionary<string, object>? parameters = null, 
            bool signed = false, 
            HttpMethodParameterPosition? parameterPosition = null,
            ArrayParametersSerialization? arraySerialization = null, 
            int requestWeight = 1,
            JsonSerializer? deserializer = null,
            Dictionary<string, string>? additionalHeaders = null
            ) where T : class
        {
            var requestId = NextId();

            if (signed)
            {
                var syncTimeResult = await apiClient.SyncTimeAsync().ConfigureAwait(false);
                if (!syncTimeResult)
                {
                    log.Write(LogLevel.Debug, $"[{requestId}] Failed to sync time, aborting request: " + syncTimeResult.Error);
                    return syncTimeResult.As<T>(default);
                }
            }

            log.Write(LogLevel.Debug, $"[{requestId}] Creating request for " + uri);
            if (signed && apiClient.AuthenticationProvider == null)
            {
                log.Write(LogLevel.Warning, $"[{requestId}] Request {uri.AbsolutePath} failed because no ApiCredentials were provided");
                return new WebCallResult<T>(new NoApiCredentialsError());
            }

            var paramsPosition = parameterPosition ?? ParameterPositions[method];
            var request = ConstructRequest(apiClient, uri, method, parameters, signed, paramsPosition, arraySerialization ?? this.arraySerialization, requestId, additionalHeaders);
            foreach (var limiter in apiClient.RateLimiters)
            {
                var limitResult = await limiter.LimitRequestAsync(log, uri.AbsolutePath, method, signed, apiClient.Options.ApiCredentials?.Key, apiClient.Options.RateLimitingBehaviour, requestWeight, cancellationToken).ConfigureAwait(false);
                if (!limitResult.Success)                
                    return new WebCallResult<T>(limitResult.Error!);
            }

            string? paramString = "";
            if (paramsPosition == HttpMethodParameterPosition.InBody)
                paramString = $" with request body '{request.Content}'";

            if (log.Level == LogLevel.Trace)
            {
                var headers = request.GetHeaders();
                if (headers.Any())
                    paramString += " with headers " + string.Join(", ", headers.Select(h => h.Key + $"=[{string.Join(",", h.Value)}]"));
            }

            apiClient.TotalRequestsMade++;
            log.Write(LogLevel.Debug, $"[{requestId}] Sending {method}{(signed ? " signed" : "")} request to {request.Uri}{paramString ?? " "}{(ClientOptions.Proxy == null ? "" : $" via proxy {ClientOptions.Proxy.Host}")}");
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
                            return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, ClientOptions.OutputOriginalData ? data : null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, parseResult.Error!);

                        // Let the library implementation see if it is an error response, and if so parse the error
                        var error = await TryParseErrorAsync(parseResult.Data).ConfigureAwait(false);
                        if (error != null)
                            return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, ClientOptions.OutputOriginalData ? data : null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, error!);

                        // Not an error, so continue deserializing
                        var deserializeResult = Deserialize<T>(parseResult.Data, deserializer, request.RequestId);
                        return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, ClientOptions.OutputOriginalData ? data: null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), deserializeResult.Data, deserializeResult.Error);
                    }
                    else
                    {
                        // Success status code, and we don't have to check for errors. Continue deserializing directly from the stream
                        var desResult = await DeserializeAsync<T>(responseStream, deserializer, request.RequestId, sw.ElapsedMilliseconds).ConfigureAwait(false);
                        responseStream.Close();
                        response.Close();

                        return new WebCallResult<T>(statusCode, headers, sw.Elapsed, ClientOptions.OutputOriginalData ? desResult.OriginalData : null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), desResult.Data, desResult.Error);
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
                    return new WebCallResult<T>(statusCode, headers, sw.Elapsed, data, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, error);
                }
            }
            catch (HttpRequestException requestException)
            {
                // Request exception, can't reach server for instance
                var exceptionInfo = requestException.ToLogString();
                log.Write(LogLevel.Warning, $"[{request.RequestId}] Request exception: " + exceptionInfo);
                return new WebCallResult<T>(null, null, null, null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, new WebError(exceptionInfo));
            }
            catch (OperationCanceledException canceledException)
            {
                if (cancellationToken != default && canceledException.CancellationToken == cancellationToken)
                {
                    // Cancellation token canceled by caller
                    log.Write(LogLevel.Warning, $"[{request.RequestId}] Request canceled by cancellation token");
                    return new WebCallResult<T>(null, null, null, null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, new CancellationRequestedError());
                }
                else
                {
                    // Request timed out
                    log.Write(LogLevel.Warning, $"[{request.RequestId}] Request timed out: " + canceledException.ToLogString());
                    return new WebCallResult<T>(null, null, null, null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, new WebError($"[{request.RequestId}] Request timed out"));
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
        /// <param name="apiClient">The API client the request is for</param>
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
            RestApiClient apiClient,
            Uri uri,
            HttpMethod method,
            Dictionary<string, object>? parameters,
            bool signed,
            HttpMethodParameterPosition parameterPosition,
            ArrayParametersSerialization arraySerialization,
            int requestId,
            Dictionary<string, string>? additionalHeaders)
        {
            parameters ??= new Dictionary<string, object>();
            if (parameterPosition == HttpMethodParameterPosition.InUri)
            {
                foreach (var parameter in parameters)
                    uri = uri.AddQueryParmeter(parameter.Key, parameter.Value.ToString());
            }

            var headers = new Dictionary<string, string>();
            var uriParameters = parameterPosition == HttpMethodParameterPosition.InUri ? new SortedDictionary<string, object>(parameters) : new SortedDictionary<string, object>();
            var bodyParameters = parameterPosition == HttpMethodParameterPosition.InBody ? new SortedDictionary<string, object>(parameters) : new SortedDictionary<string, object>();
            if (apiClient.AuthenticationProvider != null)
                apiClient.AuthenticationProvider.AuthenticateRequest(
                    apiClient, 
                    uri, 
                    method, 
                    parameters, 
                    signed, 
                    arraySerialization,
                    parameterPosition,
                    out uriParameters, 
                    out bodyParameters, 
                    out headers);
                 
            // Sanity check
            foreach(var param in parameters)
            {
                if (!uriParameters.ContainsKey(param.Key) && !bodyParameters.ContainsKey(param.Key))
                    throw new Exception($"Missing parameter {param.Key} after authentication processing. AuthenticationProvider implementation " +
                        $"should return provided parameters in either the uri or body parameters output");
            }

            // Add the auth parameters to the uri, start with a new URI to be able to sort the parameters including the auth parameters            
            uri = uri.SetParameters(uriParameters);
        
            var request = RequestFactory.Create(method, uri, requestId);
            request.Accept = Constants.JsonContentHeader;

            foreach (var header in headers)
                request.AddHeader(header.Key, header.Value);

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                    request.AddHeader(header.Key, header.Value);
            }

            if (StandardRequestHeaders != null)
            {
                foreach (var header in StandardRequestHeaders)
                    // Only add it if it isn't overwritten
                    if (additionalHeaders?.ContainsKey(header.Key) != true)
                        request.AddHeader(header.Key, header.Value);
            }

            if (parameterPosition == HttpMethodParameterPosition.InBody)
            {
                var contentType = requestBodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
                if (bodyParameters.Any())
                    WriteParamBody(request, bodyParameters, contentType);
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
        protected virtual void WriteParamBody(IRequest request, SortedDictionary<string, object> parameters, string contentType)
        {
            if (requestBodyFormat == RequestBodyFormat.Json)
            {
                // Write the parameters as json in the body
                var stringData = JsonConvert.SerializeObject(parameters);
                request.SetContent(stringData, contentType);
            }
            else if (requestBodyFormat == RequestBodyFormat.FormData)
            {
                // Write the parameters as form data in the body
                var stringData = parameters.ToFormData();
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

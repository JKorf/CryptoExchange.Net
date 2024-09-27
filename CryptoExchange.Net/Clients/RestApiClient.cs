using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Caching;
using CryptoExchange.Net.Converters.JsonNet;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.Requests;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Clients
{
    /// <summary>
    /// Base rest API client for interacting with a REST API
    /// </summary>
    public abstract class RestApiClient : BaseApiClient, IRestApiClient
    {
        /// <inheritdoc />
        public IRequestFactory RequestFactory { get; set; } = new RequestFactory();

        /// <inheritdoc />
        public abstract TimeSyncInfo? GetTimeSyncInfo();

        /// <inheritdoc />
        public abstract TimeSpan? GetTimeOffset();

        /// <inheritdoc />
        public int TotalRequestsMade { get; set; }

        /// <summary>
        /// Request body content type
        /// </summary>
        protected internal RequestBodyFormat RequestBodyFormat = RequestBodyFormat.Json;

        /// <summary>
        /// How to serialize array parameters when making requests
        /// </summary>
        protected internal ArrayParametersSerialization ArraySerialization = ArrayParametersSerialization.Array;

        /// <summary>
        /// What request body should be set when no data is send (only used in combination with postParametersPosition.InBody)
        /// </summary>
        protected internal string RequestBodyEmptyContent = "{}";

        /// <summary>
        /// Request headers to be sent with each request
        /// </summary>
        protected Dictionary<string, string>? StandardRequestHeaders { get; set; }

        /// <summary>
        /// Whether parameters need to be ordered
        /// </summary>
        protected internal bool OrderParameters { get; set; } = true;

        /// <summary>
        /// Parameter order comparer
        /// </summary>
        protected IComparer<string> ParameterOrderComparer { get; } = new OrderedStringComparer();

        /// <summary>
        /// Where to put the parameters for requests with different Http methods
        /// </summary>
        public Dictionary<HttpMethod, HttpMethodParameterPosition> ParameterPositions { get; set; } = new Dictionary<HttpMethod, HttpMethodParameterPosition>
        {
            { HttpMethod.Get, HttpMethodParameterPosition.InUri },
            { HttpMethod.Post, HttpMethodParameterPosition.InBody },
            { HttpMethod.Delete, HttpMethodParameterPosition.InBody },
            { HttpMethod.Put, HttpMethodParameterPosition.InBody },
            { new HttpMethod("Patch"), HttpMethodParameterPosition.InBody },
        };

        /// <inheritdoc />
        public new RestExchangeOptions ClientOptions => (RestExchangeOptions)base.ClientOptions;

        /// <inheritdoc />
        public new RestApiOptions ApiOptions => (RestApiOptions)base.ApiOptions;

        /// <summary>
        /// Memory cache
        /// </summary>
        private static MemoryCache _cache = new MemoryCache();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="httpClient">HttpClient to use</param>
        /// <param name="baseAddress">Base address for this API client</param>
        /// <param name="options">The base client options</param>
        /// <param name="apiOptions">The Api client options</param>
        public RestApiClient(ILogger logger, HttpClient? httpClient, string baseAddress, RestExchangeOptions options, RestApiOptions apiOptions)
            : base(logger,
                  apiOptions.OutputOriginalData ?? options.OutputOriginalData,
                  apiOptions.ApiCredentials ?? options.ApiCredentials,
                  baseAddress,
                  options,
                  apiOptions)
        {
            RequestFactory.Configure(options.Proxy, options.RequestTimeout, httpClient);
        }

        /// <summary>
        /// Create a message accessor instance
        /// </summary>
        /// <returns></returns>
        protected virtual IStreamMessageAccessor CreateAccessor() => new JsonNetStreamMessageAccessor();

        /// <summary>
        /// Create a serializer instance
        /// </summary>
        /// <returns></returns>
        protected virtual IMessageSerializer CreateSerializer() => new JsonNetMessageSerializer();

        /// <summary>
        /// Send a request to the base address based on the request definition
        /// </summary>
        /// <param name="baseAddress">Host and schema</param>
        /// <param name="definition">Request definition</param>
        /// <param name="parameters">Request parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="additionalHeaders">Additional headers for this request</param>
        /// <param name="weight">Override the request weight for this request definition, for example when the weight depends on the parameters</param>
        /// <returns></returns>
        protected virtual async Task<WebCallResult> SendAsync(
            string baseAddress,
            RequestDefinition definition,
            ParameterCollection? parameters,
            CancellationToken cancellationToken,
            Dictionary<string, string>? additionalHeaders = null,
            int? weight = null)
        { 
            var result = await SendAsync<object>(baseAddress, definition, parameters, cancellationToken, additionalHeaders, weight).ConfigureAwait(false);
            return result.AsDataless();
        }

        /// <summary>
        /// Send a request to the base address based on the request definition
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="baseAddress">Host and schema</param>
        /// <param name="definition">Request definition</param>
        /// <param name="parameters">Request parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="additionalHeaders">Additional headers for this request</param>
        /// <param name="weight">Override the request weight for this request definition, for example when the weight depends on the parameters</param>
        /// <returns></returns>
        protected virtual Task<WebCallResult<T>> SendAsync<T>(
            string baseAddress,
            RequestDefinition definition,
            ParameterCollection? parameters,
            CancellationToken cancellationToken,
            Dictionary<string, string>? additionalHeaders = null,
            int? weight = null) where T : class
        {
            var parameterPosition = definition.ParameterPosition ?? ParameterPositions[definition.Method];
            return SendAsync<T>(
                baseAddress,
                definition,
                parameterPosition == HttpMethodParameterPosition.InUri ? parameters : null,
                parameterPosition == HttpMethodParameterPosition.InBody ? parameters : null,
                cancellationToken,
                additionalHeaders,
                weight);
        }

        /// <summary>
        /// Send a request to the base address based on the request definition
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="baseAddress">Host and schema</param>
        /// <param name="definition">Request definition</param>
        /// <param name="uriParameters">Request query parameters</param>
        /// <param name="bodyParameters">Request body parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="additionalHeaders">Additional headers for this request</param>
        /// <param name="weight">Override the request weight for this request definition, for example when the weight depends on the parameters</param>
        /// <returns></returns>
        protected virtual async Task<WebCallResult<T>> SendAsync<T>(
            string baseAddress,
            RequestDefinition definition,
            ParameterCollection? uriParameters,
            ParameterCollection? bodyParameters,
            CancellationToken cancellationToken,
            Dictionary<string, string>? additionalHeaders = null,
            int? weight = null) where T : class
        {
            string? cacheKey = null;
            if (ShouldCache(definition))
            {
                cacheKey = baseAddress + definition + uriParameters?.ToFormData();
                _logger.CheckingCache(cacheKey);
                var cachedValue = _cache.Get(cacheKey, ClientOptions.CachingMaxAge);
                if (cachedValue != null)
                {
                    _logger.CacheHit(cacheKey);
                    var original = (WebCallResult<T>)cachedValue;
                    return original.Cached();
                }

                _logger.CacheNotHit(cacheKey);
            }

            int currentTry = 0;
            while (true)
            {
                currentTry++;
                var requestId = ExchangeHelpers.NextId();

                var prepareResult = await PrepareAsync(requestId, baseAddress, definition, cancellationToken, additionalHeaders, weight).ConfigureAwait(false);
                if (!prepareResult)
                    return new WebCallResult<T>(prepareResult.Error!);

                var request = CreateRequest(
                    requestId,
                    baseAddress,
                    definition,
                    uriParameters,
                    bodyParameters,
                    additionalHeaders);
                _logger.RestApiSendRequest(request.RequestId, definition, request.Content, string.IsNullOrEmpty(request.Uri.Query) ? "-" : request.Uri.Query, string.Join(", ", request.GetHeaders().Select(h => h.Key + $"=[{string.Join(",", h.Value)}]")));
                TotalRequestsMade++;
                var result = await GetResponseAsync<T>(request, definition.RateLimitGate, cancellationToken).ConfigureAwait(false);
                if (!result)
                    _logger.RestApiErrorReceived(result.RequestId, result.ResponseStatusCode, (long)Math.Floor(result.ResponseTime!.Value.TotalMilliseconds), result.Error?.ToString());
                else
                    _logger.RestApiResponseReceived(result.RequestId, result.ResponseStatusCode, (long)Math.Floor(result.ResponseTime!.Value.TotalMilliseconds), OutputOriginalData ? result.OriginalData : "[Data only available when OutputOriginal = true]");

                if (await ShouldRetryRequestAsync(definition.RateLimitGate, result, currentTry).ConfigureAwait(false))
                    continue;

                if (result.Success &&
                    ShouldCache(definition))
                {
                    _cache.Add(cacheKey!, result);
                }

                return result;
            }
        }

        /// <summary>
        /// Prepare before sending a request. Sync time between client and server and check rate limits
        /// </summary>
        /// <param name="requestId">Request id</param>
        /// <param name="baseAddress">Host and schema</param>
        /// <param name="definition">Request definition</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="additionalHeaders">Additional headers for this request</param>
        /// <param name="weight">Override the request weight for this request</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected virtual async Task<CallResult> PrepareAsync(
            int requestId,
            string baseAddress,
            RequestDefinition definition,
            CancellationToken cancellationToken,
            Dictionary<string, string>? additionalHeaders = null,
            int? weight = null)
        {
            var requestWeight = weight ?? definition.Weight;

            // Time sync
            if (definition.Authenticated)
            {
                if (AuthenticationProvider == null)
                {
                    _logger.RestApiNoApiCredentials(requestId, definition.Path);
                    return new CallResult<IRequest>(new NoApiCredentialsError());
                }

                var syncTask = SyncTimeAsync();
                var timeSyncInfo = GetTimeSyncInfo();

                if (timeSyncInfo != null && timeSyncInfo.TimeSyncState.LastSyncTime == default)
                {
                    // Initially with first request we'll need to wait for the time syncing, if it's not the first request we can just continue
                    var syncTimeResult = await syncTask.ConfigureAwait(false);
                    if (!syncTimeResult)
                    {
                        _logger.RestApiFailedToSyncTime(requestId, syncTimeResult.Error!.ToString());
                        return syncTimeResult.AsDataless();
                    }
                }
            }            

            // Rate limiting
            if (requestWeight != 0)
            {
                if (definition.RateLimitGate == null)
                    throw new Exception("Ratelimit gate not set when request weight is not 0");

                if (ClientOptions.RateLimiterEnabled)
                {
                    var limitResult = await definition.RateLimitGate.ProcessAsync(_logger, requestId, RateLimitItemType.Request, definition, baseAddress, AuthenticationProvider?._credentials.Key, requestWeight, ClientOptions.RateLimitingBehaviour, cancellationToken).ConfigureAwait(false);
                    if (!limitResult)
                        return new CallResult(limitResult.Error!);
                }
            }

            // Endpoint specific rate limiting
            if (definition.LimitGuard != null && ClientOptions.RateLimiterEnabled)
            {
                if (definition.RateLimitGate == null)
                    throw new Exception("Ratelimit gate not set when endpoint limit is specified");

                if (ClientOptions.RateLimiterEnabled)
                {
                    var limitResult = await definition.RateLimitGate.ProcessSingleAsync(_logger, requestId, definition.LimitGuard, RateLimitItemType.Request, definition, baseAddress, AuthenticationProvider?._credentials.Key, ClientOptions.RateLimitingBehaviour, cancellationToken).ConfigureAwait(false);
                    if (!limitResult)
                        return new CallResult(limitResult.Error!);
                }
            }

            return new CallResult(null);
        }

        /// <summary>
        /// Creates a request object
        /// </summary>
        /// <param name="requestId">Id of the request</param>
        /// <param name="baseAddress">Host and schema</param>
        /// <param name="definition">Request definition</param>
        /// <param name="uriParameters">The query parameters of the request</param>
        /// <param name="bodyParameters">The body parameters of the request</param>
        /// <param name="additionalHeaders">Additional headers to send with the request</param>
        /// <returns></returns>
        protected virtual IRequest CreateRequest(
            int requestId,
            string baseAddress,
            RequestDefinition definition,
            ParameterCollection? uriParameters,
            ParameterCollection? bodyParameters,
            Dictionary<string, string>? additionalHeaders)
        {
            var uriParams = uriParameters == null ? null : CreateParameterDictionary(uriParameters);
            var bodyParams = bodyParameters == null ? null : CreateParameterDictionary(bodyParameters);

            var uri = new Uri(baseAddress.AppendPath(definition.Path));
            var arraySerialization = definition.ArraySerialization ?? ArraySerialization;
            var bodyFormat = definition.RequestBodyFormat ?? RequestBodyFormat;
            var parameterPosition = definition.ParameterPosition ?? ParameterPositions[definition.Method];

            Dictionary<string, string>? headers = null;
            if (AuthenticationProvider != null)
            {
                try
                {
                    AuthenticationProvider.AuthenticateRequest(
                        this,
                        uri,
                        definition.Method,
                        ref uriParams,
                        ref bodyParams,
                        ref headers,
                        definition.Authenticated,
                        arraySerialization,
                        parameterPosition,
                        bodyFormat                        
                        );
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to authenticate request, make sure your API credentials are correct", ex);
                }
            }

            // Add the auth parameters to the uri, start with a new URI to be able to sort the parameters including the auth parameters
            if (uriParams != null)
                uri = uri.SetParameters(uriParams, arraySerialization);

            var request = RequestFactory.Create(definition.Method, uri, requestId);
            request.Accept = Constants.JsonContentHeader;

            if (headers != null)
            {
                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value);
            }

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                    request.AddHeader(header.Key, header.Value);
            }

            if (StandardRequestHeaders != null)
            {
                foreach (var header in StandardRequestHeaders)
                {
                    // Only add it if it isn't overwritten
                    if (additionalHeaders?.ContainsKey(header.Key) != true)
                        request.AddHeader(header.Key, header.Value);
                }
            }

            if (parameterPosition == HttpMethodParameterPosition.InBody)
            {
                var contentType = bodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
                if (bodyParams != null && bodyParams.Count != 0)
                    WriteParamBody(request, bodyParams, contentType);
                else
                    request.SetContent(RequestBodyEmptyContent, contentType);
            }

            return request;
        }

        /// <summary>
        /// Execute a request to the uri and returns if it was successful
        /// </summary>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <param name="signed">Whether or not the request should be authenticated</param>
        /// <param name="requestBodyFormat">The format of the body content</param>
        /// <param name="parameterPosition">Where the parameters should be placed, overwrites the value set in the client</param>
        /// <param name="arraySerialization">How array parameters should be serialized, overwrites the value set in the client</param>
        /// <param name="requestWeight">Credits used for the request</param>
        /// <param name="additionalHeaders">Additional headers to send with the request</param>
        /// <param name="gate">The ratelimit gate to use</param>
        /// <returns></returns>
        [return: NotNull]
        protected virtual async Task<WebCallResult> SendRequestAsync(
            Uri uri,
            HttpMethod method,
            CancellationToken cancellationToken,
            Dictionary<string, object>? parameters = null,
            bool signed = false,
            RequestBodyFormat? requestBodyFormat = null,
            HttpMethodParameterPosition? parameterPosition = null,
            ArrayParametersSerialization? arraySerialization = null,
            int requestWeight = 1,
            Dictionary<string, string>? additionalHeaders = null,
            IRateLimitGate? gate = null)
        {
            int currentTry = 0;
            while (true)
            {
                currentTry++;
                var request = await PrepareRequestAsync(uri, method, cancellationToken, parameters, signed, requestBodyFormat, parameterPosition, arraySerialization, requestWeight, additionalHeaders, gate).ConfigureAwait(false);
                if (!request)
                    return new WebCallResult(request.Error!);

                var result = await GetResponseAsync<object>(request.Data, gate, cancellationToken).ConfigureAwait(false);
                if (!result)
                    _logger.RestApiErrorReceived(result.RequestId, result.ResponseStatusCode, (long)Math.Floor(result.ResponseTime!.Value.TotalMilliseconds), result.Error?.ToString());
                else
                    _logger.RestApiResponseReceived(result.RequestId, result.ResponseStatusCode, (long)Math.Floor(result.ResponseTime!.Value.TotalMilliseconds), OutputOriginalData ? result.OriginalData : "[Data only available when OutputOriginal = true]");

                if (await ShouldRetryRequestAsync(gate, result, currentTry).ConfigureAwait(false))
                    continue;

                return result.AsDataless();
            }
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
        /// <param name="requestBodyFormat">The format of the body content</param>
        /// <param name="parameterPosition">Where the parameters should be placed, overwrites the value set in the client</param>
        /// <param name="arraySerialization">How array parameters should be serialized, overwrites the value set in the client</param>
        /// <param name="requestWeight">Credits used for the request</param>
        /// <param name="additionalHeaders">Additional headers to send with the request</param>
        /// <param name="gate">The ratelimit gate to use</param>
        /// <param name="preventCaching">Whether caching should be prevented for this request</param>
        /// <returns></returns>
        [return: NotNull]
        protected virtual async Task<WebCallResult<T>> SendRequestAsync<T>(
            Uri uri,
            HttpMethod method,
            CancellationToken cancellationToken,
            Dictionary<string, object>? parameters = null,
            bool signed = false,
            RequestBodyFormat? requestBodyFormat = null,
            HttpMethodParameterPosition? parameterPosition = null,
            ArrayParametersSerialization? arraySerialization = null,
            int requestWeight = 1,
            Dictionary<string, string>? additionalHeaders = null,
            IRateLimitGate? gate = null,
            bool preventCaching = false
            ) where T : class
        {
            var key = uri.ToString() + method + signed + parameters?.ToFormData();
            if (ShouldCache(method) && !preventCaching)
            {
                _logger.CheckingCache(key);
                var cachedValue = _cache.Get(key, ClientOptions.CachingMaxAge);
                if (cachedValue != null)
                {
                    _logger.CacheHit(key);
                    var original = (WebCallResult<T>)cachedValue;
                    return original.Cached();
                }

                _logger.CacheNotHit(key);
            }

            int currentTry = 0;
            while (true)
            {
                currentTry++;
                var request = await PrepareRequestAsync(uri, method, cancellationToken, parameters, signed, requestBodyFormat, parameterPosition, arraySerialization, requestWeight, additionalHeaders, gate).ConfigureAwait(false);
                if (!request)
                    return new WebCallResult<T>(request.Error!);

                var result = await GetResponseAsync<T>(request.Data, gate, cancellationToken).ConfigureAwait(false);
                if (!result)
                    _logger.RestApiErrorReceived(result.RequestId, result.ResponseStatusCode, (long)Math.Floor(result.ResponseTime!.Value.TotalMilliseconds), result.Error?.ToString());
                else
                    _logger.RestApiResponseReceived(result.RequestId, result.ResponseStatusCode, (long)Math.Floor(result.ResponseTime!.Value.TotalMilliseconds), OutputOriginalData ? result.OriginalData : "[Data only available when OutputOriginal = true]");

                if (await ShouldRetryRequestAsync(gate, result, currentTry).ConfigureAwait(false))
                    continue;

                if (result.Success &&
                    ShouldCache(method) && 
                    !preventCaching)
                {
                    _cache.Add(key, result);
                }

                return result;
            }
        }

        /// <summary>
        /// Prepares a request to be sent to the server
        /// </summary>
        /// <param name="uri">The uri to send the request to</param>
        /// <param name="method">The method of the request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="parameters">The parameters of the request</param>
        /// <param name="signed">Whether or not the request should be authenticated</param>
        /// <param name="requestBodyFormat">The format of the body content</param>
        /// <param name="parameterPosition">Where the parameters should be placed, overwrites the value set in the client</param>
        /// <param name="arraySerialization">How array parameters should be serialized, overwrites the value set in the client</param>
        /// <param name="requestWeight">Credits used for the request</param>
        /// <param name="additionalHeaders">Additional headers to send with the request</param>
        /// <param name="gate">The rate limit gate to use</param>
        /// <returns></returns>
        protected virtual async Task<CallResult<IRequest>> PrepareRequestAsync(
            Uri uri,
            HttpMethod method,
            CancellationToken cancellationToken,
            Dictionary<string, object>? parameters = null,
            bool signed = false,
            RequestBodyFormat? requestBodyFormat = null,
            HttpMethodParameterPosition? parameterPosition = null,
            ArrayParametersSerialization? arraySerialization = null,
            int requestWeight = 1,
            Dictionary<string, string>? additionalHeaders = null,
            IRateLimitGate? gate = null)
        {
            var requestId = ExchangeHelpers.NextId();

            if (signed)
            {
                if (AuthenticationProvider == null)
                {
                    _logger.RestApiNoApiCredentials(requestId, uri.AbsolutePath);
                    return new CallResult<IRequest>(new NoApiCredentialsError());
                }

                var syncTask = SyncTimeAsync();
                var timeSyncInfo = GetTimeSyncInfo();

                if (timeSyncInfo != null && timeSyncInfo.TimeSyncState.LastSyncTime == default)
                {
                    // Initially with first request we'll need to wait for the time syncing, if it's not the first request we can just continue
                    var syncTimeResult = await syncTask.ConfigureAwait(false);
                    if (!syncTimeResult)
                    {
                        _logger.RestApiFailedToSyncTime(requestId, syncTimeResult.Error!.ToString());
                        return syncTimeResult.As<IRequest>(default);
                    }
                }
            }
            
            if (requestWeight != 0)
            {
                if (gate == null)
                    throw new Exception("Ratelimit gate not set when request weight is not 0");

                if (ClientOptions.RateLimiterEnabled)
                {
                    var limitResult = await gate.ProcessAsync(_logger, requestId, RateLimitItemType.Request, new RequestDefinition(uri.AbsolutePath.TrimStart('/'), method) { Authenticated = signed }, uri.Host, AuthenticationProvider?._credentials.Key, requestWeight, ClientOptions.RateLimitingBehaviour, cancellationToken).ConfigureAwait(false);
                    if (!limitResult)
                        return new CallResult<IRequest>(limitResult.Error!);
                }
            }

            _logger.RestApiCreatingRequest(requestId, uri);
            var paramsPosition = parameterPosition ?? ParameterPositions[method];
            var request = ConstructRequest(uri, method, parameters?.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value), signed, paramsPosition, arraySerialization ?? ArraySerialization, requestBodyFormat ?? RequestBodyFormat, requestId, additionalHeaders);

            string? paramString = "";
            if (paramsPosition == HttpMethodParameterPosition.InBody)
                paramString = $" with request body '{request.Content}'";

            var headers = request.GetHeaders();
            if (headers.Any())
                paramString += " with headers " + string.Join(", ", headers.Select(h => h.Key + $"=[{string.Join(",", h.Value)}]"));

            TotalRequestsMade++;
            _logger.RestApiSendingRequest(requestId, method, signed ? "signed": "", request.Uri, paramString);
            return new CallResult<IRequest>(request);
        }

        /// <summary>
        /// Executes the request and returns the result deserialized into the type parameter class
        /// </summary>
        /// <param name="request">The request object to execute</param>
        /// <param name="gate">The ratelimit gate used</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        protected virtual async Task<WebCallResult<T>> GetResponseAsync<T>(
            IRequest request,
            IRateLimitGate? gate,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            Stream? responseStream = null;
            IResponse? response = null;
            IStreamMessageAccessor? accessor = null;
            try
            {
                response = await request.GetResponseAsync(cancellationToken).ConfigureAwait(false);
                sw.Stop();
                var statusCode = response.StatusCode;
                var headers = response.ResponseHeaders;
                var responseLength = response.ContentLength;
                responseStream = await response.GetResponseStreamAsync().ConfigureAwait(false);
                var outputOriginalData = ApiOptions.OutputOriginalData ?? ClientOptions.OutputOriginalData;

                accessor = CreateAccessor();
                if (!response.IsSuccessStatusCode)
                {
                    // Error response
                    await accessor.Read(responseStream, true).ConfigureAwait(false);

                    Error error;
                    if (response.StatusCode == (HttpStatusCode)418 || response.StatusCode == (HttpStatusCode)429)
                    {
                        var rateError = ParseRateLimitResponse((int)response.StatusCode, response.ResponseHeaders, accessor);
                        if (rateError.RetryAfter != null && gate != null && ClientOptions.RateLimiterEnabled)
                        {
                            _logger.RestApiRateLimitPauseUntil(request.RequestId, rateError.RetryAfter.Value);
                            await gate.SetRetryAfterGuardAsync(rateError.RetryAfter.Value).ConfigureAwait(false);
                        }

                        error = rateError;
                    }
                    else
                    {
                        error = ParseErrorResponse((int)response.StatusCode, response.ResponseHeaders, accessor);
                    }

                    if (error.Code == null || error.Code == 0)
                        error.Code = (int)response.StatusCode;

                    return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, responseLength, OutputOriginalData ? accessor.GetOriginalString() : null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, error!);
                }

                if (typeof(T) == typeof(object))
                    // Success status code and expected empty response, assume it's correct
                    return new WebCallResult<T>(statusCode, headers, sw.Elapsed, 0, null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, null);

                var valid = await accessor.Read(responseStream, outputOriginalData).ConfigureAwait(false);
                if (!valid)
                {
                    // Invalid json
                    var error = new ServerError("Failed to parse response: " + valid.Error!.Message, accessor.OriginalDataAvailable ? accessor.GetOriginalString() : "[Data only available when OutputOriginal = true in client options]");
                    return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, responseLength, OutputOriginalData ? accessor.GetOriginalString() : null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, error);
                }

                // Json response received
                var parsedError = TryParseError(accessor);
                if (parsedError != null)
                    // Success status code, but TryParseError determined it was an error response
                    return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, responseLength, OutputOriginalData ? accessor.GetOriginalString() : null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, parsedError);

                var deserializeResult = accessor.Deserialize<T>();
                return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, responseLength, OutputOriginalData ? accessor.GetOriginalString() : null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, deserializeResult.Data, deserializeResult.Error);
            }
            catch (HttpRequestException requestException)
            {
                // Request exception, can't reach server for instance
                var exceptionInfo = requestException.ToLogString();
                return new WebCallResult<T>(null, null, sw.Elapsed, null, null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, new WebError(exceptionInfo));
            }
            catch (OperationCanceledException canceledException)
            {
                if (cancellationToken != default && canceledException.CancellationToken == cancellationToken)
                {
                    // Cancellation token canceled by caller
                    return new WebCallResult<T>(null, null, sw.Elapsed, null, null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, new CancellationRequestedError());
                }
                else
                {
                    // Request timed out
                    return new WebCallResult<T>(null, null, sw.Elapsed, null, null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, new WebError($"Request timed out"));
                }
            }
            finally
            {
                accessor?.Clear();
                responseStream?.Close();
                response?.Close();
            }
        }

        /// <summary>
        /// Can be used to parse an error even though response status indicates success. Some apis always return 200 OK, even though there is an error.
        /// When setting manualParseError to true this method will be called for each response to be able to check if the response is an error or not.
        /// If the response is an error this method should return the parsed error, else it should return null
        /// </summary>
        /// <param name="accessor">Data accessor</param>
        /// <returns>Null if not an error, Error otherwise</returns>
        protected virtual ServerError? TryParseError(IMessageAccessor accessor) => null;

        /// <summary>
        /// Can be used to indicate that a request should be retried. Defaults to false. Make sure to retry a max number of times (based on the the tries parameter) or the request will retry forever.
        /// Note that this is always called; even when the request might be successful
        /// </summary>
        /// <typeparam name="T">WebCallResult type parameter</typeparam>
        /// <param name="gate">The rate limit gate the call used</param>
        /// <param name="callResult">The result of the call</param>
        /// <param name="tries">The current try number</param>
        /// <returns>True if call should retry, false if the call should return</returns>
        protected virtual async Task<bool> ShouldRetryRequestAsync<T>(IRateLimitGate? gate, WebCallResult<T> callResult, int tries)
        {
            if (tries >= 2)
                // Only retry once
                return false;

            if ((int?)callResult.ResponseStatusCode == 429 
                && ClientOptions.RateLimiterEnabled
                && ClientOptions.RateLimitingBehaviour != RateLimitingBehaviour.Fail
                && gate != null)
            {
                var retryTime = await gate.GetRetryAfterTime().ConfigureAwait(false);
                if (retryTime == null)
                    return false;

                if (retryTime.Value - DateTime.UtcNow < TimeSpan.FromSeconds(60))
                {
                    _logger.RestApiRateLimitRetry(callResult.RequestId!.Value, retryTime.Value);
                    return true;
                }
            }

            return false;
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
        /// <param name="bodyFormat">Format of the body content</param>
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
            RequestBodyFormat bodyFormat,
            int requestId,
            Dictionary<string, string>? additionalHeaders)
        {
            parameters ??= new Dictionary<string, object>();

            for (var i = 0; i < parameters.Count; i++)
            {
                var kvp = parameters.ElementAt(i);
                if (kvp.Value is Func<object> delegateValue)
                    parameters[kvp.Key] = delegateValue();
            }

            if (parameterPosition == HttpMethodParameterPosition.InUri)
            {
                foreach (var parameter in parameters)
                    uri = uri.AddQueryParmeter(parameter.Key, parameter.Value.ToString());
            }

            var headers = new Dictionary<string, string>();
            var uriParameters = parameterPosition == HttpMethodParameterPosition.InUri ? CreateParameterDictionary(parameters) : null;
            var bodyParameters = parameterPosition == HttpMethodParameterPosition.InBody ? CreateParameterDictionary(parameters) : null;
            if (AuthenticationProvider != null)
            {
                try
                {
                    AuthenticationProvider.AuthenticateRequest(
                        this,
                        uri,
                        method,
                        ref uriParameters,
                        ref bodyParameters,
                        ref headers,
                        signed,
                        arraySerialization,
                        parameterPosition,
                        bodyFormat
                        );
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to authenticate request, make sure your API credentials are correct", ex);
                }
            }

            // Add the auth parameters to the uri, start with a new URI to be able to sort the parameters including the auth parameters            
            if (uriParameters != null)
                uri = uri.SetParameters(uriParameters, arraySerialization);

            var request = RequestFactory.Create(method, uri, requestId);
            request.Accept = Constants.JsonContentHeader;

            if (headers != null)
            {
                foreach (var header in headers)
                    request.AddHeader(header.Key, header.Value);
            }

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                    request.AddHeader(header.Key, header.Value);
            }

            if (StandardRequestHeaders != null)
            {
                foreach (var header in StandardRequestHeaders)
                {
                    // Only add it if it isn't overwritten
                    if (additionalHeaders?.ContainsKey(header.Key) != true)
                        request.AddHeader(header.Key, header.Value);
                }
            }

            if (parameterPosition == HttpMethodParameterPosition.InBody)
            {
                var contentType = bodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
                if (bodyParameters?.Any() == true)
                    WriteParamBody(request, bodyParameters, contentType);
                else
                    request.SetContent(RequestBodyEmptyContent, contentType);
            }

            return request;
        }

        /// <summary>
        /// Writes the parameters of the request to the request object body
        /// </summary>
        /// <param name="request">The request to set the parameters on</param>
        /// <param name="parameters">The parameters to set</param>
        /// <param name="contentType">The content type of the data</param>
        protected virtual void WriteParamBody(IRequest request, IDictionary<string, object> parameters, string contentType)
        {
            if (contentType == Constants.JsonContentHeader)
            {
                // Write the parameters as json in the body
                string stringData;
                if (parameters.Count == 1 && parameters.ContainsKey(Constants.BodyPlaceHolderKey))
                    stringData = CreateSerializer().Serialize(parameters[Constants.BodyPlaceHolderKey]);
                else
                    stringData = CreateSerializer().Serialize(parameters);
                request.SetContent(stringData, contentType);
            }
            else if (contentType == Constants.FormContentHeader)
            {
                // Write the parameters as form data in the body
                var stringData = parameters.ToFormData();
                request.SetContent(stringData, contentType);
            }
        }

        /// <summary>
        /// Parse an error response from the server. Only used when server returns a status other than Success(200) or ratelimit error (429 or 418)
        /// </summary>
        /// <param name="httpStatusCode">The response status code</param>
        /// <param name="responseHeaders">The response headers</param>
        /// <param name="accessor">Data accessor</param>
        /// <returns></returns>
        protected virtual Error ParseErrorResponse(int httpStatusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders, IMessageAccessor accessor)
        {
            var message = accessor.OriginalDataAvailable ? accessor.GetOriginalString() : "[Error response content only available when OutputOriginal = true in client options]";
            return new ServerError(message);
        }

        /// <summary>
        /// Parse a rate limit error response from the server. Only used when server returns http status 429 or 418
        /// </summary>
        /// <param name="httpStatusCode">The response status code</param>
        /// <param name="responseHeaders">The response headers</param>
        /// <param name="accessor">Data accessor</param>
        /// <returns></returns>
        protected virtual ServerRateLimitError ParseRateLimitResponse(int httpStatusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders, IMessageAccessor accessor)
        {
            var message = accessor.OriginalDataAvailable ? accessor.GetOriginalString() : "[Error response content only available when OutputOriginal = true in client options]";

            // Handle retry after header
            var retryAfterHeader = responseHeaders.SingleOrDefault(r => r.Key.Equals("Retry-After", StringComparison.InvariantCultureIgnoreCase));
            if (retryAfterHeader.Value?.Any() != true)
                return new ServerRateLimitError(message);

            var value = retryAfterHeader.Value.First();
            if (int.TryParse(value, out var seconds))
                return new ServerRateLimitError(message) { RetryAfter = DateTime.UtcNow.AddSeconds(seconds) };

            if (DateTime.TryParse(value, out var datetime))
                return new ServerRateLimitError(message) { RetryAfter = datetime };

            return new ServerRateLimitError(message);
        }

        /// <summary>
        /// Create the parameter IDictionary
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected internal IDictionary<string, object> CreateParameterDictionary(IDictionary<string, object> parameters)
        {
            if (!OrderParameters)
                return parameters;

            return new SortedDictionary<string, object>(parameters, ParameterOrderComparer);
        }

        /// <summary>
        /// Retrieve the server time for the purpose of syncing time between client and server to prevent authentication issues
        /// </summary>
        /// <returns>Server time</returns>
        protected virtual Task<WebCallResult<DateTime>> GetServerTimestampAsync() => throw new NotImplementedException();

        internal async Task<WebCallResult<bool>> SyncTimeAsync()
        {
            var timeSyncParams = GetTimeSyncInfo();
            if (timeSyncParams == null)
                return new WebCallResult<bool>(null, null, null, null, null, null, null, null, null, null, ResultDataSource.Server, true, null);

            if (await timeSyncParams.TimeSyncState.Semaphore.WaitAsync(0).ConfigureAwait(false))
            {
                if (!timeSyncParams.SyncTime || DateTime.UtcNow - timeSyncParams.TimeSyncState.LastSyncTime < timeSyncParams.RecalculationInterval)
                {
                    timeSyncParams.TimeSyncState.Semaphore.Release();
                    return new WebCallResult<bool>(null, null, null, null, null, null, null, null, null, null, ResultDataSource.Server, true, null);
                }

                var localTime = DateTime.UtcNow;
                var result = await GetServerTimestampAsync().ConfigureAwait(false);
                if (!result)
                {
                    timeSyncParams.TimeSyncState.Semaphore.Release();
                    return result.As(false);
                }

                if (TotalRequestsMade == 1)
                {
                    // If this was the first request make another one to calculate the offset since the first one can be slower
                    localTime = DateTime.UtcNow;
                    result = await GetServerTimestampAsync().ConfigureAwait(false);
                    if (!result)
                    {
                        timeSyncParams.TimeSyncState.Semaphore.Release();
                        return result.As(false);
                    }
                }

                // Calculate time offset between local and server
                var offset = result.Data - localTime.AddMilliseconds(result.ResponseTime!.Value.TotalMilliseconds / 2);
                timeSyncParams.UpdateTimeOffset(offset);
                timeSyncParams.TimeSyncState.Semaphore.Release();
            }

            return new WebCallResult<bool>(null, null, null, null, null, null, null, null, null, null, ResultDataSource.Server, true, null);
        }

        private bool ShouldCache(RequestDefinition definition)
            => ClientOptions.CachingEnabled
            && definition.Method == HttpMethod.Get
            && !definition.PreventCaching;

        private bool ShouldCache(HttpMethod method)
            => ClientOptions.CachingEnabled
            && method == HttpMethod.Get;
    }
}

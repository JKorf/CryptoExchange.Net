using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Caching;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
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
        protected Dictionary<string, string> StandardRequestHeaders { get; set; } = [];

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
        private readonly static MemoryCache _cache = new MemoryCache();

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
        protected abstract IStreamMessageAccessor CreateAccessor();

        /// <summary>
        /// Create a serializer instance
        /// </summary>
        /// <returns></returns>
        protected abstract IMessageSerializer CreateSerializer();

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
        /// <param name="weightSingleLimiter">Specify the weight to apply to the individual rate limit guard for this request</param>
        /// <param name="rateLimitKeySuffix">An additional optional suffix for the key selector. Can be used to make rate limiting work based on parameters.</param>
        /// <returns></returns>
        protected virtual Task<WebCallResult<T>> SendAsync<T>(
            string baseAddress,
            RequestDefinition definition,
            ParameterCollection? parameters,
            CancellationToken cancellationToken,
            Dictionary<string, string>? additionalHeaders = null,
            int? weight = null,
            int? weightSingleLimiter = null,
            string? rateLimitKeySuffix = null)
        {
            var parameterPosition = definition.ParameterPosition ?? ParameterPositions[definition.Method];
            return SendAsync<T>(
                baseAddress,
                definition,
                parameterPosition == HttpMethodParameterPosition.InUri ? parameters : null,
                parameterPosition == HttpMethodParameterPosition.InBody ? parameters : null,
                cancellationToken,
                additionalHeaders,
                weight,
                weightSingleLimiter,
                rateLimitKeySuffix);
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
        /// <param name="weightSingleLimiter">Specify the weight to apply to the individual rate limit guard for this request</param>
        /// <param name="rateLimitKeySuffix">An additional optional suffix for the key selector. Can be used to make rate limiting work based on parameters.</param>
        /// <returns></returns>
        protected virtual async Task<WebCallResult<T>> SendAsync<T>(
            string baseAddress,
            RequestDefinition definition,
            ParameterCollection? uriParameters,
            ParameterCollection? bodyParameters,
            CancellationToken cancellationToken,
            Dictionary<string, string>? additionalHeaders = null,
            int? weight = null,
            int? weightSingleLimiter = null,
            string? rateLimitKeySuffix = null)
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

                var prepareResult = await PrepareAsync(requestId, baseAddress, definition, cancellationToken, additionalHeaders, weight, weightSingleLimiter, rateLimitKeySuffix).ConfigureAwait(false);
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
                var result = await GetResponseAsync<T>(definition, request, definition.RateLimitGate, cancellationToken).ConfigureAwait(false);
                if (result.Error is not CancellationRequestedError)
                {
                    var originalData = OutputOriginalData ? result.OriginalData : "[Data only available when OutputOriginal = true]";
                    if (!result)
                        _logger.RestApiErrorReceived(result.RequestId, result.ResponseStatusCode, (long)Math.Floor(result.ResponseTime!.Value.TotalMilliseconds), result.Error?.ToString(), originalData, result.Error?.Exception);
                    else
                        _logger.RestApiResponseReceived(result.RequestId, result.ResponseStatusCode, (long)Math.Floor(result.ResponseTime!.Value.TotalMilliseconds), originalData);
                }
                else
                {
                    _logger.RestApiCancellationRequested(result.RequestId);
                }

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
        /// <param name="weightSingleLimiter">Specify the weight to apply to the individual rate limit guard for this request</param>
        /// <param name="rateLimitKeySuffix">An additional optional suffix for the key selector</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected virtual async Task<CallResult> PrepareAsync(
            int requestId,
            string baseAddress,
            RequestDefinition definition,
            CancellationToken cancellationToken,
            Dictionary<string, string>? additionalHeaders = null,
            int? weight = null,
            int? weightSingleLimiter = null,
            string? rateLimitKeySuffix = null)
        {
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
            var requestWeight = weight ?? definition.Weight;
            if (requestWeight != 0)
            {
                if (definition.RateLimitGate == null)
                    throw new Exception("Ratelimit gate not set when request weight is not 0");

                if (ClientOptions.RateLimiterEnabled)
                {
                    var limitResult = await definition.RateLimitGate.ProcessAsync(_logger, requestId, RateLimitItemType.Request, definition, baseAddress, AuthenticationProvider?._credentials.Key, requestWeight, ClientOptions.RateLimitingBehaviour, rateLimitKeySuffix, cancellationToken).ConfigureAwait(false);
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
                    var singleRequestWeight = weightSingleLimiter ?? 1;
                    var limitResult = await definition.RateLimitGate.ProcessSingleAsync(_logger, requestId, definition.LimitGuard, RateLimitItemType.Request, definition, baseAddress, AuthenticationProvider?._credentials.Key, singleRequestWeight, ClientOptions.RateLimitingBehaviour, rateLimitKeySuffix, cancellationToken).ConfigureAwait(false);
                    if (!limitResult)
                        return new CallResult(limitResult.Error!);
                }
            }

            return CallResult.SuccessResult;
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
            var requestConfiguration = new RestRequestConfiguration(
                definition,
                baseAddress,
                uriParameters == null ? new Dictionary<string, object>() : CreateParameterDictionary(uriParameters),
                bodyParameters == null ? new Dictionary<string, object>() : CreateParameterDictionary(bodyParameters),
                new Dictionary<string, string>(additionalHeaders ?? []),
                definition.ArraySerialization ?? ArraySerialization,
                definition.ParameterPosition ?? ParameterPositions[definition.Method],
                definition.RequestBodyFormat ?? RequestBodyFormat);

            try
            {
                AuthenticationProvider?.ProcessRequest(this, requestConfiguration);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to authenticate request, make sure your API credentials are correct", ex);
            }
            
            var queryString = requestConfiguration.GetQueryString(true);
            if (!string.IsNullOrEmpty(queryString) && !queryString.StartsWith("?"))
                queryString = $"?{queryString}";

            var uri = new Uri(baseAddress.AppendPath(definition.Path) + queryString);
            var request = RequestFactory.Create(definition.Method, uri, requestId);
            request.Accept = Constants.JsonContentHeader;

            foreach (var header in requestConfiguration.Headers)
                request.AddHeader(header.Key, header.Value);            

            foreach (var header in StandardRequestHeaders)
            {
                // Only add it if it isn't overwritten
                if (!requestConfiguration.Headers.ContainsKey(header.Key))
                    request.AddHeader(header.Key, header.Value);
            }            

            if (requestConfiguration.ParameterPosition == HttpMethodParameterPosition.InBody)
            {
                var contentType = requestConfiguration.BodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
                var bodyContent = requestConfiguration.GetBodyContent();
                if (bodyContent != null)
                {
                    request.SetContent(bodyContent, contentType);
                }
                else
                {
                    if (requestConfiguration.BodyParameters != null && requestConfiguration.BodyParameters.Count != 0)
                        WriteParamBody(request, requestConfiguration.BodyParameters, contentType);
                    else
                        request.SetContent(RequestBodyEmptyContent, contentType);
                }
            }

            return request;
        }

        /// <summary>
        /// Executes the request and returns the result deserialized into the type parameter class
        /// </summary>
        /// <param name="requestDefinition">The request definition</param>
        /// <param name="request">The request object to execute</param>
        /// <param name="gate">The ratelimit gate used</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        protected virtual async Task<WebCallResult<T>> GetResponseAsync<T>(
            RequestDefinition requestDefinition,
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
                if (!response.IsSuccessStatusCode && !requestDefinition.TryParseOnNonSuccess)
                {
                    // Error response
                    var readResult = await accessor.Read(responseStream, true).ConfigureAwait(false);

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
                        error = ParseErrorResponse((int)response.StatusCode, response.ResponseHeaders, accessor, readResult.Error?.Exception);
                    }

                    if (error.Code == null || error.Code == 0)
                        error.Code = (int)response.StatusCode;

                    return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, responseLength, OutputOriginalData ? accessor.GetOriginalString() : null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, error!);
                }

                var valid = await accessor.Read(responseStream, outputOriginalData).ConfigureAwait(false);
                if (typeof(T) == typeof(object))
                    // Success status code and expected empty response, assume it's correct
                    return new WebCallResult<T>(statusCode, headers, sw.Elapsed, 0, accessor.OriginalDataAvailable ? accessor.GetOriginalString() : "[Data only available when OutputOriginal = true in client options]", request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, null);

                if (!valid)
                {
                    // Invalid json
                    return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, responseLength, OutputOriginalData ? accessor.GetOriginalString() : null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, valid.Error);
                }

                // Json response received
                var parsedError = TryParseError(requestDefinition, response.ResponseHeaders, accessor);
                if (parsedError != null)
                {
                    if (parsedError is ServerRateLimitError rateError)
                    {
                        if (rateError.RetryAfter != null && gate != null && ClientOptions.RateLimiterEnabled)
                        {
                            _logger.RestApiRateLimitPauseUntil(request.RequestId, rateError.RetryAfter.Value);
                            await gate.SetRetryAfterGuardAsync(rateError.RetryAfter.Value).ConfigureAwait(false);
                        }
                    }

                    // Success status code, but TryParseError determined it was an error response
                    return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, responseLength, OutputOriginalData ? accessor.GetOriginalString() : null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, parsedError);
                }

                var deserializeResult = accessor.Deserialize<T>();
                return new WebCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, responseLength, OutputOriginalData ? accessor.GetOriginalString() : null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, deserializeResult.Data, deserializeResult.Error);
            }
            catch (HttpRequestException requestException)
            {
                // Request exception, can't reach server for instance
                var error = new WebError(requestException.Message, requestException);
                return new WebCallResult<T>(null, null, sw.Elapsed, null, null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, error);
            }
            catch (OperationCanceledException canceledException)
            {
                if (cancellationToken != default && canceledException.CancellationToken == cancellationToken)
                {
                    // Cancellation token canceled by caller
                    return new WebCallResult<T>(null, null, sw.Elapsed, null, null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, new CancellationRequestedError(canceledException));
                }
                else
                {
                    // Request timed out
                    var error = new WebError($"Request timed out", exception: canceledException);
                    error.ErrorType = ErrorType.Timeout;
                    return new WebCallResult<T>(null, null, sw.Elapsed, null, null, request.RequestId, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), ResultDataSource.Server, default, error);
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
        /// This method will be called for each response to be able to check if the response is an error or not.
        /// If the response is an error this method should return the parsed error, else it should return null
        /// </summary>
        /// <param name="requestDefinition">Request definition</param>
        /// <param name="accessor">Data accessor</param>
        /// <param name="responseHeaders">The response headers</param>
        /// <returns>Null if not an error, Error otherwise</returns>
        protected virtual Error? TryParseError(RequestDefinition requestDefinition, KeyValuePair<string, string[]>[] responseHeaders, IMessageAccessor accessor) => null;

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

            if (callResult.Error is ServerRateLimitError
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
        /// Writes the parameters of the request to the request object body
        /// </summary>
        /// <param name="request">The request to set the parameters on</param>
        /// <param name="parameters">The parameters to set</param>
        /// <param name="contentType">The content type of the data</param>
        protected virtual void WriteParamBody(IRequest request, IDictionary<string, object> parameters, string contentType)
        {
            if (contentType == Constants.JsonContentHeader)
            {
                var serializer = CreateSerializer();
                if (serializer is not IStringMessageSerializer stringSerializer)
                    throw new InvalidOperationException("Non-string message serializer can't get serialized request body");

                // Write the parameters as json in the body
                string stringData;
                if (parameters.Count == 1 && parameters.TryGetValue(Constants.BodyPlaceHolderKey, out object? value))
                    stringData = stringSerializer.Serialize(value);
                else
                    stringData = stringSerializer.Serialize(parameters);
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
        /// <param name="exception">Exception</param>
        /// <returns></returns>
        protected virtual Error ParseErrorResponse(int httpStatusCode, KeyValuePair<string, string[]>[] responseHeaders, IMessageAccessor accessor, Exception? exception)
        {
            return new ServerError(ErrorInfo.Unknown, exception);
        }

        /// <summary>
        /// Parse a rate limit error response from the server. Only used when server returns http status 429 or 418
        /// </summary>
        /// <param name="httpStatusCode">The response status code</param>
        /// <param name="responseHeaders">The response headers</param>
        /// <param name="accessor">Data accessor</param>
        /// <returns></returns>
        protected virtual ServerRateLimitError ParseRateLimitResponse(int httpStatusCode, KeyValuePair<string, string[]>[] responseHeaders, IMessageAccessor accessor)
        {
            // Handle retry after header
            var retryAfterHeader = responseHeaders.SingleOrDefault(r => r.Key.Equals("Retry-After", StringComparison.InvariantCultureIgnoreCase));
            if (retryAfterHeader.Value?.Any() != true)
                return new ServerRateLimitError();

            var value = retryAfterHeader.Value.First();
            if (int.TryParse(value, out var seconds))
                return new ServerRateLimitError() { RetryAfter = DateTime.UtcNow.AddSeconds(seconds) };

            if (DateTime.TryParse(value, out var datetime))
                return new ServerRateLimitError() { RetryAfter = datetime };

            return new ServerRateLimitError();
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

        /// <inheritdoc />
        public override void SetOptions<T>(UpdateOptions<T> options)
        {
            base.SetOptions(options);

            RequestFactory.UpdateSettings(options.Proxy, options.RequestTimeout ?? ClientOptions.RequestTimeout);
        }

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
    }
}

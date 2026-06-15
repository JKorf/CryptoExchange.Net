using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Caching;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Logging.Extensions;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.RateLimiting;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.Requests;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public int TotalRequestsMade { get; set; }

        /// <summary>
        /// Request body content type
        /// </summary>
        protected internal RequestBodyFormat RequestBodyFormat = RequestBodyFormat.Json;

        /// <summary>
        /// What request body should be set when no data is send (only used in combination with postParametersPosition.InBody)
        /// </summary>
        protected internal string RequestBodyEmptyContent = "{}";

        /// <summary>
        /// Request headers to be sent with each request
        /// </summary>
        protected Dictionary<string, string> StandardRequestHeaders { get; set; } = [];

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

        /// <summary>
        /// Encoding/charset for the ContentType header
        /// </summary>
        protected Encoding? RequestBodyContentEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Whether to omit the ContentType header if there is no content
        /// </summary>
        protected bool OmitContentTypeHeaderWithoutContent { get; set; } = false;

        /// <inheritdoc />
        public new RestExchangeOptions ClientOptions => (RestExchangeOptions)base.ClientOptions;

        /// <inheritdoc />
        public new RestApiOptions ApiOptions => (RestApiOptions)base.ApiOptions;

        /// <summary>
        /// Memory cache
        /// </summary>
        private readonly static MemoryCache _cache = new MemoryCache();

        /// <summary>
        /// The message handler
        /// </summary>
        protected abstract IRestMessageHandler MessageHandler { get; }

        /// <summary>
        /// Get the AuthenticationProvider implementation, or null if no ApiCredentials are set
        /// </summary>
        public virtual AuthenticationProvider? GetAuthenticationProvider() => null;

        /// <summary>
        /// Configured environment name
        /// </summary>
        public abstract string EnvironmentName { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="loggerFactory">Logger factory</param>
        /// <param name="exchangeName">The exchange name</param>
        /// <param name="httpClient">HttpClient to use</param>
        /// <param name="baseAddress">Base address for this API client</param>
        /// <param name="options">The base client options</param>
        /// <param name="apiOptions">The Api client options</param>
        public RestApiClient(ILoggerFactory? loggerFactory,
            string exchangeName,
            HttpClient? httpClient,
            string baseAddress,
            RestExchangeOptions options,
            RestApiOptions apiOptions)
            : base(loggerFactory,
                  exchangeName,
                  apiOptions.OutputOriginalData ?? options.OutputOriginalData,
                  baseAddress,
                  options,
                  apiOptions)
        {
            TimeOffsetManager.RegisterRestApi(ClientName);

            RequestFactory.Configure(options, httpClient);
        }

        /// <summary>
        /// Create a serializer instance
        /// </summary>
        /// <returns></returns>
        protected abstract IMessageSerializer CreateSerializer();

        /// <summary>
        /// Send a request to the base address based on the request definition
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="definition">Request definition</param>
        /// <param name="parameters">Request parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="additionalHeaders">Additional headers for this request</param>
        /// <param name="weight">Override the request weight for this request definition, for example when the weight depends on the parameters</param>
        /// <param name="weightSingleLimiter">Specify the weight to apply to the individual rate limit guard for this request</param>
        /// <param name="rateLimitKeySuffix">An additional optional suffix for the key selector. Can be used to make rate limiting work based on parameters.</param>
        /// <returns></returns>
        protected virtual Task<HttpResult<T>> SendAsync<T>(
            RequestDefinition definition,
            Parameters? parameters,
            CancellationToken cancellationToken,
            Dictionary<string, string>? additionalHeaders = null,
            int? weight = null,
            int? weightSingleLimiter = null,
            string? rateLimitKeySuffix = null)
        {
            var parameterPosition = definition.ParameterPosition ?? ParameterPositions[definition.Method];
            return SendAsync<T>(
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
        /// <param name="definition">Request definition</param>
        /// <param name="uriParameters">Request query parameters</param>
        /// <param name="bodyParameters">Request body parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="additionalHeaders">Additional headers for this request</param>
        /// <param name="weight">Override the request weight for this request definition, for example when the weight depends on the parameters</param>
        /// <param name="weightSingleLimiter">Specify the weight to apply to the individual rate limit guard for this request</param>
        /// <param name="rateLimitKeySuffix">An additional optional suffix for the key selector. Can be used to make rate limiting work based on parameters.</param>
        /// <returns></returns>
        protected virtual async Task<HttpResult<T>> SendAsync<T>(
            RequestDefinition definition,
            Parameters? uriParameters,
            Parameters? bodyParameters,
            CancellationToken cancellationToken,
            Dictionary<string, string>? additionalHeaders = null,
            int? weight = null,
            int? weightSingleLimiter = null,
            string? rateLimitKeySuffix = null)
        {
            var requestId = ExchangeHelpers.NextId();
            if (definition.Authenticated && GetAuthenticationProvider() == null)
            {
                _logger.RestApiNoApiCredentials(requestId, definition.Path);
                return HttpResult.Fail<T>(Exchange, new NoApiCredentialsError());
            }

            string? cacheKey = null;
            if (ShouldCache(definition))
            {
                cacheKey = definition.FullUrl + definition + uriParameters?.ToFormData();
                _logger.CheckingCache(cacheKey);
                var cachedValue = _cache.Get(cacheKey, ClientOptions.CachingMaxAge);
                if (cachedValue != null)
                {
                    _logger.CacheHit(cacheKey);
                    var original = (HttpResult<T>)cachedValue;
                    return original with { DataSource = ResultDataSource.Cache };
                }

                _logger.CacheNotHit(cacheKey);
            }

            int currentTry = 0;
            while (true)
            {
                currentTry++;

                await CheckTimeSync(requestId, definition).ConfigureAwait(false);

                var error = await RateLimitAsync(
                    requestId,
                    definition,
                    weight ?? definition.Weight,
                    cancellationToken,
                    weightSingleLimiter,
                    rateLimitKeySuffix).ConfigureAwait(false);
                if (error != null)
                    return HttpResult.Fail<T>(Exchange, error);

                var request = CreateRequest(
                    requestId,
                    definition,
                    uriParameters,
                    bodyParameters,
                    additionalHeaders);

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.RestApiSendRequest(request.RequestId, definition, request.Content, string.IsNullOrEmpty(request.Uri.Query) ? "-" : request.Uri.Query, string.Join(", ", request.GetHeaders().Select(h => h.Key + $"=[{string.Join(",", h.Value)}]")));
                TotalRequestsMade++;

                var result = await GetResponseAsync2<T>(definition, request, definition.RateLimitGate, cancellationToken).ConfigureAwait(false);
                if (result.Error is not CancellationRequestedError)
                {
                    var originalData = OutputOriginalData ? result.OriginalData : "[Data only available when OutputOriginal = true]";
                    if (!result.Success)
                    {
                        _logger.RestApiErrorReceived(result.RequestId, result.ResponseStatusCode, (long)Math.Floor(result.ResponseTime!.Value.TotalMilliseconds), result.Error?.ToString(), originalData, result.Error?.Exception);
                    }
                    else
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.RestApiResponseReceived(result.RequestId, result.ResponseStatusCode, (long)Math.Floor(result.ResponseTime!.Value.TotalMilliseconds), originalData);
                    }
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
        /// Check rate limits for the request
        /// </summary>
        protected virtual async ValueTask<Error?> RateLimitAsync(
            int requestId,
            RequestDefinition definition,
            int weight,
            CancellationToken cancellationToken,
            int? weightSingleLimiter = null,
            string? rateLimitKeySuffix = null)
        {
            // Rate limiting
            var requestWeight = weight;
            if (requestWeight != 0)
            {
                if (definition.RateLimitGate == null)
                    throw new Exception("Ratelimit gate not set when request weight is not 0");

                if (ClientOptions.RateLimiterEnabled)
                {
                    var limitResult = await definition.RateLimitGate.ProcessAsync(
                        _logger,
                        requestId,
                        RateLimitItemType.Request,
                        definition,
                        GetAuthenticationProvider()?.Key,
                        requestWeight,
                        ClientOptions.RateLimitingBehaviour,
                        rateLimitKeySuffix + ClientOptions.RateLimitGroup,
                        cancellationToken).ConfigureAwait(false);
                    if (!limitResult.Success)
                        return limitResult.Error!;
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
                    var limitResult = await definition.RateLimitGate.ProcessSingleAsync(
                        _logger,
                        requestId,
                        definition.LimitGuard,
                        RateLimitItemType.Request,
                        definition,
                        GetAuthenticationProvider()?.Key,
                        singleRequestWeight,
                        ClientOptions.RateLimitingBehaviour,
                        rateLimitKeySuffix,
                        cancellationToken).ConfigureAwait(false);
                    if (!limitResult.Success)
                        return limitResult.Error!;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a request object
        /// </summary>
        /// <param name="requestId">Id of the request</param>
        /// <param name="definition">Request definition</param>
        /// <param name="uriParameters">The query parameters of the request</param>
        /// <param name="bodyParameters">The body parameters of the request</param>
        /// <param name="additionalHeaders">Additional headers to send with the request</param>
        /// <returns></returns>
        protected virtual IRequest CreateRequest(
            int requestId,
            RequestDefinition definition,
            Parameters? uriParameters,
            Parameters? bodyParameters,
            Dictionary<string, string>? additionalHeaders)
        {
            var requestConfiguration = new RestRequestConfiguration(
                definition,
                uriParameters,
                bodyParameters,
                additionalHeaders,
                definition.ParameterPosition ?? ParameterPositions[definition.Method],
                definition.RequestBodyFormat ?? RequestBodyFormat);

            try
            {
                GetAuthenticationProvider()?.ProcessRequest(this, requestConfiguration);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to authenticate request, make sure your API credentials are correct", ex);
            }

            var queryString = requestConfiguration.GetQueryString(true);
            if (!string.IsNullOrEmpty(queryString) && !queryString.StartsWith("?"))
                queryString = $"?{queryString}";

            var uri = new Uri(definition.FullUrl + queryString);
            var request = RequestFactory.Create(ClientOptions.HttpVersion, definition.Method, uri, requestId);
            request.Accept = MessageHandler.AcceptHeader;

            if (requestConfiguration.Headers != null)
            {
                foreach (var header in requestConfiguration.Headers)
                    request.AddHeader(header.Key, header.Value);
            }

            foreach (var header in StandardRequestHeaders)
            {
                // Only add it if it isn't overwritten
                if (requestConfiguration.Headers == null
                    || !requestConfiguration.Headers.ContainsKey(header.Key))
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            if (requestConfiguration.ParameterPosition == HttpMethodParameterPosition.InBody)
            {
                var contentType = requestConfiguration.BodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
                var bodyContent = requestConfiguration.GetBodyContent();
                if (bodyContent != null)
                {
                    request.SetContent(bodyContent, RequestBodyContentEncoding, contentType);
                }
                else
                {
                    if (requestConfiguration.BodyParameters != null && (requestConfiguration.BodyParameters.Count != 0 || requestConfiguration.BodyParameters.BodyValue != null))
                        WriteParamBody(request, requestConfiguration.BodyParameters, contentType);
                    else if (OmitContentTypeHeaderWithoutContent != true)
                        request.SetContent(RequestBodyEmptyContent, RequestBodyContentEncoding, contentType);
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
        protected virtual async Task<HttpResult<T>> GetResponseAsync2<T>(
            RequestDefinition requestDefinition,
            IRequest request,
            IRateLimitGate? gate,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            Stream? responseStream = null;
            IResponse? response = null;

            try
            {
                response = await request.GetResponseAsync(cancellationToken).ConfigureAwait(false);
                sw.Stop();
                responseStream = await response.GetResponseStreamAsync(cancellationToken).ConfigureAwait(false);
                string? originalData = null;
                var outputOriginalData = ApiOptions.OutputOriginalData ?? ClientOptions.OutputOriginalData;
                if (outputOriginalData || MessageHandler.RequiresSeekableStream || !response.IsSuccessStatusCode)
                {
                    // Create a seekable stream from the response stream if:
                    // 1. We need to output the original data
                    // 2. The message handler requires a seekable stream
                    // 3. The response indicates error and we want to output (part of) the returned data
                    responseStream = await CopyStreamAsync(responseStream).ConfigureAwait(false);
                    using var reader = new StreamReader(responseStream, Encoding.UTF8, false, 4096, true);
                    if (outputOriginalData)
                    {
                        originalData = await reader.ReadToEndAsync().ConfigureAwait(false);
                        responseStream.Position = 0;
                    }
                }

                if (!response.IsSuccessStatusCode && !requestDefinition.TryParseOnNonSuccess)
                {
                    // If the response status is not success it is an error by definition

                    Error error;
                    if (response.StatusCode == (HttpStatusCode)418 || response.StatusCode == (HttpStatusCode)429)
                    {
                        // Specifically handle rate limit errors
                        var rateError = await MessageHandler.ParseErrorRateLimitResponse(
                            (int)response.StatusCode,
                            response.ResponseHeaders,
                            responseStream).ConfigureAwait(false);
                        if (rateError.RetryAfter != null && gate != null && ClientOptions.RateLimiterEnabled)
                        {
                            _logger.RestApiRateLimitPauseUntil(request.RequestId, rateError.RetryAfter.Value);
                            await gate.SetRetryAfterGuardAsync(rateError.RetryAfter.Value).ConfigureAwait(false);
                        }

                        error = rateError;
                    }
                    else
                    {
                        // Handle a 'normal' error response. Can still be either a json error message or some random HTML or other string
                        try
                        {
                            error = await MessageHandler.ParseErrorResponse(
                                (int)response.StatusCode,
                                response.ResponseHeaders,
                                responseStream).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unhandled exception when parsing error response: {Message}", ex.Message);
                            var errorResult = new ServerError(ErrorInfo.Unknown with { Message = ex.Message });
                            return FailHttpRequest<T>(request, response, sw.Elapsed, originalData, errorResult);
                        }
                    }

                    return FailHttpRequest<T>(request, response, sw.Elapsed, originalData, error);
                }

                if (typeof(T) == Unit.Type)
                    // Success status code and expected empty response, assume it's correct
                    return OkHttpRequest<T>(request, response, sw.Elapsed, originalData, default!);

                // Data response received, inspect the message and check if it is an error or not
                var parsedError = await MessageHandler.CheckForErrorResponse(
                    requestDefinition,
                    response.ResponseHeaders,
                    responseStream).ConfigureAwait(false);
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
                    return FailHttpRequest<T>(request, response, sw.Elapsed, originalData, parsedError);
                }

                if (MessageHandler.RequiresSeekableStream)
                    // Reset stream read position as it might not be at the start if `CheckForErrorResponse` has read from it
                    responseStream.Position = 0;

                // Try deserialization into the expected type
                var (deserializeResult, deserializeError) = await MessageHandler.TryDeserializeAsync<T>(responseStream, cancellationToken).ConfigureAwait(false);
                if (deserializeError != null)
                    return FailHttpRequest<T>(request, response, sw.Elapsed, originalData, deserializeError, deserializeResult);

                try
                {
                    // Check the deserialized response to see if it's an error or not
                    var responseError = MessageHandler.CheckDeserializedResponse(response.ResponseHeaders, deserializeResult);
                    if (responseError != null)
                        return FailHttpRequest<T>(request, response, sw.Elapsed, originalData, responseError, deserializeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception when checking deserialized response: {Message}", ex.Message);
                    var error = new ServerError(ErrorInfo.Unknown with { Message = ex.Message });
                    return FailHttpRequest<T>(request, response, sw.Elapsed, originalData, error, deserializeResult);
                }

                return OkHttpRequest<T>(request, response, sw.Elapsed, originalData, deserializeResult!);
            }
            catch (HttpRequestException requestException)
            {
                // Request exception, can't reach server for instance
                var error = new WebError(requestException.Message, requestException);
                return FailHttpRequest<T>(request, response, sw.Elapsed, null, error);
            }
            catch (OperationCanceledException canceledException)
            {
                if (cancellationToken != default && canceledException.CancellationToken == cancellationToken)
                {
                    // Cancellation token canceled by caller
                    return FailHttpRequest<T>(request, null, sw.Elapsed, null, new CancellationRequestedError(canceledException));
                }
                else
                {
                    // Request timed out
                    var error = new WebError($"Request timed out", exception: canceledException);
                    error.ErrorType = ErrorType.Timeout;
                    return FailHttpRequest<T>(request, null, sw.Elapsed, null, error);
                }
            }
            catch (ArgumentException argumentException)
            {
                if (argumentException.Message.StartsWith("Only HTTP/"))
                {
                    // Unsupported HTTP version error .net framework
                    var error = ArgumentError.Invalid(nameof(RestExchangeOptions.HttpVersion), $"Invalid HTTP version {request.HttpVersion}: " + argumentException.Message);
                    return FailHttpRequest<T>(request, null, sw.Elapsed, null, error);
                }

                throw;
            }
            catch (NotSupportedException notSupportedException)
            {
                if (notSupportedException.Message.StartsWith("Request version value must be one of"))
                {
                    // Unsupported HTTP version error dotnet code
                    var error = ArgumentError.Invalid(nameof(RestExchangeOptions.HttpVersion), $"Invalid HTTP version {request.HttpVersion}: " + notSupportedException.Message);
                    return FailHttpRequest<T>(request, null, sw.Elapsed, null, error);
                }

                throw;
            }
            finally
            {
                responseStream?.Close();
                response?.Close();
            }
        }

        private HttpResult<T> OkHttpRequest<T>(IRequest request, IResponse response, TimeSpan elapsed, string? originalData, T result)
        {
            return HttpResult.Ok(
                Exchange,
                response.StatusCode,
                response.HttpVersion,
                response.ResponseHeaders,
                elapsed,
                response.ContentLength,
                originalData,
                request.RequestId,
                request.Uri.ToString(),
                request.Content,
                request.Method,
                request.GetHeaders(),
                ResultDataSource.Server,
                result);
        }

        private HttpResult<T> FailHttpRequest<T>(IRequest request, IResponse? response, TimeSpan elapsed, string? originalData, Error error, T? result = default)
        {
            return HttpResult.Fail<T>(
                Exchange,
                response?.StatusCode,
                response?.HttpVersion,
                response?.ResponseHeaders,
                elapsed,
                response?.ContentLength,
                originalData,
                request.RequestId,
                request.Uri.ToString(),
                request.Content,
                request.Method,
                request.GetHeaders(),
                ResultDataSource.Server,
                error,
                result);
        }

        /// <summary>
        /// Can be used to indicate that a request should be retried. Defaults to false. Make sure to retry a max number of times (based on the the tries parameter) or the request will retry forever.
        /// Note that this is always called; even when the request might be successful
        /// </summary>
        /// <typeparam name="T">HttpResult type parameter</typeparam>
        /// <param name="gate">The rate limit gate the call used</param>
        /// <param name="callResult">The result of the call</param>
        /// <param name="tries">The current try number</param>
        /// <returns>True if call should retry, false if the call should return</returns>
        protected virtual async ValueTask<bool> ShouldRetryRequestAsync<T>(IRateLimitGate? gate, HttpResult<T> callResult, int tries)
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
        protected virtual void WriteParamBody(IRequest request, Parameters parameters, string contentType)
        {
            if (contentType == Constants.JsonContentHeader)
            {
                var serializer = CreateSerializer();
                if (serializer is not IStringMessageSerializer stringSerializer)
                    throw new InvalidOperationException("Non-string message serializer can't get serialized request body");

                // Write the parameters as json in the body
                string stringData;
                if (parameters.BodyValue != null)
                    stringData = stringSerializer.Serialize(parameters.BodyValue);
                else
                    stringData = stringSerializer.Serialize(parameters);
                request.SetContent(stringData, RequestBodyContentEncoding, contentType);
            }
            else if (contentType == Constants.FormContentHeader)
            {
                // Write the parameters as form data in the body
                var stringData = parameters.ToFormData();
                request.SetContent(stringData, RequestBodyContentEncoding, contentType);
            }
        }

        /// <summary>
        /// Retrieve the server time for the purpose of syncing time between client and server to prevent authentication issues
        /// </summary>
        /// <returns>Server time</returns>
        protected virtual Task<HttpResult<DateTime>> GetServerTimestampAsync() => throw new NotImplementedException();

        private async ValueTask CheckTimeSync(int requestId, RequestDefinition definition)
        {
            if (!definition.Authenticated)
                return;

            var lastUpdateTime = TimeOffsetManager.GetRestLastUpdateTime(ClientName);
            var syncTask = CheckTimeOffsetAsync();

            if (lastUpdateTime == null)
            {
                // Initially with first request we'll need to wait for the time syncing before making the actual request.
                // If it's not the first request we can just continue and let it complete in the background
                await syncTask.ConfigureAwait(false);
            }

            return;
        }

        internal async ValueTask CheckTimeOffsetAsync()
        {
            if (!(ApiOptions.AutoTimestamp ?? ClientOptions.AutoTimestamp))
                // Time syncing not enabled
                return;

            await TimeOffsetManager.EnterAsync(ClientName).ConfigureAwait(false);
            try
            {
                var lastUpdateTime = TimeOffsetManager.GetRestLastUpdateTime(ClientName);
                if (DateTime.UtcNow - lastUpdateTime < (ApiOptions.TimestampRecalculationInterval ?? ClientOptions.TimestampRecalculationInterval))
                    // Time syncing was recently done
                    return;

                var localTime = DateTime.UtcNow;
                HttpResult<DateTime> result;
                try
                {
                    result = await GetServerTimestampAsync().ConfigureAwait(false);
                }
                catch (NotImplementedException)
                {
                    throw new ArgumentException("AutoTimestamp is not available for this API");
                }

                if (!result.Success)
                {
                    _logger.LogWarning("Failed to determine time offset between client and server, timestamping might fail");
                    return;
                }

                if (TotalRequestsMade == 1)
                {
                    // If this was the first request make another one to calculate the offset since the first one can be slower
                    localTime = DateTime.UtcNow;
                    result = await GetServerTimestampAsync().ConfigureAwait(false);
                    if (!result.Success)
                    {
                        _logger.LogWarning("Failed to determine time offset between client and server, timestamping might fail");
                        return;
                    }
                }

                // Estimate the offset as the round trip time / 2
                var offset = result.Data - localTime.AddMilliseconds(result.ResponseTime!.Value.TotalMilliseconds / 2);
                if (offset.TotalMilliseconds > 0 && offset.TotalMilliseconds < 500)
                {
                    _logger.LogInformation("{ClientName} Time offset within limits ({Offset}ms), set offset to 0ms", ClientName, Math.Round(offset.TotalMilliseconds));
                    offset = TimeSpan.Zero;
                }
                else
                {
                    _logger.LogInformation("{ClientName} Time offset set to {Offset}ms", ClientName, Math.Round(offset.TotalMilliseconds));
                }

                TimeOffsetManager.UpdateRestOffset(ClientName, offset.TotalMilliseconds);
            }
            finally
            {
                TimeOffsetManager.Release(ClientName);
            }
        }

        private async Task<Stream> CopyStreamAsync(Stream responseStream)
        {
            var memoryStream = new MemoryStream();
            await responseStream.CopyToAsync(memoryStream).ConfigureAwait(false);
            responseStream.Close();
            memoryStream.Position = 0;
            return memoryStream;
        }

        private bool ShouldCache(RequestDefinition definition)
            => ClientOptions.CachingEnabled
            && definition.Method == HttpMethod.Get
            && !definition.PreventCaching;


        /// <inheritdoc />
        public virtual void SetOptions(UpdateOptions options)
        {
            _proxyConfigured = options.Proxy != null;
            ClientOptions.Proxy = options.Proxy;
            ClientOptions.RequestTimeout = options.RequestTimeout ?? ClientOptions.RequestTimeout;

            RequestFactory.UpdateSettings(options.Proxy, options.RequestTimeout ?? ClientOptions.RequestTimeout, ClientOptions.HttpKeepAliveInterval);
        }
    }

    /// <inheritdoc />
    public abstract class RestApiClient<TEnvironment> : RestApiClient, IRestApiClient
        where TEnvironment : TradeEnvironment
    {
        /// <inheritdoc />
        public new RestExchangeOptions<TEnvironment> ClientOptions => (RestExchangeOptions<TEnvironment>)base.ClientOptions;

        /// <inheritdoc />
        public override string EnvironmentName => ClientOptions.Environment.Name;

        /// <summary>
        /// ctor
        /// </summary>
        protected RestApiClient(
            ILoggerFactory? loggerFactory,
            string exchangeName,
            HttpClient? httpClient,
            string baseAddress,
            RestExchangeOptions options,
            RestApiOptions apiOptions) : base(
                loggerFactory,
                exchangeName,
                httpClient,
                baseAddress,
                options,
                apiOptions)
        {
        }
    }

    /// <inheritdoc />
    public abstract class RestApiClient<TEnvironment, TApiCredentials> : RestApiClient<TEnvironment>, IRestApiClient<TApiCredentials>
        where TApiCredentials : ApiCredentials
        where TEnvironment : TradeEnvironment
    {
        /// <inheritdoc />
        public TApiCredentials? ApiCredentials { get; set; }

        /// <inheritdoc />
        public bool Authenticated => ApiCredentials != null;

        /// <inheritdoc />
        public new RestExchangeOptions<TEnvironment, TApiCredentials> ClientOptions => (RestExchangeOptions<TEnvironment, TApiCredentials>)base.ClientOptions;

        /// <summary>
        /// ctor
        /// </summary>
        protected RestApiClient(
            ILoggerFactory? loggerFactory,
            string exchangeName,
            HttpClient? httpClient,
            string baseAddress,
            RestExchangeOptions<TEnvironment, TApiCredentials> options,
            RestApiOptions apiOptions) : base(
                loggerFactory,
                exchangeName,
                httpClient,
                baseAddress,
                options,
                apiOptions)
        {
            ApiCredentials =  options.ApiCredentials;
        }

        /// <inheritdoc />
        public virtual void SetApiCredentials(TApiCredentials credentials)
        {
            ApiCredentials = (TApiCredentials)credentials.Copy();
        }

        /// <inheritdoc />
        public virtual void SetOptions(UpdateOptions<TApiCredentials> options)
        {
            base.SetOptions(options);

            ApiCredentials = (TApiCredentials?)options.ApiCredentials?.Copy() ?? ApiCredentials;
        }
    }

    /// <inheritdoc />
    public abstract class RestApiClient<TEnvironment, TAuthenticationProvider, TApiCredentials> : RestApiClient<TEnvironment, TApiCredentials>
        where TApiCredentials : ApiCredentials
        where TAuthenticationProvider : AuthenticationProvider<TApiCredentials>
        where TEnvironment : TradeEnvironment
    {

        private bool _authProviderInitialized = false;
        private TAuthenticationProvider? _authenticationProvider;
        /// <summary>
        /// The authentication provider for this API client. (null if no credentials are set)
        /// </summary>
        public TAuthenticationProvider? AuthenticationProvider
        {
            get
            {
                if (!_authProviderInitialized)
                {
                    if (ApiCredentials != null)
                        _authenticationProvider = CreateAuthenticationProvider(ApiCredentials);

                    _authProviderInitialized = true;
                }

                return _authenticationProvider;
            }
            internal set => _authenticationProvider = value;
        }

        /// <inheritdoc />
        public override AuthenticationProvider? GetAuthenticationProvider() => AuthenticationProvider;

        /// <summary>
        /// ctor
        /// </summary>
        protected RestApiClient(
            ILoggerFactory? loggerFactory, 
            string exchangeName,
            HttpClient? httpClient,
            string baseAddress,
            RestExchangeOptions<TEnvironment, TApiCredentials> options, 
            RestApiOptions apiOptions) : base(
                loggerFactory, 
                exchangeName,
                httpClient,
                baseAddress,
                options, 
                apiOptions)
        {
        }

        /// <summary>
        /// Create an AuthenticationProvider implementation instance based on the provided credentials
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        protected abstract TAuthenticationProvider CreateAuthenticationProvider(TApiCredentials credentials);

        /// <inheritdoc />
        public override void SetApiCredentials(TApiCredentials credentials)
        {
            base.SetApiCredentials(credentials);

            AuthenticationProvider = null;
            _authProviderInitialized = false;
            ApiCredentials = credentials;
        }

        /// <inheritdoc />
        public override void SetOptions(UpdateOptions<TApiCredentials> options)
        {
            base.SetOptions(options);

            if (options.ApiCredentials != null)
            {
                AuthenticationProvider = null;
                _authProviderInitialized = false;
                ApiCredentials = options.ApiCredentials;
            }
        }
    }
}

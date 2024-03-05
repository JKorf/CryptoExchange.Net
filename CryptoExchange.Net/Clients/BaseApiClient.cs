using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Base API for all API clients
    /// </summary>
    public abstract class BaseApiClient : IDisposable, IBaseApiClient
    {
        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger _logger;

        /// <summary>
        /// If we are disposing
        /// </summary>
        protected bool _disposing;

        /// <summary>
        /// The authentication provider for this API client. (null if no credentials are set)
        /// </summary>
        public AuthenticationProvider? AuthenticationProvider { get; private set; }

        /// <summary>
        /// Where to put the parameters for requests with different Http methods
        /// </summary>
        public Dictionary<HttpMethod, HttpMethodParameterPosition> ParameterPositions { get; set; } = new Dictionary<HttpMethod, HttpMethodParameterPosition>
        {
            { HttpMethod.Get, HttpMethodParameterPosition.InUri },
            { HttpMethod.Post, HttpMethodParameterPosition.InBody },
            { HttpMethod.Delete, HttpMethodParameterPosition.InBody },
            { HttpMethod.Put, HttpMethodParameterPosition.InBody }
        };

        /// <summary>
        /// Request body content type
        /// </summary>
        public RequestBodyFormat requestBodyFormat = RequestBodyFormat.Json;

        /// <summary>
        /// How to serialize array parameters when making requests
        /// </summary>
        public ArrayParametersSerialization arraySerialization = ArrayParametersSerialization.Array;

        /// <summary>
        /// What request body should be set when no data is send (only used in combination with postParametersPosition.InBody)
        /// </summary>
        public string requestBodyEmptyContent = "{}";

        /// <summary>
        /// The environment this client communicates to
        /// </summary>
        public string BaseAddress { get; }

        /// <summary>
        /// Output the original string data along with the deserialized object
        /// </summary>
        public bool OutputOriginalData { get; }

        ///// <summary>
        ///// The default serializer
        ///// </summary>
        //protected virtual JsonSerializer DefaultSerializer { get; set; } = JsonSerializer.Create(SerializerOptions.Default);

        /// <summary>
        /// Api options
        /// </summary>
        public ApiOptions ApiOptions { get; }

        /// <summary>
        /// Client Options
        /// </summary>
        public ExchangeOptions ClientOptions { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="outputOriginalData">Should data from this client include the orginal data in the call result</param>
        /// <param name="baseAddress">Base address for this API client</param>
        /// <param name="apiCredentials">Api credentials</param>
        /// <param name="clientOptions">Client options</param>
        /// <param name="apiOptions">Api options</param>
        protected BaseApiClient(ILogger logger, bool outputOriginalData, ApiCredentials? apiCredentials, string baseAddress, ExchangeOptions clientOptions, ApiOptions apiOptions)
        {
            _logger = logger;

            ClientOptions = clientOptions;
            ApiOptions = apiOptions;
            OutputOriginalData = outputOriginalData;
            BaseAddress = baseAddress;

            if (apiCredentials != null)
            {
                AuthenticationProvider?.Dispose();
                AuthenticationProvider = CreateAuthenticationProvider(apiCredentials.Copy());
            }
        }

        /// <summary>
        /// Create an AuthenticationProvider implementation instance based on the provided credentials
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        protected abstract AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials);

        /// <inheritdoc />
        public void SetApiCredentials<T>(T credentials) where T : ApiCredentials
        {
            if (credentials != null)
            {
                AuthenticationProvider?.Dispose();
                AuthenticationProvider = CreateAuthenticationProvider(credentials.Copy());
            }
        }

        ///// <summary>
        ///// Tries to parse the json data and return a JToken, validating the input not being empty and being valid json
        ///// </summary>
        ///// <param name="data">The data to parse</param>
        ///// <returns></returns>
        //protected CallResult<JToken> ValidateJson(string data)
        //{
        //    if (string.IsNullOrEmpty(data))
        //    {
        //        var info = "Empty data object received";
        //        _logger.Log(LogLevel.Error, info);
        //        return new CallResult<JToken>(new DeserializeError(info, data));
        //    }

        //    try
        //    {
        //        return new CallResult<JToken>(JToken.Parse(data));
        //    }
        //    catch (JsonReaderException jre)
        //    {
        //        var info = $"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}";
        //        return new CallResult<JToken>(new DeserializeError(info, data));
        //    }
        //    catch (JsonSerializationException jse)
        //    {
        //        var info = $"Deserialize JsonSerializationException: {jse.Message}";
        //        return new CallResult<JToken>(new DeserializeError(info, data));
        //    }
        //    catch (Exception ex)
        //    {
        //        var exceptionInfo = ex.ToLogString();
        //        var info = $"Deserialize Unknown Exception: {exceptionInfo}";
        //        return new CallResult<JToken>(new DeserializeError(info, data));
        //    }
        //}

        ///// <summary>
        ///// Deserialize a string into an object
        ///// </summary>
        ///// <typeparam name="T">The type to deserialize into</typeparam>
        ///// <param name="data">The data to deserialize</param>
        ///// <param name="serializer">A specific serializer to use</param>
        ///// <param name="requestId">Id of the request the data is returned from (used for grouping logging by request)</param>
        ///// <returns></returns>
        //protected CallResult<T> Deserialize<T>(string data, JsonSerializer? serializer = null, int? requestId = null)
        //{
        //    var tokenResult = ValidateJson(data);
        //    if (!tokenResult)
        //    {
        //        _logger.Log(LogLevel.Error, tokenResult.Error!.Message);
        //        return new CallResult<T>(tokenResult.Error);
        //    }

        //    return Deserialize<T>(tokenResult.Data, serializer, requestId);
        //}

        ///// <summary>
        ///// Deserialize a JToken into an object
        ///// </summary>
        ///// <typeparam name="T">The type to deserialize into</typeparam>
        ///// <param name="obj">The data to deserialize</param>
        ///// <param name="serializer">A specific serializer to use</param>
        ///// <param name="requestId">Id of the request the data is returned from (used for grouping logging by request)</param>
        ///// <returns></returns>
        //protected CallResult<T> Deserialize<T>(JToken obj, JsonSerializer? serializer = null, int? requestId = null)
        //{
        //    serializer ??= DefaultSerializer;

        //    try
        //    {
        //        return new CallResult<T>(obj.ToObject<T>(serializer)!);
        //    }
        //    catch (JsonReaderException jre)
        //    {
        //        var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonReaderException: {jre.Message} Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}, data: {obj}";
        //        _logger.Log(LogLevel.Error, info);
        //        return new CallResult<T>(new DeserializeError(info, obj));
        //    }
        //    catch (JsonSerializationException jse)
        //    {
        //        var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonSerializationException: {jse.Message} data: {obj}";
        //        _logger.Log(LogLevel.Error, info);
        //        return new CallResult<T>(new DeserializeError(info, obj));
        //    }
        //    catch (Exception ex)
        //    {
        //        var exceptionInfo = ex.ToLogString();
        //        var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize Unknown Exception: {exceptionInfo}, data: {obj}";
        //        _logger.Log(LogLevel.Error, info);
        //        return new CallResult<T>(new DeserializeError(info, obj));
        //    }
        //}

        ///// <summary>
        ///// Deserialize a stream into an object
        ///// </summary>
        ///// <typeparam name="T">The type to deserialize into</typeparam>
        ///// <param name="stream">The stream to deserialize</param>
        ///// <param name="serializer">A specific serializer to use</param>
        ///// <param name="requestId">Id of the request the data is returned from (used for grouping logging by request)</param>
        ///// <param name="elapsedMilliseconds">Milliseconds response time for the request this stream is a response for</param>
        ///// <returns></returns>
        //protected async Task<CallResult<T>> DeserializeAsync<T>(Stream stream, JsonSerializer? serializer = null, int? requestId = null, long? elapsedMilliseconds = null)
        //{
        //    serializer ??= DefaultSerializer;
        //    string? data = null;

        //    try
        //    {
        //        // Let the reader keep the stream open so we're able to seek if needed. The calling method will close the stream.
        //        using var reader = new StreamReader(stream, Encoding.UTF8, false, 512, true);
        //        // If we have to output the original json data or output the data into the logging we'll have to read to full response
        //        // in order to log/return the json data
        //        if (OutputOriginalData == true)
        //        {
        //            data = await reader.ReadToEndAsync().ConfigureAwait(false);
        //            var result = Deserialize<T>(data, serializer, requestId);
        //            result.OriginalData = data;
        //            return result;
        //        }

        //        // If we don't have to keep track of the original json data we can use the JsonTextReader to deserialize the stream directly
        //        // into the desired object, which has increased performance over first reading the string value into memory and deserializing from that
        //        using var jsonReader = new JsonTextReader(reader);
        //        return new CallResult<T>(serializer.Deserialize<T>(jsonReader)!);
        //    }
        //    catch (JsonReaderException jre)
        //    {
        //        if (data == null)
        //        {
        //            if (stream.CanSeek)
        //            {
        //                // If we can seek the stream rewind it so we can retrieve the original data that was sent
        //                stream.Seek(0, SeekOrigin.Begin);
        //                data = await ReadStreamAsync(stream).ConfigureAwait(false);
        //            }
        //            else
        //            {
        //                data = "[Data only available in Trace LogLevel]";
        //            }
        //        }
        //        _logger.Log(LogLevel.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}, data: {data}");
        //        return new CallResult<T>(new DeserializeError($"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}", data));
        //    }
        //    catch (JsonSerializationException jse)
        //    {
        //        if (data == null)
        //        {
        //            if (stream.CanSeek)
        //            {
        //                stream.Seek(0, SeekOrigin.Begin);
        //                data = await ReadStreamAsync(stream).ConfigureAwait(false);
        //            }
        //            else
        //            {
        //                data = "[Data only available in Trace LogLevel]";
        //            }
        //        }

        //        _logger.Log(LogLevel.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonSerializationException: {jse.Message}, data: {data}");
        //        return new CallResult<T>(new DeserializeError($"Deserialize JsonSerializationException: {jse.Message}", data));
        //    }
        //    catch (Exception ex)
        //    {
        //        if (data == null)
        //        {
        //            if (stream.CanSeek)
        //            {
        //                stream.Seek(0, SeekOrigin.Begin);
        //                data = await ReadStreamAsync(stream).ConfigureAwait(false);
        //            }
        //            else
        //            {
        //                data = "[Data only available in Trace LogLevel]";
        //            }
        //        }

        //        var exceptionInfo = ex.ToLogString();
        //        _logger.Log(LogLevel.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize Unknown Exception: {exceptionInfo}, data: {data}");
        //        return new CallResult<T>(new DeserializeError($"Deserialize Unknown Exception: {exceptionInfo}", data));
        //    }
        //}

        //private static async Task<string> ReadStreamAsync(Stream stream)
        //{
        //    using var reader = new StreamReader(stream, Encoding.UTF8, false, 512, true);
        //    return await reader.ReadToEndAsync().ConfigureAwait(false);
        //}

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            _disposing = true;
            AuthenticationProvider?.Dispose();
        }
    }
}

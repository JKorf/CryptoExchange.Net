using System.Collections.Generic;
using System.Net.Http;

namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Rest request configuration
    /// </summary>
    public class RestRequestConfiguration
    {
        private string? _bodyContent;
        private string? _queryString;

        /// <summary>
        /// Http method
        /// </summary>
        public HttpMethod Method { get; set; }
        /// <summary>
        /// Whether the request needs authentication
        /// </summary>
        public bool Authenticated { get; set; }
        /// <summary>
        /// Base address for the request
        /// </summary>
        public string BaseAddress { get; set; }
        /// <summary>
        /// The request path
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Query parameters
        /// </summary>
        public IDictionary<string, object>? QueryParameters { get; set; }
        /// <summary>
        /// Body parameters
        /// </summary>
        public IDictionary<string, object>? BodyParameters { get; set; }
        /// <summary>
        /// Request headers
        /// </summary>
        public IDictionary<string, string>? Headers { get; set; }
        /// <summary>
        /// Array serialization type
        /// </summary>
        public ArrayParametersSerialization ArraySerialization { get; set; }
        /// <summary>
        /// Position of the parameters
        /// </summary>
        public HttpMethodParameterPosition ParameterPosition { get; set; }
        /// <summary>
        /// Body format
        /// </summary>
        public RequestBodyFormat BodyFormat { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public RestRequestConfiguration(
            RequestDefinition requestDefinition,
            string baseAddress,
            IDictionary<string, object>? queryParams,
            IDictionary<string, object>? bodyParams,
            IDictionary<string, string>? headers,
            ArrayParametersSerialization arraySerialization,
            HttpMethodParameterPosition parametersPosition,
            RequestBodyFormat bodyFormat)
        {
            Method = requestDefinition.Method;
            Authenticated = requestDefinition.Authenticated;
            Path = requestDefinition.Path;
            BaseAddress = baseAddress;
            QueryParameters = queryParams;
            BodyParameters = bodyParams;
            Headers = headers;
            ArraySerialization = arraySerialization;
            ParameterPosition = parametersPosition;
            BodyFormat = bodyFormat;
        }

        /// <summary>
        /// Get the parameter collection based on the ParameterPosition
        /// </summary>
        public IDictionary<string, object> GetPositionParameters()
        {
            if (ParameterPosition == HttpMethodParameterPosition.InBody)
            {
                BodyParameters ??= new Dictionary<string, object>();
                return BodyParameters;
            }

            QueryParameters ??= new Dictionary<string, object>();
            return QueryParameters;
        }

        /// <summary>
        /// Get the query string. If it's not previously set it will return a newly formatted query string. If previously set return that.
        /// </summary>
        /// <param name="urlEncode">Whether to URL encode the parameter string if creating new</param>
        public string GetQueryString(bool urlEncode = true)
        {
            return _queryString ?? QueryParameters?.CreateParamString(urlEncode, ArraySerialization) ?? string.Empty;
        }

        /// <summary>
        /// Set the query string of the request. Will be returned by subsequent <see cref="GetQueryString" /> calls
        /// </summary>
        public void SetQueryString(string value)
        {
            _queryString = value;
        }

        /// <summary>
        /// Get the body content if it's previously set
        /// </summary>
        public string? GetBodyContent()
        {
            return _bodyContent;
        }

        /// <summary>
        /// Set the body content for the request
        /// </summary>
        public void SetBodyContent(string content)
        {
            _bodyContent = content;
        }
    }
}

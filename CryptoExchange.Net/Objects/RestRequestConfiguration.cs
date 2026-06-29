using CryptoExchange.Net.Interfaces;
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
        /// The request definition for the request
        /// </summary>
        public RequestDefinition RequestDefinition { get; set; }
        /// <summary>
        /// Query parameters
        /// </summary>
        public Parameters? QueryParameters { get; set; }
        /// <summary>
        /// Body parameters
        /// </summary>
        public Parameters? BodyParameters { get; set; }
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
            Parameters? queryParams,
            Parameters? bodyParams,
            IDictionary<string, string>? headers,
            HttpMethodParameterPosition parametersPosition,
            RequestBodyFormat bodyFormat)
        {
            RequestDefinition = requestDefinition;
            QueryParameters = queryParams;
            BodyParameters = bodyParams;
            Headers = headers;
            ParameterPosition = parametersPosition;
            BodyFormat = bodyFormat;
        }

        /// <summary>
        /// Get the parameter collection based on the ParameterPosition
        /// </summary>
        public Parameters GetPositionParameters()
        {
            if (ParameterPosition == HttpMethodParameterPosition.InBody)
            {
                BodyParameters ??= new Parameters(ParameterSerializationSettings.Default);
                return BodyParameters;
            }

            QueryParameters ??= new Parameters(ParameterSerializationSettings.Default);
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

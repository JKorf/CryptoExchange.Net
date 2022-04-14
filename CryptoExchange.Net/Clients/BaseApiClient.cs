using System;
using System.Collections.Generic;
using System.Net.Http;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Base API for all API clients
    /// </summary>
    public abstract class BaseApiClient: IDisposable
    {
        private ApiCredentials? _apiCredentials;
        private AuthenticationProvider? _authenticationProvider;
        private bool _created;
        private bool _disposing;

        /// <summary>
        /// The authentication provider for this API client. (null if no credentials are set)
        /// </summary>
        public AuthenticationProvider? AuthenticationProvider
        {
            get 
            {
                if (!_created && !_disposing && _apiCredentials != null)
                {
                    _authenticationProvider = CreateAuthenticationProvider(_apiCredentials);
                    _created = true;
                }

                return _authenticationProvider;
            }
        }

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
        /// Whether or not we need to manually parse an error instead of relying on the http status code
        /// </summary>
        public bool manualParseError = false;

        /// <summary>
        /// How to serialize array parameters when making requests
        /// </summary>
        public ArrayParametersSerialization arraySerialization = ArrayParametersSerialization.Array;

        /// <summary>
        /// What request body should be set when no data is send (only used in combination with postParametersPosition.InBody)
        /// </summary>
        public string requestBodyEmptyContent = "{}";

        /// <summary>
        /// The base address for this API client
        /// </summary>
        internal protected string BaseAddress { get; }

        /// <summary>
        /// Api client options
        /// </summary>
        internal ApiClientOptions Options { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options">Client options</param>
        /// <param name="apiOptions">Api client options</param>
        protected BaseApiClient(BaseClientOptions options, ApiClientOptions apiOptions)
        {
            Options = apiOptions;
            _apiCredentials = apiOptions.ApiCredentials?.Copy() ?? options.ApiCredentials?.Copy();
            BaseAddress = apiOptions.BaseAddress;
        }

        /// <summary>
        /// Create an AuthenticationProvider implementation instance based on the provided credentials
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        protected abstract AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials);

        /// <inheritdoc />
        public void SetApiCredentials(ApiCredentials credentials)
        {
            _apiCredentials = credentials?.Copy();
            _created = false;
            _authenticationProvider = null;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _disposing = true;
            _apiCredentials?.Dispose();
            AuthenticationProvider?.Credentials?.Dispose();
        }
    }
}

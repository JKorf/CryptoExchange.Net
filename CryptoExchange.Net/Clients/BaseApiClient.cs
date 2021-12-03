using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
    /// Base API for all API clients
    /// </summary>
    public abstract class BaseApiClient: IDisposable
    {
        private readonly ApiCredentials? _apiCredentials;
        private AuthenticationProvider? _authenticationProvider;
        /// <summary>
        /// The authentication provider for this API client. (null if no credentials are set)
        /// </summary>
        public AuthenticationProvider? AuthenticationProvider
        {
            get 
            {
                if (_authenticationProvider == null && _apiCredentials != null)
                    _authenticationProvider = CreateAuthenticationProvider(_apiCredentials);

                return _authenticationProvider;
            }
        }

        /// <summary>
        /// The base address for this API client
        /// </summary>
        internal protected string BaseAddress { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options">Client options</param>
        /// <param name="apiOptions">Api client options</param>
        protected BaseApiClient(BaseClientOptions options, ApiClientOptions apiOptions)
        {
            _apiCredentials = apiOptions.ApiCredentials ?? options.ApiCredentials;
            BaseAddress = apiOptions.BaseAddress;
        }

        /// <summary>
        /// Create an AuthenticationProvider implementation instance based on the provided credentials
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        protected abstract AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials);

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            AuthenticationProvider?.Credentials?.Dispose();
        }
    }
}

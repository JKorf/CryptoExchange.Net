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
    /// Base rest client
    /// </summary>
    public abstract class BaseApiClient: IDisposable
    {
        private ApiCredentials? _apiCredentials;
        private AuthenticationProvider _authenticationProvider;
        public AuthenticationProvider? AuthenticationProvider
        {
            get 
            {
                if (_authenticationProvider == null && _apiCredentials != null)
                    _authenticationProvider = CreateAuthenticationProvider(_apiCredentials);

                return _authenticationProvider;
            }
        }

        internal protected string BaseAddress { get; }

        public BaseApiClient(BaseClientOptions options, ApiClientOptions apiOptions)
        {
            _apiCredentials = apiOptions.ApiCredentials ?? options.ApiCredentials;
            BaseAddress = apiOptions.BaseAddress;
        }

        public abstract AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials);

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            AuthenticationProvider?.Credentials?.Dispose();
        }
    }
}

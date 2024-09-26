using System;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Clients
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
        /// The environment this client communicates to
        /// </summary>
        public string BaseAddress { get; }

        /// <summary>
        /// Output the original string data along with the deserialized object
        /// </summary>
        public bool OutputOriginalData { get; }

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
                AuthenticationProvider = CreateAuthenticationProvider(apiCredentials.Copy());
        }

        /// <summary>
        /// Create an AuthenticationProvider implementation instance based on the provided credentials
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        protected abstract AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials);

        /// <inheritdoc />
        public abstract string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null);

        /// <inheritdoc />
        public void SetApiCredentials<T>(T credentials) where T : ApiCredentials
        {
            if (credentials != null)
                AuthenticationProvider = CreateAuthenticationProvider(credentials.Copy());
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            _disposing = true;
        }
    }
}

using System;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects.Errors;
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
        /// Client name
        /// </summary>
        protected string? _clientName;

        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger _logger;

        /// <summary>
        /// If we are disposing
        /// </summary>
        protected bool _disposing;

        /// <summary>
        /// Whether a proxy is configured
        /// </summary>
        protected bool _proxyConfigured;

        /// <summary>
        /// Name of the client
        /// </summary>
        protected internal string ClientName
        {
            get
            {
                if (_clientName != null)
                    return _clientName;

                _clientName = GetType().Name;
                return _clientName;
            }
        }

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
        /// Mapping of a response code to known error types
        /// </summary>
        protected internal virtual ErrorMapping ErrorMapping { get; } = new ErrorMapping([]);

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="outputOriginalData">Should data from this client include the original data in the call result</param>
        /// <param name="baseAddress">Base address for this API client</param>
        /// <param name="clientOptions">Client options</param>
        /// <param name="apiOptions">Api options</param>
        protected BaseApiClient(
            ILogger logger,
            bool outputOriginalData,
            string baseAddress,
            ExchangeOptions clientOptions,
            ApiOptions apiOptions)
        {
            _logger = logger;

            ClientOptions = clientOptions;
            ApiOptions = apiOptions;
            OutputOriginalData = outputOriginalData;
            BaseAddress = baseAddress;

            _proxyConfigured = ClientOptions.Proxy != null;
        }

        /// <inheritdoc />
        public abstract string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null);

        /// <summary>
        /// Get error info for a response code
        /// </summary>
        public ErrorInfo GetErrorInfo(int code, string? message = null) => GetErrorInfo(code.ToString(), message);

        /// <summary>
        /// Get error info for a response code
        /// </summary>
        public ErrorInfo GetErrorInfo(string code, string? message = null) => ErrorMapping.GetErrorInfo(code.ToString(), message);

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            _disposing = true;
        }
    }
}

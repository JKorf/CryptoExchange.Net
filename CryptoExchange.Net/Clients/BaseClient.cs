using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net.Clients
{
    /// <summary>
    /// The base for all clients, websocket client and rest client
    /// </summary>
    public abstract class BaseClient : IDisposable
    {
        /// <summary>
        /// The name of the API the client is for
        /// </summary>
        public string Exchange { get; }

        /// <summary>
        /// Api clients in this client
        /// </summary>
        internal List<BaseApiClient> ApiClients { get; } = new List<BaseApiClient>();

        /// <summary>
        /// The log object
        /// </summary>
        protected internal ILogger _logger;

        /// <summary>
        /// Provided client options
        /// </summary>
        public ExchangeOptions ClientOptions { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="exchange">The name of the exchange this client is for</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected BaseClient(ILoggerFactory? logger, string exchange)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _logger = logger?.CreateLogger(exchange) ?? NullLoggerFactory.Instance.CreateLogger(exchange);

            Exchange = exchange;
        }

        /// <summary>
        /// Initialize the client with the specified options
        /// </summary>
        /// <param name="options"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected virtual void Initialize(ExchangeOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            ClientOptions = options;
            _logger.Log(LogLevel.Trace, $"Client configuration: {options}, CryptoExchange.Net: v{typeof(BaseClient).Assembly.GetName().Version}, {Exchange}.Net: v{GetType().Assembly.GetName().Version}");
        }

        /// <summary>
        /// Set the API credentials for this client. All Api clients in this client will use the new credentials, regardless of earlier set options.
        /// </summary>
        /// <param name="credentials">The credentials to set</param>
        protected virtual void SetApiCredentials<T>(T credentials) where T : ApiCredentials
        {
            foreach (var apiClient in ApiClients)
                apiClient.SetApiCredentials(credentials);
        }

        /// <summary>
        /// Register an API client
        /// </summary>
        /// <param name="apiClient">The client</param>
        protected T AddApiClient<T>(T apiClient) where T : BaseApiClient
        {
            if (ClientOptions == null)
                throw new InvalidOperationException("Client should have called Initialize before adding API clients");

            _logger.Log(LogLevel.Trace, $"  {apiClient.GetType().Name}, base address: {apiClient.BaseAddress}");
            ApiClients.Add(apiClient);
            return apiClient;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            _logger.Log(LogLevel.Debug, "Disposing client");
            foreach (var client in ApiClients)
                client.Dispose();
        }
    }
}

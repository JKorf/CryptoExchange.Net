using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace CryptoExchange.Net.Clients
{
    /// <inheritdoc />
    public abstract class UserClientProvider<TRestClient, TSocketClient, TRestOptions, TSocketOptions, TCredentials, TEnvironment> : IUserClientProvider<TRestClient, TSocketClient, TCredentials, TEnvironment> 
        where TRestClient : IRestClient<TCredentials>
        where TSocketClient : ISocketClient<TCredentials>
        where TRestOptions : RestExchangeOptions<TEnvironment, TCredentials>, new()
        where TSocketOptions : SocketExchangeOptions<TEnvironment, TCredentials>, new()
        where TCredentials : ApiCredentials
        where TEnvironment : TradeEnvironment
    {
        private ConcurrentDictionary<string, TRestClient> _restClients = new ConcurrentDictionary<string, TRestClient>();
        private ConcurrentDictionary<string, TSocketClient> _socketClients = new ConcurrentDictionary<string, TSocketClient>();

        private readonly IOptions<TRestOptions> _restOptions;
        private readonly IOptions<TSocketOptions> _socketOptions;
        private readonly HttpClient _httpClient;
        private readonly ILoggerFactory? _loggerFactory;

        /// <inheritdoc />
        public abstract string ExchangeName { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public UserClientProvider(
            HttpClient? httpClient,
            ILoggerFactory? loggerFactory,
            IOptions<TRestOptions> restOptions,
            IOptions<TSocketOptions> socketOptions)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = restOptions.Value.RequestTimeout;
            _loggerFactory = loggerFactory;
            _restOptions = restOptions;
            _socketOptions = socketOptions;
        }


        private IOptions<TRestOptions> SetRestEnvironment(IOptions<TRestOptions> options, TEnvironment? environment)
        {
            if (environment == null)
                return options;

            var newRestClientOptions = new TRestOptions();
            options.Value.Set(newRestClientOptions);
            newRestClientOptions.Environment = environment;
            return Options.Create(newRestClientOptions);
        }

        private IOptions<TSocketOptions> SetSocketEnvironment(IOptions<TSocketOptions> options, TEnvironment? environment)
        {
            if (environment == null)
                return options;

            var newSocketClientOptions = new TSocketOptions();
            options.Value.Set(newSocketClientOptions);
            newSocketClientOptions.Environment = environment;
            return Options.Create(newSocketClientOptions);
        }

        /// <inheritdoc />
        public void InitializeUserClient(string userIdentifier, TCredentials credentials, TEnvironment? environment = null)
        {
            CreateRestClient(userIdentifier, credentials, environment);
            CreateSocketClient(userIdentifier, credentials, environment);
        }

        /// <inheritdoc />
        public TRestClient GetRestClient(string userIdentifier, TCredentials? credentials = null, TEnvironment? environment = null)
        {
            if (!_restClients.TryGetValue(userIdentifier, out var client) || client.Disposed)
                client = CreateRestClient(userIdentifier, credentials, environment);

            return client;
        }

        /// <inheritdoc />
        public TSocketClient GetSocketClient(string userIdentifier, TCredentials? credentials = null, TEnvironment? environment = null)
        {
            if (!_socketClients.TryGetValue(userIdentifier, out var client) || client.Disposed)
                client = CreateSocketClient(userIdentifier, credentials, environment);

            return client;
        }

        private TRestClient CreateRestClient(string userIdentifier, TCredentials? credentials, TEnvironment? environment)
        {
            var clientRestOptions = SetRestEnvironment(_restOptions, environment);
            var client = ConstructRestClient(_httpClient, _loggerFactory, clientRestOptions);
            if (credentials != null)
            {
                _restClients[userIdentifier] = client;
                client.SetApiCredentials(credentials);
            }
            return client;
        }

        private TSocketClient CreateSocketClient(string userIdentifier, TCredentials? credentials, TEnvironment? environment)
        {
            var clientSocketOptions = SetSocketEnvironment(_socketOptions, environment);
            var client = ConstructSocketClient(_loggerFactory, clientSocketOptions);
            if (credentials != null)
            {
                _socketClients[userIdentifier] = client;
                client.SetApiCredentials(credentials);
            }
            return client;
        }

        /// <summary>
        /// Constructs a new instance of the rest client
        /// </summary>
        protected abstract TRestClient ConstructRestClient(
            HttpClient client,
            ILoggerFactory? loggerFactory,
            IOptions<TRestOptions> options);


        /// <summary>
        /// Constructs a new instance of the socket client
        /// </summary>
        protected abstract TSocketClient ConstructSocketClient(
            ILoggerFactory? loggerFactory,
            IOptions<TSocketOptions> options);


        /// <inheritdoc />
        public void ClearUserClients(string userIdentifier)
        {
            _restClients.TryRemove(userIdentifier, out var restClient);
            _socketClients.TryRemove(userIdentifier, out var socketClient);
            restClient?.Dispose();
            socketClient?.Dispose();
        }

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var client in _restClients.Values)
                client.Dispose();
            _restClients.Clear();

            foreach (var client in _socketClients.Values)
                client.Dispose();
            _socketClients.Clear();
        }

        /// <summary>
        /// Applies the provided options delegate to a new instance of the specified type.
        /// </summary>
        protected static T ApplyOptionsDelegate<T>(Action<T>? del) where T : new()
        {
            var opts = new T();
            del?.Invoke(opts);
            return opts;
        }
    }
}

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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Clients
{
    internal record UserClientKey(string UserIdentifier, string EnvironmentName);

    /// <summary>
    /// User client provider. Caches created client and resolves those on subsequent requests.
    /// </summary>
    /// <typeparam name="TRestClient">Rest client type</typeparam>
    /// <typeparam name="TRestOptions">Rest client options type</typeparam>
    /// <typeparam name="TCredentials">Credential type</typeparam>
    /// <typeparam name="TEnvironment">Environment type</typeparam>
    public abstract class UserClientProvider<TRestClient, TRestOptions, TCredentials, TEnvironment>
        where TRestClient : IRestClient<TCredentials>
        where TRestOptions : RestExchangeOptions<TEnvironment, TCredentials>, new()
        where TCredentials : ApiCredentials
        where TEnvironment : TradeEnvironment
    {
        /// <summary>
        /// Get the identifier for the environment
        /// </summary>
        protected static string GetEnvironmentCacheKey(TEnvironment? environment)
            => environment == null
                ? "default"
                : "environment:" + environment.Name;

        private ConcurrentDictionary<UserClientKey, TRestClient> _userRestClients = new();
        private ConcurrentDictionary<string, TRestClient> _publicRestClient = new();

        /// <summary>
        /// Rest options
        /// </summary>
        protected readonly IOptions<TRestOptions> _restOptions;
        /// <summary>
        /// Http client
        /// </summary>
        protected readonly HttpClient _httpClient;
        /// <summary>
        /// Public clients access lock
        /// </summary>
        protected readonly object _publicClientsLock = new();
        /// <summary>
        /// Logger factory
        /// </summary>
        protected readonly ILoggerFactory? _loggerFactory;
        /// <summary>
        /// API user credentials resolver
        /// </summary>
        protected readonly CredentialResolver<TCredentials, TEnvironment>? _credentialResolver; 
        /// <summary>
        /// Access semaphore
        /// </summary>
        protected readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <inheritdoc />
        public abstract string ExchangeName { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public UserClientProvider(
            HttpClient? httpClient,
            ILoggerFactory? loggerFactory,
            IOptions<TRestOptions> restOptions,
            CredentialResolver<TCredentials, TEnvironment>? credentialResolver)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = restOptions.Value.RequestTimeout;
            _loggerFactory = loggerFactory;
            _restOptions = restOptions;
            _credentialResolver = credentialResolver;
        }

        /// <summary>
        /// Create options with the specified environment
        /// </summary>
        protected IOptions<TRestOptions> SetRestEnvironment(IOptions<TRestOptions> options, TEnvironment? environment)
        {
            if (environment == null)
                return options;

            var newRestClientOptions = new TRestOptions();
            options.Value.Set(newRestClientOptions);
            newRestClientOptions.Environment = environment;
            return Options.Create(newRestClientOptions);
        }

        /// <summary>
        /// Initialize the credentials for a user
        /// </summary>
        public virtual void InitializeUserClient(string userIdentifier, TCredentials credentials, TEnvironment? environment = null)
        {
            CreateUserRestClient(userIdentifier, credentials, environment);
        }

        /// <summary>
        /// Get a public rest client
        /// </summary>
        /// <param name="environment">The environment</param>
        public TRestClient GetPublicRestClient(TEnvironment? environment = null)
        {
            var key = GetEnvironmentCacheKey(environment);

            lock (_publicClientsLock)
            {
                if (_publicRestClient.TryGetValue(key, out var client)
                    && !client.Disposed)
                {
                    return client;
                }

                client = ConstructRestClient(
                    _httpClient,
                    _loggerFactory,
                    SetRestEnvironment(_restOptions, environment));

                _publicRestClient[key] = client;
                return client;
            }
        }

        /// <inheritdoc />
        public TRestClient GetRestClient(string userIdentifier, TCredentials? credentials = null, TEnvironment? environment = null)
        {
            var key = new UserClientKey(userIdentifier, GetEnvironmentCacheKey(environment));
            if (!_userRestClients.TryGetValue(key, out var client) || client.Disposed)
                client = CreateUserRestClient(userIdentifier, credentials, environment);

            return client;
        }

        /// <inheritdoc />
        public async Task<CallResult<TRestClient>> TryGetRestClientAsync(string userIdentifier, TEnvironment? environment = null, CancellationToken ct = default)
        {
            var key = new UserClientKey(userIdentifier, GetEnvironmentCacheKey(environment));
            if (_userRestClients.TryGetValue(key, out var client) && !client.Disposed)
                return CallResult.Ok(client);

            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_userRestClients.TryGetValue(key, out client) && !client.Disposed)
                    return CallResult.Ok(client);

                return await TryCreateRestClientAsync(userIdentifier, environment, ct).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<CallResult<TRestClient>> TryCreateRestClientAsync(string userIdentifier, TEnvironment? environment, CancellationToken ct = default)
        {
            if (_credentialResolver == null)
                throw new InvalidOperationException("CredentialResolver not set");

            var clientRestOptions = SetRestEnvironment(_restOptions, environment);
            var client = ConstructRestClient(_httpClient, _loggerFactory, clientRestOptions);
            var credentials = await _credentialResolver.Invoke(userIdentifier, environment, ct).ConfigureAwait(false);
            if (credentials == null)
                return CallResult.Fail<TRestClient>(new NoApiCredentialsError("No API credentials returned by CredentialResolver"));

            var key = new UserClientKey(userIdentifier, GetEnvironmentCacheKey(environment));
            client.SetApiCredentials(credentials);
            _userRestClients[key] = client;
            return CallResult.Ok(client);
        }

        private TRestClient CreateUserRestClient(string userIdentifier, TCredentials? credentials, TEnvironment? environment)
        {
            var clientRestOptions = SetRestEnvironment(_restOptions, environment);
            var client = ConstructRestClient(_httpClient, _loggerFactory, clientRestOptions);
            if (credentials != null)
            {
                var key = new UserClientKey(userIdentifier, GetEnvironmentCacheKey(environment));
                _userRestClients[key] = client;
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

        /// <inheritdoc />
        public virtual void ClearUserClients(string userIdentifier, TEnvironment? environment = null)
        {
            var key = new UserClientKey(userIdentifier, GetEnvironmentCacheKey(environment));
            _userRestClients.TryRemove(key, out var restClient);
            restClient?.Dispose();
        }

        /// <summary>
        /// Clear and dispose all cached clients
        /// </summary>
        public virtual void Clear()
        {
            foreach (var client in _userRestClients.Values)
                client.Dispose();
            _userRestClients.Clear();
            TRestClient[] publicClients;

            lock (_publicClientsLock)
            {
                publicClients = _publicRestClient.Values.ToArray();
                _publicRestClient.Clear();
            }

            foreach (var client in publicClients)
                client.Dispose();
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

    /// <inheritdoc />
    public abstract class UserClientProvider<TRestClient, TSocketClient, TRestOptions, TSocketOptions, TCredentials, TEnvironment>
        : UserClientProvider<TRestClient, TRestOptions, TCredentials, TEnvironment>
        where TRestClient : IRestClient<TCredentials>
        where TSocketClient : ISocketClient<TCredentials>
        where TRestOptions : RestExchangeOptions<TEnvironment, TCredentials>, new()
        where TSocketOptions : SocketExchangeOptions<TEnvironment, TCredentials>, new()
        where TCredentials : ApiCredentials
        where TEnvironment : TradeEnvironment
    {
        private ConcurrentDictionary<UserClientKey, TSocketClient> _socketClients = new ConcurrentDictionary<UserClientKey, TSocketClient>();
        private ConcurrentDictionary<string, TSocketClient> _publicSocketClient = new();
        /// <summary>
        /// Socket client options
        /// </summary>
        protected readonly IOptions<TSocketOptions> _socketOptions;


        /// <summary>
        /// ctor
        /// </summary>
        public UserClientProvider(
            HttpClient? httpClient,
            ILoggerFactory? loggerFactory,
            IOptions<TRestOptions> restOptions,
            IOptions<TSocketOptions> socketOptions,
            CredentialResolver<TCredentials, TEnvironment>? credentialResolver)
            : base(httpClient, loggerFactory, restOptions, credentialResolver)
        {
            _socketOptions = socketOptions;
        }

        /// <summary>
        /// Create options with the specified environment
        /// </summary>
        protected IOptions<TSocketOptions> SetSocketEnvironment(IOptions<TSocketOptions> options, TEnvironment? environment)
        {
            if (environment == null)
                return options;

            var newSocketClientOptions = new TSocketOptions();
            options.Value.Set(newSocketClientOptions);
            newSocketClientOptions.Environment = environment;
            return Options.Create(newSocketClientOptions);
        }

        /// <inheritdoc />
        public override void InitializeUserClient(string userIdentifier, TCredentials credentials, TEnvironment? environment = null)
        {
            base.InitializeUserClient(userIdentifier, credentials, environment);
            CreateSocketClient(userIdentifier, credentials, environment);
        }

        /// <inheritdoc />
        public TSocketClient GetSocketClient(string userIdentifier, TCredentials? credentials = null, TEnvironment? environment = null)
        {
            var key = new UserClientKey(userIdentifier, GetEnvironmentCacheKey(environment));
            if (!_socketClients.TryGetValue(key, out var client) || client.Disposed)
                client = CreateSocketClient(userIdentifier, credentials, environment);

            return client;
        }

        /// <summary>
        /// Get a public socket client
        /// </summary>
        /// <param name="environment">The environment</param>
        public TSocketClient GetPublicSocketClient(TEnvironment? environment = null)
        {
            var key = GetEnvironmentCacheKey(environment);

            lock (_publicClientsLock)
            {
                if (_publicSocketClient.TryGetValue(key, out var client)
                    && !client.Disposed)
                {
                    return client;
                }

                client = ConstructSocketClient(
                    _loggerFactory,
                    SetSocketEnvironment(_socketOptions, environment));

                _publicSocketClient[key] = client;
                return client;
            }
        }

        /// <inheritdoc />
        public async Task<CallResult<TSocketClient>> TryGetSocketClientAsync(string userIdentifier, TEnvironment? environment = null, CancellationToken ct = default)
        {
            var key = new UserClientKey(userIdentifier, GetEnvironmentCacheKey(environment));
            if (_socketClients.TryGetValue(key, out var client) && !client.Disposed)
                return CallResult.Ok(client);

            await _semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_socketClients.TryGetValue(key, out client) && !client.Disposed)
                    return CallResult.Ok(client);

                return await TryCreateSocketClientAsync(userIdentifier, environment, ct).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<CallResult<TSocketClient>> TryCreateSocketClientAsync(string userIdentifier, TEnvironment? environment, CancellationToken ct = default)
        {
            var key = new UserClientKey(userIdentifier, GetEnvironmentCacheKey(environment));
            if (_credentialResolver == null)
                throw new InvalidOperationException("CredentialResolver not set");

            var clientSocketOptions = SetSocketEnvironment(_socketOptions, environment);
            var client = ConstructSocketClient(_loggerFactory, clientSocketOptions);
            var credentials = await _credentialResolver.Invoke(userIdentifier, environment, ct).ConfigureAwait(false);
            if (credentials == null)
                return CallResult.Fail<TSocketClient>(new NoApiCredentialsError("No API credentials returned by CredentialResolver"));

            client.SetApiCredentials(credentials);
            _socketClients[key] = client;
            return CallResult.Ok(client);
        }

        private TSocketClient CreateSocketClient(string userIdentifier, TCredentials? credentials, TEnvironment? environment)
        {
            var clientSocketOptions = SetSocketEnvironment(_socketOptions, environment);
            var client = ConstructSocketClient(_loggerFactory, clientSocketOptions);
            if (credentials != null)
            {
                var key = new UserClientKey(userIdentifier, GetEnvironmentCacheKey(environment));
                _socketClients[key] = client;
                client.SetApiCredentials(credentials);
            }
            return client;
        }

        /// <summary>
        /// Constructs a new instance of the socket client
        /// </summary>
        protected abstract TSocketClient ConstructSocketClient(
            ILoggerFactory? loggerFactory,
            IOptions<TSocketOptions> options);

        /// <inheritdoc />
        public override void ClearUserClients(string userIdentifier, TEnvironment? environment = null)
        {
            base.ClearUserClients(userIdentifier, environment);

            var key = new UserClientKey(userIdentifier, GetEnvironmentCacheKey(environment));
            _socketClients.TryRemove(key, out var socketClient);
            socketClient?.Dispose();
        }

        /// <inheritdoc />
        public override void Clear()
        {
            base.Clear();

            foreach (var client in _socketClients.Values)
                client.Dispose();
            _socketClients.Clear();
            TSocketClient[] publicClients;

            lock (_publicClientsLock)
            {
                publicClients = _publicSocketClient.Values.ToArray();
                _publicSocketClient.Clear();
            }

            foreach (var client in publicClients)
                client.Dispose();
        }
    }
}

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CryptoExchange.Net
{
    /// <summary>
    /// The base for all clients, websocket client and rest client
    /// </summary>
    public abstract class BaseClient : IDisposable
    {
        /// <summary>
        /// The name of the API the client is for
        /// </summary>
        internal string Name { get; }
        /// <summary>
        /// Api clients in this client
        /// </summary>
        internal List<BaseApiClient> ApiClients { get; } = new List<BaseApiClient>();
        /// <summary>
        /// The log object
        /// </summary>
        protected internal Log log;
        
        /// <summary>
        /// Provided client options
        /// </summary>
        public ClientOptions ClientOptions { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="name">The name of the API this client is for</param>
        /// <param name="options">The options for this client</param>
        protected BaseClient(string name, ClientOptions options)
        {
            log = new Log(name);
            log.UpdateWriters(options.LogWriters);
            log.Level = options.LogLevel;
            options.OnLoggingChanged += HandleLogConfigChange;

            ClientOptions = options;

            Name = name;

            log.Write(LogLevel.Trace, $"Client configuration: {options}, CryptoExchange.Net: v{typeof(BaseClient).Assembly.GetName().Version}, {name}.Net: v{GetType().Assembly.GetName().Version}");
        }

        /// <summary>
        /// Set the API credentials for this client. All Api clients in this client will use the new credentials, regardless of earlier set options.
        /// </summary>
        /// <param name="credentials">The credentials to set</param>
        public void SetApiCredentials(ApiCredentials credentials)
        {
            foreach (var apiClient in ApiClients)
                apiClient.SetApiCredentials(credentials);
        }

        /// <summary>
        /// Register an API client
        /// </summary>
        /// <param name="apiClient">The client</param>
        protected T AddApiClient<T>(T apiClient) where T:  BaseApiClient
        {
            log.Write(LogLevel.Trace, $"  {apiClient.GetType().Name} configuration: {apiClient.Options}");
            ApiClients.Add(apiClient);
            return apiClient;
        }

        /// <summary>
        /// Handle a change in the client options log config
        /// </summary>
        private void HandleLogConfigChange()
        {
            log.UpdateWriters(ClientOptions.LogWriters);
            log.Level = ClientOptions.LogLevel;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            log.Write(LogLevel.Debug, "Disposing client");
            ClientOptions.OnLoggingChanged -= HandleLogConfigChange;
            foreach (var client in ApiClients)
                client.Dispose();
        }
    }
}

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.Clients
{
    /// <summary>
    /// Base rest client
    /// </summary>
    public abstract class BaseRestClient : BaseClient, IRestClient
    {
        /// <inheritdoc />
        public int TotalRequestsMade => ApiClients.OfType<RestApiClient>().Sum(s => s.TotalRequestsMade);

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="loggerFactory">Logger factory</param>
        /// <param name="name">The name of the API this client is for</param>
        protected BaseRestClient(ILoggerFactory? loggerFactory, string name) : base(loggerFactory, name)
        {
            _logger = loggerFactory?.CreateLogger(name + ".RestClient") ?? NullLoggerFactory.Instance.CreateLogger(name);

            LibraryHelpers.StaticLogger = loggerFactory?.CreateLogger("CryptoExchange");
        }

    }

    public abstract class BaseRestClient<TEnvironment, TApiCredentials> : BaseRestClient, IRestClient<TApiCredentials>
        where TEnvironment : TradeEnvironment
        where TApiCredentials : ApiCredentials
    {
        /// <summary>
        /// Api clients in this client
        /// </summary>
        internal new List<RestApiClient<TEnvironment, TApiCredentials>> ApiClients => base.ApiClients.OfType<RestApiClient<TEnvironment, TApiCredentials>>().ToList();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="loggerFactory">Logger factory</param>
        /// <param name="name">The name of the API this client is for</param>
        protected BaseRestClient(ILoggerFactory? loggerFactory, string name) : base(loggerFactory, name)
        {
        }

        /// <summary>
        /// Set the API credentials for this client. All Api clients in this client will use the new credentials, regardless of earlier set options.
        /// </summary>
        /// <param name="credentials">The credentials to set</param>
        public void SetApiCredentials(TApiCredentials credentials)
        {
            foreach (var apiClient in ApiClients)
                apiClient.SetApiCredentials(credentials);
        }

        /// <summary>
        /// Update options
        /// </summary>
        public virtual void SetOptions(UpdateOptions<TApiCredentials> options)
        {
            foreach (var apiClient in ApiClients)
                apiClient.SetOptions(options);
        }

    }
}
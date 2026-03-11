using System.Linq;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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

    public abstract class BaseRestClient<TApiCredentials> : BaseRestClient, IRestClient<TApiCredentials>
        where TApiCredentials : ApiCredentials
    {
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
    }
}
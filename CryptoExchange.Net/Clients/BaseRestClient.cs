using System.Linq;
using CryptoExchange.Net.Interfaces;
using Microsoft.Extensions.Logging;

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
        }
    }
}

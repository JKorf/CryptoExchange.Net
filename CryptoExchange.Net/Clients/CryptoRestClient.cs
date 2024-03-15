using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.CommonClients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoExchange.Net.Clients
{
    /// <inheritdoc />
    public class CryptoRestClient : CryptoBaseClient, ICryptoRestClient
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CryptoRestClient()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public CryptoRestClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Get a list of the registered ISpotClient implementations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ISpotClient> GetSpotClients()
        {
            if (_serviceProvider == null)
                return new List<ISpotClient>();

            return _serviceProvider.GetServices<ISpotClient>().ToList();
        }

        /// <summary>
        /// Get an ISpotClient implementation by exchange name
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <returns></returns>
        public ISpotClient? SpotClient(string exchangeName) => _serviceProvider?.GetServices<ISpotClient>()?.SingleOrDefault(s => s.ExchangeName.Equals(exchangeName, StringComparison.InvariantCultureIgnoreCase));
    }
}

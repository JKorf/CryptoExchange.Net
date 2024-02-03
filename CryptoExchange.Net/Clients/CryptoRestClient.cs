using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.CommonClients;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoExchange.Net.Clients
{
    /// <inheritdoc />
    public class CryptoRestClient : CryptoBaseClient, ICryptoRestClient
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public CryptoRestClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        /// <summary>
        /// Try get a client by type for the service collection
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ISpotClient> GetSpotClients()
        {
            if (_serviceProvider == null)
                return new List<ISpotClient>();

            return _serviceProvider.GetServices<ISpotClient>().ToList();
        }
    }
}

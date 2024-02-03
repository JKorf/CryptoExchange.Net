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
    public class CryptoSocketClient : CryptoBaseClient, ICryptoSocketClient
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public CryptoSocketClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}

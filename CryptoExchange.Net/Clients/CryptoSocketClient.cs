using CryptoExchange.Net.Interfaces;
using System;

namespace CryptoExchange.Net.Clients
{
    /// <inheritdoc />
    public class CryptoSocketClient : CryptoBaseClient, ICryptoSocketClient
    {
        /// <summary>
        /// ctor
        /// </summary>
        public CryptoSocketClient()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public CryptoSocketClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}

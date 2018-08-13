using System;

namespace CryptoExchange.Net
{
    public class ApiProxy
    {
        /// <summary>
        /// The host address of the proxy
        /// </summary>
        public string Host { get; }
        /// <summary>
        /// The port of the proxy
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// Create new settings for a proxy
        /// </summary>
        /// <param name="host">The proxy hostname/ip</param>
        /// <param name="port">The proxy port</param>
        public ApiProxy(string host, int port)
        {
            if(string.IsNullOrEmpty(host) || port <=  0)
                throw new ArgumentException("Proxy host or port not filled");

            Host = host;
            Port = port;
        }
    }
}

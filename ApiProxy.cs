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

        public ApiProxy(string host, int port)
        {
            if(string.IsNullOrEmpty(host) || port <=  0)
                throw new ArgumentException("Proxy host or port not filled");

            Host = host;
            Port = port;
        }
    }
}

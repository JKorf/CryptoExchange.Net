using System;

namespace CryptoExchange.Net.Objects
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
        /// The login of the proxy
        /// </summary>
        public string Login { get; }

        /// <summary>
        /// The password of the proxy
        /// </summary>
        public string Password { get; }

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

        /// <inheritdoc />
        /// <summary>
        /// Create new settings for a proxy
        /// </summary>
        /// <param name="host">The proxy hostname/ip</param>
        /// <param name="port">The proxy port</param>
        /// <param name="login">The proxy login</param>
        /// <param name="password">The proxy password</param>
        public ApiProxy(string host, int port, string login, string password) : this(host, port)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Proxy login or password not filled");

            Login = login;
            Password = password;
        }
    }
}

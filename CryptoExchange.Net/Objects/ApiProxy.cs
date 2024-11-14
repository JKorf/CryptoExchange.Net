namespace CryptoExchange.Net.Objects
{
    /// <summary>
    /// Proxy info
    /// </summary>
    public class ApiProxy
    {
        /// <summary>
        /// The host address of the proxy
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// The port of the proxy
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The login of the proxy
        /// </summary>
        public string? Login { get; set; }

        /// <summary>
        /// The password of the proxy
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Create new settings for a proxy
        /// </summary>
        /// <param name="host">The proxy hostname/ip</param>
        /// <param name="port">The proxy port</param>
        /// <param name="login">The proxy login</param>
        /// <param name="password">The proxy password</param>
        public ApiProxy(string host, int port, string? login = null, string? password = null)
        {
            Host = host;
            Port = port;
            Login = login;
            Password = password;
        }
    }
}

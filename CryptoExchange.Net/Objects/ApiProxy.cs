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
        /// ctor
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ApiProxy() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        /// <summary>
        /// Create new settings for a proxy
        /// </summary>
        /// <param name="host">The proxy hostname/ip</param>
        /// <param name="port">The proxy port</param>
        public ApiProxy(string host, int port): this(host, port, null, null)
        {
        }

        /// <summary>
        /// Create new settings for a proxy
        /// </summary>
        /// <param name="host">The proxy hostname/ip</param>
        /// <param name="port">The proxy port</param>
        /// <param name="login">The proxy login</param>
        /// <param name="password">The proxy password</param>
        public ApiProxy(string host, int port, string? login, string? password)
        {
            Host = host;
            Port = port;
            Login = login;
            Password = password;
        }
    }
}

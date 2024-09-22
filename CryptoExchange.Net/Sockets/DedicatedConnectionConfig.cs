namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Dedicated connection configuration
    /// </summary>
    public class DedicatedConnectionConfig
    {
        /// <summary>
        /// Socket address
        /// </summary>
        public string SocketAddress { get; set; } = string.Empty;
        /// <summary>
        /// authenticated
        /// </summary>
        public bool Authenticated { get; set; }
    }
}

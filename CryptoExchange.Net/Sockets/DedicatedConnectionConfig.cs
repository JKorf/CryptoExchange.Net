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

    /// <summary>
    /// Dedicated connection state
    /// </summary>
    public class DedicatedConnectionState
    {
        /// <summary>
        /// Whether the connection is a dedicated request connection
        /// </summary>
        public bool IsDedicatedRequestConnection { get; set; }
        /// <summary>
        /// Whether the dedication request connection should be authenticated
        /// </summary>
        public bool Authenticated { get; set; }
    }
}

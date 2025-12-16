using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets.Default.Interfaces;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Sockets.HighPerf.Interfaces
{
    /// <summary>
    /// Factory for creating connections
    /// </summary>
    public interface IHighPerfConnectionFactory
    {
        /// <summary>
        /// Create a new websocket connection
        /// </summary>
        HighPerfSocketConnection<T> CreateHighPerfConnection<T>(
            ILogger logger, IWebsocketFactory factory, WebSocketParameters parameters, SocketApiClient client, string address);
    }
}

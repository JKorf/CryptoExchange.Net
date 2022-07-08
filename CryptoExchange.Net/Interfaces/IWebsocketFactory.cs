using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Sockets;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Websocket factory interface
    /// </summary>
    public interface IWebsocketFactory
    {
        /// <summary>
        /// Create a websocket for an url
        /// </summary>
        /// <param name="log">The logger</param>
        /// <param name="parameters">The parameters to use for the connection</param>
        /// <returns></returns>
        IWebsocket CreateWebsocket(Log log, WebSocketParameters parameters);
    }
}

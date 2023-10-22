using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;

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
        /// <param name="logger">The logger</param>
        /// <param name="parameters">The parameters to use for the connection</param>
        /// <returns></returns>
        IWebsocket CreateWebsocket(ILogger logger, WebSocketParameters parameters);
    }
}

using CryptoExchange.Net.Interfaces;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Default websocket factory implementation
    /// </summary>
    public class WebsocketFactory : IWebsocketFactory
    {
        /// <inheritdoc />
        public IWebsocket CreateWebsocket(ILogger logger, WebSocketParameters parameters)
        {
            return new CryptoExchangeWebSocketClient(logger, parameters);
        }
    }
}

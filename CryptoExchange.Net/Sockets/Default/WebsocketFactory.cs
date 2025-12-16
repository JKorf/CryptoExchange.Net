using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets.Default.Interfaces;
using CryptoExchange.Net.Sockets.HighPerf;
using CryptoExchange.Net.Sockets.HighPerf.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO.Pipelines;

namespace CryptoExchange.Net.Sockets.Default
{
    /// <summary>
    /// Default websocket factory implementation
    /// </summary>
    public class WebsocketFactory : IWebsocketFactory
    {
        /// <inheritdoc />
        public IWebsocket CreateWebsocket(ILogger logger, SocketConnection connection, WebSocketParameters parameters)
        {
            return new CryptoExchangeWebSocketClient(logger, connection, parameters);
        }
        /// <inheritdoc />
        public IHighPerfWebsocket CreateHighPerfWebsocket(ILogger logger, WebSocketParameters parameters, PipeWriter pipeWriter)
        {
            return new HighPerfWebSocketClient(logger, parameters, pipeWriter);
        }
    }
}

using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

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
        /// <inheritdoc />
        public IHighPerfWebsocket CreateHighPerfWebsocket(ILogger logger, WebSocketParameters parameters, PipeWriter pipeWriter)
        {
            return new HighPerfWebSocketClient(logger, parameters, pipeWriter);
        }
    }
}

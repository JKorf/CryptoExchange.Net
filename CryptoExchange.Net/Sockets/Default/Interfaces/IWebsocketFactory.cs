using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets.HighPerf.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO.Pipelines;

namespace CryptoExchange.Net.Sockets.Default.Interfaces
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
        /// <param name="connection">The socket connection</param>
        /// <param name="parameters">The parameters to use for the connection</param>
        /// <returns></returns>
        IWebsocket CreateWebsocket(ILogger logger, SocketConnection connection, WebSocketParameters parameters);
        
        /// <summary>
        /// Create high performance websocket
        /// </summary>
        IHighPerfWebsocket CreateHighPerfWebsocket(ILogger logger, WebSocketParameters parameters, PipeWriter pipeWriter);
    }
}

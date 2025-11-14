using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

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
        
        IHighPerfWebsocket CreateHighPerfWebsocket(ILogger logger, WebSocketParameters parameters, PipeWriter pipeWriter);
    }
}

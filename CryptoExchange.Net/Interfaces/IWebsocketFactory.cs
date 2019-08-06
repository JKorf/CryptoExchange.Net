using System.Collections.Generic;
using CryptoExchange.Net.Logging;

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
        /// <param name="log"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        IWebsocket CreateWebsocket(Log log, string url);
        /// <summary>
        /// Create a websocket for an url
        /// </summary>
        /// <param name="log"></param>
        /// <param name="url"></param>
        /// <param name="cookies"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        IWebsocket CreateWebsocket(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers);
    }
}

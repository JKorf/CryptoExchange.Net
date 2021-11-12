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
        /// <param name="log">The logger</param>
        /// <param name="url">The url the socket is fo</param>
        /// <returns></returns>
        IWebsocket CreateWebsocket(Log log, string url);
        /// <summary>
        /// Create a websocket for an url
        /// </summary>
        /// <param name="log">The logger</param>
        /// <param name="url">The url the socket is fo</param>
        /// <param name="cookies">Cookies to be send in the initial request</param>
        /// <param name="headers">Headers to be send in the initial request</param>
        /// <returns></returns>
        IWebsocket CreateWebsocket(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers);
    }
}

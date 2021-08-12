using System.Collections.Generic;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Default weboscket factory implementation
    /// </summary>
    public class WebsocketFactory : IWebsocketFactory
    {
        /// <inheritdoc />
        public IWebsocket CreateWebsocket(Log log, string url)
        {
            return new CryptoExchangeWebSocketClient(log, url);
        }

        /// <inheritdoc />
        public IWebsocket CreateWebsocket(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers)
        {
            return new CryptoExchangeWebSocketClient(log, url, cookies, headers);
        }
    }
}

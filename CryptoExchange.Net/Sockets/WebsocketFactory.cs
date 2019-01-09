using System.Collections.Generic;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;

namespace CryptoExchange.Net.Sockets
{
    public class WebsocketFactory : IWebsocketFactory
    {
        public IWebsocket CreateWebsocket(Log log, string url)
        {
            return new BaseSocket(log, url);
        }

        public IWebsocket CreateWebsocket(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers)
        {
            return new BaseSocket(log, url, cookies, headers);
        }
    }
}

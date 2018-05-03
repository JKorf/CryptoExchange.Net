using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;

namespace CryptoExchange.Net.Implementation
{
    public class WebsocketFactory : IWebsocketFactory
    {
        public IWebsocket CreateWebsocket(Log log, string url)
        {
            return new BaseSocket(log, url);
        }
    }
}

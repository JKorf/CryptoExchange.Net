using CryptoExchange.Net.Interfaces;

namespace CryptoExchange.Net.Implementation
{
    public class WebsocketFactory : IWebsocketFactory
    {
        public IWebsocket CreateWebsocket(string url)
        {
            return new BaseSocket(url);
        }
    }
}

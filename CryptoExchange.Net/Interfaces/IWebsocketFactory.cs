using CryptoExchange.Net.Logging;

namespace CryptoExchange.Net.Interfaces
{
    public interface IWebsocketFactory
    {
        IWebsocket CreateWebsocket(Log log, string url);
    }
}

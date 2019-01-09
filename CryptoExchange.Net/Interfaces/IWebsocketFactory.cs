using System.Collections.Generic;
using CryptoExchange.Net.Logging;

namespace CryptoExchange.Net.Interfaces
{
    public interface IWebsocketFactory
    {
        IWebsocket CreateWebsocket(Log log, string url);
        IWebsocket CreateWebsocket(Log log, string url, IDictionary<string, string> cookies, IDictionary<string, string> headers);
    }
}

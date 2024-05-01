using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Testing.Implementations
{
    internal class TestWebsocketFactory : IWebsocketFactory
    {
        private readonly TestSocket _socket;
        public TestWebsocketFactory(TestSocket socket)
        {
            _socket = socket;
        }

        public IWebsocket CreateWebsocket(ILogger logger, WebSocketParameters parameters) => _socket;
    }
}

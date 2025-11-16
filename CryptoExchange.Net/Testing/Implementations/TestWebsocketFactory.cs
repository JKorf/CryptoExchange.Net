using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Testing.Implementations
{
    internal class TestWebsocketFactory : IWebsocketFactory
    {
        private readonly TestSocket _socket;
        public TestWebsocketFactory(TestSocket socket)
        {
            _socket = socket;
        }

        public IHighPerfWebsocket CreateHighPerfWebsocket(ILogger logger, WebSocketParameters parameters, PipeWriter pipeWriter) => throw new NotImplementedException();
        public IWebsocket CreateWebsocket(ILogger logger, SocketConnection connection, WebSocketParameters parameters) => _socket;
    }
}

using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Interfaces;
using CryptoExchange.Net.Sockets.HighPerf.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipelines;

namespace CryptoExchange.Net.Testing.Implementations
{
    internal class TestWebsocketFactory : IWebsocketFactory
    {
        private readonly TestSocket _socket;
        public TestWebsocketFactory(TestSocket socket)
        {
            _socket = socket;
        }

        public IHighPerfWebsocket CreateHighPerfWebsocket(ILogger logger, WebSocketParameters parameters, PipeWriter pipeWriter) 
            => throw new NotImplementedException();
        
        public IWebsocket CreateWebsocket(ILogger logger, SocketConnection connection, WebSocketParameters parameters)
        {
            _socket.Connection = connection;
            return _socket;
        }
    }
}

using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoExchange.Net.Objects.Testing
{
    public class TestWebsocketFactory : IWebsocketFactory
    {
        private readonly Func<ILogger, WebSocketParameters, IWebsocket> _websocketFactory;

        public TestWebsocketFactory(Func<ILogger, WebSocketParameters, IWebsocket> websocketFactory)
        {
            _websocketFactory = websocketFactory;
        }

        public IWebsocket CreateWebsocket(ILogger logger, WebSocketParameters parameters)
        {
            return _websocketFactory(logger, parameters);
        }
    }
}

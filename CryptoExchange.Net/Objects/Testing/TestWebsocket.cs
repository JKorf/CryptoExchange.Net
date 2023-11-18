using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.Objects.Testing
{
    public class TestWebsocket : CryptoExchangeWebSocketClient
    {
        public TestWebsocket(ILogger logger, WebSocketParameters websocketParameters) : base(logger, websocketParameters)
        {
        }

        public override bool IsClosed => false;
        public override bool IsOpen => true;

        public override Task<bool> ConnectAsync() => Task.FromResult(true);

        public override Task CloseAsync() => Task.CompletedTask;

        public override Task ReconnectAsync() => Task.CompletedTask;

        public override void Send(int id, string data, int weight) { }

        public void Receive(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var stream = new MemoryStream(bytes);
            stream.Position = 0;
            _ = ProcessData(System.Net.WebSockets.WebSocketMessageType.Text, stream);
        }
    }
}

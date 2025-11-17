using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CryptoExchange.Net.Sockets.HighPerf
{
    public class HighPerfJsonConnectionFactory : IHighPerfConnectionFactory
    {
        private readonly JsonSerializerOptions _options;

        public HighPerfJsonConnectionFactory(JsonSerializerOptions options)
        {
            _options = options;
        }

        public HighPerfSocketConnection<T> CreateHighPerfConnection<T>(
            ILogger logger, IWebsocketFactory factory, WebSocketParameters parameters, SocketApiClient client, string address)
        {
            return new HighPerfJsonSocketConnection<T>(logger, factory, parameters, client, _options, address);
        }
    }
}

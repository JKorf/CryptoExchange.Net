using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CryptoExchange.Net.Sockets.HighPerf
{
    /// <inheritdoc />
    public class HighPerfJsonSocketConnectionFactory : IHighPerfConnectionFactory
    {
        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// ctor
        /// </summary>
        public HighPerfJsonSocketConnectionFactory(JsonSerializerOptions options)
        {
            _options = options;
        }

        /// <inheritdoc />
        public HighPerfSocketConnection<T> CreateHighPerfConnection<T>(
            ILogger logger, IWebsocketFactory factory, WebSocketParameters parameters, SocketApiClient client, string address)
        {
            return new HighPerfJsonSocketConnection<T>(logger, factory, parameters, client, _options, address);
        }
    }
}

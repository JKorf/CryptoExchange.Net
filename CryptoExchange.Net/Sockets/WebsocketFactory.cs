using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.OpenTelemetry;
using Microsoft.Extensions.Logging;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Default websocket factory implementation
    /// </summary>
    public class WebsocketFactory : IWebsocketFactory
    {
        /// <inheritdoc />
        public IWebsocket CreateWebsocket(ILogger logger, WebSocketParameters parameters, Telemetry? telemetry)
        {
            return new CryptoExchangeWebSocketClient(logger, parameters, telemetry);
        }
    }
}

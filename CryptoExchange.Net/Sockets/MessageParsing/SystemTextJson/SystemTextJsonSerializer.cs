using CryptoExchange.Net.Sockets.MessageParsing.Interfaces;
using System.Text.Json;

namespace CryptoExchange.Net.Sockets.MessageParsing.SystemTextJson
{
    /// <inheritdoc />
    public class SystemTextJsonSerializer : IMessageSerializer
    {
        /// <inheritdoc />
        public string Serialize(object message) => JsonSerializer.Serialize(message);
    }
}

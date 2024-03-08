using CryptoExchange.Net.Interfaces;
using System.Text.Json;

namespace CryptoExchange.Net.Converters.SystemTextJson
{
    /// <inheritdoc />
    public class SystemTextJsonMessageSerializer : IMessageSerializer
    {
        /// <inheritdoc />
        public string Serialize(object message) => JsonSerializer.Serialize(message, SerializerOptions.WithConverters);
    }
}

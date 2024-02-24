using CryptoExchange.Net.Sockets.MessageParsing.Interfaces;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Sockets.MessageParsing
{
    /// <inheritdoc />
    public class JsonNetSerializer : IMessageSerializer
    {
        /// <inheritdoc />
        public string Serialize(object message) => JsonConvert.SerializeObject(message, Formatting.None);
    }
}

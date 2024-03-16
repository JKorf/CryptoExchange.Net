using CryptoExchange.Net.Interfaces;
using Newtonsoft.Json;

namespace CryptoExchange.Net.Converters.JsonNet
{
    /// <inheritdoc />
    public class JsonNetMessageSerializer : IMessageSerializer
    {
        /// <inheritdoc />
        public string Serialize(object message) => JsonConvert.SerializeObject(message, Formatting.None);
    }
}

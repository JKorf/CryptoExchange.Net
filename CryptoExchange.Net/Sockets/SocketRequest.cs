using Newtonsoft.Json;

namespace CryptoExchange.Net.Sockets
{
    public class SocketRequest
    {
        [JsonIgnore]
        public bool Signed { get; set; }
    }
}

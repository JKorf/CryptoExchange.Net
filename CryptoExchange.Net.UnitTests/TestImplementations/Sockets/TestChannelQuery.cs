using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoExchange.Net.UnitTests.TestImplementations.Sockets
{
    internal class SubResponse
    {

        [JsonProperty("action")]
        public string Action { get; set; } = null!;

        [JsonProperty("channel")]
        public string Channel { get; set; } = null!;

        [JsonProperty("status")]
        public string Status { get; set; } = null!;
    }

    internal class UnsubResponse
    {
        [JsonProperty("action")]
        public string Action { get; set; } = null!;

        [JsonProperty("status")]
        public string Status { get; set; } = null!;
    }

    internal class TestChannelQuery : Query<SubResponse>
    {
        public override HashSet<string> ListenerIdentifiers { get; set; }

        public TestChannelQuery(string channel, string request, bool authenticated, int weight = 1) : base(request, authenticated, weight)
        {
            ListenerIdentifiers = new HashSet<string> { request + "-" + channel };
        }

        public override CallResult<SubResponse> HandleMessage(SocketConnection connection, DataEvent<SubResponse> message)
        {
            if (!message.Data.Status.Equals("confirmed", StringComparison.OrdinalIgnoreCase))
            {
                return new CallResult<SubResponse>(new ServerError(message.Data.Status));
            }

            return base.HandleMessage(connection, message);
        }
    }
}

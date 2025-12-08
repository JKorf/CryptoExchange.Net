using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using System;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.UnitTests.TestImplementations.Sockets
{
    internal class SubResponse
    {

        [JsonPropertyName("action")]
        public string Action { get; set; } = null!;

        [JsonPropertyName("channel")]
        public string Channel { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }

    internal class UnsubResponse
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }

    internal class TestChannelQuery : Query<SubResponse>
    {
        public TestChannelQuery(string channel, string request, bool authenticated, int weight = 1) : base(request, authenticated, weight)
        {
            MessageMatcher = MessageMatcher.Create<SubResponse>(request + "-" + channel, HandleMessage);
        }

        public CallResult<SubResponse> HandleMessage(SocketConnection connection, DataEvent<SubResponse> message)
        {
            if (!message.Data.Status.Equals("confirmed", StringComparison.OrdinalIgnoreCase))
            {
                return new CallResult<SubResponse>(new ServerError(ErrorInfo.Unknown with { Message = message.Data.Status }));
            }

            return message.ToCallResult();
        }
    }
}

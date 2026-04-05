using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.UnitTests.Implementations
{
    internal record TestSocketMessage
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;
    }
}

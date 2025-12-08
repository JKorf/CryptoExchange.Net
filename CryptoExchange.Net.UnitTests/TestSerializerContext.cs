using CryptoExchange.Net.UnitTests.TestImplementations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.UnitTests
{
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(IDictionary<string, string>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    [JsonSerializable(typeof(IDictionary<string, object>))]
    [JsonSerializable(typeof(TestObject))]
    internal partial class TestSerializerContext : JsonSerializerContext
    {
    }
}

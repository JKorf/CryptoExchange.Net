using System.Text.Json.Serialization;

namespace CryptoExchange.Net.UnitTests.TestImplementations
{
    public class TestObject
    {
        [JsonPropertyName("other")]
        public string StringData { get; set; }
        [JsonPropertyName("intData")]
        public int IntData { get; set; }
        [JsonPropertyName("decimalData")]
        public decimal DecimalData { get; set; }
    }
}

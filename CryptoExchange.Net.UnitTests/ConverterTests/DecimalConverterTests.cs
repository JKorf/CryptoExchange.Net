using CryptoExchange.Net.Converters.SystemTextJson;
using NUnit.Framework;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.UnitTests.ConverterTests
{
    public class DecimalConverterTests
    {
        [TestCase("1", 1)]
        [TestCase("1.1", 1.1)]
        [TestCase("-1.1", -1.1)]
        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("null", null)]
        [TestCase("nan", null)]
        [TestCase("1E+2", 100)]
        [TestCase("1E-2", 0.01)]
        [TestCase("Infinity", 999)] // 999 is workaround for not being able to specify decimal.MinValue
        [TestCase("-Infinity", -999)] // -999 is workaround for not being able to specify decimal.MaxValue
        [TestCase("80228162514264337593543950335", 999)] // 999 is workaround for not being able to specify decimal.MaxValue
        [TestCase("-80228162514264337593543950335", -999)] // -999 is workaround for not being able to specify decimal.MaxValue
        public void TestDecimalConverterString(string value, decimal? expected)
        {
            var result = JsonSerializer.Deserialize<STJDecimalObject>("{ \"test\": \"" + value + "\"}");
            Assert.That(result!.Test, Is.EqualTo(expected == -999 ? decimal.MinValue : expected == 999 ? decimal.MaxValue : expected));
        }

        [TestCase("1", 1)]
        [TestCase("1.1", 1.1)]
        [TestCase("-1.1", -1.1)]
        [TestCase("null", null)]
        [TestCase("1E+2", 100)]
        [TestCase("1E-2", 0.01)]
        [TestCase("80228162514264337593543950335", -999)] // -999 is workaround for not being able to specify decimal.MaxValue
        public void TestDecimalConverterNumber(string value, decimal? expected)
        {
            var result = JsonSerializer.Deserialize<STJDecimalObject>("{ \"test\": " + value + "}");
            Assert.That(result!.Test, Is.EqualTo(expected == -999 ? decimal.MaxValue : expected));
        }
    }

    public class STJDecimalObject
    {
        [JsonConverter(typeof(DecimalConverter))]
        [JsonPropertyName("test")]
        public decimal? Test { get; set; }
    }
}
